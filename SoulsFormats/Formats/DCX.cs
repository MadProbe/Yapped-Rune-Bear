using System;
using System.IO;
using System.IO.Compression;
using SoulsFormats.Util;

namespace SoulsFormats.Formats {
    /// <summary>
    /// A general-purpose single compressed file wrapper used in DS1, DSR, DS2, DS3, DeS, and BB.
    /// </summary>
    public static class DCX {
        internal static bool Is(BinaryReaderEx br) {
            if (br.Stream.Length < 4) {
                return false;
            }

            string magic = br.GetASCII(0, 4);
            return magic is "DCP\0" or "DCX\0";
        }

        /// <summary>
        /// Returns true if the bytes appear to be a DCX file.
        /// </summary>
        public static bool Is(byte[] bytes) {
            var br = new BinaryReaderEx(true, bytes);
            return Is(br);
        }

        /// <summary>
        /// Returns true if the file appears to be a DCX file.
        /// </summary>
        public static bool Is(string path) {
            using FileStream stream = File.OpenRead(path);
            var br = new BinaryReaderEx(true, stream);
            return Is(br);
        }

        #region Public Decompress
        /// <summary>
        /// Decompress a DCX file from an array of bytes and return the detected DCX type.
        /// </summary>
        public static byte[] Decompress(byte[] data, out Type type) {
            var br = new BinaryReaderEx(true, data);
            return Decompress(br, out type);
        }

        /// <summary>
        /// Decompress a DCX file from an array of bytes.
        /// </summary>
        public static byte[] Decompress(byte[] data) => Decompress(data, out _);

        /// <summary>
        /// Decompress a DCX file from the specified path and return the detected DCX type.
        /// </summary>
        public static byte[] Decompress(string path, out Type type) {
            using FileStream stream = File.OpenRead(path);
            var br = new BinaryReaderEx(true, stream);
            return Decompress(br, out type);
        }

        /// <summary>
        /// Decompress a DCX file from the specified path.
        /// </summary>
        public static byte[] Decompress(string path) => Decompress(path, out _);
        #endregion

        internal static byte[] Decompress(BinaryReaderEx br, out Type type) {
            br.BigEndian = true;
            type = Type.Unknown;

            string magic = br.ReadASCII(4);
            if (magic == "DCP\0") {
                string format = br.GetASCII(4, 4);
                if (format == "DFLT") {
                    type = Type.DCP_DFLT;
                } else if (format == "EDGE") {
                    type = Type.DCP_EDGE;
                }
            } else if (magic == "DCX\0") {
                string format = br.GetASCII(0x28, 4);
                if (format == "EDGE") {
                    type = Type.DCX_EDGE;
                } else if (format == "DFLT") {
                    int unk04 = br.GetInt32(0x4);
                    int unk10 = br.GetInt32(0x10);
                    byte unk30 = br.GetByte(0x30);
                    byte unk38 = br.GetByte(0x38);

                    if (unk04 == 0x10000 && unk10 == 0x24 && unk30 == 9 && unk38 == 0) {
                        type = Type.DCX_DFLT_10000_24_9;
                    } else if (unk04 == 0x10000 && unk10 == 0x44 && unk30 == 9 && unk38 == 0) {
                        type = Type.DCX_DFLT_10000_44_9;
                    } else if (unk04 == 0x11000 && unk10 == 0x44 && unk30 == 8 && unk38 == 0) {
                        type = Type.DCX_DFLT_11000_44_8;
                    } else if (unk04 == 0x11000 && unk10 == 0x44 && unk30 == 9 && unk38 == 0) {
                        type = Type.DCX_DFLT_11000_44_9;
                    } else if (unk04 == 0x11000 && unk10 == 0x44 && unk30 == 9 && unk38 == 15) {
                        type = Type.DCX_DFLT_11000_44_9_15;
                    }
                } else if (format == "KRAK") {
                    type = Type.DCX_KRAK;
                }
            } else {
                byte b0 = br.GetByte(0);
                byte b1 = br.GetByte(1);
                if (b0 == 0x78 && (b1 == 0x01 || b1 == 0x5E || b1 == 0x9C || b1 == 0xDA)) {
                    type = Type.Zlib;
                }
            }

            br.Position = 0;
            if (type == Type.Zlib) {
                return SFUtil.ReadZlib(br, (int)br.Length);
            } else if (type == Type.DCP_EDGE) {
                return DecompressDCPEDGE(br);
            } else {
                return type == Type.DCP_DFLT
                    ? DecompressDCPDFLT(br)
                    : type == Type.DCX_EDGE
                                    ? DecompressDCXEDGE(br)
                                    : type is Type.DCX_DFLT_10000_24_9
                                                                or Type.DCX_DFLT_10000_44_9
                                                                or Type.DCX_DFLT_11000_44_8
                                                                or Type.DCX_DFLT_11000_44_9
                                                                or Type.DCX_DFLT_11000_44_9_15
                                                    ? DecompressDCXDFLT(br, type)
                                                    : type == Type.DCX_KRAK ? DecompressDCXKRAK(br) : throw new FormatException("Unknown DCX format.");
            }
        }

