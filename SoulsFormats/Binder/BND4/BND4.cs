using SoulsFormats.Util;

namespace SoulsFormats.Binder.BND4 {
    /// <summary>
    /// A general-purpose file container used since DS2. Extension: .*bnd
    /// </summary>
    public class BND4 : SoulsFile<BND4>, IBinder, IBND4 {
        /// <summary>
        /// The files contained within this BND4.
        /// </summary>
        public List<BinderFile> Files { get; set; } = [];

        /// <summary>
        /// A timestamp or version number, 8 characters maximum.
        /// </summary>
        public string Version { get; set; } = SFUtil.DateToBinderTimestamp(DateTime.Now);

        /// <summary>
        /// Indicates the format of this BND4.
        /// </summary>
        public Binder.Format Format { get; set; } = Binder.Format.IDs | Binder.Format.Names1 | Binder.Format.Names2 | Binder.Format.Compression;

        /// <summary>
        /// Unknown.
        /// </summary>
        public bool Unk04 { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public bool Unk05 { get; set; }

        /// <summary>
        /// Whether to write in big-endian format or not (little-endian).
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// Controls ordering of flag bits.
        /// </summary>
        public bool BitBigEndian { get; set; }

        /// <summary>
        /// Whether to encode filenames as UTF-8 or Shift JIS.
        /// </summary>
        public bool Unicode { get; set; } = true;

        /// <summary>
        /// Indicates presence of filename hash table.
        /// </summary>
        public byte Extended { get; set; } = 4;

        /// <summary>
        /// Creates an empty BND4 formatted for DS3.
        /// </summary>
        public BND4() { }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected internal override bool Is(BinaryReaderEx br) => br.Length >= 4 && br.GetASCII(0, 4) == "BND4";

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            List<BinderFileHeader> fileHeaders = ReadHeader(this, br);
            int count = fileHeaders.Count;
            this.Files = new List<BinderFile>(count);
            BinderFile[] files = this.Files.AsContents();
            for (int i = 0; i < count; i++) {
                files[i] = fileHeaders[i].ReadFileData(br);
            }
            this.Files.SetLength(count);
        }

        internal static List<BinderFileHeader> ReadHeader(IBND4 bnd, BinaryReaderEx br) {
            _ = br.AssertASCII("BND4");

            bnd.Unk04 = br.ReadBoolean();
            bnd.Unk05 = br.ReadBoolean();
            _ = br.AssertByte(0);
            _ = br.AssertByte(0);

            _ = br.AssertByte(0);
            bnd.BigEndian = br.ReadBoolean();
            bnd.BitBigEndian = !br.ReadBoolean();
            _ = br.AssertByte(0);

            br.BigEndian = bnd.BigEndian;

            int fileCount = br.ReadInt32();
            _ = br.AssertInt64(0x40); // Header size
            bnd.Version = br.ReadFixStr(8);
            long fileHeaderSize = br.ReadInt64();
            _ = br.ReadInt64(); // Headers end (includes hash table)

            bnd.Unicode = br.ReadBoolean();
            bnd.Format = Binder.ReadFormat(br, bnd.BitBigEndian);
            bnd.Extended = br.AssertByte(0, 1, 4, 0x80);
            _ = br.AssertByte(0);

            _ = br.AssertInt32(0);

            if (bnd.Extended == 4) {
                long hashTableOffset = br.ReadInt64();
                br.StepIn(hashTableOffset);
                BinderHashTable.Assert(br);
                br.StepOut();
            } else {
                _ = br.AssertInt64(0);
            }

            if (fileHeaderSize != Binder.GetBND4FileHeaderSize(bnd.Format)) {
                throw new FormatException($"File header size for format {bnd.Format} is expected to be 0x{Binder.GetBND4FileHeaderSize(bnd.Format):X}, but was 0x{fileHeaderSize:X}");
            }

            var fileHeaders = new List<BinderFileHeader>(fileCount);
            for (int i = 0; i < fileCount; i++) {
                fileHeaders.Add(BinderFileHeader.ReadBinder4FileHeader(br, bnd.Format, bnd.BitBigEndian, bnd.Unicode));
            }

            return fileHeaders;
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected internal override void Write(BinaryWriterEx bw) {
            var fileHeaders = new BinderFileHeader[this.Files.Count];
            for (int i = 0; i < fileHeaders.Length; i++) {
                fileHeaders[i] = new BinderFileHeader(this.Files[i]);
            }

            WriteHeader(this, bw, fileHeaders);
            for (int i = 0; i < fileHeaders.Length; i++) {
                fileHeaders[i].WriteBinder4FileData(bw, bw, this.Format, i, this.Files[i].Bytes);
            }
        }

        internal static void WriteHeader(IBND4 bnd, BinaryWriterEx bw, BinderFileHeader[] fileHeaders) {
            bw.BigEndian = bnd.BigEndian;

            bw.WriteASCII("BND4");

            bw.WriteBoolean(bnd.Unk04);
            bw.WriteBoolean(bnd.Unk05);
            bw.WriteByte(0);
            bw.WriteByte(0);

            bw.WriteByte(0);
            bw.WriteBoolean(bnd.BigEndian);
            bw.WriteBoolean(!bnd.BitBigEndian);
            bw.WriteByte(0);

            bw.WriteInt32(fileHeaders.Length);
            bw.WriteInt64(0x40);
            bw.WriteFixStr(bnd.Version, 8);
            bw.WriteInt64(Binder.GetBND4FileHeaderSize(bnd.Format));
            bw.ReserveInt64("HeadersEnd");

            bw.WriteBoolean(bnd.Unicode);
            Binder.WriteFormat(bw, bnd.BitBigEndian, bnd.Format);
            bw.WriteByte(bnd.Extended);
            bw.WriteByte(0);

            bw.WriteInt32(0);
            bw.ReserveInt64("HashTableOffset");

            for (int i = 0; i < fileHeaders.Length; i++) {
                fileHeaders[i].WriteBinder4FileHeader(bw, bnd.Format, bnd.BitBigEndian, i);
            }

            for (int i = 0; i < fileHeaders.Length; i++) {
                fileHeaders[i].WriteFileName(bw, bnd.Format, bnd.Unicode, i);
            }

            if (bnd.Extended == 4) {
                bw.Pad(0x8);
                bw.FillInt64("HashTableOffset", bw.Position);
                BinderHashTable.Write(bw, fileHeaders);
            } else {
                bw.FillInt64("HashTableOffset", 0);
            }

            bw.FillInt64("HeadersEnd", bw.Position);
        }
    }
}
