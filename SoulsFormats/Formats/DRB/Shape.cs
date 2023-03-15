using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class DRB {
        internal enum ShapeType {
            Dialog,
            GouraudFrame,
            GouraudRect,
            GouraudSprite,
            Mask,
            MonoFrame,
            MonoRect,
            Null,
            ScrollText,
            Sprite,
            Text
        }

        /// <summary>
        /// Describes the appearance of a UI element.
        /// </summary>
        public abstract class Shape {
            internal abstract ShapeType Type { get; }

            /// <summary>
            /// Left bound of this element, relative to 1280x720.
            /// </summary>
            public short LeftEdge { get; set; }

            /// <summary>
            /// Top bound of this element, relative to 1280x720.
            /// </summary>
            public short TopEdge { get; set; }

            /// <summary>
            /// Right bound of this element, relative to 1280x720.
            /// </summary>
            public short RightEdge { get; set; }

            /// <summary>
            /// Bottom bound of this element, relative to 1280x720.
            /// </summary>
            public short BottomEdge { get; set; }

            /// <summary>
            /// For DSR, the X coordinate which the element scales relative to.
            /// </summary>
            public short ScalingOriginX { get; set; }

            /// <summary>
            /// For DSR, the Y coordinate which the element scales relative to.
            /// </summary>
            public short ScalingOriginY { get; set; }

            /// <summary>
            /// For DSR, the behavior of scaling for this element.
            /// </summary>
            public short ScalingMode { get; set; }

            internal Shape() {
                this.ScalingOriginX = -1;
                this.ScalingOriginY = -1;
            }

            internal static Shape Read(BinaryReaderEx br, DRBVersion version, Dictionary<int, string> strings, long shprStart) {
                int typeOffset = br.ReadInt32();
                int shprOffset = br.ReadInt32();
                string type = strings[typeOffset];

                Shape result;
                br.StepIn(shprStart + shprOffset);
                {
                    if (type == "Dialog") {
                        result = new Dialog(br, version);
                    } else if (type == "GouraudFrame") {
                        result = new GouraudFrame(br, version);
                    } else if (type == "GouraudRect") {
                        result = new GouraudRect(br, version);
                    } else if (type == "GouraudSprite") {
                        result = new GouraudSprite(br, version);
                    } else if (type == "Mask") {
                        result = new Mask(br, version, strings);
                    } else if (type == "MonoFrame") {
                        result = new MonoFrame(br, version);
                    } else if (type == "MonoRect") {
                        result = new MonoRect(br, version);
                    } else {
                        result = type == "Null"
                            ? new Null(br, version)
                            : type == "ScrollText"
                                                    ? new ScrollText(br, version, strings)
                                                    : type == "Sprite"
                                                                            ? new Sprite(br, version)
                                                                            : type == "Text" ? (Shape)new Text(br, version, strings) : throw new InvalidDataException($"Unknown shape type: {type}");
                    }
                }
                br.StepOut();
                return result;
            }

            internal Shape(BinaryReaderEx br, DRBVersion version) {
                this.LeftEdge = br.ReadInt16();
                this.TopEdge = br.ReadInt16();
                this.RightEdge = br.ReadInt16();
                this.BottomEdge = br.ReadInt16();

                if (version == DRBVersion.DarkSoulsRemastered && this.Type != ShapeType.Null) {
                    this.ScalingOriginX = br.ReadInt16();
                    this.ScalingOriginY = br.ReadInt16();
                    this.ScalingMode = br.ReadInt16();
                    _ = br.AssertInt16(0);
                } else {
                    this.ScalingOriginX = -1;
                    this.ScalingOriginY = -1;
                    this.ScalingMode = 0;
                }
            }

            internal void WriteData(BinaryWriterEx bw, DRBVersion version, Dictionary<string, int> stringOffsets) {
                bw.WriteInt16(this.LeftEdge);
                bw.WriteInt16(this.TopEdge);
                bw.WriteInt16(this.RightEdge);
                bw.WriteInt16(this.BottomEdge);

                if (version == DRBVersion.DarkSoulsRemastered && this.Type != ShapeType.Null) {
                    bw.WriteInt16(this.ScalingOriginX);
                    bw.WriteInt16(this.ScalingOriginY);
                    bw.WriteInt16(this.ScalingMode);
                    bw.WriteInt16(0);
                }

                this.WriteSpecific(bw, stringOffsets);
            }

            internal abstract void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets);

            internal void WriteHeader(BinaryWriterEx bw, Dictionary<string, int> stringOffsets, Queue<int> shprOffsets) {
                bw.WriteInt32(stringOffsets[this.Type.ToString()]);
                bw.WriteInt32(shprOffsets.Dequeue());
            }

            /// <summary>
            /// Returns the type and bounds of this Shape.
            /// </summary>
            public override string ToString() => $"{this.Type} ({this.LeftEdge}, {this.TopEdge}) ({this.RightEdge}, {this.BottomEdge})";

            /// <summary>
            /// Color blending mode of various shapes.
            /// </summary>
            public enum BlendingMode : byte {
                /// <summary>
                /// No transparency.
                /// </summary>
                Opaque = 0,

                /// <summary>
                /// Alpha transparency.
                /// </summary>
                Alpha = 1,

                /// <summary>
                /// Color is added to the underlying color value.
                /// </summary>
                Add = 2,

                /// <summary>
                /// Color is subtracted from the underlying color value.
                /// </summary>
                Subtract = 3,
            }

            /// <summary>
            /// Rotation and mirroring properties of a Sprite.
            /// </summary>
            [Flags]
            public enum SpriteOrientation : byte {
                /// <summary>
                /// No modification.
                /// </summary>
                None = 0,

                /// <summary>
                /// Rotate texture 90 degrees clockwise.
                /// </summary>
                RotateCW = 0x10,

                /// <summary>
                /// Rotate texture 180 degrees.
                /// </summary>
                Rotate180 = 0x20,

                /// <summary>
                /// Flip texture vertically.
                /// </summary>
                FlipVertical = 0x40,

                /// <summary>
                /// Flip texture horizontally.
                /// </summary>
                FlipHorizontal = 0x80,
            }

            /// <summary>
            /// Displays a region of a texture.
            /// </summary>
            public abstract class SpriteBase : Shape {
                /// <summary>
                /// Left bound of the texture region displayed by this element.
                /// </summary>
                public short TexLeftEdge { get; set; }

                /// <summary>
                /// Top bound of the texture region displayed by this element.
                /// </summary>
                public short TexTopEdge { get; set; }

                /// <summary>
                /// Right bound of the texture region displayed by this element.
                /// </summary>
                public short TexRightEdge { get; set; }

                /// <summary>
                /// Bottom bound of the texture region displayed by this element.
                /// </summary>
                public short TexBottomEdge { get; set; }

                /// <summary>
                /// The texture to display, indexing menu.tpf.
                /// </summary>
                public short TextureIndex { get; set; }

                /// <summary>
                /// Flags modifying the orientation of the texture.
                /// </summary>
                public SpriteOrientation Orientation { get; set; }

                /// <summary>
                /// Color blending mode.
                /// </summary>
                public BlendingMode BlendMode { get; set; }

                internal SpriteBase() : base() => this.BlendMode = BlendingMode.Alpha;

                internal SpriteBase(BinaryReaderEx br, DRBVersion version) : base(br, version) {
                    this.TexLeftEdge = br.ReadInt16();
                    this.TexTopEdge = br.ReadInt16();
                    this.TexRightEdge = br.ReadInt16();
                    this.TexBottomEdge = br.ReadInt16();
                    this.TextureIndex = br.ReadInt16();
                    this.Orientation = (SpriteOrientation)br.ReadByte();
                    this.BlendMode = br.ReadEnum8<BlendingMode>();
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets) {
                    bw.WriteInt16(this.TexLeftEdge);
                    bw.WriteInt16(this.TexTopEdge);
                    bw.WriteInt16(this.TexRightEdge);
                    bw.WriteInt16(this.TexBottomEdge);
                    bw.WriteInt16(this.TextureIndex);
                    bw.WriteByte((byte)this.Orientation);
                    bw.WriteByte((byte)this.BlendMode);
                }
            }

            /// <summary>
            /// Indicates the positioning of text within its element.
            /// </summary>
            [Flags]
            public enum AlignFlags : byte {
                /// <summary>
                /// Anchor to top left.
                /// </summary>
                TopLeft = 0,

                /// <summary>
                /// Anchor to right side.
                /// </summary>
                Right = 1,

                /// <summary>
                /// Center horizontally.
                /// </summary>
                CenterHorizontal = 2,

                /// <summary>
                /// Anchor to bottom side.
                /// </summary>
                Bottom = 4,

                /// <summary>
                /// Center vertically.
                /// </summary>
                CenterVertical = 8
            }

            /// <summary>
            /// Indicates the source of a text element's text.
            /// </summary>
            public enum TxtType : byte {
                /// <summary>
                /// Text is a literal value stored in the DRB.
                /// </summary>
                Literal = 0,

                /// <summary>
                /// Text is a static FMG ID.
                /// </summary>
                FMG = 1,

                /// <summary>
                /// Text is assigned at runtime.
                /// </summary>
                Dynamic = 2
            }

            /// <summary>
            /// Either a fixed text or scrolling text element.
            /// </summary>
            public abstract class TextBase : Shape {
                /// <summary>
                /// Color blending mode.
                /// </summary>
                public BlendingMode BlendMode { get; set; }

                /// <summary>
                /// Distance between each line of text.
                /// </summary>
                public short LineSpacing { get; set; }

                /// <summary>
                /// Index of a color in the menu color param, or 0 to use the CustomColor.
                /// </summary>
                public int PaletteColor { get; set; }

                /// <summary>
                /// Custom color used when PaletteColor is 0.
                /// </summary>
                public Color CustomColor { get; set; }

                /// <summary>
                /// From 0-11, different sizes of the menu font. 12 is the subtitle font.
                /// </summary>
                public short FontSize { get; set; }

                /// <summary>
                /// The horizontal and vertical alignment of the text.
                /// </summary>
                public AlignFlags Alignment { get; set; }

                /// <summary>
                /// Whether the element uses a text literal, a static FMG ID, or is assigned at runtime.
                /// </summary>
                public TxtType TextType { get; set; }

                /// <summary>
                /// The maximum characters to display.
                /// </summary>
                public int CharLength { get; set; }

                /// <summary>
                /// If TextType is Literal, the text to display, otherwise null.
                /// </summary>
                public string TextLiteral { get; set; }

                /// <summary>
                /// If TextType is FMG, the FMG ID to display, otherwise -1.
                /// </summary>
                public int TextID { get; set; }

                /// <summary>
                /// An unknown optional structure which is always present in practice.
                /// </summary>
                public UnknownA Unknown { get; set; }

                internal TextBase() : base() {
                    this.BlendMode = BlendingMode.Alpha;
                    this.TextType = TxtType.FMG;
                    this.TextID = -1;
                    this.Unknown = new UnknownA();
                }

                internal TextBase(BinaryReaderEx br, DRBVersion version, Dictionary<int, string> strings) : base(br, version) {
                    this.BlendMode = br.ReadEnum8<BlendingMode>();
                    bool unk01 = br.ReadBoolean();
                    this.LineSpacing = br.ReadInt16();
                    this.PaletteColor = br.ReadInt32();
                    this.CustomColor = ReadColor(br);
                    this.FontSize = br.ReadInt16();
                    this.Alignment = (AlignFlags)br.ReadByte();
                    this.TextType = br.ReadEnum8<TxtType>();
                    _ = br.AssertInt32(0x1C); // Local offset to variable data below

                    if (this.TextType == TxtType.Literal) {
                        int textOffset = br.ReadInt32();
                        this.TextLiteral = strings[textOffset];
                        this.CharLength = -1;
                        this.TextID = -1;
                    } else if (this.TextType == TxtType.FMG) {
                        this.CharLength = br.ReadInt32();
                        this.TextID = br.ReadInt32();
                        this.TextLiteral = null;
                    } else if (this.TextType == TxtType.Dynamic) {
                        this.CharLength = br.ReadInt32();
                        this.TextLiteral = null;
                        this.TextID = -1;
                    }

                    this.ReadSubtype(br);

                    if (unk01) {
                        this.Unknown = new UnknownA(br);
                    }
                }

                internal virtual void ReadSubtype(BinaryReaderEx br) { }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets) {
                    bw.WriteByte((byte)this.BlendMode);
                    bw.WriteBoolean(this.Unknown != null);
                    bw.WriteInt16(this.LineSpacing);
                    bw.WriteInt32(this.PaletteColor);
                    WriteColor(bw, this.CustomColor);
                    bw.WriteInt16(this.FontSize);
                    bw.WriteByte((byte)this.Alignment);
                    bw.WriteByte((byte)this.TextType);
                    bw.WriteInt32(0x1C);

                    if (this.TextType == TxtType.Literal) {
                        bw.WriteInt32(stringOffsets[this.TextLiteral]);
                    } else if (this.TextType == TxtType.FMG) {
                        bw.WriteInt32(this.CharLength);
                        bw.WriteInt32(this.TextID);
                    } else if (this.TextType == TxtType.Dynamic) {
                        bw.WriteInt32(this.CharLength);
                    }

                    this.WriteSubtype(bw);

                    this.Unknown?.Write(bw);
                }

                internal virtual void WriteSubtype(BinaryWriterEx bw) { }

                /// <summary>
                /// Unknown.
                /// </summary>
                public class UnknownA {
                    /// <summary>
                    /// Unknown; always 0.
                    /// </summary>
                    public int Unk00 { get; set; }

                    /// <summary>
                    /// Unknown optional structure, null if not present.
                    /// </summary>
                    public UnknownB SubUnknown { get; set; }

                    /// <summary>
                    /// Creates an UnknownA with default values.
                    /// </summary>
                    public UnknownA() { }

                    internal UnknownA(BinaryReaderEx br) {
                        this.Unk00 = br.ReadInt32();
                        short unk04 = br.AssertInt16(0, 1);
                        if (unk04 == 1) {
                            this.SubUnknown = new UnknownB(br);
                        }
                    }

                    internal void Write(BinaryWriterEx bw) {
                        bw.WriteInt32(this.Unk00);
                        bw.WriteInt16((short)(this.SubUnknown == null ? 0 : 1));
                        this.SubUnknown?.Write(bw);
                    }
                }

                /// <summary>
                /// Unknown structure only observed in Shadow Assault: Tenchu.
                /// </summary>
                public class UnknownB {
                    /// <summary>
                    /// Unknown; always 0.
                    /// </summary>
                    public int Unk00 { get; set; }

                    /// <summary>
                    /// Unknown; always 0xFF.
                    /// </summary>
                    public short Unk04 { get; set; }

                    /// <summary>
                    /// Unknown; always 0 or 2.
                    /// </summary>
                    public short Unk06 { get; set; }

                    /// <summary>
                    /// Unknown; always 0 or 2.
                    /// </summary>
                    public short Unk08 { get; set; }

                    /// <summary>
                    /// Creates an UnknownB with default values.
                    /// </summary>
                    public UnknownB() => this.Unk04 = 0xFF;

                    internal UnknownB(BinaryReaderEx br) {
                        this.Unk00 = br.ReadInt32();
                        this.Unk04 = br.ReadInt16();
                        this.Unk06 = br.ReadInt16();
                        this.Unk08 = br.ReadInt16();
                    }

                    internal void Write(BinaryWriterEx bw) {
                        bw.WriteInt32(this.Unk00);
                        bw.WriteInt16(this.Unk04);
                        bw.WriteInt16(this.Unk06);
                        bw.WriteInt16(this.Unk08);
                    }
                }
            }

            /// <summary>
            /// Displays another referenced group of elements.
            /// </summary>
            public class Dialog : Shape {
                internal override ShapeType Type => ShapeType.Dialog;

                /// <summary>
                /// Dlg referenced by this element; must be found in the parent DRB's Dlg list.
                /// </summary>
                public Dlg Dlg { get; set; }
                internal short DlgIndex;

                /// <summary>
                /// Unknown; always 0 or 1.
                /// </summary>
                public byte Unk02 { get; set; }

                /// <summary>
                /// Unknown; always 0 or 1. Determines whether the optional structure is loaded.
                /// </summary>
                public byte Unk03 { get; set; }

                /// <summary>
                /// Index of a color in the menu color param, or 0 to use the CustomColor.
                /// </summary>
                public int PaletteColor { get; set; }

                /// <summary>
                /// Custom color used when PaletteColor is 0.
                /// </summary>
                public Color CustomColor { get; set; }

                /// <summary>
                /// An unknown, optional, and unused structure; only loaded if Unk03 is non-zero.
                /// </summary>
                public UnknownA Unknown { get; set; }

                /// <summary>
                /// Creates a Dialog with default values.
                /// </summary>
                public Dialog() {
                    this.DlgIndex = -1;
                    this.Unk02 = 1;
                    this.Unk03 = 1;
                    this.CustomColor = Color.White;
                }

                internal Dialog(BinaryReaderEx br, DRBVersion version) : base(br, version) {
                    this.DlgIndex = br.ReadInt16();
                    this.Unk02 = br.ReadByte();
                    this.Unk03 = br.ReadByte();
                    this.PaletteColor = br.ReadInt32();
                    this.CustomColor = ReadColor(br);
                    bool unk0C = br.ReadBoolean();
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);

                    if (unk0C) {
                        this.Unknown = new UnknownA(br);
                    }
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets) {
                    bw.WriteInt16(this.DlgIndex);
                    bw.WriteByte(this.Unk02);
                    bw.WriteByte(this.Unk03);
                    bw.WriteInt32(this.PaletteColor);
                    WriteColor(bw, this.CustomColor);
                    bw.WriteByte((byte)(this.Unknown == null ? 0 : 1));
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                }

                /// <summary>
                /// Unknown.
                /// </summary>
                public class UnknownA {
                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public short Unk00 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public short Unk02 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk04 { get; set; }

                    /// <summary>
                    /// Creates an UnknownA with default values.
                    /// </summary>
                    public UnknownA() { }

                    internal UnknownA(BinaryReaderEx br) {
                        this.Unk00 = br.ReadInt16();
                        this.Unk02 = br.ReadInt16();
                        this.Unk04 = br.ReadInt32();
                    }

                    internal void Write(BinaryWriterEx bw) {
                        bw.WriteInt16(this.Unk00);
                        bw.WriteInt16(this.Unk02);
                        bw.WriteInt32(this.Unk04);
                    }
                }
            }

            /// <summary>
            /// A rectangular frame with color interpolated between each corner.
            /// </summary>
            public class GouraudFrame : Shape {
                internal override ShapeType Type => ShapeType.GouraudFrame;

                /// <summary>
                /// Color blending mode.
                /// </summary>
                public BlendingMode BlendMode { get; set; }

                /// <summary>
                /// Thickness of the border.
                /// </summary>
                public byte Thickness { get; set; }

                /// <summary>
                /// The color at the top left corner.
                /// </summary>
                public Color TopLeftColor { get; set; }

                /// <summary>
                /// The color at the top right corner.
                /// </summary>
                public Color TopRightColor { get; set; }

                /// <summary>
                /// The color at the bottom right corner.
                /// </summary>
                public Color BottomRightColor { get; set; }

                /// <summary>
                /// The color at the bottom left corner.
                /// </summary>
                public Color BottomLeftColor { get; set; }

                /// <summary>
                /// Creates a GouraudFrame with default values.
                /// </summary>
                public GouraudFrame() : base() {
                    this.BlendMode = BlendingMode.Alpha;
                    this.Thickness = 1;
                    this.TopLeftColor = Color.Black;
                    this.TopRightColor = Color.Black;
                    this.BottomRightColor = Color.Black;
                    this.BottomLeftColor = Color.Black;
                }

                internal GouraudFrame(BinaryReaderEx br, DRBVersion version) : base(br, version) {
                    this.BlendMode = br.ReadEnum8<BlendingMode>();
                    _ = br.AssertInt16(0);
                    this.Thickness = br.ReadByte();
                    this.TopLeftColor = ReadColor(br);
                    this.TopRightColor = ReadColor(br);
                    this.BottomRightColor = ReadColor(br);
                    this.BottomLeftColor = ReadColor(br);
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets) {
                    bw.WriteByte((byte)this.BlendMode);
                    bw.WriteInt16(0);
                    bw.WriteByte(this.Thickness);
                    WriteColor(bw, this.TopLeftColor);
                    WriteColor(bw, this.TopRightColor);
                    WriteColor(bw, this.BottomRightColor);
                    WriteColor(bw, this.BottomLeftColor);
                }
            }

            /// <summary>
            /// A rectangle with color interpolated between each corner.
            /// </summary>
            public class GouraudRect : Shape {
                internal override ShapeType Type => ShapeType.GouraudRect;

                /// <summary>
                /// Color blending mode.
                /// </summary>
                public BlendingMode BlendMode { get; set; }

                /// <summary>
                /// The color at the top left corner.
                /// </summary>
                public Color TopLeftColor { get; set; }

                /// <summary>
                /// The color at the top right corner.
                /// </summary>
                public Color TopRightColor { get; set; }

                /// <summary>
                /// The color at the bottom right corner.
                /// </summary>
                public Color BottomRightColor { get; set; }

                /// <summary>
                /// The color at the bottom left corner.
                /// </summary>
                public Color BottomLeftColor { get; set; }

                /// <summary>
                /// Creates a GouraudRect with default values.
                /// </summary>
                public GouraudRect() : base() {
                    this.BlendMode = BlendingMode.Alpha;
                    this.TopLeftColor = Color.Black;
                    this.TopRightColor = Color.Black;
                    this.BottomRightColor = Color.Black;
                    this.BottomLeftColor = Color.Black;
                }

                internal GouraudRect(BinaryReaderEx br, DRBVersion version) : base(br, version) {
                    this.BlendMode = br.ReadEnum8<BlendingMode>();
                    _ = br.AssertInt16(0);
                    _ = br.AssertByte(0);
                    this.TopLeftColor = ReadColor(br);
                    this.TopRightColor = ReadColor(br);
                    this.BottomRightColor = ReadColor(br);
                    this.BottomLeftColor = ReadColor(br);
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets) {
                    bw.WriteByte((byte)this.BlendMode);
                    bw.WriteInt16(0);
                    bw.WriteByte(0);
                    WriteColor(bw, this.TopLeftColor);
                    WriteColor(bw, this.TopRightColor);
                    WriteColor(bw, this.BottomRightColor);
                    WriteColor(bw, this.BottomLeftColor);
                }
            }

            /// <summary>
            /// Displays a texture region with color interpolated between each corner.
            /// </summary>
            public class GouraudSprite : SpriteBase {
                internal override ShapeType Type => ShapeType.GouraudSprite;

                /// <summary>
                /// The color at the top left corner.
                /// </summary>
                public Color TopLeftColor { get; set; }

                /// <summary>
                /// The color at the top right corner.
                /// </summary>
                public Color TopRightColor { get; set; }

                /// <summary>
                /// The color at the bottom right corner.
                /// </summary>
                public Color BottomRightColor { get; set; }

                /// <summary>
                /// The color at the bottom left corner.
                /// </summary>
                public Color BottomLeftColor { get; set; }

                /// <summary>
                /// Creates a GouraudSprite with default values.
                /// </summary>
                public GouraudSprite() : base() {
                    this.TopLeftColor = Color.White;
                    this.TopRightColor = Color.White;
                    this.BottomRightColor = Color.White;
                    this.BottomLeftColor = Color.White;
                }

                internal GouraudSprite(BinaryReaderEx br, DRBVersion version) : base(br, version) {
                    this.TopLeftColor = ReadColor(br);
                    this.TopRightColor = ReadColor(br);
                    this.BottomRightColor = ReadColor(br);
                    this.BottomLeftColor = ReadColor(br);
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets) {
                    base.WriteSpecific(bw, stringOffsets);
                    WriteColor(bw, this.TopLeftColor);
                    WriteColor(bw, this.TopRightColor);
                    WriteColor(bw, this.BottomRightColor);
                    WriteColor(bw, this.BottomLeftColor);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Mask : Shape {
                internal override ShapeType Type => ShapeType.Mask;

                /// <summary>
                /// Unknown.
                /// </summary>
                public string DlgoName { get; set; }

                /// <summary>
                /// Unknown; always 1.
                /// </summary>
                public byte Unk04 { get; set; }

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public int Unk05 { get; set; }

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public int Unk09 { get; set; }

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public short Unk0D { get; set; }

                /// <summary>
                /// Creates a Mask with default values.
                /// </summary>
                public Mask() : base() => this.Unk04 = 1;

                internal Mask(BinaryReaderEx br, DRBVersion version, Dictionary<int, string> strings) : base(br, version) {
                    this.DlgoName = strings[br.ReadInt32()];
                    this.Unk04 = br.ReadByte();
                    this.Unk05 = br.ReadInt32();
                    this.Unk09 = br.ReadInt32();
                    this.Unk0D = br.ReadInt16();
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets) {
                    bw.WriteInt32(stringOffsets[this.DlgoName]);
                    bw.WriteByte(this.Unk04);
                    bw.WriteInt32(this.Unk05);
                    bw.WriteInt32(this.Unk09);
                    bw.WriteInt16(this.Unk0D);
                }
            }

            /// <summary>
            /// A rectangular frame with a solid color.
            /// </summary>
            public class MonoFrame : Shape {
                internal override ShapeType Type => ShapeType.MonoFrame;

                /// <summary>
                /// Color blending mode.
                /// </summary>
                public BlendingMode BlendMode { get; set; }

                /// <summary>
                /// Thickness of the border.
                /// </summary>
                public byte Thickness { get; set; }

                /// <summary>
                /// Index of a color in the menu color param, or 0 to use the CustomColor.
                /// </summary>
                public int PaletteColor { get; set; }

                /// <summary>
                /// Custom color used when PaletteColor is 0.
                /// </summary>
                public Color CustomColor { get; set; }

                /// <summary>
                /// Creates a MonoFrame with default values.
                /// </summary>
                public MonoFrame() : base() {
                    this.BlendMode = BlendingMode.Alpha;
                    this.Thickness = 1;
                    this.CustomColor = Color.Black;
                }

                internal MonoFrame(BinaryReaderEx br, DRBVersion version) : base(br, version) {
                    this.BlendMode = br.ReadEnum8<BlendingMode>();
                    _ = br.AssertInt16(0);
                    this.Thickness = br.ReadByte();
                    this.PaletteColor = br.ReadInt32();
                    this.CustomColor = ReadColor(br);
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets) {
                    bw.WriteByte((byte)this.BlendMode);
                    bw.WriteInt16(0);
                    bw.WriteByte(this.Thickness);
                    bw.WriteInt32(this.PaletteColor);
                    WriteColor(bw, this.CustomColor);
                }
            }

            /// <summary>
            /// A rectangle with a solid color.
            /// </summary>
            public class MonoRect : Shape {
                internal override ShapeType Type => ShapeType.MonoRect;

                /// <summary>
                /// Color blending mode.
                /// </summary>
                public BlendingMode BlendMode { get; set; }

                /// <summary>
                /// Index of a color in the menu color param, or 0 to use the CustomColor.
                /// </summary>
                public int PaletteColor { get; set; }

                /// <summary>
                /// Custom color used when PaletteColor is 0.
                /// </summary>
                public Color CustomColor { get; set; }

                /// <summary>
                /// Creates a MonoRect with default values.
                /// </summary>
                public MonoRect() : base() {
                    this.BlendMode = BlendingMode.Alpha;
                    this.CustomColor = Color.Black;
                }

                internal MonoRect(BinaryReaderEx br, DRBVersion version) : base(br, version) {
                    this.BlendMode = br.ReadEnum8<BlendingMode>();
                    _ = br.AssertInt16(0);
                    _ = br.AssertByte(0);
                    this.PaletteColor = br.ReadInt32();
                    this.CustomColor = ReadColor(br);
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets) {
                    bw.WriteByte((byte)this.BlendMode);
                    bw.WriteInt16(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(this.PaletteColor);
                    WriteColor(bw, this.CustomColor);
                }
            }

            /// <summary>
            /// An invisible element used to mark a position.
            /// </summary>
            public class Null : Shape {
                internal override ShapeType Type => ShapeType.Null;

                /// <summary>
                /// Creates a Null with default values.
                /// </summary>
                public Null() : base() { }

                internal Null(BinaryReaderEx br, DRBVersion version) : base(br, version) { }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets) { }
            }

            /// <summary>
            /// A scrolling text field.
            /// </summary>
            public class ScrollText : TextBase {
                internal override ShapeType Type => ShapeType.ScrollText;

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkX00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkX02 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkX04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkX06 { get; set; }

                /// <summary>
                /// Unknown; always 15 or 100.
                /// </summary>
                public short ScrollSpeed { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkX0A { get; set; }

                /// <summary>
                /// Creates a ScrollText with default values.
                /// </summary>
                public ScrollText() : base() => this.ScrollSpeed = 15;

                internal ScrollText(BinaryReaderEx br, DRBVersion version, Dictionary<int, string> strings) : base(br, version, strings) { }

                internal override void ReadSubtype(BinaryReaderEx br) {
                    this.UnkX00 = br.ReadInt16();
                    this.UnkX02 = br.ReadInt16();
                    this.UnkX04 = br.ReadInt16();
                    this.UnkX06 = br.ReadInt16();
                    this.ScrollSpeed = br.ReadInt16();
                    this.UnkX0A = br.ReadInt16();
                }

                internal override void WriteSubtype(BinaryWriterEx bw) {
                    bw.WriteInt16(this.UnkX00);
                    bw.WriteInt16(this.UnkX02);
                    bw.WriteInt16(this.UnkX04);
                    bw.WriteInt16(this.UnkX06);
                    bw.WriteInt16(this.ScrollSpeed);
                    bw.WriteInt16(this.UnkX0A);
                }
            }

            /// <summary>
            /// Displays a texture region with color tinting.
            /// </summary>
            public class Sprite : SpriteBase {
                internal override ShapeType Type => ShapeType.Sprite;

                /// <summary>
                /// Index of a color in the menu color param, or 0 to use the CustomColor.
                /// </summary>
                public int PaletteColor { get; set; }

                /// <summary>
                /// Custom color used when PaletteColor is 0.
                /// </summary>
                public Color CustomColor { get; set; }

                /// <summary>
                /// Creates a Sprite with default values.
                /// </summary>
                public Sprite() : base() => this.CustomColor = Color.White;

                internal Sprite(BinaryReaderEx br, DRBVersion version) : base(br, version) {
                    this.PaletteColor = br.ReadInt32();
                    this.CustomColor = ReadColor(br);
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets) {
                    base.WriteSpecific(bw, stringOffsets);
                    bw.WriteInt32(this.PaletteColor);
                    WriteColor(bw, this.CustomColor);
                }
            }

            /// <summary>
            /// A fixed text field.
            /// </summary>
            public class Text : TextBase {
                internal override ShapeType Type => ShapeType.Text;

                /// <summary>
                /// Creates a Text with default values.
                /// </summary>
                public Text() : base() { }

                internal Text(BinaryReaderEx br, DRBVersion version, Dictionary<int, string> strings) : base(br, version, strings) { }
            }
        }
    }
}
