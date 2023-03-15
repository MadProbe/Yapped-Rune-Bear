using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SoulsFormats.Util;

namespace SoulsFormats.Formats.TPF {
    /// <summary>
    /// A multi-file texture container used throughout the series. Extension: .tpf
    /// </summary>
    public partial class TPF : SoulsFile<TPF>, IEnumerable<TPF.Texture> {
        /// <summary>
        /// The textures contained within this TPF.
        /// </summary>
        public List<Texture> Textures { get; set; }

        /// <summary>
        /// The platform this TPF will be used on.
        /// </summary>
        public TPFPlatform Platform { get; set; }

        /// <summary>
        /// Indicates encoding used for texture names.
        /// </summary>
        public byte Encoding { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public byte Flag2 { get; set; }

        /// <summary>
        /// Creates an empty TPF configured for DS3.
        /// </summary>
        public TPF() {
            this.Textures = new List<Texture>();
            this.Platform = TPFPlatform.PC;
            this.Encoding = 1;
            this.Flag2 = 3;
        }

        /// <summary>
        /// Returns true if the data appears to be a TPF.
        /// </summary>
        protected internal override bool Is(BinaryReaderEx br) {
            if (br.Length < 4) {
                return false;
            }

            string magic = br.GetASCII(0, 4);
            return magic == "TPF\0";
        }

        /// <summary>
        /// Reads TPF data from a BinaryReaderEx.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            br.BigEndian = false;
            _ = br.AssertASCII("TPF\0");
            this.Platform = br.GetEnum8<TPFPlatform>(0xC);
            br.BigEndian = this.Platform is TPFPlatform.Xbox360 or TPFPlatform.PS3;

            _ = br.ReadInt32(); // Data length
            int fileCount = br.ReadInt32();
            br.Skip(1); // Platform
            this.Flag2 = br.AssertByte(0, 1, 2, 3);
            this.Encoding = br.AssertByte(0, 1, 2);
            _ = br.AssertByte(0);

            this.Textures = new List<Texture>(fileCount);
            for (int i = 0; i < fileCount; i++) {
                this.Textures.Add(new Texture(br, this.Platform, this.Flag2, this.Encoding));
            }
        }

        /// <summary>
        /// Writes TPF data to a BinaryWriterEx.
        /// </summary>
        protected internal override void Write(BinaryWriterEx bw) {
            bw.BigEndian = this.Platform is TPFPlatform.Xbox360 or TPFPlatform.PS3;
            bw.WriteASCII("TPF\0");
            bw.ReserveInt32("DataSize");
            bw.WriteInt32(this.Textures.Count);
            bw.WriteByte((byte)this.Platform);
            bw.WriteByte(this.Flag2);
            bw.WriteByte(this.Encoding);
            bw.WriteByte(0);

            for (int i = 0; i < this.Textures.Count; i++) {
                this.Textures[i].WriteHeader(bw, i, this.Platform, this.Flag2);
            }

            for (int i = 0; i < this.Textures.Count; i++) {
                this.Textures[i].WriteName(bw, i, this.Encoding);
            }

            long dataStart = bw.Position;
            for (int i = 0; i < this.Textures.Count; i++) {
                // Padding for texture data varies wildly across games,
                // so don't worry about this too much
                if (this.Textures[i].Bytes.Length > 0) {
                    bw.Pad(4);
                }

                this.Textures[i].WriteData(bw, i);
            }
            bw.FillInt32("DataSize", (int)(bw.Position - dataStart));
        }

        /// <summary>
        /// Returns an enumerator that iterates through the list of Textures.
        /// </summary>
        public IEnumerator<Texture> GetEnumerator() => this.Textures.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <summary>
        /// A DDS texture in a TPF container.
        /// </summary>
        public class Texture {
            /// <summary>
            /// The name of the texture; should not include a path or extension.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Indicates format of the texture.
            /// </summary>
            public byte Format { get; set; }

            /// <summary>
            /// Whether this texture is a cubemap.
            /// </summary>
            public TexType Type { get; set; }

