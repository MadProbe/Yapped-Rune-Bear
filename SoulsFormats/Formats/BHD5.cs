using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using SoulsFormats.Util;

namespace SoulsFormats.Formats {
    /// <summary>
    /// The header file of the dvdbnd container format used to package all game files with hashed filenames.
    /// </summary>
    public class BHD5 {
        /// <summary>
        /// Format the file should be written in.
        /// </summary>
        public Game Format { get; set; }

        /// <summary>
        /// Whether the header is big-endian.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// Unknown; possibly whether crypto is allowed? Offsets are present regardless.
        /// </summary>
        public bool Unk05 { get; set; }

        /// <summary>
        /// A salt used to calculate SHA hashes for file data.
        /// </summary>
        public string Salt { get; set; }

        /// <summary>
        /// Collections of files grouped by their hash value for faster lookup.
        /// </summary>
        public List<Bucket> Buckets { get; set; }

        /// <summary>
        /// Read a dvdbnd header from the given stream, formatted for the given game. Must already be decrypted, if applicable.
        /// </summary>
        public static BHD5 Read(Stream bhdStream, Game game) {
            var br = new BinaryReaderEx(false, bhdStream);
            return new BHD5(br, game);
        }

        /// <summary>
        /// Write a dvdbnd header to the given stream.
        /// </summary>
        public void Write(Stream bhdStream) {
            var bw = new BinaryWriterEx(false, bhdStream);
            this.Write(bw);
            bw.Finish();
        }

        /// <summary>
        /// Creates an empty BHD5.
        /// </summary>
        public BHD5(Game game) {
            this.Format = game;
            this.Salt = "";
            this.Buckets = new List<Bucket>();
        }

        private BHD5(BinaryReaderEx br, Game game) {
            this.Format = game;

            _ = br.AssertASCII("BHD5");
            this.BigEndian = br.AssertSByte(0, -1) == 0;
            br.BigEndian = this.BigEndian;
            this.Unk05 = br.ReadBoolean();
            _ = br.AssertByte(0);
            _ = br.AssertByte(0);
            _ = br.AssertInt32(1);
            _ = br.ReadInt32(); // File size
            int bucketCount = br.ReadInt32();
            int bucketsOffset = br.ReadInt32();

            if (game >= Game.DarkSouls2) {
                int saltLength = br.ReadInt32();
                this.Salt = br.ReadASCII(saltLength);
                // No padding
            }

            br.Position = bucketsOffset;
            this.Buckets = new List<Bucket>(bucketCount);
            for (int i = 0; i < bucketCount; i++) {
                this.Buckets.Add(new Bucket(br, game));
            }
        }

        private void Write(BinaryWriterEx bw) {
            bw.BigEndian = this.BigEndian;
            bw.WriteASCII("BHD5");
            bw.WriteSByte((sbyte)(this.BigEndian ? 0 : -1));
            bw.WriteBoolean(this.Unk05);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteInt32(1);
            bw.ReserveInt32("FileSize");
            bw.WriteInt32(this.Buckets.Count);
            bw.ReserveInt32("BucketsOffset");

            if (this.Format >= Game.DarkSouls2) {
                bw.WriteInt32(this.Salt.Length);
                bw.WriteASCII(this.Salt);
            }

            bw.FillInt32("BucketsOffset", (int)bw.Position);
            for (int i = 0; i < this.Buckets.Count; i++) {
                this.Buckets[i].Write(bw, i);
            }

            for (int i = 0; i < this.Buckets.Count; i++) {
                this.Buckets[i].WriteFileHeaders(bw, this.Format, i);
            }

            for (int i = 0; i < this.Buckets.Count; i++) {
                for (int j = 0; j < this.Buckets[i].Count; j++) {
                    this.Buckets[i][j].WriteHashAndKey(bw, this.Format, i, j);
                }
            }

            bw.FillInt32("FileSize", (int)bw.Position);
        }

        /// <summary>
        /// Indicates the format of a dvdbnd.
        /// </summary>
        public enum Game {
            /// <summary>
            /// Dark Souls 1, both PC and console versions.
            /// </summary>
            DarkSouls1,

            /// <summary>
            /// Dark Souls 2 and Scholar of the First Sin on PC.
            /// </summary>
            DarkSouls2,

            /// <summary>
            /// Dark Souls 3 and Sekiro on PC.
            /// </summary>
            DarkSouls3,

            /// <summary>
            /// Elden Ring on PC.
            /// </summary>
            EldenRing,
        }

        /// <summary>
        /// A collection of files grouped by their hash.
        /// </summary>
        public class Bucket : List<FileHeader> {
            /// <summary>
            /// Creates an empty Bucket.
            /// </summary>
            public Bucket() : base() { }

