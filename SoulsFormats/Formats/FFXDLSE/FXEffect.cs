using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class FFXDLSE {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class FXEffect : FXSerializable {
            internal override string ClassName => "FXSerializableEffect";

            internal override int Version => 5;

            [XmlAttribute]
            public int ID { get; set; }

            public ParamList ParamList1 { get; set; }

            public ParamList ParamList2 { get; set; }

            public StateMap StateMap { get; set; }

            public ResourceSet ResourceSet { get; set; }

            public FXEffect() {
                this.ParamList1 = new ParamList();
                this.ParamList2 = new ParamList();
                this.StateMap = new StateMap();
                this.ResourceSet = new ResourceSet();
            }

            internal FXEffect(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                _ = br.AssertInt32(0);
                this.ID = br.ReadInt32();
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(2); // Param list count?
                _ = br.AssertInt16(0);
                _ = br.AssertInt16(2); // Judging by the order of class names, this must be an always-empty DLVector
                _ = br.AssertInt32(0);

                this.ParamList1 = new ParamList(br, classNames);
                this.ParamList2 = new ParamList(br, classNames);

                this.StateMap = new StateMap(br, classNames);
                this.ResourceSet = new ResourceSet(br, classNames);
                _ = br.AssertByte(0);
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                DLVector.AddClassNames(classNames);

                this.ParamList1.AddClassNames(classNames);
                this.ParamList2.AddClassNames(classNames);

                this.StateMap.AddClassNames(classNames);
                this.ResourceSet.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                bw.WriteInt32(0);
                bw.WriteInt32(this.ID);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(2);
                bw.WriteInt16(0);
                bw.WriteInt16(2);
                bw.WriteInt32(0);

                this.ParamList1.Write(bw, classNames);
                this.ParamList2.Write(bw, classNames);

                this.StateMap.Write(bw, classNames);
                this.ResourceSet.Write(bw, classNames);
                bw.WriteByte(0);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
