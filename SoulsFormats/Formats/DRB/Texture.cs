﻿using System.Collections.Generic;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class DRB {
        /// <summary>
        /// A texture available to be referenced by UI elements.
        /// </summary>
        public class Texture {
            /// <summary>
            /// A friendly name for the texture.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The network path to the texture.
            /// </summary>
            public string Path { get; set; }

            /// <summary>
            /// Creates a Texture with default values.
            /// </summary>
            public Texture() {
                this.Name = "";
                this.Path = "";
            }

            /// <summary>
            /// Creates a Texture with the given name and path.
            /// </summary>
            public Texture(string name, string path) {
                this.Name = name;
                this.Path = path;
            }

            internal Texture(BinaryReaderEx br, Dictionary<int, string> strings) {
                int nameOffset = br.ReadInt32();
                int pathOffset = br.ReadInt32();
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);

                this.Name = strings[nameOffset];
                this.Path = strings[pathOffset];
            }

            internal void Write(BinaryWriterEx bw, Dictionary<string, int> stringOffsets) {
                bw.WriteInt32(stringOffsets[this.Name]);
                bw.WriteInt32(stringOffsets[this.Path]);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
            }

            /// <summary>
            /// Returns the name and path of the texture.
            /// </summary>
            public override string ToString() => $"{this.Name} - {this.Path}";
        }
    }
}
