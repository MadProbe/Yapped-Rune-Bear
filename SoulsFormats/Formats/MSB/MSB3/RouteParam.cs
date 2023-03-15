using System.Collections.Generic;
using System.IO;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MSB3 {
        /// <summary>
        /// A section containing routes. Purpose unknown.
        /// </summary>
        private class RouteParam : Param<Route> {
            internal override int Version => 3;
            internal override string Type => "ROUTE_PARAM_ST";

            /// <summary>
            /// The routes in this section.
            /// </summary>
            public List<Route> Routes { get; set; }

            /// <summary>
            /// Creates a new RouteParam with no routes.
            /// </summary>
            public RouteParam() => this.Routes = new List<Route>();

            /// <summary>
            /// Returns every route in the order they will be written.
            /// </summary>
            public override List<Route> GetEntries() => this.Routes;

            internal override Route ReadEntry(BinaryReaderEx br) => this.Routes.EchoAdd(new Route(br));
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        public class Route : NamedEntry {
            /// <summary>
            /// The name of this route.
            /// </summary>
            public override string Name { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk08 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk0C { get; set; }

            /// <summary>
            /// Creates a new Route with default values.
            /// </summary>
            public Route() => this.Name = "XX-XX";

            /// <summary>
            /// Creates a deep copy of the route.
            /// </summary>
            public Route DeepCopy() => (Route)this.MemberwiseClone();

            internal Route(BinaryReaderEx br) {
                long start = br.Position;

                long nameOffset = br.ReadInt64();
                this.Unk08 = br.ReadInt32();
                this.Unk0C = br.ReadInt32();
                _ = br.AssertInt32(4); // Type
                _ = br.ReadInt32(); // ID
                br.AssertPattern(0x68, 0x00);

                if (nameOffset == 0) {
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0.");
                }

                br.Position = start + nameOffset;
                this.Name = br.ReadUTF16();
            }

            internal override void Write(BinaryWriterEx bw, int id) {
                long start = bw.Position;

                bw.ReserveInt64("NameOffset");
                bw.WriteInt32(this.Unk08);
                bw.WriteInt32(this.Unk0C);
                bw.WriteInt32(4);
                bw.WriteInt32(id);
                bw.WritePattern(0x68, 0x00);

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(this.Name, true);
                bw.Pad(8);
            }

            /// <summary>
            /// Returns the name and values of this route.
            /// </summary>
            public override string ToString() => $"\"{this.Name}\" {this.Unk08} {this.Unk0C}";
        }
    }
}
