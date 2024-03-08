using SoulsFormats.Formats;
using SoulsFormats.Util;
using static SoulsFormats.Binder.Binder;

namespace SoulsFormats.Binder {
    /// <summary>
    /// Metadata for a file in a binder container.
    /// </summary>
    public class BinderFileHeader {
        /// <summary>
        /// Flags indicating compression, and possibly other things.
        /// </summary>
        public FileFlags Flags;

        /// <summary>
        /// ID of the file, or -1 for none.
        /// </summary>
        public int ID;

        /// <summary>
        /// Name of the file, or null for none.
        /// </summary>
        public string Name;

        /// <summary>
        /// If compressed, which type of compression to use.
        /// </summary>
        public DCX.Type CompressionType;

        /// <summary>
        /// Size of the file after compression (or just the size of the file, if not compressed). Do not modify unless you know what you're doing.
        /// </summary>
        public long CompressedSize;

        /// <summary>
        /// Size of the file without compression. Do not modify unless you know what you're doing.
        /// </summary>
        public long UncompressedSize;

        /// <summary>
        /// Location of file data in the BND or BXF. Do not modify unless you know what you're doing.
        /// </summary>
        public long DataOffset;

        /// <summary>
        /// Creates a BinderFileHeader with the given ID and name.
        /// </summary>
        public BinderFileHeader(int id, string name) : this(FileFlags.Flag1, id, name) { }

        /// <summary>
        /// Creates a BinderFileHeader with the given flags, ID, and name.
        /// </summary>
        public BinderFileHeader(FileFlags flags, int id, string name) : this(flags, id, name, -1, -1, -1) { }

        internal BinderFileHeader(BinderFile file) : this(file.Flags, file.ID, file.Name, -1, -1, -1) => this.CompressionType = file.CompressionType;

        private BinderFileHeader(FileFlags flags, int id, string name, long compressedSize, long uncompressedSize, long dataOffset) {
            this.Flags = flags;
            this.ID = id;
            this.Name = name;
            this.CompressionType = DCX.Type.Zlib;
            this.CompressedSize = compressedSize;
            this.UncompressedSize = uncompressedSize;
            this.DataOffset = dataOffset;
        }

        /// <summary>
        /// Returns a string representation of the object.
        /// </summary>
        public override string ToString() => $"{this.ID} {this.Name}";

        internal static BinderFileHeader ReadBinder3FileHeader(BinaryReaderEx br, Format format, bool bitBigEndian) {
            FileFlags flags = ReadFileFlags(br, bitBigEndian);
            _ = br.AssertByte(0);
            _ = br.AssertByte(0);
            _ = br.AssertByte(0);

            int compressedSize = br.ReadInt32();

            long dataOffset = HasLongOffsets(format) ? br.ReadInt64() : br.ReadUInt32();
            int id = -1;
            if (HasIDs(format)) {
                id = br.ReadInt32();
            }

            string name = null;
            if (HasNames(format)) {
                int nameOffset = br.ReadInt32();
                name = br.GetShiftJIS(nameOffset);
            }

            int uncompressedSize = -1;
            if (HasCompression(format)) {
                uncompressedSize = br.ReadInt32();
            }

            return new BinderFileHeader(flags, id, name, compressedSize, uncompressedSize, dataOffset);
        }

        internal static BinderFileHeader ReadBinder4FileHeader(BinaryReaderEx br, Format format, bool bitBigEndian, bool unicode) {
            FileFlags flags = ReadFileFlags(br, bitBigEndian);
            _ = br.AssertByte(0);
            _ = br.AssertByte(0);
            _ = br.AssertByte(0);
            _ = br.AssertInt32(-1);

            long compressedSize = br.ReadInt64();

            long uncompressedSize = -1;
            if (HasCompression(format)) {
                uncompressedSize = br.ReadInt64();
            }

            long dataOffset = HasLongOffsets(format) ? br.ReadInt64() : br.ReadUInt32();
            int id = -1;
            if (HasIDs(format)) {
                id = br.ReadInt32();
            }

            string name = null;
            if (HasNames(format)) {
                uint nameOffset = br.ReadUInt32();
                name = unicode ? br.GetUTF16(nameOffset) : br.GetShiftJIS(nameOffset);
            }

            // This is a very strange case that (as far as I know) only appears in PC save files.
            // I do not know how to handle it elegantly and this is definitely not actually an ID,
            // but it is non-zero in some cases.
            if (format == Format.Names1) {
                id = br.ReadInt32();
                _ = br.AssertInt32(0);
            }

            return new BinderFileHeader(flags, id, name, compressedSize, uncompressedSize, dataOffset);
        }

