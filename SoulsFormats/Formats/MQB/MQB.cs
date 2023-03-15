using System;
using System.Collections.Generic;
using SoulsFormats.Util;

namespace SoulsFormats {
    /// <summary>
    /// A cutscene definition format used since DS2, short for MovieSequencer Binary. Extension: .mqb
    /// </summary>
    public partial class MQB : SoulsFile<MQB> {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public enum MQBVersion : uint {
            DarkSouls2 = 0x94,
            DarkSouls2Scholar = 0xCA,
            Bloodborne = 0xCB,
            DarkSouls3 = 0xCC,
        }

        public bool BigEndian { get; set; }

        public MQBVersion Version { get; set; }

        public string Name { get; set; }

        public float Framerate { get; set; }

        public List<Resource> Resources { get; set; }

        public List<Cut> Cuts { get; set; }

        public string ResourceDirectory { get; set; }

        protected internal override bool Is(BinaryReaderEx br) => br.Length >= 4 && br.GetASCII(0, 4) == "MQB ";

        protected internal override void Read(BinaryReaderEx br) {
            _ = br.AssertASCII("MQB ");
            br.BigEndian = this.BigEndian = br.AssertSByte(0, -1) == -1;
            _ = br.AssertByte(0);
            sbyte longFormat = br.AssertSByte(0, -1);
            _ = br.AssertByte(0);
            this.Version = br.ReadEnum32<MQBVersion>();
            int headerSize = br.ReadInt32();

            if (this.Version != MQBVersion.DarkSouls2Scholar && longFormat == -1
                || this.Version == MQBVersion.DarkSouls2Scholar && longFormat == 0) {
                throw new FormatException($"Unexpected long format {longFormat} for version {this.Version}.");
            }

            if (this.Version == MQBVersion.DarkSouls2 && headerSize != 0x14
                || this.Version == MQBVersion.DarkSouls2Scholar && headerSize != 0x28
                || this.Version == MQBVersion.Bloodborne && headerSize != 0x20
                || this.Version == MQBVersion.DarkSouls3 && headerSize != 0x24) {
                throw new FormatException($"Unexpected header size {headerSize} for version {this.Version}.");
            }

            br.VarintLong = this.Version == MQBVersion.DarkSouls2Scholar;
            long resourcePathsOffset = br.ReadVarint();
            if (this.Version == MQBVersion.DarkSouls2Scholar) {
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);
            } else if (this.Version >= MQBVersion.Bloodborne) {
                _ = br.AssertInt32(1);
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);
                if (this.Version >= MQBVersion.DarkSouls3) {
                    _ = br.AssertInt32(0);
                }
            }

            this.Name = br.ReadFixStrW(0x40);
            this.Framerate = br.ReadSingle();
            int resourceCount = br.ReadInt32();
            int cutCount = br.ReadInt32();
            _ = br.AssertInt32(0);
            _ = br.AssertInt32(0);
            _ = br.AssertInt32(0);
            _ = br.AssertInt32(0);
            _ = br.AssertInt32(0);

            this.Resources = new List<Resource>(resourceCount);
            for (int i = 0; i < resourceCount; i++) {
                this.Resources.Add(new Resource(br, i));
            }

            this.Cuts = new List<Cut>(cutCount);
            for (int i = 0; i < cutCount; i++) {
                this.Cuts.Add(new Cut(br, this.Version));
            }

            br.Position = resourcePathsOffset;
            long[] resourcePathOffsets = br.ReadVarints(resourceCount);
            this.ResourceDirectory = br.ReadUTF16();
            for (int i = 0; i < resourceCount; i++) {
                long offset = resourcePathOffsets[i];
                if (offset != 0) {
                    this.Resources[i].Path = br.GetUTF16(offset);
                }
            }
        }

        protected internal override void Write(BinaryWriterEx bw) {
            bw.BigEndian = this.BigEndian;
            bw.VarintLong = this.Version == MQBVersion.DarkSouls2Scholar;

            bw.WriteASCII("MQB ");
            bw.WriteSByte((sbyte)(this.BigEndian ? -1 : 0));
            bw.WriteByte(0);
            bw.WriteSByte((sbyte)(this.Version == MQBVersion.DarkSouls2Scholar ? -1 : 0));
            bw.WriteByte(0);
            bw.WriteUInt32((uint)this.Version);
            switch (this.Version) {
                case MQBVersion.DarkSouls2: bw.WriteInt32(0x14); break;
                case MQBVersion.DarkSouls2Scholar: bw.WriteInt32(0x28); break;
                case MQBVersion.Bloodborne: bw.WriteInt32(0x20); break;
                case MQBVersion.DarkSouls3: bw.WriteInt32(0x24); break;
                default:
                    throw new NotImplementedException($"Missing header size for version {this.Version}.");
            }

            bw.ReserveVarint("ResourcePathsOffset");
            if (this.Version == MQBVersion.DarkSouls2Scholar) {
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
            } else if (this.Version >= MQBVersion.Bloodborne) {
                bw.WriteInt32(1);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                if (this.Version >= MQBVersion.DarkSouls3) {
                    bw.WriteInt32(0);
                }
            }

            bw.WriteFixStrW(this.Name, 0x40, 0x00);
            bw.WriteSingle(this.Framerate);
            bw.WriteInt32(this.Resources.Count);
            bw.WriteInt32(this.Cuts.Count);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);

            var allCustomData = new List<CustomData>();
            var customDataValueOffsets = new List<long>();

            for (int i = 0; i < this.Resources.Count; i++) {
                this.Resources[i].Write(bw, i, allCustomData, customDataValueOffsets);
            }

            var offsetsByDispos = new Dictionary<Disposition, long>();
            for (int i = 0; i < this.Cuts.Count; i++) {
                this.Cuts[i].Write(bw, this.Version, offsetsByDispos, i, allCustomData, customDataValueOffsets);
            }

            for (int i = 0; i < this.Cuts.Count; i++) {
                this.Cuts[i].WriteTimelines(bw, this.Version, i);
            }

            for (int i = 0; i < this.Cuts.Count; i++) {
                this.Cuts[i].WriteTimelineCustomData(bw, i, allCustomData, customDataValueOffsets);
            }

            for (int i = 0; i < this.Cuts.Count; i++) {
                this.Cuts[i].WriteDisposOffsets(bw, offsetsByDispos, i);
            }

            bw.FillVarint("ResourcePathsOffset", bw.Position);
            for (int i = 0; i < this.Resources.Count; i++) {
                bw.ReserveVarint($"ResourcePathOffset{i}");
            }

            bw.WriteUTF16(this.ResourceDirectory, true);
            for (int i = 0; i < this.Resources.Count; i++) {
                if (this.Resources[i].Path == null) {
                    bw.FillVarint($"ResourcePathOffset{i}", 0);
                } else {
                    bw.FillVarint($"ResourcePathOffset{i}", bw.Position);
                    bw.WriteUTF16(this.Resources[i].Path, true);
                }
            }

            // I know this is weird, but trust me.
            if (this.Version >= MQBVersion.Bloodborne) {
                bw.WriteInt16(0);
                bw.Pad(4);
            }

            for (int i = 0; i < allCustomData.Count; i++) {
                allCustomData[i].WriteSequences(bw, i, customDataValueOffsets[i]);
            }

            for (int i = 0; i < allCustomData.Count; i++) {
                allCustomData[i].WriteSequencePoints(bw, i);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
