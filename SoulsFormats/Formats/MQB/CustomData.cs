using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MQB {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class CustomData {
            public enum DataType : uint {
                Bool = 1,
                SByte = 2,
                Byte = 3,
                Short = 4,
                Int = 6,
                UInt = 7,
                Float = 8,
                String = 10,
                Custom = 11,
                Color = 13,
            }

            public string Name { get; set; }

            public DataType Type { get; set; }

            public int Unk44 { get; set; }

            public object Value { get; set; }

            public List<Sequence> Sequences { get; set; }

            public CustomData() {
                this.Name = "";
                this.Type = DataType.Int;
                this.Value = 0;
                this.Sequences = new List<Sequence>();
            }

            internal CustomData(BinaryReaderEx br) {
                this.Name = br.ReadFixStrW(0x40);
                this.Type = br.ReadEnum32<DataType>();
                _ = br.AssertInt32(this.Type == DataType.Color ? 3 : 0);

                long valueOffset = br.Position;
                this.Value = this.Type switch {
                    DataType.Bool => br.ReadBoolean(),
                    DataType.SByte => br.ReadSByte(),
                    DataType.Byte => br.ReadByte(),
                    DataType.Short => br.ReadInt16(),
                    DataType.Int => br.ReadInt32(),
                    DataType.UInt => br.ReadUInt32(),
                    DataType.Float => br.ReadSingle(),
                    DataType.String or DataType.Custom or DataType.Color => br.ReadInt32(),
                    _ => throw new NotImplementedException($"Unimplemented custom data type: {this.Type}"),
                };
                if (this.Type is DataType.Bool or DataType.SByte or DataType.Byte) {
                    _ = br.AssertByte(0);
                    _ = br.AssertInt16(0);
                } else if (this.Type == DataType.Short) {
                    _ = br.AssertInt16(0);
                }

                _ = br.AssertInt32(0);
                int sequencesOffset = br.ReadInt32();
                int sequenceCount = br.ReadInt32();
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);

                if (this.Type is DataType.String or DataType.Color or DataType.Custom) {
                    int length = (int)this.Value;
                    if (this.Type == DataType.String) {
                        if (length == 0 || length % 0x10 != 0) {
                            throw new InvalidDataException($"Unexpected custom data string length: {length}");
                        }

                        this.Value = br.ReadFixStrW(length);
                    } else if (this.Type == DataType.Custom) {
                        if (length % 4 != 0) {
                            throw new InvalidDataException($"Unexpected custom data custom length: {length}");
                        }

                        this.Value = br.ReadBytes(length);
                    } else if (this.Type == DataType.Color) {
                        if (length != 4) {
                            throw new InvalidDataException($"Unexpected custom data color length: {length}");
                        }

                        valueOffset = br.Position;
                        this.Value = Color.FromArgb(br.ReadByte(), br.ReadByte(), br.ReadByte());
                        _ = br.AssertByte(0);
                    }
                }

                this.Sequences = new List<Sequence>(sequenceCount);
                if (sequenceCount > 0) {
                    br.StepIn(sequencesOffset);
                    {
                        for (int i = 0; i < sequenceCount; i++) {
                            this.Sequences.Add(new Sequence(br, valueOffset));
                        }
                    }
                    br.StepOut();
                }
            }

            internal void Write(BinaryWriterEx bw, List<CustomData> allCustomData, List<long> customDataValueOffsets) {
                bw.WriteFixStrW(this.Name, 0x40, 0x00);
                bw.WriteUInt32((uint)this.Type);
                bw.WriteInt32(this.Type == DataType.Color ? 3 : 0);

                int length = -1;
                if (this.Type == DataType.String) {
                    length = SFEncoding.UTF16.GetByteCount((string)this.Value + '\0');
                    if (length % 0x10 != 0) {
                        length += 0x10 - length % 0x10;
                    }
                } else if (this.Type == DataType.Custom) {
                    length = ((byte[])this.Value).Length;
                    if (length % 4 != 0) {
                        throw new InvalidDataException($"Unexpected custom data custom length: {length}");
                    }
                } else if (this.Type == DataType.Color) {
                    length = 4;
                }

                long valueOffset = bw.Position;
                switch (this.Type) {
                    case DataType.Bool: bw.WriteBoolean((bool)this.Value); break;
                    case DataType.SByte: bw.WriteSByte((sbyte)this.Value); break;
                    case DataType.Byte: bw.WriteByte((byte)this.Value); break;
                    case DataType.Short: bw.WriteInt16((short)this.Value); break;
                    case DataType.Int: bw.WriteInt32((int)this.Value); break;
                    case DataType.UInt: bw.WriteUInt32((uint)this.Value); break;
                    case DataType.Float: bw.WriteSingle((float)this.Value); break;
                    case DataType.String:
                    case DataType.Custom:
                    case DataType.Color: bw.WriteInt32(length); break;
                    default: throw new NotImplementedException($"Unimplemented custom data type: {this.Type}");
                }

                if (this.Type is DataType.Bool or DataType.SByte or DataType.Byte) {
                    bw.WriteByte(0);
                    bw.WriteInt16(0);
                } else if (this.Type == DataType.Short) {
                    bw.WriteInt16(0);
                }

                // This is probably wrong for the 64-bit format
                bw.WriteInt32(0);
                bw.ReserveInt32($"SequencesOffset[{allCustomData.Count}]");
                bw.WriteInt32(this.Sequences.Count);
                bw.WriteInt32(0);
                bw.WriteInt32(0);

                if (this.Type == DataType.String) {
                    bw.WriteFixStrW((string)this.Value, length, 0x00);
                } else if (this.Type == DataType.Custom) {
                    bw.WriteBytes((byte[])this.Value);
                } else if (this.Type == DataType.Color) {
                    var color = (Color)this.Value;
                    valueOffset = bw.Position;
                    bw.WriteByte(color.R);
                    bw.WriteByte(color.G);
                    bw.WriteByte(color.B);
                    bw.WriteByte(0);
                }

                allCustomData.Add(this);
                customDataValueOffsets.Add(valueOffset);
            }

            internal void WriteSequences(BinaryWriterEx bw, int customDataIndex, long valueOffset) {
                if (this.Sequences.Count == 0) {
                    bw.FillInt32($"SequencesOffset[{customDataIndex}]", 0);
                } else {
                    bw.FillInt32($"SequencesOffset[{customDataIndex}]", (int)bw.Position);
                    for (int i = 0; i < this.Sequences.Count; i++) {
                        this.Sequences[i].Write(bw, customDataIndex, i, valueOffset);
                    }
                }
            }

            internal void WriteSequencePoints(BinaryWriterEx bw, int customDataIndex) {
                for (int i = 0; i < this.Sequences.Count; i++) {
                    this.Sequences[i].WritePoints(bw, customDataIndex, i);
                }
            }

            public class Sequence {
                public DataType ValueType { get; set; }

                public int PointType { get; set; }

                public int ValueIndex { get; set; }

                public List<Point> Points { get; set; }

                public Sequence() {
                    this.ValueType = DataType.Byte;
                    this.PointType = 1;
                    this.Points = new List<Point>();
                }

                internal Sequence(BinaryReaderEx br, long parentValueOffset) {
                    _ = br.AssertInt32(0x1C); // Sequence size
                    int pointCount = br.ReadInt32();
                    this.ValueType = br.ReadEnum32<DataType>();
                    this.PointType = br.AssertInt32(1, 2);
                    _ = br.AssertInt32(this.PointType == 1 ? 0x10 : 0x18); // Point size
                    int pointsOffset = br.ReadInt32();
                    int valueOffset = br.ReadInt32();

                    if (this.ValueType == DataType.Byte) {
                        if (valueOffset < parentValueOffset || valueOffset > parentValueOffset + 2) {
                            throw new InvalidDataException($"Unexpected value offset {valueOffset:X}/{parentValueOffset:X} for value type {this.ValueType}.");
                        }

                        this.ValueIndex = valueOffset - (int)parentValueOffset;
                    } else if (this.ValueType == DataType.Float) {
                        if (valueOffset != parentValueOffset) {
                            throw new InvalidDataException($"Unexpected value offset {valueOffset:X}/{parentValueOffset:X} for value type {this.ValueType}.");
                        }

                        this.ValueIndex = 0;
                    } else {
                        throw new NotSupportedException($"Unsupported sequence value type: {this.ValueType}");
                    }

                    br.StepIn(pointsOffset);
                    {
                        this.Points = new List<Point>(pointCount);
                        for (int i = 0; i < pointCount; i++) {
                            this.Points.Add(new Point(br, this.ValueType, this.PointType));
                        }
                    }
                    br.StepOut();
                }

                internal void Write(BinaryWriterEx bw, int customDataIndex, int sequenceIndex, long parentValueOffset) {
                    bw.WriteInt32(0x1C);
                    bw.WriteInt32(this.Points.Count);
                    bw.WriteUInt32((uint)this.ValueType);
                    bw.WriteInt32(this.PointType);
                    bw.WriteInt32(this.PointType == 1 ? 0x10 : 0x18);
                    bw.ReserveInt32($"PointsOffset[{customDataIndex}:{sequenceIndex}]");
                    if (this.ValueType == DataType.Byte) {
                        bw.WriteInt32((int)parentValueOffset + this.ValueIndex);
                    } else if (this.ValueType == DataType.Float) {
                        bw.WriteInt32((int)parentValueOffset);
                    }
                }

                internal void WritePoints(BinaryWriterEx bw, int customDataIndex, int sequenceIndex) {
                    bw.FillInt32($"PointsOffset[{customDataIndex}:{sequenceIndex}]", (int)bw.Position);
                    foreach (Point point in this.Points) {
                        point.Write(bw, this.ValueType, this.PointType);
                    }
                }

                public class Point {
                    public object Value { get; set; }

                    public int Unk08 { get; set; }

                    public float Unk10 { get; set; }

                    public float Unk14 { get; set; }

                    public Point() => this.Value = (byte)0;

                    internal Point(BinaryReaderEx br, DataType valueType, int pointType) {
                        this.Value = valueType switch {
                            DataType.Byte => br.ReadByte(),
                            DataType.Float => (object)br.ReadSingle(),
                            _ => throw new NotSupportedException($"Unsupported sequence value type: {valueType}"),
                        };
                        if (valueType == DataType.Byte) {
                            _ = br.AssertInt16(0);
                            _ = br.AssertByte(0);
                        }

                        _ = br.AssertInt32(0);
                        this.Unk08 = br.ReadInt32();
                        _ = br.AssertInt32(0);

                        // I suspect these are also variable type, but in the few instances of pointType 2
                        // with valueType 3, they're all just 0.
                        if (pointType == 2) {
                            this.Unk10 = br.ReadSingle();
                            this.Unk14 = br.ReadSingle();
                        }
                    }

                    internal void Write(BinaryWriterEx bw, DataType valueType, int pointType) {
                        switch (valueType) {
                            case DataType.Byte: bw.WriteByte((byte)this.Value); break;
                            case DataType.Float: bw.WriteSingle((float)this.Value); break;
                            default: throw new NotSupportedException($"Unsupported sequence value type: {valueType}");
                        }

                        if (valueType == DataType.Byte) {
                            bw.WriteInt16(0);
                            bw.WriteByte(0);
                        }

                        bw.WriteInt32(0);
                        bw.WriteInt32(this.Unk08);
                        bw.WriteInt32(0);

                        if (pointType == 2) {
                            bw.WriteSingle(this.Unk10);
                            bw.WriteSingle(this.Unk14);
                        }
                    }
                }
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
