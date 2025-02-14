﻿using SoulsFormats.Util;

namespace SoulsFormats {
    /// <summary>
    /// A general-purpose configuration file used throughout the series.
    /// </summary>
    public partial class PARAM : SoulsFile<PARAM> {
        /// <summary>
        /// Whether the file is big-endian; true for PS3/360 files, false otherwise.
        /// </summary>
        public bool BigEndian;

        /// <summary>
        /// Flags indicating format of the file.
        /// </summary>
        public FormatFlags1 Format2D;

        /// <summary>
        /// More flags indicating format of the file.
        /// </summary>
        public FormatFlags2 Format2E;

        /// <summary>
        /// Originally matched the paramdef for version 101, but since is always 0 or 0xFF.
        /// </summary>
        public byte ParamdefFormatVersion;

        /// <summary>
        /// Unknown.
        /// </summary>
        public short Unk06;

        /// <summary>
        /// Indicates a revision of the row data structure.
        /// </summary>
        public short ParamdefDataVersion;

        /// <summary>
        /// Identifies corresponding params and paramdefs.
        /// </summary>
        public string ParamType;

        /// <summary>
        /// Automatically determined based on spacing of row offsets; -1 if param had no rows.
        /// </summary>
        public long DetectedSize { get; private set; }

        /// <summary>
        /// The rows of this param; must be loaded with PARAM.ApplyParamdef() before cells can be used.
        /// </summary>
        public List<Row> Rows;

        /// <summary>
        /// Cells contained in this row. Must be loaded with PARAM.ApplyParamdef() before use.
        /// </summary>
        public Allocator MiniCellsAllocator;

        /// <summary>
        /// The current applied PARAMDEF.
        /// </summary>
        public PARAMDEF AppliedParamdef { get; private set; }

        private BinaryReaderEx RowReader;

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            br.Position = 0x2C;
            br.BigEndian = this.BigEndian = br.AssertByte(0, 0xFF) == 0xFF;
            this.Format2D = (FormatFlags1)br.ReadByte();
            this.Format2E = (FormatFlags2)br.ReadByte();
            this.ParamdefFormatVersion = br.ReadByte();
            br.Position = 0;

            // Make a private copy of the file to read row data from later
            byte[] copy = br.GetBytes(0, (int)br.Stream.Length);
            this.RowReader = new BinaryReaderEx(this.BigEndian, copy);

            // The strings offset in the header is highly unreliable; only use it as a last resort
            long actualStringsOffset = 0;
            long stringsOffset = br.ReadUInt32();
            if (this.Format2D.HasFlag(FormatFlags1.Flag01) && this.Format2D.HasFlag(FormatFlags1.IntDataOffset) || this.Format2D.HasFlag(FormatFlags1.LongDataOffset)) {
                _ = br.AssertInt16(0);
            } else {
                _ = br.ReadUInt16(); // Data start
            }
            this.Unk06 = br.ReadInt16();
            this.ParamdefDataVersion = br.ReadInt16();
            ushort rowCount = br.ReadUInt16();
            if (this.Format2D.HasFlag(FormatFlags1.OffsetParamType)) {
                _ = br.AssertInt32(0);
                long paramTypeOffset = br.ReadInt64();
                br.AssertPattern(0x14, 0x00);
                this.ParamType = br.GetASCII(paramTypeOffset);
                actualStringsOffset = paramTypeOffset;
            } else {
                this.ParamType = br.ReadFixStr(0x20);
            }
            br.Skip(4); // Format
            if (this.Format2D.HasFlag(FormatFlags1.Flag01) && this.Format2D.HasFlag(FormatFlags1.IntDataOffset)) {
                _ = br.ReadInt32(); // Data start
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);
            } else if (this.Format2D.HasFlag(FormatFlags1.LongDataOffset)) {
                _ = br.ReadInt64(); // Data start
                _ = br.AssertInt64(0);
            }

