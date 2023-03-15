using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class FFXDLSE {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        #region XmlInclude
        [
            XmlInclude(typeof(EvaluatableConstant)),
            XmlInclude(typeof(Evaluatable2)),
            XmlInclude(typeof(Evaluatable3)),
            XmlInclude(typeof(EvaluatableCurrentTick)),
            XmlInclude(typeof(EvaluatableTotalTick)),
            XmlInclude(typeof(EvaluatableAnd)),
            XmlInclude(typeof(EvaluatableOr)),
            XmlInclude(typeof(EvaluatableGE)),
            XmlInclude(typeof(EvaluatableGT)),
            XmlInclude(typeof(EvaluatableLE)),
            XmlInclude(typeof(EvaluatableLT)),
            XmlInclude(typeof(EvaluatableEQ)),
            XmlInclude(typeof(EvaluatableNE)),
            XmlInclude(typeof(EvaluatableNot)),
            XmlInclude(typeof(EvaluatableChildExists)),
            XmlInclude(typeof(EvaluatableParentExists)),
            XmlInclude(typeof(EvaluatableDistanceFromCamera)),
            XmlInclude(typeof(EvaluatableEmittersStopped)),
            ]
        #endregion
        public abstract class Evaluatable : FXSerializable {
            internal override string ClassName => "FXSerializableEvaluatable<dl_int32>";

            internal override int Version => 1;

            internal abstract int Opcode { get; }

            internal abstract int Type { get; }

            public Evaluatable() { }

            internal Evaluatable(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                _ = br.AssertInt32(this.Opcode);
                _ = br.AssertInt32(this.Type);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                bw.WriteInt32(this.Opcode);
                bw.WriteInt32(this.Type);
            }

            internal static Evaluatable Read(BinaryReaderEx br, List<string> classNames) {
                // Don't @ me.
                int opcode = br.GetInt32(br.Position + 0xA);
                return opcode switch {
                    1 => new EvaluatableConstant(br, classNames),
                    2 => new Evaluatable2(br, classNames),
                    3 => new Evaluatable3(br, classNames),
                    4 => new EvaluatableCurrentTick(br, classNames),
                    5 => new EvaluatableTotalTick(br, classNames),
                    8 => new EvaluatableAnd(br, classNames),
                    9 => new EvaluatableOr(br, classNames),
                    10 => new EvaluatableGE(br, classNames),
                    11 => new EvaluatableGT(br, classNames),
                    12 => new EvaluatableLE(br, classNames),
                    13 => new EvaluatableLT(br, classNames),
                    14 => new EvaluatableEQ(br, classNames),
                    15 => new EvaluatableNE(br, classNames),
                    20 => new EvaluatableNot(br, classNames),
                    21 => new EvaluatableChildExists(br, classNames),
                    22 => new EvaluatableParentExists(br, classNames),
                    23 => new EvaluatableDistanceFromCamera(br, classNames),
                    24 => new EvaluatableEmittersStopped(br, classNames),
                    _ => throw new NotImplementedException($"Unimplemented evaluatable opcode: {opcode}"),
                };
            }
        }

        public abstract class EvaluatableUnary : Evaluatable {
            internal override int Type => 1;

            public Evaluatable Operand { get; set; }

            public EvaluatableUnary() { }

            internal EvaluatableUnary(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.Operand = Evaluatable.Read(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                this.Operand.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                this.Operand.Write(bw, classNames);
            }
        }

        public abstract class EvaluatableBinary : Evaluatable {
            internal override int Type => 1;

            public Evaluatable Left { get; set; }

            public Evaluatable Right { get; set; }

            public EvaluatableBinary() { }

            internal EvaluatableBinary(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.Right = Evaluatable.Read(br, classNames);
                this.Left = Evaluatable.Read(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                this.Left.AddClassNames(classNames);
                this.Right.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                this.Right.Write(bw, classNames);
                this.Left.Write(bw, classNames);
            }
        }

        public class EvaluatableConstant : Evaluatable {
            internal override int Opcode => 1;

            internal override int Type => 3;

            [XmlAttribute]
            public int Value { get; set; }

            public EvaluatableConstant() { }

            internal EvaluatableConstant(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.Value = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.Value);
            }

            public override string ToString() => $"{this.Value}";
        }

        public class Evaluatable2 : Evaluatable {
            internal override int Opcode => 2;

            internal override int Type => 3;

            [XmlAttribute]
            public int Unk00 { get; set; }

            [XmlAttribute]
            public int ArgIndex { get; set; }

            public Evaluatable2() { }

            internal Evaluatable2(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.Unk00 = br.ReadInt32();
                this.ArgIndex = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.Unk00);
                bw.WriteInt32(this.ArgIndex);
            }

            public override string ToString() => $"{{2: {this.Unk00}, {this.ArgIndex}}}";
        }

        public class Evaluatable3 : Evaluatable {
            internal override int Opcode => 3;

            internal override int Type => 3;

            [XmlAttribute]
            public int Unk00 { get; set; }

            [XmlAttribute]
            public int ArgIndex { get; set; }

            public Evaluatable3() { }

            internal Evaluatable3(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.Unk00 = br.ReadInt32();
                this.ArgIndex = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.Unk00);
                bw.WriteInt32(this.ArgIndex);
            }

            public override string ToString() => $"{{3: {this.Unk00}, {this.ArgIndex}}}";
        }

        public class EvaluatableCurrentTick : Evaluatable {
            internal override int Opcode => 4;

            internal override int Type => 3;

            public EvaluatableCurrentTick() { }

            internal EvaluatableCurrentTick(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            public override string ToString() => "CurrentTick";
        }

        public class EvaluatableTotalTick : Evaluatable {
            internal override int Opcode => 5;

            internal override int Type => 3;

            public EvaluatableTotalTick() { }

            internal EvaluatableTotalTick(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            public override string ToString() => "TotalTick";
        }

        public class EvaluatableAnd : EvaluatableBinary {
            internal override int Opcode => 8;

            public EvaluatableAnd() { }

            internal EvaluatableAnd(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            public override string ToString() => $"({this.Left}) && ({this.Right})";
        }

        public class EvaluatableOr : EvaluatableBinary {
            internal override int Opcode => 9;

            public EvaluatableOr() { }

            internal EvaluatableOr(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            public override string ToString() => $"({this.Left}) || ({this.Right})";
        }

        public class EvaluatableGE : EvaluatableBinary {
            internal override int Opcode => 10;

            public EvaluatableGE() { }

            internal EvaluatableGE(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            public override string ToString() => $"({this.Left}) >= ({this.Right})";
        }

        public class EvaluatableGT : EvaluatableBinary {
            internal override int Opcode => 11;

            public EvaluatableGT() { }

            internal EvaluatableGT(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            public override string ToString() => $"({this.Left}) > ({this.Right})";
        }

        public class EvaluatableLE : EvaluatableBinary {
            internal override int Opcode => 12;

            public EvaluatableLE() { }

            internal EvaluatableLE(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            public override string ToString() => $"({this.Left}) <= ({this.Right})";
        }

        public class EvaluatableLT : EvaluatableBinary {
            internal override int Opcode => 13;

            public EvaluatableLT() { }

            internal EvaluatableLT(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            public override string ToString() => $"({this.Left}) < ({this.Right})";
        }

        public class EvaluatableEQ : EvaluatableBinary {
            internal override int Opcode => 14;

            public EvaluatableEQ() { }

            internal EvaluatableEQ(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            public override string ToString() => $"({this.Left}) == ({this.Right})";
        }

        public class EvaluatableNE : EvaluatableBinary {
            internal override int Opcode => 15;

            public EvaluatableNE() { }

            internal EvaluatableNE(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            public override string ToString() => $"({this.Left}) != ({this.Right})";
        }

        public class EvaluatableNot : EvaluatableUnary {
            internal override int Opcode => 20;

            public EvaluatableNot() { }

            internal EvaluatableNot(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            public override string ToString() => $"!({this.Operand})";
        }

        public class EvaluatableChildExists : Evaluatable {
            internal override int Opcode => 21;

            internal override int Type => 3;

            public EvaluatableChildExists() { }

            internal EvaluatableChildExists(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            public override string ToString() => "ChildExists";
        }

        public class EvaluatableParentExists : Evaluatable {
            internal override int Opcode => 22;

            internal override int Type => 3;

            public EvaluatableParentExists() { }

            internal EvaluatableParentExists(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            public override string ToString() => "ParentExists";
        }

        public class EvaluatableDistanceFromCamera : Evaluatable {
            internal override int Opcode => 23;

            internal override int Type => 3;

            public EvaluatableDistanceFromCamera() { }

            internal EvaluatableDistanceFromCamera(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            public override string ToString() => "DistanceFromCamera";
        }

        public class EvaluatableEmittersStopped : Evaluatable {
            internal override int Opcode => 24;

            internal override int Type => 3;

            public EvaluatableEmittersStopped() { }

            internal EvaluatableEmittersStopped(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            public override string ToString() => "EmittersStopped";
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
