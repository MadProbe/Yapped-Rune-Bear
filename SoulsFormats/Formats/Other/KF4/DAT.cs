using System.Collections.Generic;
using SoulsFormats.Util;

namespace SoulsFormats.Formats.Other.KF4 {
    /// <summary>
    /// Specifically KF4.DAT, the main archive.
    /// </summary>
    public class DAT : SoulsFile<DAT> {
        /// <summary>
        /// Files in the archive.
        /// </summary>
        public List<File> Files;

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            br.BigEndian = false;

            _ = br.AssertByte(0x00);
            _ = br.AssertByte(0x80);
            _ = br.AssertByte(0x04);
            _ = br.AssertByte(0x1E);

            int fileCount = br.ReadInt32();

            for (int i = 0; i < 0x38; i++) {
                _ = br.AssertByte(0);
            }

            this.Files = new List<File>(fileCount);
            for (int i = 0; i < fileCount; i++) {
                this.Files.Add(new File(br));
            }
        }

        /// <summary>
        /// A file in a DAT archive.
        /// </summary>
        public class File {
            /// <summary>
            /// The path of the file.
            /// </summary>
            public string Name;

            /// <summary>
            /// The file's data.
            /// </summary>
            public byte[] Bytes;

            internal File(BinaryReaderEx br) {
                this.Name = br.ReadFixStr(0x34);
                int size = br.ReadInt32();
                _ = br.ReadInt32();
                int offset = br.ReadInt32();

                this.Bytes = br.GetBytes(offset, size);
            }
        }
    }
}