        private static byte[] DecompressDCPDFLT(BinaryReaderEx br) {
            _ = br.AssertASCII("DCP\0");
            _ = br.AssertASCII("DFLT");
            _ = br.AssertInt32(0x20);
            _ = br.AssertInt32(0x9000000);
            _ = br.AssertInt32(0);
            _ = br.AssertInt32(0);
            _ = br.AssertInt32(0);
            _ = br.AssertInt32(0x00010100);

            _ = br.AssertASCII("DCS\0");
            _ = br.ReadInt32();
            int compressedSize = br.ReadInt32();

            byte[] decompressed = SFUtil.ReadZlib(br, compressedSize);

            _ = br.AssertASCII("DCA\0");
            _ = br.AssertInt32(8);

            return decompressed;
        }

        private static byte[] DecompressDCPEDGE(BinaryReaderEx br) {
            _ = br.AssertASCII("DCP\0");
            _ = br.AssertASCII("EDGE");
            _ = br.AssertInt32(0x20);
            _ = br.AssertInt32(0x9000000);
            _ = br.AssertInt32(0x10000);
            _ = br.AssertInt32(0x0);
            _ = br.AssertInt32(0x0);
            _ = br.AssertInt32(0x00100100);

            _ = br.AssertASCII("DCS\0");
            int uncompressedSize = br.ReadInt32();
            int compressedSize = br.ReadInt32();
            _ = br.AssertInt32(0);
            long dataStart = br.Position;
            br.Skip(compressedSize);

            _ = br.AssertASCII("DCA\0");
            _ = br.ReadInt32();
            // ???
            _ = br.AssertASCII("EgdT");
            _ = br.AssertInt32(0x00010000);
            _ = br.AssertInt32(0x20);
            _ = br.AssertInt32(0x10);
            _ = br.AssertInt32(0x10000);
            int egdtSize = br.ReadInt32();
            int chunkCount = br.ReadInt32();
            _ = br.AssertInt32(0x100000);

            if (egdtSize != 0x20 + chunkCount * 0x10) {
                throw new InvalidDataException("Unexpected EgdT size in EDGE DCX.");
            }

            byte[] decompressed = new byte[uncompressedSize];
            using (var dcmpStream = new MemoryStream(decompressed)) {
                for (int i = 0; i < chunkCount; i++) {
                    _ = br.AssertInt32(0);
                    int offset = br.ReadInt32();
                    int size = br.ReadInt32();
                    bool compressed = br.AssertInt32(0, 1) == 1;

                    byte[] chunk = br.GetBytes(dataStart + offset, size);

                    if (compressed) {
                        using var cmpStream = new MemoryStream(chunk);
                        using var dfltStream = new DeflateStream(cmpStream, CompressionMode.Decompress);
                        dfltStream.CopyTo(dcmpStream);
                    } else {
                        dcmpStream.Write(chunk, 0, chunk.Length);
                    }
                }
            }

            return decompressed;
        }