            internal Bucket(BinaryReaderEx br, Game game) : base() {
                int fileHeaderCount = br.ReadInt32();
                int fileHeadersOffset = br.ReadInt32();
                this.Capacity = fileHeaderCount;

                br.StepIn(fileHeadersOffset);
                {
                    for (int i = 0; i < fileHeaderCount; i++) {
                        this.Add(new FileHeader(br, game));
                    }
                }
                br.StepOut();
            }

            internal void Write(BinaryWriterEx bw, int index) {
                bw.WriteInt32(this.Count);
                bw.ReserveInt32($"FileHeadersOffset{index}");
            }

            internal void WriteFileHeaders(BinaryWriterEx bw, Game game, int index) {
                bw.FillInt32($"FileHeadersOffset{index}", (int)bw.Position);
                for (int i = 0; i < this.Count; i++) {
                    this[i].Write(bw, game, index, i);
                }
            }
        }

        /// <summary>
        /// Information about an individual file in the dvdbnd.
        /// </summary>
        public class FileHeader {
            /// <summary>
            /// Hash of the full file path using From's algorithm found in SFUtil.FromPathHash.
            /// </summary>
            public ulong FileNameHash { get; set; }

            /// <summary>
            /// Full size of the file data in the BDT.
            /// </summary>
            public int PaddedFileSize { get; set; }

            /// <summary>
            /// File size after decryption; only included in DS3.
            /// </summary>
            public long UnpaddedFileSize { get; set; }

            /// <summary>
            /// Beginning of file data in the BDT.
            /// </summary>
            public long FileOffset { get; set; }

            /// <summary>
            /// Hashing information for this file.
            /// </summary>
            public SHAHash SHAHash { get; set; }

            /// <summary>
            /// Encryption information for this file.
            /// </summary>
            public AESKey AESKey { get; set; }

            /// <summary>
            /// Creates a FileHeader with default values.
            /// </summary>
            public FileHeader() { }

            internal FileHeader(BinaryReaderEx br, Game game) {
                long shaHashOffset = 0;
                long aesKeyOffset = 0;
                this.UnpaddedFileSize = -1;

                if (game >= Game.EldenRing) {
                    this.FileNameHash = br.ReadUInt64();
                    this.PaddedFileSize = br.ReadInt32();
                    this.UnpaddedFileSize = br.ReadInt32();
                    this.FileOffset = br.ReadInt64();
                    shaHashOffset = br.ReadInt64();
                    aesKeyOffset = br.ReadInt64();
                } else {
                    this.FileNameHash = br.ReadUInt32();
                    this.PaddedFileSize = br.ReadInt32();
                    this.FileOffset = br.ReadInt64();

                    if (game >= Game.DarkSouls2) {
                        shaHashOffset = br.ReadInt64();
                        aesKeyOffset = br.ReadInt64();
                    }

                    if (game >= Game.DarkSouls3) {
                        this.UnpaddedFileSize = br.ReadInt64();
                    }
                }

                if (shaHashOffset != 0) {
                    br.StepIn(shaHashOffset);
                    {
                        this.SHAHash = new SHAHash(br);
                    }
                    br.StepOut();
                }

                if (aesKeyOffset != 0) {
                    br.StepIn(aesKeyOffset);
                    {
                        this.AESKey = new AESKey(br);
                    }
                    br.StepOut();
                }
            }

            internal void Write(BinaryWriterEx bw, Game game, int bucketIndex, int fileIndex) {
                if (game >= Game.EldenRing) {
                    bw.WriteUInt64(this.FileNameHash);
                    bw.WriteInt32(this.PaddedFileSize);
                    bw.WriteInt32((int)this.UnpaddedFileSize);
                    bw.WriteInt64(this.FileOffset);
                    bw.ReserveInt64($"AESKeyOffset{bucketIndex}:{fileIndex}");
                    bw.ReserveInt64($"SHAHashOffset{bucketIndex}:{fileIndex}");
                } else {
                    bw.WriteUInt32((uint)this.FileNameHash);
                    bw.WriteInt32(this.PaddedFileSize);
                    bw.WriteInt64(this.FileOffset);

                    if (game >= Game.DarkSouls2) {
                        bw.ReserveInt64($"SHAHashOffset{bucketIndex}:{fileIndex}");
                        bw.ReserveInt64($"AESKeyOffset{bucketIndex}:{fileIndex}");
                    }

                    if (game >= Game.DarkSouls3) {
                        bw.WriteInt64(this.UnpaddedFileSize);
                    }
                }
            }

            internal void WriteHashAndKey(BinaryWriterEx bw, Game game, int bucketIndex, int fileIndex) {
                if (game >= Game.DarkSouls2) {
                    if (this.SHAHash == null) {
                        bw.FillInt64($"SHAHashOffset{bucketIndex}:{fileIndex}", 0);
                    } else {
                        bw.FillInt64($"SHAHashOffset{bucketIndex}:{fileIndex}", bw.Position);
                        this.SHAHash.Write(bw);
                    }

                    if (this.AESKey == null) {
                        bw.FillInt64($"AESKeyOffset{bucketIndex}:{fileIndex}", 0);
                    } else {
                        bw.FillInt64($"AESKeyOffset{bucketIndex}:{fileIndex}", bw.Position);
                        this.AESKey.Write(bw);
                    }
                }
            }

