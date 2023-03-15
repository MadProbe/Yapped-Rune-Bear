using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using SoulsFormats.Formats.PARAM;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class PARAM : SoulsFile<PARAM> {
        /// <summary>
        /// The layout of cell data within each row in a param.
        /// </summary>
        [Obsolete]
        public class Layout : List<Layout.Entry> {
            /// <summary>
            /// Collections of named values which may be referenced by cells.
            /// </summary>
            public Dictionary<string, Enum> Enums;

            /// <summary>
            /// The size of a row, determined automatically from the layout.
            /// </summary>
            public int Size {
                get {
                    int size = 0;

                    for (int i = 0; i < this.Count; i++) {
                        CellType type = this[i].Type;

                        void ConsumeBools(CellType boolType, int fieldSize) {
                            size += fieldSize;

                            int j;
                            for (j = 0; j < fieldSize * 8; j++) {
                                if (i + j >= this.Count || this[i + j].Type != boolType) {
                                    break;
                                }
                            }
                            i += j - 1;
                        }

                        if (type == CellType.b8) {
                            ConsumeBools(type, 1);
                        } else if (type == CellType.b16) {
                            ConsumeBools(type, 2);
                        } else if (type == CellType.b32) {
                            ConsumeBools(type, 4);
                        } else {
                            size += this[i].Size;
                        }
                    }

                    return size;
                }
            }

            /// <summary>
            /// Read a PARAM layout from an XML file.
            /// </summary>
            public static Layout ReadXMLFile(string path) {
                var xml = new XmlDocument();
                xml.Load(path);
                return new Layout(xml);
            }

            /// <summary>
            /// Read a PARAM layout from an XML string.
            /// </summary>
            public static Layout ReadXMLText(string text) {
                var xml = new XmlDocument();
                xml.LoadXml(text);
                return new Layout(xml);
            }

            /// <summary>
            /// Read a PARAM layout from an XML document.
            /// </summary>
            public static Layout ReadXMLDoc(XmlDocument xml) => new(xml);

            /// <summary>
            /// Creates a new empty layout.
            /// </summary>
            public Layout() : base() => this.Enums = new Dictionary<string, Enum>();

            private Layout(XmlDocument xml) : base() {
                this.Enums = new Dictionary<string, Enum>();
                foreach (XmlNode node in xml.SelectNodes("/layout/enum")) {
                    string enumName = node.Attributes["name"].InnerText;
                    this.Enums[enumName] = new Enum(node);
                }

                foreach (XmlNode node in xml.SelectNodes("/layout/entry")) {
                    this.Add(new Entry(node));
                }
            }

            /// <summary>
            /// Write the layout to an XML file.
            /// </summary>
            public void Write(string path) {
                var xws = new XmlWriterSettings() {
                    Indent = true,
                };
                var xw = XmlWriter.Create(path, xws);
                xw.WriteStartElement("layout");

                foreach (Entry entry in this) {
                    entry.Write(xw);
                }

                xw.WriteEndElement();
                xw.Close();
            }

            /// <summary>
            /// Converts the layout to a paramdef with the given type, and all child enums to paramtdfs.
            /// </summary>
            public PARAMDEF ToParamdef(string paramType, out List<PARAMTDF> paramtdfs) {
                paramtdfs = new List<PARAMTDF>(this.Enums.Count);
                foreach (string enumName in this.Enums.Keys) {
                    paramtdfs.Add(this.Enums[enumName].ToParamtdf(enumName));
                }

                var def = new PARAMDEF { ParamType = paramType, Unicode = true, FormatVersion = 201 };
                foreach (Entry entry in this) {
                    PARAMDEF.DefType fieldType = entry.Type switch {
                        CellType.dummy8 => PARAMDEF.DefType.dummy8,
                        CellType.b8 or CellType.u8 or CellType.x8 => PARAMDEF.DefType.u8,
                        CellType.s8 => PARAMDEF.DefType.s8,
                        CellType.b16 or CellType.u16 or CellType.x16 => PARAMDEF.DefType.u16,
                        CellType.s16 => PARAMDEF.DefType.s16,
                        CellType.b32 or CellType.u32 or CellType.x32 => PARAMDEF.DefType.u32,
                        CellType.s32 => PARAMDEF.DefType.s32,
                        CellType.f32 => PARAMDEF.DefType.f32,
                        CellType.fixstr => PARAMDEF.DefType.fixstr,
                        CellType.fixstrW => PARAMDEF.DefType.fixstrW,
                        _ => throw new NotImplementedException($"DefType not specified for CellType {entry.Type}."),
                    };
                    var field = new PARAMDEF.Field(def, fieldType, entry.Name) {
                        Description = entry.Description
                    };
                    if (entry.Enum != null) {
                        field.InternalType = entry.Enum;
                    }

                    if (entry.Type == CellType.s8) {
                        field.Default = (sbyte)entry.Default;
                    } else if (entry.Type is CellType.u8 or CellType.x8) {
                        field.Default = (byte)entry.Default;
                    } else if (entry.Type == CellType.s16) {
                        field.Default = (short)entry.Default;
                    } else if (entry.Type is CellType.u16 or CellType.x16) {
                        field.Default = (ushort)entry.Default;
                    } else if (entry.Type == CellType.s32) {
                        field.Default = (int)entry.Default;
                    } else if (entry.Type is CellType.u32 or CellType.x32) {
                        field.Default = (uint)entry.Default;
                    } else if (entry.Type is CellType.dummy8 or CellType.fixstr) {
                        field.ArrayLength = entry.Size;
                    } else if (entry.Type == CellType.fixstrW) {
                        field.ArrayLength = entry.Size / 2;
                    } else if (entry.Type is CellType.b8 or CellType.b16 or CellType.b32) {
                        field.Default = (bool)entry.Default ? 1 : 0;
                        field.BitSize = 1;
                    }

                    def.Fields.Add(field);
                }
                return def;
            }

            /// <summary>
            /// Parse a string according to the given param type and culture.
            /// </summary>
            public static object ParseParamValue(CellType type, string value, CultureInfo culture) {
                if (type is CellType.fixstr or CellType.fixstrW) {
                    return value;
                } else if (type is CellType.b8 or CellType.b16 or CellType.b32) {
                    return bool.Parse(value);
                } else if (type == CellType.s8) {
                    return sbyte.Parse(value);
                } else if (type == CellType.u8) {
                    return byte.Parse(value);
                } else if (type == CellType.x8) {
                    return Convert.ToByte(value, 16);
                } else if (type == CellType.s16) {
                    return short.Parse(value);
                } else if (type == CellType.u16) {
                    return ushort.Parse(value);
                } else if (type == CellType.x16) {
                    return Convert.ToUInt16(value, 16);
                } else {
                    return type == CellType.s32
                        ? int.Parse(value)
                        : type == CellType.u32
                                            ? uint.Parse(value)
                                            : type == CellType.x32
                                                                ? Convert.ToUInt32(value, 16)
                                                                : type == CellType.f32 ? (object)float.Parse(value, culture) : throw new InvalidCastException("Unparsable type: " + type);
                }
            }

            /// <summary>
            /// Parse a string according to the given param type and invariant culture.
            /// </summary>
            public static object ParseParamValue(CellType type, string value) => ParseParamValue(type, value, CultureInfo.InvariantCulture);

            /// <summary>
            /// Convert a param value of the specified type to a string using the given culture.
            /// </summary>
            public static string ParamValueToString(CellType type, object value, CultureInfo culture) => type == CellType.x8
                    ? $"0x{value:X2}"
                    : type == CellType.x16
                        ? $"0x{value:X4}"
                        : type == CellType.x32 ? $"0x{value:X8}" : type == CellType.f32 ? Convert.ToString(value, culture) : value.ToString();

            /// <summary>
            /// Convert a param value of the specified type to a string using invariant culture.
            /// </summary>
            public static string ParamValueToString(CellType type, object value) => ParamValueToString(type, value, CultureInfo.InvariantCulture);

            /// <summary>
            /// The type and name of one cell in a row.
            /// </summary>
            public class Entry {
                /// <summary>
                /// The type of the cell.
                /// </summary>
                public CellType Type { get; set; }

                /// <summary>
                /// The name of the cell.
                /// </summary>
                public string Name { get; set; }

                /// <summary>
                /// Size in bytes of the entry; may only be set for fixstr, fixstrW, and dummy8.
                /// </summary>
                public int Size {
                    get {
                        if (this.IsVariableSize) {
                            return this.size;
                        } else {
                            return this.Type is CellType.s8 or CellType.u8 or CellType.x8
                                ? 1
                                : this.Type is CellType.s16 or CellType.u16 or CellType.x16
                                                            ? 2
                                                            : this.Type is CellType.s32 or CellType.u32 or CellType.x32 or CellType.f32
                                                                                        ? 4
                                                                                        : this.Type is CellType.b8 or CellType.b16 or CellType.b32
                                                                                                                ? 0
                                                                                                                : throw new InvalidCastException("Unknown type: " + this.Type);
                        }
                    }

                    set => this.size = this.IsVariableSize
                            ? value
                            : throw new InvalidOperationException("Size may only be set for variable-width types: fixstr, fixstrW, and dummy8.");
                }
                private int size;

                /// <summary>
                /// The default value to use when creating a new row.
                /// </summary>
                public object Default {
                    get => this.Type == CellType.dummy8 ? (new byte[this.Size]) : this.def;

                    set => this.def = this.Type == CellType.dummy8 ? throw new InvalidOperationException("Default may not be set for dummy8.") : value;
                }
                private object def;

                /// <summary>
                /// Whether the size can be modified.
                /// </summary>
                public bool IsVariableSize => this.Type is CellType.fixstr or CellType.fixstrW or CellType.dummy8;

                /// <summary>
                /// A description of this field's purpose; may be null.
                /// </summary>
                public string Description;

                /// <summary>
                /// If not null, the enum containing possible values for this cell.
                /// </summary>
                public string Enum;

                /// <summary>
                /// Create a new entry of a fixed-width type.
                /// </summary>
                public Entry(CellType type, string name, object def) {
                    this.Type = type;
                    this.Name = name;
                    this.Default = def;
                }

                /// <summary>
                /// Create a new entry of a variable-width type. Default is ignored for dummy8.
                /// </summary>
                public Entry(CellType type, string name, int size, object def) {
                    this.Type = type;
                    this.Name = name;
                    this.Size = size;
                    this.def = this.Type == CellType.dummy8 ? null : def;
                }

                internal Entry(XmlNode node) {
                    this.Name = node.SelectSingleNode("name").InnerText;
                    this.Type = (CellType)System.Enum.Parse(typeof(CellType), node.SelectSingleNode("type").InnerText, true);

                    if (this.IsVariableSize) {
                        this.size = int.Parse(node.SelectSingleNode("size").InnerText);
                    }

                    if (this.Type != CellType.dummy8) {
                        this.Default = ParseParamValue(this.Type, node.SelectSingleNode("default").InnerText);
                    }

                    this.Description = node.SelectSingleNode("description")?.InnerText;
                    this.Enum = node.SelectSingleNode("enum")?.InnerText;
                }

                internal void Write(XmlWriter xw) {
                    xw.WriteStartElement("entry");
                    xw.WriteElementString("name", this.Name);
                    xw.WriteElementString("type", this.Type.ToString());

                    if (this.IsVariableSize) {
                        xw.WriteElementString("size", this.Size.ToString());
                    }

                    if (this.Type != CellType.dummy8) {
                        xw.WriteElementString("default", ParamValueToString(this.Type, this.Default));
                    }

                    if (this.Description != null) {
                        xw.WriteElementString("description", this.Description);
                    }

                    if (this.Enum != null) {
                        xw.WriteElementString("enum", this.Enum);
                    }

                    xw.WriteEndElement();
                }
            }
        }

        /// <summary>
        /// Possible types for values in a param.
        /// </summary>
        [Obsolete]
        public enum CellType {
            /// <summary>
            /// Array of bytes.
            /// </summary>
            dummy8,

            /// <summary>
            /// 1-bit bool in a 1-byte field.
            /// </summary>
            b8,

            /// <summary>
            /// 1-bit bool in a 2-byte field.
            /// </summary>
            b16,

            /// <summary>
            /// 1-bit bool in a 4-byte field.
            /// </summary>
            b32,

            /// <summary>
            /// Unsigned byte.
            /// </summary>
            u8,

            /// <summary>
            /// Unsigned byte, display as hex.
            /// </summary>
            x8,

            /// <summary>
            /// Signed byte.
            /// </summary>
            s8,

            /// <summary>
            /// Unsigned short.
            /// </summary>
            u16,

            /// <summary>
            /// Unsigned short, display as hex.
            /// </summary>
            x16,

            /// <summary>
            /// Signed short.
            /// </summary>
            s16,

            /// <summary>
            /// Unsigned int.
            /// </summary>
            u32,

            /// <summary>
            /// Unsigned int, display as hex.
            /// </summary>
            x32,

            /// <summary>
            /// Signed int.
            /// </summary>
            s32,

            /// <summary>
            /// Single-precision float.
            /// </summary>
            f32,

            /// <summary>
            /// Shift-JIS encoded string.
            /// </summary>
            fixstr,

            /// <summary>
            /// UTF-16 encoded string.
            /// </summary>
            fixstrW,
        }
    }
}