        private static byte[] DecompressDCXEDGE(BinaryReaderEx br) {
            _ = br.AssertASCII("DCX\0");
            _ = br.AssertInt32(0x10000);
            _ = br.AssertInt32(0x18);
            _ = br.AssertInt32(0x24);
            _ = br.AssertInt32(0x24);
            int unk1 = br.ReadInt32();

            _ = br.AssertASCII("DCS\0");
            int uncompressedSize = br.ReadInt32();
            _ = br.ReadInt32();

            _ = br.AssertASCII("DCP\0");
            _ = br.AssertASCII("EDGE");
            _ = br.AssertInt32(0x20);
            _ = br.AssertInt32(0x9000000);
            _ = br.AssertInt32(0x10000);
            _ = br.AssertInt32(0x0);
            _ = br.AssertInt32(0x0);
            _ = br.AssertInt32(0x00100100);

            long dcaStart = br.Position;
            _ = br.AssertASCII("DCA\0");
            int dcaSize = br.ReadInt32();
            // ???
            _ = br.AssertASCII("EgdT");
            _ = br.AssertInt32(0x00010100);
            _ = br.AssertInt32(0x24);
            _ = br.AssertInt32(0x10);
            _ = br.AssertInt32(0x10000);
            // Uncompressed size of last block
            _ = br.AssertInt32(uncompressedSize % 0x10000, 0x10000);
            int egdtSize = br.ReadInt32();
            int chunkCount = br.ReadInt32();
            _ = br.AssertInt32(0x100000);

            if (unk1 != 0x50 + chunkCount * 0x10) {
                throw new InvalidDataException("Unexpected unk1 value in EDGE DCX.");
            }

            if (egdtSize != 0x24 + chunkCount * 0x10) {
                throw new InvalidDataException("Unexpected EgdT size in EDGE DCX.");
            }

            byte[] decompressed = new byte[uncompressedSize];
            using (var dcmpStream = new MemoryStream(decompressed)) {
                for (int i = 0; i < chunkCount; i++) {
                    _ = br.AssertInt32(0);
                    int offset = br.ReadInt32();
                    int size = br.ReadInt32();
                    bool compressed = br.AssertInt32(0, 1) == 1;

                    byte[] chunk = br.GetBytes(dcaStart + dcaSize + offset, size);

                    if (compressed) {
                        using var cmpStream = new MemoryStream(chunk);
                        using var dfltStream = new DeflateStream(cmpStream, CompressionMode.Decompress);
                        dfltStream.CopyTo(dcmpStream);
                    } else {
                        dcmpStream.Write(chunk, 0, chunk.Length);
                    }
                }
            }

            return decompressed;
        }

        private static byte[] DecompressDCXDFLT(BinaryReaderEx br, Type type) {
            int unk04 = type is Type.DCX_DFLT_10000_24_9 or Type.DCX_DFLT_10000_44_9 ? 0x10000 : 0x11000;
            int unk10 = type == Type.DCX_DFLT_10000_24_9 ? 0x24 : 0x44;
            int unk14 = type == Type.DCX_DFLT_10000_24_9 ? 0x2C : 0x4C;
            byte unk30 = (byte)(type == Type.DCX_DFLT_11000_44_8 ? 8 : 9);
            byte unk38 = (byte)(type == Type.DCX_DFLT_11000_44_9_15 ? 15 : 0);

            _ = br.AssertASCII("DCX\0");
            _ = br.AssertInt32(unk04);
            _ = br.AssertInt32(0x18);
            _ = br.AssertInt32(0x24);
            _ = br.AssertInt32(unk10);
            _ = br.AssertInt32(unk14);

            _ = br.AssertASCII("DCS\0");
            _ = br.ReadInt32();
            int compressedSize = br.ReadInt32();

            _ = br.AssertASCII("DCP\0");
            _ = br.AssertASCII("DFLT");
            _ = br.AssertInt32(0x20);
            _ = br.AssertByte(unk30);
            _ = br.AssertByte(0);
            _ = br.AssertByte(0);
            _ = br.AssertByte(0);
            _ = br.AssertInt32(0x0);
            _ = br.AssertByte(unk38);
            _ = br.AssertByte(0);
            _ = br.AssertByte(0);
            _ = br.AssertByte(0);
            _ = br.AssertInt32(0x0);
            // These look suspiciously like flags
            _ = br.AssertInt32(0x00010100);

            _ = br.AssertASCII("DCA\0");
            _ = br.ReadInt32();

            return SFUtil.ReadZlib(br, compressedSize);
        }

