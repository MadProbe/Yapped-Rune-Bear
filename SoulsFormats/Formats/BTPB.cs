using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using SoulsFormats.Util;

namespace SoulsFormats.Formats {
    /// <summary>
    /// A collection of spherical harmonics light probes for lighting characters and objects in a map. Extension: .btpb
    /// </summary>
    public class BTPB : SoulsFile<BTPB> {
        /// <summary>
        /// Indicates the format of the file and supported features.
        /// </summary>
        public BTPBVersion Version { get; set; }

        /// <summary>
        /// Unknown; probably bounding box min.
        /// </summary>
        public Vector3 Unk1C { get; set; }

        /// <summary>
        /// Unknown; probably bounding box max.
        /// </summary>
        public Vector3 Unk28 { get; set; }

        /// <summary>
        /// Groups of light probes in the map.
        /// </summary>
        public List<Group> Groups { get; set; }

        /// <summary>
        /// Creates an empty BTPB formatted for Dark Souls 3.
        /// </summary>
        public BTPB() {
            this.Version = BTPBVersion.DarkSouls3;
            this.Groups = new List<Group>();
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            bool bigEndian = br.BigEndian = br.GetBoolean(0x10);

            int unk00 = br.AssertInt32(2, 3);
            int unk04 = br.AssertInt32(0, 1);
            int groupCount = br.ReadInt32();
            int dataLength = br.ReadInt32();
            _ = br.AssertBoolean(bigEndian);
            br.AssertPattern(3, 0x00);
            int groupSize = br.AssertInt32(0x40, 0x48, 0x98);
            int probeSize = br.AssertInt32(0x1C, 0x48);
            this.Unk1C = br.ReadVector3();
            this.Unk28 = br.ReadVector3();
            _ = br.AssertInt64(0);

            this.Version = !bigEndian && unk00 == 2 && unk04 == 1 && groupSize == 0x40 && probeSize == 0x1C
                ? BTPBVersion.DarkSouls2LE
                : bigEndian && unk00 == 2 && unk04 == 1 && groupSize == 0x40 && probeSize == 0x1C
                    ? BTPBVersion.DarkSouls2BE
                    : !bigEndian && unk00 == 2 && unk04 == 1 && groupSize == 0x48 && probeSize == 0x48
                                    ? BTPBVersion.Bloodborne
                                    : !bigEndian && unk00 == 3 && unk04 == 0 && groupSize == 0x98 && probeSize == 0x48
                                                ? BTPBVersion.DarkSouls3
                                                : throw new InvalidDataException($"Unknown BTPB format. {nameof(bigEndian)}:{bigEndian} {nameof(unk00)}:0x{unk00:X}" +
                                                    $" {nameof(unk04)}:0x{unk04:X} {nameof(groupSize)}:0x{groupSize:X} {nameof(probeSize)}:0x{probeSize:X}");
            br.VarintLong = this.Version >= BTPBVersion.Bloodborne;

            long dataStart = br.Position;
            br.Skip(dataLength);
            this.Groups = new List<Group>(groupCount);
            for (int i = 0; i < groupCount; i++) {
                this.Groups.Add(new Group(br, this.Version, dataStart));
            }
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected internal override void Write(BinaryWriterEx bw) {
            bool bigEndian;
            int unk00, unk04, groupSize, probeSize;
            if (this.Version is BTPBVersion.DarkSouls2LE or BTPBVersion.DarkSouls2BE) {
                bigEndian = this.Version == BTPBVersion.DarkSouls2BE;
                unk00 = 2;
                unk04 = 1;
                groupSize = 0x40;
                probeSize = 0x1C;
            } else if (this.Version == BTPBVersion.Bloodborne) {
                bigEndian = false;
                unk00 = 2;
                unk04 = 1;
                groupSize = 0x48;
                probeSize = 0x48;
            } else if (this.Version == BTPBVersion.DarkSouls3) {
                bigEndian = false;
                unk00 = 3;
                unk04 = 0;
                groupSize = 0x98;
                probeSize = 0x48;
            } else {
                throw new NotImplementedException($"Write is apparently not supported for BTPB version {this.Version}.");
            }

            bw.BigEndian = bigEndian;
            bw.VarintLong = this.Version >= BTPBVersion.Bloodborne;

            bw.WriteInt32(unk00);
            bw.WriteInt32(unk04);
            bw.WriteInt32(this.Groups.Count);
            bw.ReserveInt32("DataLength");
            bw.WriteBoolean(bigEndian);
            bw.WritePattern(3, 0x00);
            bw.WriteInt32(groupSize);
            bw.WriteInt32(probeSize);
            bw.WriteVector3(this.Unk1C);
            bw.WriteVector3(this.Unk28);
            bw.WriteInt64(0);

            long[] nameOffsets = new long[this.Groups.Count];
            long[] probesOffsets = new long[this.Groups.Count];

            long dataStart = bw.Position;
            for (int i = 0; i < this.Groups.Count; i++) {
                this.Groups[i].WriteData(bw, this.Version, dataStart, out nameOffsets[i], out probesOffsets[i]);
            }

            bw.FillInt32("DataLength", (int)(bw.Position - dataStart));

            for (int i = 0; i < this.Groups.Count; i++) {
                this.Groups[i].Write(bw, this.Version, nameOffsets[i], probesOffsets[i]);
            }
        }

        /// <summary>
        /// Supported BTPB formats.
        /// </summary>
        public enum BTPBVersion {
            /// <summary>
            /// Dark Souls 2 on PC and SotFS on all platforms.
            /// </summary>
            DarkSouls2LE,

            /// <summary>
            /// Dark Souls 2 on PS3 and X360.
            /// </summary>
            DarkSouls2BE,

            /// <summary>
            /// Bloodborne.
            /// </summary>
            Bloodborne,

            /// <summary>
            /// Dark Souls 3 on all platforms.
            /// </summary>
            DarkSouls3,
        }

        /// <summary>
        /// A volume containing light probes with some additional configuration.
        /// </summary>
        public class Group {
            /// <summary>
            /// An optional name for the group. Presence appears to be indicated by the lowest bit of Flags08.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Appears to be flags, highly speculative.
            /// </summary>
            public int Flags08 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk10 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk14 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk18 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk1C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk20 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk24 { get; set; }

            /// <summary>
            /// Unknown; probably bounding box min.
            /// </summary>
            public Vector3 Unk28 { get; set; }

            /// <summary>
            /// Unknown; probably bounding box max.
            /// </summary>
            public Vector3 Unk34 { get; set; }

            /// <summary>
            /// Light probes in this group.
            /// </summary>
            public List<Probe> Probes { get; set; }

            /// <summary>
            /// Unknown; only present since DS3.
            /// </summary>
            public float Unk48 { get; set; }

            /// <summary>
            /// Unknown; only present since DS3.
            /// </summary>
            public float Unk4C { get; set; }

            /// <summary>
            /// Unknown; only present since DS3.
            /// </summary>
            public float Unk50 { get; set; }

            /// <summary>
            /// Unknown; only present since DS3.
            /// </summary>
            public byte Unk94 { get; set; }

            /// <summary>
            /// Unknown; only present since DS3.
            /// </summary>
            public byte Unk95 { get; set; }

            /// <summary>
            /// Unknown; only present since DS3.
            /// </summary>
            public byte Unk96 { get; set; }

            /// <summary>
            /// Creates an empty Group with default values.
            /// </summary>
            public Group() => this.Probes = new List<Probe>();

            internal Group(BinaryReaderEx br, BTPBVersion version, long dataStart) {
                long nameOffset = br.ReadVarint();
                this.Flags08 = br.ReadInt32();
                int probeCount = br.ReadInt32();
                this.Unk10 = br.ReadInt32();
                this.Unk14 = br.ReadInt32();
                this.Unk18 = br.ReadInt32();
                this.Unk1C = br.ReadSingle();
                this.Unk20 = br.ReadSingle();
                this.Unk24 = br.ReadSingle();
                this.Unk28 = br.ReadVector3();
                this.Unk34 = br.ReadVector3();
                long probesOffset = br.ReadVarint();

                if (version >= BTPBVersion.DarkSouls3) {
                    this.Unk48 = br.ReadSingle();
                    this.Unk4C = br.ReadSingle();
                    this.Unk50 = br.ReadSingle();
                    br.AssertPattern(0x40, 0x00);
                    this.Unk94 = br.ReadByte();
                    this.Unk95 = br.ReadByte();
                    this.Unk96 = br.ReadByte();
                    _ = br.AssertByte(0);
                }

                if ((this.Flags08 & 1) != 0) {
                    this.Name = br.GetUTF16(dataStart + nameOffset);
                }

                br.StepIn(dataStart + probesOffset);
                {
                    this.Probes = new List<Probe>(probeCount);
                    for (int i = 0; i < probeCount; i++) {
                        this.Probes.Add(new Probe(br, version));
                    }
                }
                br.StepOut();
            }

            internal void WriteData(BinaryWriterEx bw, BTPBVersion version, long dataStart, out long nameOffset, out long probesOffset) {
                if ((this.Flags08 & 1) != 0) {
                    nameOffset = bw.Position - dataStart;
                    bw.WriteUTF16(this.Name, true);
                    if ((bw.Position - dataStart) % 8 != 0) {
                        bw.Position += 8 - (bw.Position - dataStart) % 8;
                    }
                } else {
                    nameOffset = 0;
                }

                probesOffset = bw.Position - dataStart;
                foreach (Probe probe in this.Probes) {
                    probe.Write(bw, version);
                }
            }

            internal void Write(BinaryWriterEx bw, BTPBVersion version, long nameOffset, long probesOffset) {
                bw.WriteVarint(nameOffset);
                bw.WriteInt32(this.Flags08);
                bw.WriteInt32(this.Probes.Count);
                bw.WriteInt32(this.Unk10);
                bw.WriteInt32(this.Unk14);
                bw.WriteInt32(this.Unk18);
                bw.WriteSingle(this.Unk1C);
                bw.WriteSingle(this.Unk20);
                bw.WriteSingle(this.Unk24);
                bw.WriteVector3(this.Unk28);
                bw.WriteVector3(this.Unk34);
                bw.WriteVarint(probesOffset);

                if (version >= BTPBVersion.DarkSouls3) {
                    bw.WriteSingle(this.Unk48);
                    bw.WriteSingle(this.Unk4C);
                    bw.WriteSingle(this.Unk50);
                    bw.WritePattern(0x40, 0x00);
                    bw.WriteByte(this.Unk94);
                    bw.WriteByte(this.Unk95);
                    bw.WriteByte(this.Unk96);
                    bw.WriteByte(0);
                }
            }
        }

        /// <summary>
        /// A probe giving directional lighting information at a given point.
        /// </summary>
        public class Probe {
            /// <summary>
            /// First-order spherical harmonics coefficients in R0G0B0R1G1B1... order.
            /// </summary>
            public short[] Coefficients { get; private set; }

            /// <summary>
            /// Multiplies sun lighting, where 0 is 0% sun and 1024 is 100%.
            /// </summary>
            public short LightMask { get; set; }

            /// <summary>
            /// Unknown; always 0 outside the chalice BTPB.
            /// </summary>
            public short Unk1A { get; set; }

            /// <summary>
            /// The position of the probe; not present in DS2.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Creates a Probe with default values.
            /// </summary>
            public Probe() => this.Coefficients = new short[12];

            internal Probe(BinaryReaderEx br, BTPBVersion version) {
                this.Coefficients = br.ReadInt16s(12);
                this.LightMask = br.ReadInt16();
                this.Unk1A = br.ReadInt16();

                if (version >= BTPBVersion.Bloodborne) {
                    this.Position = br.ReadVector3();
                    br.AssertPattern(0x20, 0x00);
                }
            }

            internal void Write(BinaryWriterEx bw, BTPBVersion version) {
                bw.WriteInt16s(this.Coefficients);
                bw.WriteInt16(this.LightMask);
                bw.WriteInt16(this.Unk1A);

                if (version >= BTPBVersion.Bloodborne) {
                    bw.WriteVector3(this.Position);
                    bw.WritePattern(0x20, 0x00);
                }
            }
        }
    }
}
