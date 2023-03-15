using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using SoulsFormats.Formats.PARAM;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class PARAMDEF {
        private static partial class XmlSerializer {
            public const int CURRENT_XML_VERSION = 3;

            public static PARAMDEF Deserialize(XmlDocument xml) {
                var def = new PARAMDEF();
                XmlNode root = xml.SelectSingleNode("PARAMDEF");
                // In the interest of maximum compatibility, we will no longer check the XML version;
                // just try everything and hope it works.

                def.ParamType = root.SelectSingleNode("ParamType").InnerText;
                def.DataVersion = root.ReadInt16IfExist("DataVersion") ?? root.ReadInt16("Unk06");
                def.BigEndian = root.ReadBoolean("BigEndian");
                def.Unicode = root.ReadBoolean("Unicode");
                def.FormatVersion = root.ReadInt16IfExist("FormatVersion") ?? root.ReadInt16("Version");

                def.Fields = new List<Field>();
                foreach (XmlNode node in root.SelectNodes("Fields/Field")) {
                    def.Fields.Add(DeserializeField(def, node));
                }

                return def;
            }

            public static void Serialize(PARAMDEF def, XmlWriter xw, int xmlVersion) {
                if (xmlVersion is < 0 or > CURRENT_XML_VERSION) {
                    throw new InvalidOperationException($"XML version {xmlVersion} not recognized.");
                }

                xw.WriteStartDocument();
                xw.WriteStartElement("PARAMDEF");
                xw.WriteAttributeString("XmlVersion", xmlVersion.ToString());
                xw.WriteElementString("ParamType", def.ParamType);
                xw.WriteElementString(xmlVersion == 0 ? "Unk06" : "DataVersion", def.DataVersion.ToString());
                xw.WriteElementString("BigEndian", def.BigEndian.ToString());
                xw.WriteElementString("Unicode", def.Unicode.ToString());
                xw.WriteElementString(xmlVersion == 0 ? "Version" : "FormatVersion", def.FormatVersion.ToString());

                xw.WriteStartElement("Fields");
                foreach (Field field in def.Fields) {
                    xw.WriteStartElement("Field");
                    SerializeField(def, field, xw);
                    xw.WriteEndElement();
                }
                xw.WriteEndElement();

                xw.WriteEndElement();
            }


            private static readonly Regex defOuterRx = MyRegex();
            private static readonly Regex defBitRx = MyRegex1();
            private static readonly Regex defArrayRx = MyRegex2();

            private static Field DeserializeField(PARAMDEF def, XmlNode node) {
                var field = new Field();
                string fieldDef = node.Attributes["Def"].InnerText;
                Match outerMatch = defOuterRx.Match(fieldDef);
                field.Def = def;
                field.DisplayType = Enum.Parse<DefType>(outerMatch.Groups["type"].Value.Trim());
                field.Default = outerMatch.Groups["default"].Success
                    ? float.Parse(outerMatch.Groups["default"].Value, CultureInfo.InvariantCulture)
                    : ParamUtil.GetDefaultDefault(def, field.DisplayType);

                string internalName = outerMatch.Groups["name"].Value.Trim();
                Match bitMatch = defBitRx.Match(internalName);
                Match arrayMatch = defArrayRx.Match(internalName);
                field.BitSize = -1;
                field.ArrayLength = 1;
                if (ParamUtil.IsBitType(field.DisplayType) && bitMatch.Success) {
                    field.BitSize = int.Parse(bitMatch.Groups["size"].Value);
                    internalName = bitMatch.Groups["name"].Value;
                } else if (ParamUtil.IsArrayType(field.DisplayType)) {
                    field.ArrayLength = int.Parse(arrayMatch.Groups["length"].Value);
                    internalName = arrayMatch.Groups["name"].Value;
                }
                field.InternalName = internalName;

                field.DisplayName = node.ReadStringOrDefault("DisplayName", field.InternalName);
                field.InternalType = node.ReadStringOrDefault("Enum", field.DisplayType.ToString());
                field.Description = node.ReadStringIfExist("Description");
                field.DisplayFormat = node.ReadStringOrDefault("DisplayFormat", ParamUtil.GetDefaultFormat(field.DisplayType));
                field.EditFlags = Enum.Parse<EditFlags>(node.ReadStringOrDefault("EditFlags", ParamUtil.GetDefaultEditFlags(field.DisplayType).ToString()));
                field.Minimum = ReadVariableValueOrDefault(def, node, field.DisplayType, "Minimum", ParamUtil.GetDefaultMinimum(def, field.DisplayType));
                field.Maximum = ReadVariableValueOrDefault(def, node, field.DisplayType, "Maximum", ParamUtil.GetDefaultMaximum(def, field.DisplayType));
                field.Increment = ReadVariableValueOrDefault(def, node, field.DisplayType, "Increment", ParamUtil.GetDefaultIncrement(def, field.DisplayType));
                field.SortID = node.ReadInt32OrDefault("SortID", 0);

                field.UnkB8 = node.ReadStringIfExist("UnkB8");
                field.UnkC0 = node.ReadStringIfExist("UnkC0");
                field.UnkC8 = node.ReadStringIfExist("UnkC8");
                return field;
            }

            private static object ParseVariableValue(PARAMDEF def, DefType type, string text) => def.VariableEditorValueTypes
                    ? type switch {
                        DefType.s8 or DefType.u8 or DefType.s16 or DefType.u16 or DefType.s32 or DefType.u32 or DefType.b32 => int.Parse(text),
                        DefType.f32 or DefType.angle32 => float.Parse(text, CultureInfo.InvariantCulture),
                        DefType.f64 => double.Parse(text, CultureInfo.InvariantCulture),
                        DefType.dummy8 or DefType.fixstr or DefType.fixstrW => null,
                        _ => throw new NotImplementedException($"Missing variable parse for type: {type}"),
                    }
                    : float.Parse(text, CultureInfo.InvariantCulture);

            private static object ReadVariableValueOrDefault(PARAMDEF def, XmlNode node, DefType type, string xpath, object defaultValue) => def.VariableEditorValueTypes
                    ? type switch {
                        DefType.s8 or DefType.u8 or DefType.s16 or DefType.u16 or DefType.s32 or DefType.u32 or DefType.b32 => node.ReadInt32OrDefault(xpath, (int)defaultValue),
                        DefType.f32 or DefType.angle32 => node.ReadSingleOrDefault(xpath, (float)defaultValue, CultureInfo.InvariantCulture),
                        DefType.f64 => node.ReadDoubleOrDefault(xpath, (double)defaultValue, CultureInfo.InvariantCulture),
                        DefType.dummy8 or DefType.fixstr or DefType.fixstrW => null,
                        _ => throw new NotImplementedException($"Missing variable read for type: {type}"),
                    }
                    : node.ReadSingleOrDefault(xpath, (float)defaultValue, CultureInfo.InvariantCulture);

            private static void SerializeField(PARAMDEF def, Field field, XmlWriter xw) {
                string fieldDef = $"{field.DisplayType} {field.InternalName}";
                if (ParamUtil.IsBitType(field.DisplayType) && field.BitSize != -1) {
                    fieldDef += $":{field.BitSize}";
                } else if (ParamUtil.IsArrayType(field.DisplayType)) {
                    fieldDef += $"[{field.ArrayLength}]";
                }

                if (!Equals(field.Default, ParamUtil.GetDefaultDefault(def, field.DisplayType))) {
                    fieldDef += $" = {VariableValueToString(def, field.DisplayType, field.Default)}";
                }

                xw.WriteAttributeString("Def", fieldDef);
                xw.WriteDefaultElement("DisplayName", field.DisplayName, field.InternalName);
                xw.WriteDefaultElement("Enum", field.InternalType, field.DisplayType.ToString());
                xw.WriteDefaultElement("Description", field.Description, null);
                xw.WriteDefaultElement("DisplayFormat", field.DisplayFormat, ParamUtil.GetDefaultFormat(field.DisplayType));
                xw.WriteDefaultElement("EditFlags", field.EditFlags.ToString(), ParamUtil.GetDefaultEditFlags(field.DisplayType).ToString());
                WriteVariableValue(def, xw, field.DisplayType, "Minimum", field.Minimum, ParamUtil.GetDefaultMinimum(def, field.DisplayType));
                WriteVariableValue(def, xw, field.DisplayType, "Maximum", field.Maximum, ParamUtil.GetDefaultMaximum(def, field.DisplayType));
                WriteVariableValue(def, xw, field.DisplayType, "Increment", field.Increment, ParamUtil.GetDefaultIncrement(def, field.DisplayType));
                xw.WriteDefaultElement("SortID", field.SortID, 0);

                xw.WriteDefaultElement("UnkB8", field.UnkB8, null);
                xw.WriteDefaultElement("UnkC0", field.UnkC0, null);
                xw.WriteDefaultElement("UnkC8", field.UnkC8, null);
            }

            private static string VariableValueToString(PARAMDEF def, DefType type, object value) => def.VariableEditorValueTypes
                    ? type switch {
                        DefType.s8 or DefType.u8 or DefType.s16 or DefType.u16 or DefType.s32 or DefType.u32 or DefType.b32 => Convert.ToInt32(value).ToString(),
                        DefType.f32 or DefType.angle32 => Convert.ToSingle(value).ToString(),
                        DefType.f64 => Convert.ToDouble(value).ToString(),
                        DefType.dummy8 or DefType.fixstr or DefType.fixstrW => "null",
                        _ => throw new NotImplementedException($"Missing variable tostring for type: {type}"),
                    }
                    : Convert.ToSingle(value).ToString("R", CultureInfo.InvariantCulture);

            private static void WriteVariableValue(PARAMDEF def, XmlWriter xw, DefType type, string localName, object value, object defaultValue) {
                if (def.VariableEditorValueTypes) {
                    switch (type) {
                        case DefType.s8:
                        case DefType.u8:
                        case DefType.s16:
                        case DefType.u16:
                        case DefType.s32:
                        case DefType.u32:
                        case DefType.b32:
                            xw.WriteDefaultElement(localName, Convert.ToInt32(value), (int)defaultValue);
                            break;

                        case DefType.f32:
                        case DefType.angle32:
                            xw.WriteDefaultElement(localName, Convert.ToSingle(value), (float)defaultValue);
                            break;

                        case DefType.f64:
                            xw.WriteDefaultElement(localName, Convert.ToDouble(value), (double)defaultValue);
                            break;

                        case DefType.dummy8:
                        case DefType.fixstr:
                        case DefType.fixstrW:
                            break;

                        default:
                            break;
                    }
                } else {
                    xw.WriteDefaultElement(localName, Convert.ToSingle(value), Convert.ToSingle(defaultValue), "R", CultureInfo.InvariantCulture);
                }
            }

            [GeneratedRegex("^(?<type>\\S+)\\s+(?<name>.+?)(?:\\s*=\\s*(?<default>\\S+))?$")]
            private static partial Regex MyRegex();
            [GeneratedRegex("^(?<name>.+?)\\s*:\\s*(?<size>\\d+)$")]
            private static partial Regex MyRegex1();
            [GeneratedRegex("^(?<name>.+?)\\s*\\[\\s*(?<length>\\d+)\\]$")]
            private static partial Regex MyRegex2();
        }
    }
}
