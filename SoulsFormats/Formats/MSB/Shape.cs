using System;
using System.Collections.Generic;
using SoulsFormats.Formats.MSB;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MSB {
        internal enum ShapeType : uint {
            None = 0xFFFFFFFF,
            Point = 0,
            Circle = 1,
            Sphere = 2,
            Cylinder = 3,
            Rect = 4,
            Box = 5,
            Composite = 6,
        }

        /// <summary>
        /// The shape of a map region.
        /// </summary>
        public abstract class Shape {
            internal abstract ShapeType Type { get; }
            internal abstract bool HasShapeData { get; }

            /// <summary>
            /// Creates a deep copy of the Shape.
            /// </summary>
            public abstract Shape DeepCopy();

            internal virtual void ReadShapeData(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(ReadShapeData)}.");

            internal virtual void WriteShapeData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(WriteShapeData)}.");

            internal static Shape Create(ShapeType type) => type switch {
                ShapeType.Point => new Point(),
                ShapeType.Circle => new Circle(),
                ShapeType.Sphere => new Sphere(),
                ShapeType.Cylinder => new Cylinder(),
                ShapeType.Rect => new Rect(),
                ShapeType.Box => new Box(),
                ShapeType.Composite => new Composite(),
                _ => throw new NotImplementedException($"Unimplemented shape type: {type}"),
            };

            /// <summary>
            /// A single point.
            /// </summary>
            public class Point : Shape {
                internal override ShapeType Type => ShapeType.Point;
                internal override bool HasShapeData => false;

                /// <summary>
                /// Creates a deep copy of the Point.
                /// </summary>
                public override Shape DeepCopy() => new Point();
            }

            /// <summary>
            /// A flat circle.
            /// </summary>
            public class Circle : Shape {
                internal override ShapeType Type => ShapeType.Circle;
                internal override bool HasShapeData => true;

                /// <summary>
                /// The radius of the circle.
                /// </summary>
                public float Radius { get; set; }

                /// <summary>
                /// Creates a Circle with default dimensions.
                /// </summary>
                public Circle() : this(1) { }

                /// <summary>
                /// Creates a Circle with the given dimensions.
                /// </summary>
                public Circle(float radius) => this.Radius = radius;

                /// <summary>
                /// Creates a deep copy of the Circle.
                /// </summary>
                public override Shape DeepCopy() => new Circle(this.Radius);

                internal override void ReadShapeData(BinaryReaderEx br) => this.Radius = br.ReadSingle();

                internal override void WriteShapeData(BinaryWriterEx bw) => bw.WriteSingle(this.Radius);
            }

            /// <summary>
            /// A volumetric sphere.
            /// </summary>
            public class Sphere : Shape {
                internal override ShapeType Type => ShapeType.Sphere;
                internal override bool HasShapeData => true;

                /// <summary>
                /// The radius of the sphere.
                /// </summary>
                public float Radius { get; set; }

                /// <summary>
                /// Creates a Sphere with default dimensions.
                /// </summary>
                public Sphere() : this(1) { }

                /// <summary>
                /// Creates a Sphere with the given dimensions.
                /// </summary>
                public Sphere(float radius) => this.Radius = radius;

                /// <summary>
                /// Creates a deep copy of the Sphere.
                /// </summary>
                public override Shape DeepCopy() => new Sphere(this.Radius);

                internal override void ReadShapeData(BinaryReaderEx br) => this.Radius = br.ReadSingle();

                internal override void WriteShapeData(BinaryWriterEx bw) => bw.WriteSingle(this.Radius);
            }

            /// <summary>
            /// A volumetric cylinder.
            /// </summary>
            public class Cylinder : Shape {
                internal override ShapeType Type => ShapeType.Cylinder;
                internal override bool HasShapeData => true;

                /// <summary>
                /// The radius of the cylinder.
                /// </summary>
                public float Radius { get; set; }

                /// <summary>
                /// The height of the cylinder.
                /// </summary>
                public float Height { get; set; }

                /// <summary>
                /// Creates a Cylinder with default dimensions.
                /// </summary>
                public Cylinder() : this(1, 1) { }

                /// <summary>
                /// Creates a Cylinder with the given dimensions.
                /// </summary>
                public Cylinder(float radius, float height) {
                    this.Radius = radius;
                    this.Height = height;
                }

                /// <summary>
                /// Creates a deep copy of the Cylinder.
                /// </summary>
                public override Shape DeepCopy() => new Cylinder(this.Radius, this.Height);

                internal override void ReadShapeData(BinaryReaderEx br) {
                    this.Radius = br.ReadSingle();
                    this.Height = br.ReadSingle();
                }

                internal override void WriteShapeData(BinaryWriterEx bw) {
                    bw.WriteSingle(this.Radius);
                    bw.WriteSingle(this.Height);
                }
            }

            /// <summary>
            /// A flat rectangle.
            /// </summary>
            public class Rect : Shape {
                internal override ShapeType Type => ShapeType.Rect;
                internal override bool HasShapeData => true;

                /// <summary>
                /// The width of the rectangle.
                /// </summary>
                public float Width { get; set; }

                /// <summary>
                /// The depth of the rectangle.
                /// </summary>
                public float Depth { get; set; }

                /// <summary>
                /// Creates a Rect with default dimensions.
                /// </summary>
                public Rect() : this(1, 1) { }

                /// <summary>
                /// Creates a Rect with the given dimensions.
                /// </summary>
                public Rect(float width, float depth) {
                    this.Width = width;
                    this.Depth = depth;
                }

                /// <summary>
                /// Creates a deep copy of the Rect.
                /// </summary>
                public override Shape DeepCopy() => new Rect(this.Width, this.Depth);

                internal override void ReadShapeData(BinaryReaderEx br) {
                    this.Width = br.ReadSingle();
                    this.Depth = br.ReadSingle();
                }

                internal override void WriteShapeData(BinaryWriterEx bw) {
                    bw.WriteSingle(this.Width);
                    bw.WriteSingle(this.Depth);
                }
            }

            /// <summary>
            /// A rectangular prism.
            /// </summary>
            public class Box : Shape {
                internal override ShapeType Type => ShapeType.Box;
                internal override bool HasShapeData => true;

                /// <summary>
                /// The width of the box.
                /// </summary>
                public float Width { get; set; }

                /// <summary>
                /// The depth of the box.
                /// </summary>
                public float Depth { get; set; }

                /// <summary>
                /// The height of the box.
                /// </summary>
                public float Height { get; set; }

                /// <summary>
                /// Creates a Box with default dimensions.
                /// </summary>
                public Box() : this(1, 1, 1) { }

                /// <summary>
                /// Creates a Box with the given dimensions.
                /// </summary>
                public Box(float width, float depth, float height) {
                    this.Width = width;
                    this.Depth = depth;
                    this.Height = height;
                }

                /// <summary>
                /// Creates a deep copy of the Box.
                /// </summary>
                public override Shape DeepCopy() => new Box(this.Width, this.Depth, this.Height);

                internal override void ReadShapeData(BinaryReaderEx br) {
                    this.Width = br.ReadSingle();
                    this.Depth = br.ReadSingle();
                    this.Height = br.ReadSingle();
                }

                internal override void WriteShapeData(BinaryWriterEx bw) {
                    bw.WriteSingle(this.Width);
                    bw.WriteSingle(this.Depth);
                    bw.WriteSingle(this.Height);
                }
            }

            /// <summary>
            /// A shape composed of references to other regions' shapes.
            /// </summary>
            public class Composite : Shape {
                internal override ShapeType Type => ShapeType.Composite;
                internal override bool HasShapeData => true;

                /// <summary>
                /// Other regions referenced by this shape.
                /// </summary>
                public Child[] Children { get; private set; }

                /// <summary>
                /// Creates a Composite with 8 empty references.
                /// </summary>
                public Composite() {
                    this.Children = new Child[8];
                    for (int i = 0; i < 8; i++) {
                        this.Children[i] = new Child();
                    }
                }

                /// <summary>
                /// Creates a deep copy of the Composite.
                /// </summary>
                public override Shape DeepCopy() {
                    var comp = new Composite();
                    for (int i = 0; i < 8; i++) {
                        comp.Children[i] = this.Children[i].DeepCopy();
                    }

                    return comp;
                }

                internal override void ReadShapeData(BinaryReaderEx br) {
                    this.Children = new Child[8];
                    for (int i = 0; i < 8; i++) {
                        this.Children[i] = new Child(br);
                    }
                }

                internal override void WriteShapeData(BinaryWriterEx bw) {
                    for (int i = 0; i < 8; i++) {
                        this.Children[i].Write(bw);
                    }
                }

                internal void GetNames<T>(List<T> regions) where T : IMsbRegion {
                    foreach (Child child in this.Children) {
                        child.GetNames(regions);
                    }
                }

                internal void GetIndices<T>(List<T> regions) where T : IMsbRegion {
                    foreach (Child child in this.Children) {
                        child.GetIndices(regions);
                    }
                }

                /// <summary>
                /// A reference to another region.
                /// </summary>
                public class Child {
                    /// <summary>
                    /// The name of the child region.
                    /// </summary>
                    public string RegionName { get; set; }
                    private int RegionIndex;

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk04 { get; set; }

                    /// <summary>
                    /// Creates a Child with default values.
                    /// </summary>
                    public Child() { }

                    internal Child(BinaryReaderEx br) {
                        this.RegionIndex = br.ReadInt32();
                        this.Unk04 = br.ReadInt32();
                    }

                    /// <summary>
                    /// Creates a deep copy of the Child.
                    /// </summary>
                    public Child DeepCopy() => new() { RegionName = RegionName, Unk04 = Unk04 };

                    internal void Write(BinaryWriterEx bw) {
                        bw.WriteInt32(this.RegionIndex);
                        bw.WriteInt32(this.Unk04);
                    }

                    internal void GetNames<T>(List<T> regions) where T : IMsbRegion => this.RegionName = FindName(regions, this.RegionIndex);

                    internal void GetIndices<T>(List<T> regions) where T : IMsbRegion => this.RegionIndex = FindIndex(regions, this.RegionName);
                }
            }
        }
    }
}
