using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class FFXDLSE {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class StateMap : FXSerializable, IXmlSerializable {
            internal override string ClassName => "FXSerializableStateMap";

            internal override int Version => 1;

            public List<State> States { get; set; }

            public StateMap() => this.States = new List<State>();

            internal StateMap(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                int stateCount = br.ReadInt32();
                this.States = new List<State>(stateCount);
                for (int i = 0; i < stateCount; i++) {
                    this.States.Add(new State(br, classNames));
                }
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                foreach (State state in this.States) {
                    state.AddClassNames(classNames);
                }
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                bw.WriteInt32(this.States.Count);
                foreach (State state in this.States) {
                    state.Write(bw, classNames);
                }
            }

            #region IXmlSerializable
            XmlSchema IXmlSerializable.GetSchema() => null;

            void IXmlSerializable.ReadXml(XmlReader reader) {
                _ = reader.MoveToContent();
                bool empty = reader.IsEmptyElement;
                reader.ReadStartElement();

                if (!empty) {
                    while (reader.IsStartElement(nameof(State))) {
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                        this.States.Add((State)StateSerializer.Deserialize(reader));
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                    }

                    reader.ReadEndElement();
                }
            }

            void IXmlSerializable.WriteXml(XmlWriter writer) {
                for (int i = 0; i < this.States.Count; i++) {
                    writer.WriteComment($" State {i} ");
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                    StateSerializer.Serialize(writer, this.States[i]);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                }
            }
            #endregion
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
