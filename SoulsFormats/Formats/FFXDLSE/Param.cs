using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class FFXDLSE {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        #region XmlInclude
        [
            XmlInclude(typeof(Param1)),
            XmlInclude(typeof(Param2)),
            XmlInclude(typeof(Param5)),
            XmlInclude(typeof(Param6)),
            XmlInclude(typeof(Param7)),
            XmlInclude(typeof(Param9)),
            XmlInclude(typeof(Param11)),
            XmlInclude(typeof(Param12)),
            XmlInclude(typeof(Param13)),
            XmlInclude(typeof(Param15)),
            XmlInclude(typeof(Param17)),
            XmlInclude(typeof(Param18)),
            XmlInclude(typeof(Param19)),
            XmlInclude(typeof(Param20)),
            XmlInclude(typeof(Param21)),
            XmlInclude(typeof(Param37)),
            XmlInclude(typeof(Param38)),
            XmlInclude(typeof(Param40)),
            XmlInclude(typeof(Param41)),
            XmlInclude(typeof(Param44)),
            XmlInclude(typeof(Param45)),
            XmlInclude(typeof(Param46)),
            XmlInclude(typeof(Param47)),
            XmlInclude(typeof(Param59)),
            XmlInclude(typeof(Param60)),
            XmlInclude(typeof(Param66)),
            XmlInclude(typeof(Param68)),
            XmlInclude(typeof(Param69)),
            XmlInclude(typeof(Param70)),
            XmlInclude(typeof(Param71)),
            XmlInclude(typeof(Param79)),
            XmlInclude(typeof(Param81)),
            XmlInclude(typeof(Param82)),
            XmlInclude(typeof(Param83)),
            XmlInclude(typeof(Param84)),
            XmlInclude(typeof(Param85)),
            XmlInclude(typeof(Param87)),
            ]
        #endregion
        public abstract class Param : FXSerializable {
            internal override string ClassName => "FXSerializableParam";

            internal override int Version => 2;

            internal abstract int Type { get; }

            public Param() { }

            internal Param(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) => br.AssertInt32(this.Type);

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) => bw.WriteInt32(this.Type);

            internal static Param Read(BinaryReaderEx br, List<string> classNames) {
                // Don't @ me.
                int type = br.GetInt32(br.Position + 0xA);
                return type switch {
                    1 => new Param1(br, classNames),
                    2 => new Param2(br, classNames),
                    5 => new Param5(br, classNames),
                    6 => new Param6(br, classNames),
                    7 => new Param7(br, classNames),
                    9 => new Param9(br, classNames),
                    11 => new Param11(br, classNames),
                    12 => new Param12(br, classNames),
                    13 => new Param13(br, classNames),
                    15 => new Param15(br, classNames),
                    17 => new Param17(br, classNames),
                    18 => new Param18(br, classNames),
                    19 => new Param19(br, classNames),
                    20 => new Param20(br, classNames),
                    21 => new Param21(br, classNames),
                    37 => new Param37(br, classNames),
                    38 => new Param38(br, classNames),
                    40 => new Param40(br, classNames),
                    41 => new Param41(br, classNames),
                    44 => new Param44(br, classNames),
                    45 => new Param45(br, classNames),
                    46 => new Param46(br, classNames),
                    47 => new Param47(br, classNames),
                    59 => new Param59(br, classNames),
                    60 => new Param60(br, classNames),
                    66 => new Param66(br, classNames),
                    68 => new Param68(br, classNames),
                    69 => new Param69(br, classNames),
                    70 => new Param70(br, classNames),
                    71 => new Param71(br, classNames),
                    79 => new Param79(br, classNames),
                    81 => new Param81(br, classNames),
                    82 => new Param82(br, classNames),
                    83 => new Param83(br, classNames),
                    84 => new Param84(br, classNames),
                    85 => new Param85(br, classNames),
                    87 => new Param87(br, classNames),
                    _ => throw new NotImplementedException($"Unimplemented param type: {type}"),
                };
            }
        }

        public class Param1 : Param {
            internal override int Type => 1;

            [XmlAttribute]
            public int Int { get; set; }

            public Param1() { }

            internal Param1(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.Int = PrimitiveInt.Read(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                PrimitiveInt.AddClassName(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                PrimitiveInt.Write(bw, classNames, this.Int);
            }
        }

        public class Param2 : Param {
            internal override int Type => 2;

            public List<int> Ints { get; set; }

            public Param2() => this.Ints = new List<int>();

            internal Param2(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                int count = br.ReadInt32();
                this.Ints = new List<int>(count);
                for (int i = 0; i < count; i++) {
                    this.Ints.Add(PrimitiveInt.Read(br, classNames));
                }
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                PrimitiveInt.AddClassName(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.Ints.Count);
                foreach (int value in this.Ints) {
                    PrimitiveInt.Write(bw, classNames, value);
                }
            }
        }

        public class Param5 : Param {
            internal override int Type => 5;

            public List<TickInt> TickInts { get; set; }

            public Param5() => this.TickInts = new List<TickInt>();

            internal Param5(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                int count = br.ReadInt32();
                this.TickInts = new List<TickInt>(count);
                for (int i = 0; i < count; i++) {
                    this.TickInts.Add(new TickInt(br, classNames));
                }
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                foreach (TickInt tickInt in this.TickInts) {
                    tickInt.AddClassNames(classNames);
                }
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.TickInts.Count);
                foreach (TickInt tickInt in this.TickInts) {
                    tickInt.Write(bw, classNames);
                }
            }
        }

        public class Param6 : Param {
            internal override int Type => 6;

            public List<TickInt> TickInts { get; set; }

            public Param6() => this.TickInts = new List<TickInt>();

            internal Param6(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                int count = br.ReadInt32();
                this.TickInts = new List<TickInt>(count);
                for (int i = 0; i < count; i++) {
                    this.TickInts.Add(new TickInt(br, classNames));
                }
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                foreach (TickInt tickInt in this.TickInts) {
                    tickInt.AddClassNames(classNames);
                }
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.TickInts.Count);
                foreach (TickInt tickInt in this.TickInts) {
                    tickInt.Write(bw, classNames);
                }
            }
        }

        public class Param7 : Param {
            internal override int Type => 7;

            [XmlAttribute]
            public float Float { get; set; }

            public Param7() { }

            internal Param7(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.Float = PrimitiveFloat.Read(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                PrimitiveFloat.AddClassName(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                PrimitiveFloat.Write(bw, classNames, this.Float);
            }
        }

        public class Param9 : Param {
            internal override int Type => 9;

            public List<TickFloat> TickFloats { get; set; }

            public Param9() => this.TickFloats = new List<TickFloat>();

            internal Param9(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                int count = br.ReadInt32();
                this.TickFloats = new List<TickFloat>(count);
                for (int i = 0; i < count; i++) {
                    this.TickFloats.Add(new TickFloat(br, classNames));
                }
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                foreach (TickFloat tickFloat in this.TickFloats) {
                    tickFloat.AddClassNames(classNames);
                }
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.TickFloats.Count);
                foreach (TickFloat tickFloat in this.TickFloats) {
                    tickFloat.Write(bw, classNames);
                }
            }
        }

        public class Param11 : Param {
            internal override int Type => 11;

            public List<TickFloat> TickFloats { get; set; }

            public Param11() => this.TickFloats = new List<TickFloat>();

            internal Param11(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                int count = br.ReadInt32();
                this.TickFloats = new List<TickFloat>(count);
                for (int i = 0; i < count; i++) {
                    this.TickFloats.Add(new TickFloat(br, classNames));
                }
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                foreach (TickFloat tickFloat in this.TickFloats) {
                    tickFloat.AddClassNames(classNames);
                }
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.TickFloats.Count);
                foreach (TickFloat tickFloat in this.TickFloats) {
                    tickFloat.Write(bw, classNames);
                }
            }
        }

        public class Param12 : Param {
            internal override int Type => 12;

            public List<TickFloat> TickFloats { get; set; }

            public Param12() => this.TickFloats = new List<TickFloat>();

            internal Param12(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                int count = br.ReadInt32();
                this.TickFloats = new List<TickFloat>(count);
                for (int i = 0; i < count; i++) {
                    this.TickFloats.Add(new TickFloat(br, classNames));
                }
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                foreach (TickFloat tickFloat in this.TickFloats) {
                    tickFloat.AddClassNames(classNames);
                }
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.TickFloats.Count);
                foreach (TickFloat tickFloat in this.TickFloats) {
                    tickFloat.Write(bw, classNames);
                }
            }
        }

        public class Param13 : Param {
            internal override int Type => 13;

            public List<TickFloat3> TickFloat3s { get; set; }

            public Param13() => this.TickFloat3s = new List<TickFloat3>();

            internal Param13(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                int count = br.ReadInt32();
                this.TickFloat3s = new List<TickFloat3>(count);
                for (int i = 0; i < count; i++) {
                    this.TickFloat3s.Add(new TickFloat3(br, classNames));
                }
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                foreach (TickFloat3 tickFloat3 in this.TickFloat3s) {
                    tickFloat3.AddClassNames(classNames);
                }
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.TickFloat3s.Count);
                foreach (TickFloat3 tickFloat3 in this.TickFloat3s) {
                    tickFloat3.Write(bw, classNames);
                }
            }
        }

        public class Param15 : Param {
            internal override int Type => 15;

            public PrimitiveColor Color { get; set; }

            public Param15() => this.Color = new PrimitiveColor();

            internal Param15(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.Color = new PrimitiveColor(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                this.Color.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                this.Color.Write(bw, classNames);
            }
        }

        public class Param17 : Param {
            internal override int Type => 17;

            public List<TickColor> TickColors { get; set; }

            public Param17() => this.TickColors = new List<TickColor>();

            internal Param17(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                int count = br.ReadInt32();
                this.TickColors = new List<TickColor>(count);
                for (int i = 0; i < count; i++) {
                    this.TickColors.Add(new TickColor(br, classNames));
                }
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                foreach (TickColor tickColor in this.TickColors) {
                    tickColor.AddClassNames(classNames);
                }
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.TickColors.Count);
                foreach (TickColor tickColor in this.TickColors) {
                    tickColor.Write(bw, classNames);
                }
            }
        }

        public class Param18 : Param {
            internal override int Type => 18;

            public List<TickColor> TickColors { get; set; }

            public Param18() => this.TickColors = new List<TickColor>();

            internal Param18(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                int count = br.ReadInt32();
                this.TickColors = new List<TickColor>(count);
                for (int i = 0; i < count; i++) {
                    this.TickColors.Add(new TickColor(br, classNames));
                }
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                foreach (TickColor tickColor in this.TickColors) {
                    tickColor.AddClassNames(classNames);
                }
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.TickColors.Count);
                foreach (TickColor tickColor in this.TickColors) {
                    tickColor.Write(bw, classNames);
                }
            }
        }

        public class Param19 : Param {
            internal override int Type => 19;

            public List<TickColor> TickColors { get; set; }

            public Param19() => this.TickColors = new List<TickColor>();

            internal Param19(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                int count = br.ReadInt32();
                this.TickColors = new List<TickColor>(count);
                for (int i = 0; i < count; i++) {
                    this.TickColors.Add(new TickColor(br, classNames));
                }
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                foreach (TickColor tickColor in this.TickColors) {
                    tickColor.AddClassNames(classNames);
                }
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.TickColors.Count);
                foreach (TickColor tickColor in this.TickColors) {
                    tickColor.Write(bw, classNames);
                }
            }
        }

        public class Param20 : Param {
            internal override int Type => 20;

            public List<TickColor> TickColors { get; set; }

            public Param20() => this.TickColors = new List<TickColor>();

            internal Param20(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                int count = br.ReadInt32();
                this.TickColors = new List<TickColor>(count);
                for (int i = 0; i < count; i++) {
                    this.TickColors.Add(new TickColor(br, classNames));
                }
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                foreach (TickColor tickColor in this.TickColors) {
                    tickColor.AddClassNames(classNames);
                }
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.TickColors.Count);
                foreach (TickColor tickColor in this.TickColors) {
                    tickColor.Write(bw, classNames);
                }
            }
        }

        public class Param21 : Param {
            internal override int Type => 21;

            public List<TickColor3> TickColor3s { get; set; }

            public Param21() => this.TickColor3s = new List<TickColor3>();

            internal Param21(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                int count = br.ReadInt32();
                this.TickColor3s = new List<TickColor3>(count);
                for (int i = 0; i < count; i++) {
                    this.TickColor3s.Add(new TickColor3(br, classNames));
                }
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                foreach (TickColor3 tickColor3 in this.TickColor3s) {
                    tickColor3.AddClassNames(classNames);
                }
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.TickColor3s.Count);
                foreach (TickColor3 tickColor3 in this.TickColor3s) {
                    tickColor3.Write(bw, classNames);
                }
            }
        }

        public class Param37 : Param {
            internal override int Type => 37;

            [XmlAttribute]
            public int EffectID { get; set; }

            public ParamList ParamList { get; set; }

            public Param37() => this.ParamList = new ParamList();

            internal Param37(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.EffectID = br.ReadInt32();
                this.ParamList = new ParamList(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                this.ParamList.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.EffectID);
                this.ParamList.Write(bw, classNames);
            }
        }

        public class Param38 : Param {
            internal override int Type => 38;

            [XmlAttribute]
            public int ActionID { get; set; }

            public ParamList ParamList { get; set; }

            public Param38() => this.ParamList = new ParamList();

            internal Param38(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.ActionID = br.ReadInt32();
                this.ParamList = new ParamList(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                this.ParamList.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.ActionID);
                this.ParamList.Write(bw, classNames);
            }
        }

        public class Param40 : Param {
            internal override int Type => 40;

            [XmlAttribute]
            public int TextureID { get; set; }

            public Param40() { }

            internal Param40(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.TextureID = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.TextureID);
            }
        }

        public class Param41 : Param {
            internal override int Type => 41;

            [XmlAttribute]
            public int Unk04 { get; set; }

            public Param41() { }

            internal Param41(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.Unk04 = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.Unk04);
            }
        }

        public class Param44 : Param {
            internal override int Type => 44;

            [XmlAttribute]
            public int Unk04 { get; set; }

            [XmlAttribute]
            public int ArgIndex { get; set; }

            public Param44() { }

            internal Param44(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.Unk04 = br.ReadInt32();
                this.ArgIndex = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.Unk04);
                bw.WriteInt32(this.ArgIndex);
            }
        }

        public class Param45 : Param {
            internal override int Type => 45;

            [XmlAttribute]
            public int Unk04 { get; set; }

            [XmlAttribute]
            public int ArgIndex { get; set; }

            public Param45() { }

            internal Param45(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.Unk04 = br.ReadInt32();
                this.ArgIndex = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.Unk04);
                bw.WriteInt32(this.ArgIndex);
            }
        }

        public class Param46 : Param {
            internal override int Type => 46;

            [XmlAttribute]
            public int Unk04 { get; set; }

            [XmlAttribute]
            public int ArgIndex { get; set; }

            public Param46() { }

            internal Param46(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.Unk04 = br.ReadInt32();
                this.ArgIndex = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.Unk04);
                bw.WriteInt32(this.ArgIndex);
            }
        }

        public class Param47 : Param {
            internal override int Type => 47;

            [XmlAttribute]
            public int Unk04 { get; set; }

            [XmlAttribute]
            public int ArgIndex { get; set; }

            public Param47() { }

            internal Param47(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.Unk04 = br.ReadInt32();
                this.ArgIndex = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.Unk04);
                bw.WriteInt32(this.ArgIndex);
            }
        }

        public class Param59 : Param {
            internal override int Type => 59;

            [XmlAttribute]
            public int Unk04 { get; set; }

            [XmlAttribute]
            public int ArgIndex { get; set; }

            public Param59() { }

            internal Param59(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.Unk04 = br.ReadInt32();
                this.ArgIndex = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.Unk04);
                bw.WriteInt32(this.ArgIndex);
            }
        }

        public class Param60 : Param {
            internal override int Type => 60;

            [XmlAttribute]
            public int Unk04 { get; set; }

            [XmlAttribute]
            public int ArgIndex { get; set; }

            public Param60() { }

            internal Param60(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.Unk04 = br.ReadInt32();
                this.ArgIndex = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.Unk04);
                bw.WriteInt32(this.ArgIndex);
            }
        }

        public class Param66 : Param {
            internal override int Type => 66;

            [XmlAttribute]
            public int Unk04 { get; set; }

            [XmlAttribute]
            public int ArgIndex { get; set; }

            public Param66() { }

            internal Param66(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.Unk04 = br.ReadInt32();
                this.ArgIndex = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.Unk04);
                bw.WriteInt32(this.ArgIndex);
            }
        }

        public class Param68 : Param {
            internal override int Type => 68;

            [XmlAttribute]
            public int SoundID { get; set; }

            public Param68() { }

            internal Param68(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.SoundID = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.SoundID);
            }
        }

        public class Param69 : Param {
            internal override int Type => 69;

            [XmlAttribute]
            public int Unk04 { get; set; }

            public Param69() { }

            internal Param69(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.Unk04 = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.Unk04);
            }
        }

        public class Param70 : Param {
            internal override int Type => 70;

            [XmlAttribute]
            public float Tick { get; set; }

            public Param70() { }

            internal Param70(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.Tick = PrimitiveTick.Read(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                PrimitiveTick.AddClassName(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                PrimitiveTick.Write(bw, classNames, this.Tick);
            }
        }

        public class Param71 : Param {
            internal override int Type => 71;

            [XmlAttribute]
            public int Unk04 { get; set; }

            [XmlAttribute]
            public int ArgIndex { get; set; }

            public Param71() { }

            internal Param71(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.Unk04 = br.ReadInt32();
                this.ArgIndex = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.Unk04);
                bw.WriteInt32(this.ArgIndex);
            }
        }

        public class Param79 : Param {
            internal override int Type => 79;

            [XmlAttribute]
            public int Int1 { get; set; }

            [XmlAttribute]
            public int Int2 { get; set; }

            public Param79() { }

            internal Param79(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.Int1 = PrimitiveInt.Read(br, classNames);
                this.Int2 = PrimitiveInt.Read(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                PrimitiveInt.AddClassName(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                PrimitiveInt.Write(bw, classNames, this.Int1);
                PrimitiveInt.Write(bw, classNames, this.Int2);
            }
        }

        public class Param81 : Param {
            internal override int Type => 81;

            [XmlAttribute]
            public float Float1 { get; set; }

            [XmlAttribute]
            public float Float2 { get; set; }

            public Param81() { }

            internal Param81(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.Float1 = PrimitiveFloat.Read(br, classNames);
                this.Float2 = PrimitiveFloat.Read(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                PrimitiveFloat.AddClassName(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                PrimitiveFloat.Write(bw, classNames, this.Float1);
                PrimitiveFloat.Write(bw, classNames, this.Float2);
            }
        }

        public class Param82 : Param {
            internal override int Type => 82;

            public Param Param { get; set; }

            public float Float { get; set; }

            public Param82() => this.Param = new Param1();

            internal Param82(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.Param = Param.Read(br, classNames);
                this.Float = PrimitiveFloat.Read(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                this.Param.AddClassNames(classNames);
                PrimitiveFloat.AddClassName(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                this.Param.Write(bw, classNames);
                PrimitiveFloat.Write(bw, classNames, this.Float);
            }
        }

        public class Param83 : Param {
            internal override int Type => 83;

            public PrimitiveColor Color1 { get; set; }

            public PrimitiveColor Color2 { get; set; }

            public Param83() {
                this.Color1 = new PrimitiveColor();
                this.Color2 = new PrimitiveColor();
            }

            internal Param83(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.Color1 = new PrimitiveColor(br, classNames);
                this.Color2 = new PrimitiveColor(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                this.Color1.AddClassNames(classNames);
                this.Color2.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                this.Color1.Write(bw, classNames);
                this.Color2.Write(bw, classNames);
            }
        }

        public class Param84 : Param {
            internal override int Type => 84;

            public Param Param { get; set; }

            public PrimitiveColor Color { get; set; }

            public Param84() {
                this.Param = new Param1();
                this.Color = new PrimitiveColor();
            }

            internal Param84(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.Param = Param.Read(br, classNames);
                this.Color = new PrimitiveColor(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                this.Param.AddClassNames(classNames);
                this.Color.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                this.Param.Write(bw, classNames);
                this.Color.Write(bw, classNames);
            }
        }

        public class Param85 : Param {
            internal override int Type => 85;

            [XmlAttribute]
            public float Tick1 { get; set; }

            [XmlAttribute]
            public float Tick2 { get; set; }

            public Param85() { }

            internal Param85(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.Tick1 = PrimitiveTick.Read(br, classNames);
                this.Tick2 = PrimitiveTick.Read(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                PrimitiveTick.AddClassName(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                PrimitiveTick.Write(bw, classNames, this.Tick1);
                PrimitiveTick.Write(bw, classNames, this.Tick2);
            }
        }

        public class Param87 : Param {
            internal override int Type => 87;

            [XmlAttribute]
            public int Unk04 { get; set; }

            [XmlAttribute]
            public int ArgIndex { get; set; }

            public Param87() { }

            internal Param87(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                base.Deserialize(br, classNames);
                this.Unk04 = br.ReadInt32();
                this.ArgIndex = br.ReadInt32();
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                base.Serialize(bw, classNames);
                bw.WriteInt32(this.Unk04);
                bw.WriteInt32(this.ArgIndex);
            }
        }

        public class TickInt {
            [XmlAttribute]
            public float Tick { get; set; }

            [XmlAttribute]
            public int Int { get; set; }

            public TickInt() { }

            public TickInt(float tick, int primInt) {
                this.Tick = tick;
                this.Int = primInt;
            }

            internal TickInt(BinaryReaderEx br, List<string> classNames) {
                this.Tick = PrimitiveTick.Read(br, classNames);
                this.Int = PrimitiveInt.Read(br, classNames);
            }

            internal void AddClassNames(List<string> classNames) {
                PrimitiveTick.AddClassName(classNames);
                PrimitiveInt.AddClassName(classNames);
            }

            internal void Write(BinaryWriterEx bw, List<string> classNames) {
                PrimitiveTick.Write(bw, classNames, this.Tick);
                PrimitiveInt.Write(bw, classNames, this.Int);
            }
        }

        public class TickFloat {
            [XmlAttribute]
            public float Tick { get; set; }

            [XmlAttribute]
            public float Float { get; set; }

            public TickFloat() { }

            public TickFloat(float tick, float primFloat) {
                this.Tick = tick;
                this.Float = primFloat;
            }

            internal TickFloat(BinaryReaderEx br, List<string> classNames) {
                this.Tick = PrimitiveTick.Read(br, classNames);
                this.Float = PrimitiveFloat.Read(br, classNames);
            }

            internal void AddClassNames(List<string> classNames) {
                PrimitiveTick.AddClassName(classNames);
                PrimitiveFloat.AddClassName(classNames);
            }

            internal void Write(BinaryWriterEx bw, List<string> classNames) {
                PrimitiveTick.Write(bw, classNames, this.Tick);
                PrimitiveFloat.Write(bw, classNames, this.Float);
            }
        }

        public class TickFloat3 {
            [XmlAttribute]
            public float Tick { get; set; }

            public float Float1 { get; set; }

            public float Float2 { get; set; }

            public float Float3 { get; set; }

            public TickFloat3() { }

            public TickFloat3(float tick, float float1, float float2, float float3) {
                this.Tick = tick;
                this.Float1 = float1;
                this.Float2 = float2;
                this.Float3 = float3;
            }

            internal TickFloat3(BinaryReaderEx br, List<string> classNames) {
                this.Tick = PrimitiveTick.Read(br, classNames);
                this.Float1 = PrimitiveFloat.Read(br, classNames);
                this.Float2 = PrimitiveFloat.Read(br, classNames);
                this.Float3 = PrimitiveFloat.Read(br, classNames);
            }

            internal void AddClassNames(List<string> classNames) {
                PrimitiveTick.AddClassName(classNames);
                PrimitiveFloat.AddClassName(classNames);
            }

            internal void Write(BinaryWriterEx bw, List<string> classNames) {
                PrimitiveTick.Write(bw, classNames, this.Tick);
                PrimitiveFloat.Write(bw, classNames, this.Float1);
                PrimitiveFloat.Write(bw, classNames, this.Float2);
                PrimitiveFloat.Write(bw, classNames, this.Float3);
            }
        }

        public class TickColor {
            [XmlAttribute]
            public float Tick { get; set; }

            public PrimitiveColor Color { get; set; }

            public TickColor() => this.Color = new PrimitiveColor();

            public TickColor(float tick, PrimitiveColor color) {
                this.Tick = tick;
                this.Color = color;
            }

            internal TickColor(BinaryReaderEx br, List<string> classNames) {
                this.Tick = PrimitiveTick.Read(br, classNames);
                this.Color = new PrimitiveColor(br, classNames);
            }

            internal void AddClassNames(List<string> classNames) {
                PrimitiveTick.AddClassName(classNames);
                this.Color.AddClassNames(classNames);
            }

            internal void Write(BinaryWriterEx bw, List<string> classNames) {
                PrimitiveTick.Write(bw, classNames, this.Tick);
                this.Color.Write(bw, classNames);
            }
        }

        public class TickColor3 {
            [XmlAttribute]
            public float Tick { get; set; }

            public PrimitiveColor Color1 { get; set; }

            public PrimitiveColor Color2 { get; set; }

            public PrimitiveColor Color3 { get; set; }

            public TickColor3() {
                this.Color1 = new PrimitiveColor();
                this.Color2 = new PrimitiveColor();
                this.Color3 = new PrimitiveColor();
            }

            public TickColor3(float tick, PrimitiveColor color1, PrimitiveColor color2, PrimitiveColor color3) {
                this.Tick = tick;
                this.Color1 = color1;
                this.Color2 = color2;
                this.Color3 = color3;
            }

            internal TickColor3(BinaryReaderEx br, List<string> classNames) {
                this.Tick = PrimitiveTick.Read(br, classNames);
                this.Color1 = new PrimitiveColor(br, classNames);
                this.Color2 = new PrimitiveColor(br, classNames);
                this.Color3 = new PrimitiveColor(br, classNames);
            }

            internal void AddClassNames(List<string> classNames) {
                PrimitiveTick.AddClassName(classNames);
                this.Color1.AddClassNames(classNames);
                this.Color2.AddClassNames(classNames);
                this.Color3.AddClassNames(classNames);
            }

            internal void Write(BinaryWriterEx bw, List<string> classNames) {
                PrimitiveTick.Write(bw, classNames, this.Tick);
                this.Color1.Write(bw, classNames);
                this.Color2.Write(bw, classNames);
                this.Color3.Write(bw, classNames);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