            /// <summary>
            /// Number of mipmap levels in this texture.
            /// </summary>
            public byte Mipmaps { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Flags1 { get; set; }

            /// <summary>
            /// The raw data of the texture.
            /// </summary>
            public byte[] Bytes { get; set; }

            /// <summary>
            /// Extended metadata present in headerless console TPF textures.
            /// </summary>
            public TexHeader Header { get; set; }

            /// <summary>
            /// Unknown optional data; null if not present.
            /// </summary>
            public FloatStruct FloatStruct { get; set; }

            /// <summary>
            /// Creates an empty Texture.
            /// </summary>
            public Texture() {
                this.Name = "Unnamed";
                this.Bytes = new byte[0];
            }

            /// <summary>
            /// Create a new PC Texture with the specified information; Cubemap and Mipmaps are determined based on bytes.
            /// </summary>
            public Texture(string name, byte format, byte flags1, byte[] bytes) {
                this.Name = name;
                this.Format = format;
                this.Flags1 = flags1;
                this.Bytes = bytes;

                var dds = new DDS(bytes);
                this.Type = dds.dwCaps2.HasFlag(DDS.DDSCAPS2.CUBEMAP)
                    ? TexType.Cubemap
                    : dds.dwCaps2.HasFlag(DDS.DDSCAPS2.VOLUME) ? TexType.Volume : TexType.Texture;

                this.Mipmaps = (byte)dds.dwMipMapCount;
            }

            internal Texture(BinaryReaderEx br, TPFPlatform platform, byte flag2, byte encoding) {
                uint fileOffset = br.ReadUInt32();
                int fileSize = br.ReadInt32();

                this.Format = br.ReadByte();
                this.Type = br.ReadEnum8<TexType>();
                this.Mipmaps = br.ReadByte();
                this.Flags1 = br.AssertByte(0, 1, 2, 3);

                if (platform != TPFPlatform.PC) {
                    this.Header = new TexHeader {
                        Width = br.ReadInt16(),
                        Height = br.ReadInt16()
                    };

                    if (platform == TPFPlatform.Xbox360) {
                        _ = br.AssertInt32(0);
                    } else if (platform == TPFPlatform.PS3) {
                        this.Header.Unk1 = br.ReadInt32();
                        if (flag2 != 0) {
                            this.Header.Unk2 = br.AssertInt32(0, 0x69E0, 0xAAE4);
                        }
                    } else if (platform is TPFPlatform.PS4 or TPFPlatform.Xbone) {
                        this.Header.TextureCount = br.AssertInt32(1, 6);
                        this.Header.Unk2 = br.AssertInt32(0xD);
                    }
                }

                uint nameOffset = br.ReadUInt32();
                bool hasFloatStruct = br.AssertInt32(0, 1) == 1;

                if (platform is TPFPlatform.PS4 or TPFPlatform.Xbone) {
                    this.Header.DXGIFormat = br.ReadInt32();
                }

                if (hasFloatStruct) {
                    this.FloatStruct = new FloatStruct(br);
                }

                this.Bytes = br.GetBytes(fileOffset, fileSize);
                if (this.Flags1 is 2 or 3) {
                    this.Bytes = DCX.Decompress(this.Bytes, out DCX.Type type);
                    if (type != DCX.Type.DCP_EDGE) {
                        throw new NotImplementedException($"TPF compression is expected to be DCP_EDGE, but it was {type}");
                    }
                }

                if (encoding == 1) {
                    this.Name = br.GetUTF16(nameOffset);
                } else if (encoding is 0 or 2) {
                    this.Name = br.GetShiftJIS(nameOffset);
                }
            }

            internal void WriteHeader(BinaryWriterEx bw, int index, TPFPlatform platform, byte flag2) {
                if (platform == TPFPlatform.PC) {
                    var dds = new DDS(this.Bytes);
                    this.Type = dds.dwCaps2.HasFlag(DDS.DDSCAPS2.CUBEMAP)
                        ? TexType.Cubemap
                        : dds.dwCaps2.HasFlag(DDS.DDSCAPS2.VOLUME) ? TexType.Volume : TexType.Texture;

                    this.Mipmaps = (byte)dds.dwMipMapCount;
                }

                bw.ReserveUInt32($"FileData{index}");
                bw.ReserveInt32($"FileSize{index}");

                bw.WriteByte(this.Format);
                bw.WriteByte((byte)this.Type);
                bw.WriteByte(this.Mipmaps);
                bw.WriteByte(this.Flags1);

                if (platform != TPFPlatform.PC) {
                    bw.WriteInt16(this.Header.Width);
                    bw.WriteInt16(this.Header.Height);

                    if (platform == TPFPlatform.Xbox360) {
                        bw.WriteInt32(0);
                    } else if (platform == TPFPlatform.PS3) {
                        bw.WriteInt32(this.Header.Unk1);
                        if (flag2 != 0) {
                            bw.WriteInt32(this.Header.Unk2);
                        }
                    } else if (platform is TPFPlatform.PS4 or TPFPlatform.Xbone) {
                        bw.WriteInt32(this.Header.TextureCount);
                        bw.WriteInt32(this.Header.Unk2);
                    }
                }

                bw.ReserveUInt32($"FileName{index}");
                bw.WriteInt32(this.FloatStruct == null ? 0 : 1);

                if (platform is TPFPlatform.PS4 or TPFPlatform.Xbone) {
                    bw.WriteInt32(this.Header.DXGIFormat);
                }

                if (this.FloatStruct != null) {
                    this.FloatStruct.Write(bw);
                }
            }

            internal void WriteName(BinaryWriterEx bw, int index, byte encoding) {
                bw.FillUInt32($"FileName{index}", (uint)bw.Position);
                if (encoding == 1) {
                    bw.WriteUTF16(this.Name, true);
                } else if (encoding is 0 or 2) {
                    bw.WriteShiftJIS(this.Name, true);
                }
            }

            internal void WriteData(BinaryWriterEx bw, int index) {
                bw.FillUInt32($"FileData{index}", (uint)bw.Position);

                byte[] bytes = this.Bytes;
                if (this.Flags1 is 2 or 3) {
                    bytes = DCX.Compress(bytes, DCX.Type.DCP_EDGE);
                }

                bw.FillInt32($"FileSize{index}", bytes.Length);
                bw.WriteBytes(bytes);
            }

            /// <summary>
            /// Attempt to create a full DDS file from headerless console textures. Very very very poor support at the moment.
            /// </summary>
            public byte[] Headerize() => Headerizer.Headerize(this);

            /// <summary>
            /// Returns the name of this texture.
            /// </summary>
            public override string ToString() => $"[{this.Format} {this.Type}] {this.Name}";
        }

