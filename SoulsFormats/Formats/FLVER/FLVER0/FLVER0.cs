using System.Collections.Generic;
using System.Numerics;
using SoulsFormats.Formats.FLVER;
using SoulsFormats.Util;

namespace SoulsFormats {
    /// <summary>
    /// 3D models from Armored Core: For Answer to Another Century's Episode R. Extension: .flv, .flver
    /// </summary>
    public partial class FLVER0 : SoulsFile<FLVER0>, IFlver {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool BigEndian { get; set; }

        public int Version { get; set; }

        public Vector3 BoundingBoxMin { get; set; }

        public Vector3 BoundingBoxMax { get; set; }

        public byte VertexIndexSize { get; set; }

        public bool Unicode { get; set; }

        public byte Unk4A { get; set; }

        public byte Unk4B { get; set; }

        public int Unk4C { get; set; }

        public int Unk5C { get; set; }

        public List<FLVER.Dummy> Dummies { get; set; }
        IReadOnlyList<FLVER.Dummy> IFlver.Dummies => this.Dummies;

        public List<Material> Materials { get; set; }
        IReadOnlyList<IFlverMaterial> IFlver.Materials => this.Materials;

        public List<FLVER.Bone> Bones { get; set; }
        IReadOnlyList<FLVER.Bone> IFlver.Bones => this.Bones;

        public List<Mesh> Meshes { get; set; }
        IReadOnlyList<IFlverMesh> IFlver.Meshes => this.Meshes;

        protected internal override bool Is(BinaryReaderEx br) {
            if (br.Length < 0xC) {
                return false;
            }

            string magic = br.ReadASCII(6);
            string endian = br.ReadASCII(2);
            if (endian == "L\0") {
                br.BigEndian = false;
            } else if (endian == "B\0") {
                br.BigEndian = true;
            }

            int version = br.ReadInt32();
            return magic == "FLVER\0" && version >= 0x00000 && version < 0x20000;
        }

        protected internal override void Read(BinaryReaderEx br) {
            _ = br.AssertASCII("FLVER\0");
            this.BigEndian = br.AssertASCII("L\0", "B\0") == "B\0";
            br.BigEndian = this.BigEndian;

            // 10002, 10003 - Another Century's Episode R
            this.Version = br.AssertInt32(0x0E, 0x0F, 0x10, 0x12, 0x13, 0x14, 0x15,
                0x10002, 0x10003);
            int dataOffset = br.ReadInt32();
            _ = br.ReadInt32(); // Data length
            int dummyCount = br.ReadInt32();
            int materialCount = br.ReadInt32();
            int boneCount = br.ReadInt32();
            int meshCount = br.ReadInt32();
            _ = br.ReadInt32(); // Vertex buffer count
            this.BoundingBoxMin = br.ReadVector3();
            this.BoundingBoxMax = br.ReadVector3();
            _ = br.ReadInt32(); // Face count not including motion blur meshes or degenerate faces
            _ = br.ReadInt32(); // Total face count
            this.VertexIndexSize = br.AssertByte(16, 32);
            this.Unicode = br.ReadBoolean();
            this.Unk4A = br.ReadByte();
            this.Unk4B = br.ReadByte();
            this.Unk4C = br.ReadInt32();
            _ = br.AssertInt32(0);
            _ = br.AssertInt32(0);
            _ = br.AssertInt32(0);
            this.Unk5C = br.ReadByte();
            _ = br.AssertByte(0);
            _ = br.AssertByte(0);
            _ = br.AssertByte(0);
            br.AssertPattern(0x20, 0x00);

            this.Dummies = new List<FLVER.Dummy>(dummyCount);
            for (int i = 0; i < dummyCount; i++) {
                this.Dummies.Add(new FLVER.Dummy(br, this.Version));
            }

            this.Materials = new List<Material>(materialCount);
            for (int i = 0; i < materialCount; i++) {
                this.Materials.Add(new Material(br, this));
            }

            this.Bones = new List<FLVER.Bone>(boneCount);
            for (int i = 0; i < boneCount; i++) {
                this.Bones.Add(new FLVER.Bone(br, this.Unicode));
            }

            this.Meshes = new List<Mesh>(meshCount);
            for (int i = 0; i < meshCount; i++) {
                this.Meshes.Add(new Mesh(br, this, dataOffset));
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