        internal BinderFile ReadFileData(BinaryReaderEx br) {
            DCX.Type compressionType = DCX.Type.Zlib;
            byte[] bytes = br.GetBytes(this.DataOffset, (int)this.CompressedSize);
            if (IsCompressed(this.Flags)) {
                bytes = DCX.Decompress(bytes, out compressionType);
            }

            return new BinderFile(this.Flags, this.ID, this.Name, bytes) {
                CompressionType = compressionType,
            };
        }

        internal void WriteBinder3FileHeader(BinaryWriterEx bw, Format format, bool bitBigEndian, int index) {
            WriteFileFlags(bw, bitBigEndian, this.Flags);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteByte(0);

            bw.ReserveInt32($"FileCompressedSize{index}");

            if (HasLongOffsets(format)) {
                bw.ReserveInt64($"FileDataOffset{index}");
            } else {
                bw.ReserveUInt32($"FileDataOffset{index}");
            }

            if (HasIDs(format)) {
                bw.WriteInt32(this.ID);
            }

            if (HasNames(format)) {
                bw.ReserveInt32($"FileNameOffset{index}");
            }

            if (HasCompression(format)) {
                bw.ReserveInt32($"FileUncompressedSize{index}");
            }
        }

        internal void WriteBinder4FileHeader(BinaryWriterEx bw, Format format, bool bitBigEndian, int index) {
            WriteFileFlags(bw, bitBigEndian, this.Flags);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteInt32(-1);

            bw.ReserveInt64($"FileCompressedSize{index}");

            if (HasCompression(format)) {
                bw.ReserveInt64($"FileUncompressedSize{index}");
            }

            if (HasLongOffsets(format)) {
                bw.ReserveInt64($"FileDataOffset{index}");
            } else {
                bw.ReserveUInt32($"FileDataOffset{index}");
            }

            if (HasIDs(format)) {
                bw.WriteInt32(this.ID);
            }

            if (HasNames(format)) {
                bw.ReserveInt32($"FileNameOffset{index}");
            }

            if (format != Format.Names1) return;
            bw.WriteInt32(this.ID);
            bw.WriteInt32(0);
        }

        private void WriteFileData(BinaryWriterEx bw, byte[] bytes) {
            if (bytes.LongLength > 0) {
                bw.Pad(0x10);
            }

            this.DataOffset = bw.Position;
            this.UncompressedSize = bytes.LongLength;
            if (IsCompressed(this.Flags)) {
                byte[] compressed = DCX.Compress(bytes, this.CompressionType);
                this.CompressedSize = compressed.LongLength;
                bw.WriteBytes(compressed);
            } else {
                this.CompressedSize = bytes.LongLength;
                bw.WriteBytes(bytes);
            }
        }

        internal void WriteBinder3FileData(BinaryWriterEx bwHeader, BinaryWriterEx bwData, Format format, int index, byte[] bytes) {
            this.WriteFileData(bwData, bytes);

            bwHeader.FillInt32($"FileCompressedSize{index}", (int)this.CompressedSize);

            if (HasCompression(format)) {
                bwHeader.FillInt32($"FileUncompressedSize{index}", (int)this.UncompressedSize);
            }

            if (HasLongOffsets(format)) {
                bwHeader.FillInt64($"FileDataOffset{index}", this.DataOffset);
            } else {
                bwHeader.FillUInt32($"FileDataOffset{index}", (uint)this.DataOffset);
            }
        }

        internal void WriteBinder4FileData(BinaryWriterEx bwHeader, BinaryWriterEx bwData, Format format, int index, byte[] bytes) {
            this.WriteFileData(bwData, bytes);

            bwHeader.FillInt64($"FileCompressedSize{index}", this.CompressedSize);

            if (HasCompression(format)) {
                bwHeader.FillInt64($"FileUncompressedSize{index}", this.UncompressedSize);
            }

            if (HasLongOffsets(format)) {
                bwHeader.FillInt64($"FileDataOffset{index}", this.DataOffset);
            } else {
                bwHeader.FillUInt32($"FileDataOffset{index}", (uint)this.DataOffset);
            }
        }

        internal void WriteFileName(BinaryWriterEx bw, Format format, bool unicode, int index) {
            if (!HasNames(format)) return;
            bw.FillInt32($"FileNameOffset{index}", (int)bw.Position);
            if (unicode) {
                bw.WriteUTF16(this.Name, true);
            } else {
                bw.WriteShiftJIS(this.Name, true);
            }
        }
    }
}
