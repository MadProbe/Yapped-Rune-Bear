using System.Collections.Generic;
using SoulsFormats.Util;

namespace SoulsFormats.Formats.Other.Kuon {
    /// <summary>
    /// Kuon's main archive ALL/ELL. Extension: .bnd
    /// </summary>
    public class DVDBND0 : SoulsFile<DVDBND0> {
        /// <summary>
        /// Files in this BND.
        /// </summary>
        public List<File> Files;

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            br.BigEndian = false;

            _ = br.AssertASCII("BND\0");
            _ = br.AssertInt32(0xCA);
            _ = br.ReadInt32();
            int fileCount = br.ReadInt32();

            this.Files = new List<File>(fileCount);
            for (int i = 0; i < fileCount; i++) {
                this.Files.Add(new File(br));
            }
        }

        /// <summary>
        /// A file in a DVDBND0.
        /// </summary>
        public class File {
            /// <summary>
            /// ID of this file.
            /// </summary>
            public int ID;

            /// <summary>
            /// Name of this file.
            /// </summary>
            public string Name;

            /// <summary>
            /// File data.
            /// </summary>
            public byte[] Bytes;

            internal File(BinaryReaderEx br) {
                this.ID = br.ReadInt32();
                int dataOffset = br.ReadInt32();
                int dataSize = br.ReadInt32();
                int nameOffset = br.ReadInt32();

                this.Name = br.GetShiftJIS(nameOffset);
                this.Bytes = br.GetBytes(dataOffset, dataSize);
            }
        }
    }
}