        private static byte[] DecompressDCXKRAK(BinaryReaderEx br) {
            _ = br.AssertASCII("DCX\0");
            _ = br.AssertInt32(0x11000);
            _ = br.AssertInt32(0x18);
            _ = br.AssertInt32(0x24);
            _ = br.AssertInt32(0x44);
            _ = br.AssertInt32(0x4C);
            _ = br.AssertASCII("DCS\0");
            uint uncompressedSize = br.ReadUInt32();
            uint compressedSize = br.ReadUInt32();
            _ = br.AssertASCII("DCP\0");
            _ = br.AssertASCII("KRAK");
            _ = br.AssertInt32(0x20);
            _ = br.AssertInt32(0x6000000);
            _ = br.AssertInt32(0);
            _ = br.AssertInt32(0);
            _ = br.AssertInt32(0);
            _ = br.AssertInt32(0x10100);
            _ = br.AssertASCII("DCA\0");
            _ = br.AssertInt32(8);

            byte[] compressed = br.ReadBytes((int)compressedSize);
            return Oodle26.Decompress(compressed, uncompressedSize);
        }

        #region Public Compress
        /// <summary>
        /// Compress a DCX file to an array of bytes using the specified DCX type.
        /// </summary>
        public static byte[] Compress(byte[] data, Type type) {
            var bw = new BinaryWriterEx(true);
            Compress(data, bw, type);
            return bw.FinishBytes();
        }

        /// <summary>
        /// Compress a DCX file to the specified path using the specified DCX type.
        /// </summary>
        public static void Compress(byte[] data, Type type, string path) {
            using FileStream stream = File.Create(path);
            var bw = new BinaryWriterEx(true, stream);
            Compress(data, bw, type);
            bw.Finish();
        }
        #endregion

        internal static void Compress(byte[] data, BinaryWriterEx bw, Type type) {
            bw.BigEndian = true;
            if (type == Type.Zlib) {
                _ = SFUtil.WriteZlib(bw, 0xDA, data);
            } else if (type == Type.DCP_DFLT) {
                CompressDCPDFLT(data, bw);
            } else if (type == Type.DCX_EDGE) {
                CompressDCXEDGE(data, bw);
            } else if (type is Type.DCX_DFLT_10000_24_9
                or Type.DCX_DFLT_10000_44_9
                or Type.DCX_DFLT_11000_44_8
                or Type.DCX_DFLT_11000_44_9
                or Type.DCX_DFLT_11000_44_9_15) {
                CompressDCXDFLT(data, bw, type);
            } else if (type == Type.DCX_KRAK) {
                CompressDCXKRAK(data, bw);
            } else if (type == Type.Unknown) {
                throw new ArgumentException("You cannot compress a DCX with an unknown type.");
            } else {
                throw new NotImplementedException("Compression for the given type is not implemented.");
            }
        }

        private static void CompressDCPDFLT(byte[] data, BinaryWriterEx bw) {
            bw.WriteASCII("DCP\0");
            bw.WriteASCII("DFLT");
            bw.WriteInt32(0x20);
            bw.WriteInt32(0x9000000);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0x00010100);

            bw.WriteASCII("DCS\0");
            bw.WriteInt32(data.Length);
            bw.ReserveInt32("CompressedSize");

            int compressedSize = SFUtil.WriteZlib(bw, 0xDA, data);
            bw.FillInt32("CompressedSize", compressedSize);

            bw.WriteASCII("DCA\0");
            bw.WriteInt32(8);
        }

        private static void CompressDCXEDGE(byte[] data, BinaryWriterEx bw) {
            int chunkCount = data.Length / 0x10000;
            if (data.Length % 0x10000 > 0) {
                chunkCount++;
            }

            bw.WriteASCII("DCX\0");
            bw.WriteInt32(0x10000);
            bw.WriteInt32(0x18);
            bw.WriteInt32(0x24);
            bw.WriteInt32(0x24);
            bw.WriteInt32(0x50 + chunkCount * 0x10);

            bw.WriteASCII("DCS\0");
            bw.WriteInt32(data.Length);
            bw.ReserveInt32("CompressedSize");

            bw.WriteASCII("DCP\0");
            bw.WriteASCII("EDGE");
            bw.WriteInt32(0x20);
            bw.WriteInt32(0x9000000);
            bw.WriteInt32(0x10000);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0x00100100);

