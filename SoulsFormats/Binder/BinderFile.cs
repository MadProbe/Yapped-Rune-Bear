using SoulsFormats.Formats;
using static SoulsFormats.Binder.Binder;

namespace SoulsFormats.Binder {
    /// <summary>
    /// A generic file in a BND3, BND4, BXF3, or BXF4 container.
    /// </summary>
    public class BinderFile {
        /// <summary>
        /// If compressed, which type of compression to use.
        /// </summary>
        public DCX.Type CompressionType = DCX.Type.Zlib;

        /// <summary>
        /// Flags indicating compression, and possibly other things.
        /// </summary>
        public FileFlags Flags;

        /// <summary>
        /// ID of the file, or -1 for none.
        /// </summary>
        public int ID;

        /// <summary>
        /// Name of the file, or null for none.
        /// </summary>
        public string Name;

        /// <summary>
        /// Raw file data.
        /// </summary>
        public byte[] Bytes;

        /// <summary>
        /// Creates a new file with 0 bytes and no ID or name.
        /// </summary>
        public BinderFile() : this(FileFlags.Flag1, -1, null, Array.Empty<byte>()) { }

        /// <summary>
        /// Creates a new file with no ID or name.
        /// </summary>
        public BinderFile(FileFlags flags, byte[] bytes) : this(flags, -1, null, bytes) { }

        /// <summary>
        /// Creates a new file with no name.
        /// </summary>
        public BinderFile(FileFlags flags, int id, byte[] bytes) : this(flags, id, null, bytes) { }

        /// <summary>
        /// Creates a new file with no ID.
        /// </summary>
        public BinderFile(FileFlags flags, string name, byte[] bytes) : this(flags, -1, name, bytes) { }

        /// <summary>
        /// Creates a new file.
        /// </summary>
        public BinderFile(FileFlags flags, int id, string name, byte[] bytes) {
            this.Flags = flags;
            this.ID = id;
            this.Name = name;
            this.Bytes = bytes;
        }

        /// <summary>
        /// Returns the file flags, ID, name, and byte length as a string.
        /// </summary>
        public override string ToString() => $"Flags: 0x{(byte)this.Flags:X2} | ID: {this.ID} | Name: {this.Name} | Length: {this.Bytes.Length}";
    }
}
