using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using SoulsFormats.Util;

namespace SoulsFormats.Formats {
    /// <summary>
    /// Point light sources in a map, used in BB, DS3, and Sekiro.
    /// </summary>
    public class BTL : SoulsFile<BTL> {
        /// <summary>
        /// Indicates the version, probably.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Whether offsets are 64-bit; set to false for Dark Souls 2.
        /// </summary>
        public bool LongOffsets { get; set; }

        /// <summary>
        /// Light sources in this BTL.
        /// </summary>
        public List<Light> Lights { get; set; }

        /// <summary>
        /// Creates a BTL with Sekiro's version and no lights.
        /// </summary>
        public BTL() {
            this.Version = 16;
            this.LongOffsets = true;
            this.Lights = new List<Light>();
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            br.BigEndian = false;

            _ = br.AssertInt32(2);
            this.Version = br.AssertInt32(1, 2, 5, 6, 16, 18);
            int lightCount = br.ReadInt32();
            int namesLength = br.ReadInt32();
            _ = br.AssertInt32(0);
            int lightSize = br.AssertInt32(0xC0, 0xC8, 0xE8);
            br.AssertPattern(0x24, 0x00);
            this.LongOffsets = br.VarintLong = lightSize != 0xC0;

            long namesStart = br.Position;
            br.Skip(namesLength);
            this.Lights = new List<Light>(lightCount);
            for (int i = 0; i < lightCount; i++) {
                this.Lights.Add(new Light(br, namesStart, this.Version, this.LongOffsets));
            }
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected internal override void Write(BinaryWriterEx bw) {
            bw.BigEndian = false;
            bw.VarintLong = this.LongOffsets;

            bw.WriteInt32(2);
            bw.WriteInt32(this.Version);
            bw.WriteInt32(this.Lights.Count);
            bw.ReserveInt32("NamesLength");
            bw.WriteInt32(0);
            bw.WriteInt32(this.Version >= 16 ? 0xE8 : this.LongOffsets ? 0xC8 : 0xC0);
            bw.WritePattern(0x24, 0x00);

            long namesStart = bw.Position;
            var nameOffsets = new List<long>(this.Lights.Count);
            foreach (Light entry in this.Lights) {
                long nameOffset = bw.Position - namesStart;
                nameOffsets.Add(nameOffset);
                bw.WriteUTF16(entry.Name, true);
                if (nameOffset % 0x10 != 0) {
                    bw.WritePattern((int)(0x10 - nameOffset % 0x10), 0x00);
                }
            }

            bw.FillInt32("NamesLength", (int)(bw.Position - namesStart));
            for (int i = 0; i < this.Lights.Count; i++) {
                this.Lights[i].Write(bw, nameOffsets[i], this.Version, this.LongOffsets);
            }
        }

        /// <summary>
        /// Type of a light source.
        /// </summary>
        public enum LightType : uint {
            /// <summary>
            /// Omnidirectional light.
            /// </summary>
            Point = 0,

            /// <summary>
            /// Cone of light.
            /// </summary>
            Spot = 1,

            /// <summary>
            /// Light at a constant angle.
            /// </summary>
            Directional = 2,
        }

        /// <summary>
        /// An omnidirectional and/or spot light source.
        /// </summary>
        public class Light {
            /// <summary>
            /// Unknown.
            /// </summary>
            public byte[] Unk00 { get; private set; }

            /// <summary>
            /// Name of this light.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public LightType Type { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool Unk1C { get; set; }

            /// <summary>
            /// Color of the light on diffuse surfaces.
            /// </summary>
            [SupportsAlpha(false)]
            public Color DiffuseColor { get; set; }

            /// <summary>
            /// Intensity of diffuse lighting.
            /// </summary>
            public float DiffusePower { get; set; }

            /// <summary>
            /// Color of the light on reflective surfaces.
            /// </summary>
            [SupportsAlpha(false)]
            public Color SpecularColor { get; set; }

            /// <summary>
            /// Whether the light casts shadows.
            /// </summary>
            public bool CastShadows { get; set; }

            /// <summary>
            /// Intensity of specular lighting.
            /// </summary>
            public float SpecularPower { get; set; }

            /// <summary>
            /// Tightness of the spot light beam.
            /// </summary>
            public float ConeAngle { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk30 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk34 { get; set; }

            /// <summary>
            /// Center of the light.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Rotation of a spot light.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk50 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk54 { get; set; }

            /// <summary>
            /// Distance the light shines.
            /// </summary>
            public float Radius { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk5C { get; set; }

            /// <summary>
            /// Unknown; 4 bytes.
            /// </summary>
            public byte[] Unk64 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk68 { get; set; }

            /// <summary>
            /// Color of shadows cast by the light; alpha is relative to 100.
            /// </summary>
            [SupportsAlpha(true)]
            public Color ShadowColor { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk70 { get; set; }

            /// <summary>
            /// Minimum time between flickers.
            /// </summary>
            public float FlickerIntervalMin { get; set; }

            /// <summary>
            /// Maximum time between flickers.
            /// </summary>
            public float FlickerIntervalMax { get; set; }

            /// <summary>
            /// Multiplies the brightness of the light while flickering.
            /// </summary>
            public float FlickerBrightnessMult { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk80 { get; set; }

            /// <summary>
            /// Unknown; 4 bytes.
            /// </summary>
            public byte[] Unk84 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk88 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk90 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk98 { get; set; }

            /// <summary>
            /// Distance at which spot light beam starts.
            /// </summary>
            public float NearClip { get; set; }

            /// <summary>
            /// Unknown; 4 bytes.
            /// </summary>
            public byte[] UnkA0 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Sharpness { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float UnkAC { get; set; }

            /// <summary>
            /// Stretches the spot light beam.
            /// </summary>
            public float Width { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float UnkBC { get; set; }

            /// <summary>
            /// Unknown; 4 bytes.
            /// </summary>
            public byte[] UnkC0 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float UnkC4 { get; set; }

            /// <summary>
            /// Unknown; not present before Sekiro.
            /// </summary>
            public float UnkC8 { get; set; }

            /// <summary>
            /// Unknown; not present before Sekiro.
            /// </summary>
            public float UnkCC { get; set; }

            /// <summary>
            /// Unknown; not present before Sekiro.
            /// </summary>
            public float UnkD0 { get; set; }

            /// <summary>
            /// Unknown; not present before Sekiro.
            /// </summary>
            public float UnkD4 { get; set; }

            /// <summary>
            /// Unknown; not present before Sekiro.
            /// </summary>
            public float UnkD8 { get; set; }

            /// <summary>
            /// Unknown; not present before Sekiro.
            /// </summary>
            public int UnkDC { get; set; }

            /// <summary>
            /// Unknown; not present before Sekiro.
            /// </summary>
            public float UnkE0 { get; set; }

            /// <summary>
            /// Unknown; not present before Sekiro.
            /// </summary>
            public int UnkE4 { get; set; }

            /// <summary>
            /// Creates a Light with default values.
            /// </summary>
            public Light() {
                this.Unk00 = new byte[16];
                this.Name = "";
                this.Unk1C = true;
                this.DiffuseColor = Color.White;
                this.DiffusePower = 1;
                this.SpecularColor = Color.White;
                this.SpecularPower = 1;
                this.Unk50 = 4;
                this.Radius = 10;
                this.Unk5C = -1;
                this.Unk64 = new byte[4] { 0, 0, 0, 1 };
                this.ShadowColor = Color.FromArgb(100, 0, 0, 0);
                this.FlickerBrightnessMult = 1;
                this.Unk80 = -1;
                this.Unk84 = new byte[4];
                this.Unk98 = 1;
                this.NearClip = 1;
                this.UnkA0 = new byte[4] { 1, 0, 2, 1 };
                this.Sharpness = 1;
                this.UnkC0 = new byte[4];
            }

            /// <summary>
            /// Creates a clone of an existing Light.
            /// </summary>
            public Light Clone() {
                var clone = (Light)this.MemberwiseClone();
                clone.Unk00 = (byte[])this.Unk00.Clone();
                clone.Unk64 = (byte[])this.Unk64.Clone();
                clone.Unk84 = (byte[])this.Unk84.Clone();
                clone.UnkA0 = (byte[])this.UnkA0.Clone();
                clone.UnkC0 = (byte[])this.UnkC0.Clone();
                return clone;
            }

            internal Light(BinaryReaderEx br, long namesStart, int version, bool longOffsets) {
                this.Unk00 = br.ReadBytes(16);
                this.Name = br.GetUTF16(namesStart + br.ReadVarint());
                this.Type = br.ReadEnum32<LightType>();
                this.Unk1C = br.ReadBoolean();
                this.DiffuseColor = ReadRGB(br);
                this.DiffusePower = br.ReadSingle();
                this.SpecularColor = ReadRGB(br);
                this.CastShadows = br.ReadBoolean();
                this.SpecularPower = br.ReadSingle();
                this.ConeAngle = br.ReadSingle();
                this.Unk30 = br.ReadSingle();
                this.Unk34 = br.ReadSingle();
                this.Position = br.ReadVector3();
                this.Rotation = br.ReadVector3();
                this.Unk50 = br.ReadInt32();
                this.Unk54 = br.ReadSingle();
                this.Radius = br.ReadSingle();
                this.Unk5C = br.ReadInt32();
                _ = br.AssertInt32(0);
                this.Unk64 = br.ReadBytes(4);
                this.Unk68 = br.ReadSingle();
                this.ShadowColor = br.ReadRGBA();
                this.Unk70 = br.ReadSingle();
                this.FlickerIntervalMin = br.ReadSingle();
                this.FlickerIntervalMax = br.ReadSingle();
                this.FlickerBrightnessMult = br.ReadSingle();
                this.Unk80 = br.ReadInt32();
                this.Unk84 = br.ReadBytes(4);
                this.Unk88 = br.ReadSingle();
                _ = br.AssertInt32(0);
                this.Unk90 = br.ReadSingle();
                _ = br.AssertInt32(0);
                this.Unk98 = br.ReadSingle();
                this.NearClip = br.ReadSingle();
                this.UnkA0 = br.ReadBytes(4);
                this.Sharpness = br.ReadSingle();
                _ = br.AssertInt32(0);
                this.UnkAC = br.ReadSingle();
                _ = br.AssertVarint(0);
                this.Width = br.ReadSingle();
                this.UnkBC = br.ReadSingle();
                this.UnkC0 = br.ReadBytes(4);
                this.UnkC4 = br.ReadSingle();

                if (version >= 16) {
                    this.UnkC8 = br.ReadSingle();
                    this.UnkCC = br.ReadSingle();
                    this.UnkD0 = br.ReadSingle();
                    this.UnkD4 = br.ReadSingle();
                    this.UnkD8 = br.ReadSingle();
                    this.UnkDC = br.ReadInt32();
                    this.UnkE0 = br.ReadSingle();
                    this.UnkE4 = br.ReadInt32();
                }
            }

            internal void Write(BinaryWriterEx bw, long nameOffset, int version, bool longOffsets) {
                bw.WriteBytes(this.Unk00);
                bw.WriteVarint(nameOffset);
                bw.WriteUInt32((uint)this.Type);
                bw.WriteBoolean(this.Unk1C);
                WriteRGB(bw, this.DiffuseColor);
                bw.WriteSingle(this.DiffusePower);
                WriteRGB(bw, this.SpecularColor);
                bw.WriteBoolean(this.CastShadows);
                bw.WriteSingle(this.SpecularPower);
                bw.WriteSingle(this.ConeAngle);
                bw.WriteSingle(this.Unk30);
                bw.WriteSingle(this.Unk34);
                bw.WriteVector3(this.Position);
                bw.WriteVector3(this.Rotation);
                bw.WriteInt32(this.Unk50);
                bw.WriteSingle(this.Unk54);
                bw.WriteSingle(this.Radius);
                bw.WriteInt32(this.Unk5C);
                bw.WriteInt32(0);
                bw.WriteBytes(this.Unk64);
                bw.WriteSingle(this.Unk68);
                bw.WriteRGBA(this.ShadowColor);
                bw.WriteSingle(this.Unk70);
                bw.WriteSingle(this.FlickerIntervalMin);
                bw.WriteSingle(this.FlickerIntervalMax);
                bw.WriteSingle(this.FlickerBrightnessMult);
                bw.WriteInt32(this.Unk80);
                bw.WriteBytes(this.Unk84);
                bw.WriteSingle(this.Unk88);
                bw.WriteInt32(0);
                bw.WriteSingle(this.Unk90);
                bw.WriteInt32(0);
                bw.WriteSingle(this.Unk98);
                bw.WriteSingle(this.NearClip);
                bw.WriteBytes(this.UnkA0);
                bw.WriteSingle(this.Sharpness);
                bw.WriteInt32(0);
                bw.WriteSingle(this.UnkAC);
                bw.WriteVarint(0);
                bw.WriteSingle(this.Width);
                bw.WriteSingle(this.UnkBC);
                bw.WriteBytes(this.UnkC0);
                bw.WriteSingle(this.UnkC4);

                if (version >= 16) {
                    bw.WriteSingle(this.UnkC8);
                    bw.WriteSingle(this.UnkCC);
                    bw.WriteSingle(this.UnkD0);
                    bw.WriteSingle(this.UnkD4);
                    bw.WriteSingle(this.UnkD8);
                    bw.WriteInt32(this.UnkDC);
                    bw.WriteSingle(this.UnkE0);
                    bw.WriteInt32(this.UnkE4);
                }
            }

            /// <summary>
            /// Returns the name of the light.
            /// </summary>
            public override string ToString() => this.Name;

            private static Color ReadRGB(BinaryReaderEx br) {
                byte[] rgb = br.ReadBytes(3);
                return Color.FromArgb(255, rgb[0], rgb[1], rgb[2]);
            }

            private static void WriteRGB(BinaryWriterEx bw, Color color) {
                bw.WriteByte(color.R);
                bw.WriteByte(color.G);
                bw.WriteByte(color.B);
            }
        }
    }
}
