using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using SoulsFormats.Formats.MSB;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MSB1 {
        /// <summary>
        /// A collection of points and trigger volumes used by scripts and events.
        /// </summary>
        public class PointParam : Param<Region>, IMsbParam<IMsbRegion> {
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
            /// Describes the physical shape of the region.
            /// </summary>
            public MSB.Shape Shape {
                get => this._shape;
                set {
                    if (value is MSB.Shape.Composite) {
                        throw new ArgumentException("Dark Souls 1 does not support composite shapes.");
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
                int nameOffset = br.ReadInt32();
                _ = br.AssertInt32(0);
                _ = br.ReadInt32(); // ID
                MSB.ShapeType shapeType = br.ReadEnum32<MSB.ShapeType>();
                this.Position = br.ReadVector3();
                this.Rotation = br.ReadVector3();
                int unkOffsetA = br.ReadInt32();
                int unkOffsetB = br.ReadInt32();
                int shapeDataOffset = br.ReadInt32();
                int entityDataOffset = br.ReadInt32();
                _ = br.AssertInt32(0);

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
                this.Name = br.ReadShiftJIS();

                br.Position = start + unkOffsetA;
                _ = br.AssertInt32(0);

                br.Position = start + unkOffsetB;
                _ = br.AssertInt32(0);

                if (this.Shape.HasShapeData) {
                    br.Position = start + shapeDataOffset;
                    this.Shape.ReadShapeData(br);
                }

                br.Position = start + entityDataOffset;
                this.EntityID = br.ReadInt32();
            }

            internal override void Write(BinaryWriterEx bw, int id) {
                long start = bw.Position;
                bw.ReserveInt32("NameOffset");
                bw.WriteInt32(0);
                bw.WriteInt32(id);
                bw.WriteUInt32((uint)this.Shape.Type);
                bw.WriteVector3(this.Position);
                bw.WriteVector3(this.Rotation);
                bw.ReserveInt32("UnkOffsetA");
                bw.ReserveInt32("UnkOffsetB");
                bw.ReserveInt32("ShapeDataOffset");
                bw.ReserveInt32("EntityDataOffset");
                bw.WriteInt32(0);

                bw.FillInt32("NameOffset", (int)(bw.Position - start));
                bw.WriteShiftJIS(MSB.ReambiguateName(this.Name), true);
                bw.Pad(4);

                bw.FillInt32("UnkOffsetA", (int)(bw.Position - start));
                bw.WriteInt32(0);

                bw.FillInt32("UnkOffsetB", (int)(bw.Position - start));
                bw.WriteInt32(0);

                if (this.Shape.HasShapeData) {
                    bw.FillInt32("ShapeDataOffset", (int)(bw.Position - start));
                    this.Shape.WriteShapeData(bw);
                } else {
                    bw.FillInt32("ShapeDataOffset", 0);
                }

                bw.FillInt32("EntityDataOffset", (int)(bw.Position - start));
                bw.WriteInt32(this.EntityID);
            }
        }
    }
}
