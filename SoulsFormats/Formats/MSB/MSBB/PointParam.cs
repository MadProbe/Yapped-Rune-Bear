using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using SoulsFormats.Formats.MSB;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MSBB {
        /// <summary>
        /// A collection of points and trigger volumes used by scripts and events.
        /// </summary>
        public class PointParam : Param<Region>, IMsbParam<IMsbRegion> {
            internal override int Version => 3;
            internal override string Name => "POINT_PARAM_ST";

            /// <summary>
            /// All regions in the map.
            /// </summary>
            public List<Region> Regions { get; set; }

            /// <summary>
            /// Creates an empty PointParam.
            /// </summary>
            public PointParam() : base() => this.Regions = new List<Region>();

            /// <summary>
            /// Adds a region to the list; returns the region.
            /// </summary>
            public Region Add(Region region) {
                this.Regions.Add(region);
                return region;
            }
            IMsbRegion IMsbParam<IMsbRegion>.Add(IMsbRegion item) => this.Add((Region)item);

            /// <summary>
            /// Returns the list of regions.
            /// </summary>
            public override List<Region> GetEntries() => this.Regions;
            IReadOnlyList<IMsbRegion> IMsbParam<IMsbRegion>.GetEntries() => this.GetEntries();

            internal override Region ReadEntry(BinaryReaderEx br) => this.Regions.EchoAdd(new Region(br));
        }

        /// <summary>
        /// A point or volume used by scripts or events.
        /// </summary>
        public class Region : Entry, IMsbRegion {
            /// <summary>
            /// The name of the region.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Describes the physical shape of the region.
            /// </summary>
            public MSB.Shape Shape {
                get => this._shape;
                set {
                    if (value is MSB.Shape.Composite) {
                        throw new ArgumentException("Bloodborne does not support composite shapes.");
                    }

                    this._shape = value;
                }
            }
            private MSB.Shape _shape;

            /// <summary>
            /// Location of the region.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Rotation of the region, in degrees.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// Identifies the region in external files.
            /// </summary>
            public int EntityID { get; set; }

            /// <summary>
            /// Creates a Region with default values.
            /// </summary>
            public Region() {
                this.Name = "Region";
                this.Shape = new MSB.Shape.Point();
                this.EntityID = -1;
            }

            /// <summary>
            /// Creates a deep copy of the region.
            /// </summary>
            public Region DeepCopy() {
                var region = (Region)this.MemberwiseClone();
                region.Shape = this.Shape.DeepCopy();
                return region;
            }
            IMsbRegion IMsbRegion.DeepCopy() => this.DeepCopy();

            internal Region(BinaryReaderEx br) {
                long start = br.Position;
                long nameOffset = br.ReadInt64();
                _ = br.AssertInt32(0);
                _ = br.ReadInt32(); // ID
                MSB.ShapeType shapeType = br.ReadEnum32<MSB.ShapeType>();
                this.Position = br.ReadVector3();
                this.Rotation = br.ReadVector3();
                _ = br.AssertInt32(0);
                long unkOffsetA = br.ReadInt64();
                long unkOffsetB = br.ReadInt64();
                long shapeDataOffset = br.ReadInt64();
                long entityDataOffset = br.ReadInt64();

                this.Shape = MSB.Shape.Create(shapeType);

                if (nameOffset == 0) {
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {this.GetType()}.");
                }

                if (unkOffsetA == 0) {
                    throw new InvalidDataException($"{nameof(unkOffsetA)} must not be 0 in type {this.GetType()}.");
                }

                if (unkOffsetB == 0) {
                    throw new InvalidDataException($"{nameof(unkOffsetB)} must not be 0 in type {this.GetType()}.");
                }

                if (this.Shape.HasShapeData ^ shapeDataOffset != 0) {
                    throw new InvalidDataException($"Unexpected {nameof(shapeDataOffset)} 0x{shapeDataOffset:X} in type {this.GetType()}.");
                }

                br.Position = start + nameOffset;
                this.Name = br.ReadUTF16();

                br.Position = start + unkOffsetA;
                _ = br.AssertInt16(0);

                br.Position = start + unkOffsetB;
                _ = br.AssertInt16(0);

                if (this.Shape.HasShapeData) {
                    br.Position = start + shapeDataOffset;
                    this.Shape.ReadShapeData(br);
                }

                br.Position = start + entityDataOffset;
                this.EntityID = br.ReadInt32();
            }

            internal override void Write(BinaryWriterEx bw, int id) {
                long start = bw.Position;
                bw.ReserveInt64("NameOffset");
                bw.WriteInt32(0);
                bw.WriteInt32(id);
                bw.WriteUInt32((uint)this.Shape.Type);
                bw.WriteVector3(this.Position);
                bw.WriteVector3(this.Rotation);
                bw.WriteInt32(0);
                bw.ReserveInt64("UnkOffsetA");
                bw.ReserveInt64("UnkOffsetB");
                bw.ReserveInt64("ShapeDataOffset");
                bw.ReserveInt64("EntityDataOffset");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(MSB.ReambiguateName(this.Name), true);
                bw.Pad(4);

                bw.FillInt64("UnkOffsetA", bw.Position - start);
                bw.WriteInt16(0);
                bw.Pad(4);

                bw.FillInt64("UnkOffsetB", bw.Position - start);
                bw.WriteInt16(0);
                bw.Pad(8);

                if (this.Shape.HasShapeData) {
                    bw.FillInt64("ShapeDataOffset", bw.Position - start);
                    this.Shape.WriteShapeData(bw);
                } else {
                    bw.FillInt64("ShapeDataOffset", 0);
                }

                bw.FillInt64("EntityDataOffset", bw.Position - start);
                bw.WriteInt32(this.EntityID);
                bw.Pad(8);
            }
        }
    }
}