            long dcaStart = bw.Position;
            bw.WriteASCII("DCA\0");
            bw.ReserveInt32("DCASize");
            long egdtStart = bw.Position;
            bw.WriteASCII("EgdT");
            bw.WriteInt32(0x00010100);
            bw.WriteInt32(0x24);
            bw.WriteInt32(0x10);
            bw.WriteInt32(0x10000);
            bw.WriteInt32(data.Length % 0x10000);
            bw.ReserveInt32("EGDTSize");
            bw.WriteInt32(chunkCount);
            bw.WriteInt32(0x100000);

            for (int i = 0; i < chunkCount; i++) {
                bw.WriteInt32(0);
                bw.ReserveInt32($"ChunkOffset{i}");
                bw.ReserveInt32($"ChunkSize{i}");
                bw.ReserveInt32($"ChunkCompressed{i}");
            }

            bw.FillInt32("DCASize", (int)(bw.Position - dcaStart));
            bw.FillInt32("EGDTSize", (int)(bw.Position - egdtStart));
            long dataStart = bw.Position;

            int compressedSize = 0;
            for (int i = 0; i < chunkCount; i++) {
                int chunkSize = 0x10000;
                if (i == chunkCount - 1) {
                    chunkSize = data.Length % 0x10000;
                }

                byte[] chunk;
                using (var cmpStream = new MemoryStream())
                using (var dcmpStream = new MemoryStream(data, i * 0x10000, chunkSize)) {
                    var dfltStream = new DeflateStream(cmpStream, CompressionMode.Compress);
                    dcmpStream.CopyTo(dfltStream);
                    dfltStream.Close();
                    chunk = cmpStream.ToArray();
                }

                if (chunk.Length < chunkSize) {
                    bw.FillInt32($"ChunkCompressed{i}", 1);
                } else {
                    bw.FillInt32($"ChunkCompressed{i}", 0);
                    chunk = data;
                }

                compressedSize += chunk.Length;
                bw.FillInt32($"ChunkOffset{i}", (int)(bw.Position - dataStart));
                bw.FillInt32($"ChunkSize{i}", chunk.Length);
                bw.WriteBytes(chunk);
                bw.Pad(0x10);
            }

