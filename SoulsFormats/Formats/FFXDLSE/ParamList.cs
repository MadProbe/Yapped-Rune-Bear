using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class FFXDLSE {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class ParamList : FXSerializable, IXmlSerializable {
            internal override string ClassName => "FXSerializableParamList";

            internal override int Version => 2;

            [XmlAttribute]
            public int Unk04 { get; set; }

            public List<Param> Params { get; set; }

            public ParamList() => this.Params = new List<Param>();

            internal ParamList(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                int paramCount = br.ReadInt32();
                this.Unk04 = br.ReadInt32();
                this.Params = new List<Param>(paramCount);
                for (int i = 0; i < paramCount; i++) {
                    this.Params.Add(Param.Read(br, classNames));
                }
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                foreach (Param param in this.Params) {
                    param.AddClassNames(classNames);
                }
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                bw.WriteInt32(this.Params.Count);
                bw.WriteInt32(this.Unk04);
                foreach (Param param in this.Params) {
                    param.Write(bw, classNames);
                }
            }

            #region IXmlSerializable
            XmlSchema IXmlSerializable.GetSchema() => null;

            void IXmlSerializable.ReadXml(XmlReader reader) {
                _ = reader.MoveToContent();
                bool empty = reader.IsEmptyElement;
                this.Unk04 = int.Parse(reader.GetAttribute(nameof(this.Unk04)));
                reader.ReadStartElement();

                if (!empty) {
                    while (reader.IsStartElement(nameof(Param))) {
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                        this.Params.Add((Param)ParamSerializer.Deserialize(reader));
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                    }

                    reader.ReadEndElement();
                }
            }

            void IXmlSerializable.WriteXml(XmlWriter writer) {
                writer.WriteAttributeString(nameof(this.Unk04), this.Unk04.ToString());
                for (int i = 0; i < this.Params.Count; i++) {
                    //writer.WriteComment($" {i} ");
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                    ParamSerializer.Serialize(writer, this.Params[i]);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                }
            }
            #endregion
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