            /// <summary>
            /// Read and decrypt (if necessary) file data from the BDT.
            /// </summary>
            public byte[] ReadFile(FileStream bdtStream) {
                byte[] bytes = new byte[this.PaddedFileSize];
                bdtStream.Position = this.FileOffset;
                _ = bdtStream.Read(bytes, 0, this.PaddedFileSize);
                this.AESKey?.Decrypt(bytes);
                return bytes;
            }
        }

        /// <summary>
        /// Hash information for a file in the dvdbnd.
        /// </summary>
        public class SHAHash {
            /// <summary>
            /// 32-byte salted SHA hash.
            /// </summary>
            public byte[] Hash { get; set; }

            /// <summary>
            /// Hashed sections of the file.
            /// </summary>
            public List<Range> Ranges { get; set; }

            /// <summary>
            /// Creates a SHAHash with default values.
            /// </summary>
            public SHAHash() {
                this.Hash = new byte[32];
                this.Ranges = new List<Range>();
            }

            internal SHAHash(BinaryReaderEx br) {
                this.Hash = br.ReadBytes(32);
                int rangeCount = br.ReadInt32();
                this.Ranges = new List<Range>(rangeCount);
                for (int i = 0; i < rangeCount; i++) {
                    this.Ranges.Add(new Range(br));
                }
            }

            internal void Write(BinaryWriterEx bw) {
                if (this.Hash.Length != 32) {
                    throw new InvalidDataException("SHA hash must be 32 bytes long.");
                }

                bw.WriteBytes(this.Hash);
                bw.WriteInt32(this.Ranges.Count);
                foreach (Range range in this.Ranges) {
                    range.Write(bw);
                }
            }
        }

        /// <summary>
        /// Encryption information for a file in the dvdbnd.
        /// </summary>
        public class AESKey {
            private static readonly Aes AES = Aes.Create();
            static AESKey() {
                AES.Mode = CipherMode.ECB;
                AES.Padding = PaddingMode.None;
                AES.KeySize = 128;
            }

            /// <summary>
            /// 16-byte encryption key.
            /// </summary>
            public byte[] Key { get; set; }

            /// <summary>
            /// Encrypted sections of the file.
            /// </summary>
            public List<Range> Ranges { get; set; }

            /// <summary>
            /// Creates an AESKey with default values.
            /// </summary>
            public AESKey() {
                this.Key = new byte[16];
                this.Ranges = new List<Range>();
            }

            internal AESKey(BinaryReaderEx br) {
                this.Key = br.ReadBytes(16);
                int rangeCount = br.ReadInt32();
                this.Ranges = new List<Range>(rangeCount);
                for (int i = 0; i < rangeCount; i++) {
                    this.Ranges.Add(new Range(br));
                }
            }

            internal void Write(BinaryWriterEx bw) {
                if (this.Key.Length != 16) {
                    throw new InvalidDataException("AES key must be 16 bytes long.");
                }

                bw.WriteBytes(this.Key);
                bw.WriteInt32(this.Ranges.Count);
                foreach (Range range in this.Ranges) {
                    range.Write(bw);
                }
            }

            /// <summary>
            /// Decrypt file data in-place.
            /// </summary>
            public void Decrypt(byte[] bytes) {
                using ICryptoTransform decryptor = AES.CreateDecryptor(this.Key, new byte[16]);
                foreach (Range range in this.Ranges.Where(r => r.StartOffset != -1 && r.EndOffset != -1 && r.StartOffset != r.EndOffset)) {
                    int start = (int)range.StartOffset;
                    int count = (int)(range.EndOffset - range.StartOffset);
                    _ = decryptor.TransformBlock(bytes, start, count, bytes, start);
                }
            }
        }

        /// <summary>
        /// Indicates a hashed or encrypted section of a file.
        /// </summary>
        public struct Range {
            /// <summary>
            /// The beginning of the range, inclusive.
            /// </summary>
            public long StartOffset { get; set; }

            /// <summary>
            /// The end of the range, exclusive.
            /// </summary>
            public long EndOffset { get; set; }

            /// <summary>
            /// Creates a Range with the given values.
            /// </summary>
            public Range(long startOffset, long endOffset) {
                this.StartOffset = startOffset;
                this.EndOffset = endOffset;
            }

            internal Range(BinaryReaderEx br) {
                this.StartOffset = br.ReadInt64();
                this.EndOffset = br.ReadInt64();
            }

            internal void Write(BinaryWriterEx bw) {
                bw.WriteInt64(this.StartOffset);
                bw.WriteInt64(this.EndOffset);
            }
        }
    }
}