            bw.FillInt32("CompressedSize", compressedSize);
        }

        private static void CompressDCXDFLT(byte[] data, BinaryWriterEx bw, Type type) {
            bw.WriteASCII("DCX\0");

            if (type is Type.DCX_DFLT_10000_24_9 or Type.DCX_DFLT_10000_44_9) {
                bw.WriteInt32(0x10000);
            } else if (type is Type.DCX_DFLT_11000_44_8 or Type.DCX_DFLT_11000_44_9 or Type.DCX_DFLT_11000_44_9_15) {
                bw.WriteInt32(0x11000);
            }

            bw.WriteInt32(0x18);
            bw.WriteInt32(0x24);

            if (type == Type.DCX_DFLT_10000_24_9) {
                bw.WriteInt32(0x24);
                bw.WriteInt32(0x2C);
            } else if (type is Type.DCX_DFLT_10000_44_9 or Type.DCX_DFLT_11000_44_8 or Type.DCX_DFLT_11000_44_9 or Type.DCX_DFLT_11000_44_9_15) {
                bw.WriteInt32(0x44);
                bw.WriteInt32(0x4C);
            }

            bw.WriteASCII("DCS\0");
            bw.WriteInt32(data.Length);
            bw.ReserveInt32("CompressedSize");
            bw.WriteASCII("DCP\0");
            bw.WriteASCII("DFLT");
            bw.WriteInt32(0x20);

            if (type is Type.DCX_DFLT_10000_24_9 or Type.DCX_DFLT_10000_44_9 or Type.DCX_DFLT_11000_44_9 or Type.DCX_DFLT_11000_44_9_15) {
                bw.WriteByte(9);
            } else if (type == Type.DCX_DFLT_11000_44_8) {
                bw.WriteByte(8);
            }
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteByte(0);

            bw.WriteInt32(0);

            if (type == Type.DCX_DFLT_11000_44_9_15) {
                bw.WriteByte(15);
            } else {
                bw.WriteByte(0);
            }
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteByte(0);

            bw.WriteInt32(0);
            bw.WriteInt32(0x00010100);
            bw.WriteASCII("DCA\0");
            bw.WriteInt32(8);

            long compressedStart = bw.Position;
            _ = SFUtil.WriteZlib(bw, 0xDA, data);
            bw.FillInt32("CompressedSize", (int)(bw.Position - compressedStart));
        }

        private static void CompressDCXKRAK(byte[] data, BinaryWriterEx bw) {
            byte[] compressed = Oodle26.Compress(data, Oodle26.OodleLZ_Compressor.OodleLZ_Compressor_Kraken, Oodle26.OodleLZ_CompressionLevel.OodleLZ_CompressionLevel_Optimal2);

            bw.WriteASCII("DCX\0");
            bw.WriteInt32(0x11000);
            bw.WriteInt32(0x18);
            bw.WriteInt32(0x24);
            bw.WriteInt32(0x44);
            bw.WriteInt32(0x4C);
            bw.WriteASCII("DCS\0");
            bw.WriteUInt32((uint)data.Length);
            bw.WriteUInt32((uint)compressed.Length);
            bw.WriteASCII("DCP\0");
            bw.WriteASCII("KRAK");
            bw.WriteInt32(0x20);
            bw.WriteInt32(0x6000000);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0x10100);
            bw.WriteASCII("DCA\0");
            bw.WriteInt32(8);
            bw.WriteBytes(compressed);
            bw.Pad(0x10);
        }

        /// <summary>
        /// Specific compression format used for a certain file.
        /// </summary>
        public enum Type : byte {
            /// <summary>
            /// DCX type could not be detected.
            /// </summary>
            Unknown,

            /// <summary>
            /// The file is not compressed.
            /// </summary>
            None,

            /// <summary>
            /// Plain zlib-wrapped data; not really DCX, but it's convenient to support it here.
            /// </summary>
            Zlib,

            /// <summary>
            /// DCP header, chunked deflate compression. Used in ACE:R TPFs.
            /// </summary>
            DCP_EDGE,

            /// <summary>
            /// DCP header, deflate compression. Used in DeS test maps.
            /// </summary>
            DCP_DFLT,

            /// <summary>
            /// DCX header, chunked deflate compression. Primarily used in DeS.
            /// </summary>
            DCX_EDGE,

            /// <summary>
            /// DCX header, deflate compression. Primarily used in DS1 and DS2.
            /// </summary>
            DCX_DFLT_10000_24_9,

            /// <summary>
            /// DCX header, deflate compression. Primarily used in BB and DS3.
            /// </summary>
            DCX_DFLT_10000_44_9,

            /// <summary>
            /// DCX header, deflate compression. Used for the backup regulation in DS3 save files.
            /// </summary>
            DCX_DFLT_11000_44_8,

            /// <summary>
            /// DCX header, deflate compression. Used in Sekiro.
            /// </summary>
            DCX_DFLT_11000_44_9,

            /// <summary>
            /// DCX header, deflate compression. Used in the ER regulation.
            /// </summary>
            DCX_DFLT_11000_44_9_15,

            /// <summary>
            /// DCX header, Oodle compression. Used in Sekiro.
            /// </summary>
            DCX_KRAK,
        }

        /// <summary>
        /// Standard compression types used by various games; may be cast directly to DCX.Type.
        /// </summary>
        public enum DefaultType : byte {
            /// <summary>
            /// Most common compression format for Demon's Souls.
            /// </summary>
            DemonsSouls = Type.DCX_EDGE,

            /// <summary>
            /// Most common compression format for Dark Souls 1.
            /// </summary>
            DarkSouls1 = Type.DCX_DFLT_10000_24_9,

            /// <summary>
            /// Most common compression format for Dark Souls 2.
            /// </summary>
            DarkSouls2 = Type.DCX_DFLT_10000_24_9,

            /// <summary>
            /// Most common compression format for Bloodborne.
            /// </summary>
            Bloodborne = Type.DCX_DFLT_10000_44_9,

            /// <summary>
            /// Most common compression format for Dark Souls 3.
            /// </summary>
            DarkSouls3 = Type.DCX_DFLT_10000_44_9,

            /// <summary>
            /// Most common compression format for Sekiro.
            /// </summary>
            Sekiro = Type.DCX_KRAK,

            /// <summary>
            /// Most common compression format for Elden Ring.
            /// </summary>
            EldenRing = Type.DCX_KRAK,
        }
    }
}
