using System.Xml;
using SoulsFormats.Formats.PARAM;
using SoulsFormats.Util;

namespace SoulsFormats {
    /// <summary>
    /// A companion format to params that describes each field present in the rows. Extension: .def, .paramdef
    /// </summary>
    public partial class PARAMDEF : SoulsFile<PARAMDEF> {
        /// <summary>
        /// Indicates a revision of the row data structure.
        /// </summary>
        public short DataVersion;

        /// <summary>
        /// Identifies corresponding params and paramdefs.
        /// </summary>
        public string ParamType;

        /// <summary>
        /// True for PS3 and X360 games, otherwise false.
        /// </summary>
        public bool BigEndian;

        /// <summary>
        /// If true, certain strings are written as UTF-16; if false, as Shift-JIS.
        /// </summary>
        public bool Unicode;

        /// <summary>
        /// Determines format of the file.
        /// </summary>
        // 101 - Enchanted Arms, Chromehounds, Armored Core 4/For Answer/V/Verdict Day, Shadow Assault: Tenchu
        // 102 - Demon's Souls
        // 103 - Ninja Blade, Another Century's Episode: R
        // 104 - Dark Souls, Steel Battalion: Heavy Armor
        // 106 - Elden Ring (deprecated ObjectParam)
        // 201 - Bloodborne
        // 202 - Dark Souls 3
        // 203 - Elden Ring
        public short FormatVersion;

        /// <summary>
        /// Whether field default, minimum, maximum, and increment may be variable type. If false, they are always floats.
        /// </summary>
        public bool VariableEditorValueTypes => this.FormatVersion >= 203;

        /// <summary>
        /// Fields in each param row, in order of appearance.
        /// </summary>
        public List<Field> Fields;

        /// <summary>
        /// Creates a PARAMDEF formatted for DS1.
        /// </summary>
        public PARAMDEF() {
            this.ParamType = "";
            this.FormatVersion = 104;
            this.Fields = new List<Field>();
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            br.BigEndian = this.BigEndian = br.GetSByte(0x2C) == -1;
            this.FormatVersion = br.GetInt16(0x2E);
            br.VarintLong = this.FormatVersion >= 200;

            _ = br.ReadInt32(); // File size
            short headerSize = br.AssertInt16(0x30, 0xFF);
            this.DataVersion = br.ReadInt16();
            short fieldCount = br.ReadInt16();
            short fieldSize = br.AssertInt16(0x48, 0x68, 0x6C, 0x88, 0x8C, 0xAC, 0xB0, 0xD0);

            if (this.FormatVersion >= 202) {
                _ = br.AssertInt32(0);
                // Is there a reason that I used GetShiftJIS instead of GetASCII here?
                this.ParamType = br.GetShiftJIS(br.ReadInt64());
                _ = br.AssertInt64(0);
                _ = br.AssertInt64(0);
                _ = br.AssertInt32(0);
            } else if (this.FormatVersion is >= 106 and < 200) {
                this.ParamType = br.GetShiftJIS(br.ReadInt32());
                _ = br.AssertInt64(0);
                _ = br.AssertInt64(0);
                _ = br.AssertInt64(0);
                _ = br.AssertInt32(0);
            } else {
                this.ParamType = br.ReadFixStr(0x20);
            }

            _ = br.AssertSByte(0, -1); // Big-endian
            this.Unicode = br.ReadBoolean();
            _ = br.AssertInt16(101, 102, 103, 104, 106, 201, 202, 203); // Format version
            if (this.FormatVersion >= 200) {
                _ = br.AssertInt64(0x38);
            }

            if ((this.FormatVersion >= 200 || headerSize != 0x30) && (this.FormatVersion < 200 || headerSize != 0xFF)) {
                throw new InvalidDataException($"Unexpected header size 0x{headerSize:X} for version {this.FormatVersion}.");
            }

            // Please note that for version 103 this value is wrong.
            if (!(this.FormatVersion == 101 && fieldSize == 0x8C
                || this.FormatVersion == 102 && fieldSize == 0xAC
                || this.FormatVersion == 103 && fieldSize == 0x6C
                || this.FormatVersion == 104 && fieldSize == 0xB0
                || this.FormatVersion == 106 && fieldSize == 0x48
                || this.FormatVersion == 201 && fieldSize == 0xD0
                || this.FormatVersion == 202 && fieldSize == 0x68
                || this.FormatVersion == 203 && fieldSize == 0x88)) {
                throw new InvalidDataException($"Unexpected field size 0x{fieldSize:X} for version {this.FormatVersion}.");
            }

            Field[] fields = (this.Fields = new List<Field>(fieldCount)).AsContents();
            for (int i = 16, l = (fieldCount << 3) + 16; i < l; i += 16) {
                fields.AssignAt(i, new Field(br, this));
            }
        }