        /// <summary>
        /// The platform of the game a TPF is for.
        /// </summary>
        public enum TPFPlatform : byte {
            /// <summary>
            /// Headered DDS with minimal metadata.
            /// </summary>
            PC = 0,

            /// <summary>
            /// Headerless DDS with pre-DX10 metadata.
            /// </summary>
            Xbox360 = 1,

            /// <summary>
            /// Headerless DDS with pre-DX10 metadata.
            /// </summary>
            PS3 = 2,

            /// <summary>
            /// Headerless DDS with DX10 metadata.
            /// </summary>
            PS4 = 4,

            /// <summary>
            /// Headerless DDS with DX10 metadata.
            /// </summary>
            Xbone = 5,
        }

        /// <summary>
        /// Type of texture in a TPF.
        /// </summary>
        public enum TexType : byte {
            /// <summary>
            /// One 2D texture.
            /// </summary>
            Texture = 0,

            /// <summary>
            /// Six 2D textures.
            /// </summary>
            Cubemap = 1,

            /// <summary>
            /// One 3D texture.
            /// </summary>
            Volume = 2,
        }

        /// <summary>
        /// Extra metadata for headerless textures used in console versions.
        /// </summary>
        public class TexHeader {
            /// <summary>
            /// Width of the texture, in pixels.
            /// </summary>
            public short Width { get; set; }

            /// <summary>
            /// Height of the texture, in pixels.
            /// </summary>
            public short Height { get; set; }

            /// <summary>
            /// Number of textures in the array, either 1 for normal textures or 6 for cubemaps.
            /// </summary>
            public int TextureCount { get; set; }

            /// <summary>
            /// Unknown; PS3 only.
            /// </summary>
            public int Unk1 { get; set; }

            /// <summary>
            /// Unknown; 0x0 or 0xAAE4 in DeS, 0xD in DS3.
            /// </summary>
            public int Unk2 { get; set; }

            /// <summary>
            /// Microsoft DXGI_FORMAT.
            /// </summary>
            public int DXGIFormat { get; set; }
        }

        /// <summary>
        /// Unknown optional data for textures.
        /// </summary>
        public class FloatStruct {
            /// <summary>
            /// Unknown; probably some kind of ID.
            /// </summary>
            public int Unk00 { get; set; }

            /// <summary>
            /// Unknown; not confirmed to always be floats.
            /// </summary>
            public List<float> Values { get; set; }

            /// <summary>
            /// Creates an empty FloatStruct.
            /// </summary>
            public FloatStruct() => this.Values = new List<float>();

            internal FloatStruct(BinaryReaderEx br) {
                this.Unk00 = br.ReadInt32();
                int length = br.ReadInt32();
                if (length < 0 || length % 4 != 0) {
                    throw new InvalidDataException($"Unexpected FloatStruct length: {length}");
                }

                this.Values = new List<float>(br.ReadSingles(length / 4));
            }

            internal void Write(BinaryWriterEx bw) {
                bw.WriteInt32(this.Unk00);
                bw.WriteInt32(this.Values.Count * 4);
                bw.WriteSingles(this.Values);
            }
        }
    }
}