            List<Row> rows = this.Rows = new List<Row>(rowCount);
            ref Row rowRef = ref MemoryMarshal.GetArrayDataReference(this.Rows.AsContents());
            for (ref Row end = ref Unsafe.Add(ref rowRef, rowCount); Unsafe.IsAddressLessThan(ref rowRef, ref end); rowRef = ref Unsafe.Add(ref rowRef, 1)) {
                rowRef = new Row(br, this, ref actualStringsOffset);
            }
            rows.SetLength(rowCount);

            this.DetectedSize = rowCount > 1
                ? this.Rows[1].DataOffset - this.Rows[0].DataOffset
                : rowCount == 1 ? (actualStringsOffset == 0 ? stringsOffset : actualStringsOffset) - this.Rows[0].DataOffset : -1;
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected internal override void Write(BinaryWriterEx bw) {
            if (this.AppliedParamdef == null) {
                throw new InvalidOperationException("Params cannot be written without applying a paramdef.");
            }

            bw.BigEndian = this.BigEndian;

            bw.ReserveUInt32("StringsOffset");
            if (this.Format2D.HasFlag(FormatFlags1.Flag01) && this.Format2D.HasFlag(FormatFlags1.IntDataOffset) || this.Format2D.HasFlag(FormatFlags1.LongDataOffset)) {
                bw.WriteInt16(0);
            } else {
                bw.ReserveUInt16("DataStart");
            }
            bw.WriteInt16(this.Unk06);
            bw.WriteInt16(this.ParamdefDataVersion);
            bw.WriteUInt16((ushort)this.Rows.Count);
            if (this.Format2D.HasFlag(FormatFlags1.OffsetParamType)) {
                bw.WriteInt32(0);
                bw.ReserveInt64("ParamTypeOffset");
                bw.WritePattern(0x14, 0x00);
            } else {
                // This padding heuristic isn't completely accurate, not that it matters
                bw.WriteFixStr(this.ParamType, 0x20, (byte)(this.Format2D.HasFlag(FormatFlags1.Flag01) ? 0x20 : 0x00));
            }
            bw.WriteByte((byte)(this.BigEndian ? 0xFF : 0x00));
            bw.WriteByte((byte)this.Format2D);
            bw.WriteByte((byte)this.Format2E);
            bw.WriteByte(this.ParamdefFormatVersion);
            if (this.Format2D.HasFlag(FormatFlags1.Flag01) && this.Format2D.HasFlag(FormatFlags1.IntDataOffset)) {
                bw.ReserveUInt32("DataStart");
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
            } else if (this.Format2D.HasFlag(FormatFlags1.LongDataOffset)) {
                bw.ReserveInt64("DataStart");
                bw.WriteInt64(0);
            }

            for (var i = 0; i < this.Rows.Count; i++) {
                this.Rows[i].WriteHeader(bw, this, i);
            }

            // This is probably pretty stupid
            if (this.Format2D == FormatFlags1.Flag01) {
                bw.WritePattern(0x20, 0x00);
            }

            if (this.Format2D.HasFlag(FormatFlags1.Flag01) && this.Format2D.HasFlag(FormatFlags1.IntDataOffset)) {
                bw.FillUInt32("DataStart", (uint)bw.Position);
            } else if (this.Format2D.HasFlag(FormatFlags1.LongDataOffset)) {
                bw.FillInt64("DataStart", bw.Position);
            } else {
                bw.FillUInt16("DataStart", (ushort)bw.Position);
            }

            for (var i = 0; i < this.Rows.Count; i++) {
                this.Rows[i].WriteCells(bw, this, i);
            }

            bw.FillUInt32("StringsOffset", (uint)bw.Position);

            if (this.Format2D.HasFlag(FormatFlags1.OffsetParamType)) {
                bw.FillInt64("ParamTypeOffset", bw.Position);
                bw.WriteASCII(this.ParamType, true);
            }

            for (var i = 0; i < this.Rows.Count; i++) {
                this.Rows[i].WriteName(bw, this, i);
            }
            // DeS and BB sometimes (but not always) include some useless padding here
        }