        /// <summary>
        /// Verifies that the file can be written safely.
        /// </summary>
        public override bool Validate(out Exception ex) {
            const string FIELD_NAME = nameof(Fields);
            if (this.FormatVersion is not (101 or 102 or 103 or 104 or 106
                or 201 or 202 or 203)) {
                ex = new InvalidDataException($"Unknown version: {this.FormatVersion}");
                return false;
            }

            if (!ValidateNull(this.ParamType, () => $"{nameof(ParamType)} can't be null.", out ex)
                || !ValidateNull(this.Fields, () => $"{FIELD_NAME} can't be null.", out ex)) {
                return false;
            }
            Field[] fields = this.Fields.AsContents();
            for (int i = 16, l = (this.Fields.Count << 3) + 16; i < l; i += 8) {
                Field field = fields.At(i);
                if (!ValidateNull(field, () => $"{FIELD_NAME}[{(i - 16) >> 3}]: {nameof(Field)} can't be null.", out ex)
                    || !ValidateNull(field.DisplayName, () => $"{FIELD_NAME}[{(i - 16) >> 3}]: {nameof(Field.DisplayName)} can't be null.", out ex)
                    || !ValidateNull(field.DisplayFormat, () => $"{FIELD_NAME}[{(i - 16) >> 3}]: {nameof(Field.DisplayFormat)} can't be null.", out ex)
                    || !ValidateNull(field.InternalType, () => $"{FIELD_NAME}[{(i - 16) >> 3}]: {nameof(Field.InternalType)} can't be null.", out ex)
                    || this.FormatVersion >= 102 && !ValidateNull(field.InternalName, () => $"{FIELD_NAME}[{(i - 16) >> 3}]: {nameof(Field.InternalName)} can't be null on version {this.FormatVersion}.", out ex)) {
                    return false;
                }
            }

            ex = null;
            return true;
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected internal override void Write(BinaryWriterEx bw) {
            bw.BigEndian = this.BigEndian;
            bw.VarintLong = this.FormatVersion >= 200;

            bw.ReserveInt32("FileSize");
            bw.WriteInt16((short)(this.FormatVersion >= 200 ? 0xFF : 0x30));
            bw.WriteInt16(this.DataVersion);
            bw.WriteInt16((short)this.Fields.Count);

            if (this.FormatVersion == 101) {
                bw.WriteInt16(0x8C);
            } else if (this.FormatVersion == 102) {
                bw.WriteInt16(0xAC);
            } else if (this.FormatVersion == 103) {
                bw.WriteInt16(0x6C);
            } else if (this.FormatVersion == 104) {
                bw.WriteInt16(0xB0);
            } else if (this.FormatVersion == 106) {
                bw.WriteInt16(0x48);
            } else if (this.FormatVersion == 201) {
                bw.WriteInt16(0xD0);
            } else if (this.FormatVersion == 202) {
                bw.WriteInt16(0x68);
            } else if (this.FormatVersion == 203) {
                bw.WriteInt16(0x88);
            }

            if (this.FormatVersion >= 202) {
                bw.WriteInt32(0);
                bw.ReserveVarint("ParamTypeOffset");
                bw.WriteInt64(0);
                bw.WriteInt64(0);
                bw.WriteInt32(0);
            } else if (this.FormatVersion is >= 106 and < 200) {
                bw.ReserveVarint("ParamTypeOffset");
                bw.WriteInt64(0);
                bw.WriteInt64(0);
                bw.WriteInt64(0);
                bw.WriteInt32(0);
            } else {
                bw.WriteFixStr(this.ParamType, 0x20, (byte)(this.FormatVersion >= 200 ? 0x00 : 0x20));
            }

            bw.WriteSByte((sbyte)(this.BigEndian ? -1 : 0));
            bw.WriteBoolean(this.Unicode);
            bw.WriteInt16(this.FormatVersion);
            if (this.FormatVersion >= 200) {
                bw.WriteInt64(0x38);
            }

            for (int i = 0; i < this.Fields.Count; i++) {
                this.Fields[i].Write(bw, this, i);
            }

            if (this.FormatVersion is >= 202 or >= 106 and < 200) {
                bw.FillVarint("ParamTypeOffset", bw.Position);
                bw.WriteShiftJIS(this.ParamType, true);
            }

            long fieldStringsStart = bw.Position;
            var sharedStringOffsets = new Dictionary<string, long>();
            for (int i = 0; i < this.Fields.Count; i++) {
                this.Fields[i].WriteStrings(bw, this, i, sharedStringOffsets);
            }

            // This entire heuristic seems extremely dubious
            if (this.FormatVersion is 104 or 201) {
                long fieldStringsLength = bw.Position - fieldStringsStart;
                if (fieldStringsLength % 0x10 != 0) {
                    bw.WritePattern((int)(0x10 - fieldStringsLength % 0x10), 0x00);
                }
            } else {
                if (this.FormatVersion >= 202 && bw.Position % 0x10 == 0) {
                    bw.WritePattern(0x10, 0x00);
                }

                bw.Pad(0x10);
            }
            bw.FillInt32("FileSize", (int)bw.Position);
        }

        /// <summary>
        /// Calculates the size of cell data for each row.
        /// </summary>
        public int GetRowSize() {
            int size = 0;
            for (int i = 0; i < this.Fields.Count; i++) {
                Field field = this.Fields[i];
                DefType type = field.DisplayType;
                if (ParamUtil.IsArrayType(type)) {
                    size += ParamUtil.GetValueSize(type) * field.ArrayLength;
                } else {
                    size += ParamUtil.GetValueSize(type);
                }

                if (ParamUtil.IsBitType(type) && field.BitSize != -1) {
                    int bitOffset = field.BitSize;
                    DefType bitType = type == DefType.dummy8 ? DefType.u8 : type;
                    int bitLimit = ParamUtil.GetBitLimit(bitType);

                    for (; i < this.Fields.Count - 1; i++) {
                        Field nextField = this.Fields[i + 1];
                        DefType nextType = nextField.DisplayType;
                        if (!ParamUtil.IsBitType(nextType) || nextField.BitSize == -1 || bitOffset + nextField.BitSize > bitLimit
                            || (nextType == DefType.dummy8 ? DefType.u8 : nextType) != bitType) {
                            break;
                        }

                        bitOffset += nextField.BitSize;
                    }
                }
            }
            return size;
        }

        /// <summary>
        /// Returns a string representation of the PARAMDEF.
        /// </summary>
        public override string ToString() => $"{this.ParamType} v{this.DataVersion}";

        /// <summary>
        /// Reads an XML-formatted PARAMDEF from a file.
        /// </summary>
        public static PARAMDEF XmlDeserialize(string path) {
            var xml = new XmlDocument();
            xml.Load(path);
            return XmlSerializer.Deserialize(xml);
        }

        /// <summary>
        /// Writes an XML-formatted PARAMDEF to a file using the current XML version.
        /// </summary>
        public void XmlSerialize(string path) => this.XmlSerialize(path, XmlSerializer.CURRENT_XML_VERSION);

        /// <summary>
        /// Writes an XML-formatted PARAMDEF to a file using the given XML version.
        /// </summary>
        public void XmlSerialize(string path, int xmlVersion) {
            _ = Directory.CreateDirectory(Path.GetDirectoryName(path));
            var xws = new XmlWriterSettings { Indent = true };
            using var xw = XmlWriter.Create(path, xws);
            XmlSerializer.Serialize(this, xw, xmlVersion);
        }
    }
}
