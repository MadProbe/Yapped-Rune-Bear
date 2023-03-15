using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using SoulsFormats.Formats.PARAM;

namespace SoulsFormats {
    public partial class PARAM {
        /// <summary>
        /// A collection of named values.
        /// </summary>
        [Obsolete]
        public class Enum : List<Enum.Item> {
            /// <summary>
            /// The type of values in this enum, which should match the types of the cells using it.
            /// </summary>
            public CellType Type { get; }

            /// <summary>
            /// Creates a new empty Enum with the given type.
            /// </summary>
            public Enum(CellType type) : base() => this.Type = type;

            internal Enum(XmlNode node) {
                this.Type = (CellType)System.Enum.Parse(typeof(CellType), node.Attributes["type"].InnerText);
                foreach (XmlNode itemNode in node.SelectNodes("item")) {
                    string itemName = itemNode.Attributes["name"].InnerText;
                    object itemValue = Layout.ParseParamValue(this.Type, itemNode.Attributes["value"].InnerText);
                    this.Add(new Item(itemName, itemValue));
                }
            }

            /// <summary>
            /// Converts the enum to a paramtdf with the given name.
            /// </summary>
            public PARAMTDF ToParamtdf(string name) {
                PARAMDEF.DefType tdfType = this.Type switch {
                    CellType.u8 or CellType.x8 => PARAMDEF.DefType.u8,
                    CellType.s8 => PARAMDEF.DefType.s8,
                    CellType.u16 or CellType.x16 => PARAMDEF.DefType.u16,
                    CellType.s16 => PARAMDEF.DefType.s16,
                    CellType.u32 or CellType.x32 => PARAMDEF.DefType.u32,
                    CellType.s32 => PARAMDEF.DefType.s32,
                    _ => throw new InvalidDataException($"Layout.Enum type {this.Type} may not be used in a TDF."),
                };
                var tdf = new PARAMTDF { Name = name, Type = tdfType };
                foreach (Item item in this) {
                    tdf.Entries.Add(new PARAMTDF.Entry(item.Name, item.Value));
                }
                return tdf;
            }

            /// <summary>
            /// A value and corresponding name in an Enum.
            /// </summary>
            public class Item {
                /// <summary>
                /// The name of the value.
                /// </summary>
                public string Name { get; }

                /// <summary>
                /// The value of the value.
                /// </summary>
                public object Value { get; }

                /// <summary>
                /// Creates a new Item with the given properties.
                /// </summary>
                public Item(string name, object value) {
                    this.Name = name;
                    this.Value = value;
                }
            }
        }
    }
}