        /// <summary>
        /// Interprets row data according to the given paramdef and stores it for later writing.
        /// </summary>
        public void ApplyParamdef(PARAMDEF paramdef) {
            this.AppliedParamdef = paramdef;
            this.MiniCellsAllocator = paramdef.FieldBitOffsetMap.CreateAllocator((nuint)this.Rows.Count);
            ref Row rows = ref MemoryMarshal.GetArrayDataReference(this.Rows.AsContents());
            for (ref Row end = ref Unsafe.Add(ref rows, this.Rows.Count); Unsafe.IsAddressLessThan(ref rows, ref end); rows = ref Unsafe.Add(ref rows, 1)) {
                rows.ReadCells(this.RowReader, paramdef);
            }
            this.RowReader.Stream.Dispose();
            this.RowReader = null;
        }

        /// <summary>
        /// Applies a paramdef only if its param type, data version, and row size match this param's. Returns true if applied.
        /// </summary>
        public bool ApplyParamdefCarefully(PARAMDEF paramdef) {
            //if (paramdef.ParamType.Contains("WW", StringComparison.InvariantCultureIgnoreCase) && this.ParamType.Contains("WW", StringComparison.InvariantCultureIgnoreCase)) {
            //    new object();
            //}
            if (this.ParamType != paramdef.ParamType || this.ParamdefDataVersion != paramdef.DataVersion
                                                     || (this.DetectedSize != -1 && this.DetectedSize != paramdef.GetRowSize())) return false;
            this.ApplyParamdef(paramdef);
            return true;
        }

        /// <summary>
        /// Applies the first paramdef in the sequence whose param type, data version, and row size match this param's, if any. Returns true if applied. 
        /// </summary>
        public bool ApplyParamdefCarefully(IEnumerable<PARAMDEF> paramdefs) => paramdefs.Any(this.ApplyParamdefCarefully);

        /// <summary>
        /// Returns the first row with the given ID, or null if not found.
        /// </summary>
        public Row this[int id] => this.Rows.Find(row => row.ID == id);

        /// <summary>
        /// Returns a string representation of the PARAM.
        /// </summary>
        public override string ToString() => $"{this.ParamType} v{this.ParamdefDataVersion} [{this.Rows.Count}]";

        /// <summary>
        /// First set of flags indicating file format; highly speculative.
        /// </summary>
        [Flags]
        public enum FormatFlags1 : byte {
            /// <summary>
            /// No flags set.
            /// </summary>
            None = 0,

            /// <summary>
            /// Unknown.
            /// </summary>
            Flag01 = 0x1,

            /// <summary>
            /// Expanded header with 32-bit data offset.
            /// </summary>
            IntDataOffset = 0x2,

            /// <summary>
            /// Expanded header with 64-bit data offset.
            /// </summary>
            LongDataOffset = 0x4,

            /// <summary>
            /// Unused?
            /// </summary>
            Flag08 = 0x8,

            /// <summary>
            /// Unused?
            /// </summary>
            Flag10 = 0x10,

            /// <summary>
            /// Unused?
            /// </summary>
            Flag20 = 0x20,

            /// <summary>
            /// Unused?
            /// </summary>
            Flag40 = 0x40,

            /// <summary>
            /// Param type string is written separately instead of fixed-width in the header.
            /// </summary>
            OffsetParamType = 0x80,
        }

        /// <summary>
        /// Second set of flags indicating file format; highly speculative.
        /// </summary>
        [Flags]
        public enum FormatFlags2 : byte {
            /// <summary>
            /// No flags set.
            /// </summary>
            None = 0,

            /// <summary>
            /// Row names are written as UTF-16.
            /// </summary>
            UnicodeRowNames = 0x1,

            /// <summary>
            /// Unknown.
            /// </summary>
            Flag02 = 0x2,

            /// <summary>
            /// Unknown.
            /// </summary>
            Flag04 = 0x4,

            /// <summary>
            /// Unused?
            /// </summary>
            Flag08 = 0x8,

            /// <summary>
            /// Unused?
            /// </summary>
            Flag10 = 0x10,

            /// <summary>
            /// Unused?
            /// </summary>
            Flag20 = 0x20,

            /// <summary>
            /// Unused?
            /// </summary>
            Flag40 = 0x40,

            /// <summary>
            /// Unused?
            /// </summary>
            Flag80 = 0x80,
        }
    }
}
