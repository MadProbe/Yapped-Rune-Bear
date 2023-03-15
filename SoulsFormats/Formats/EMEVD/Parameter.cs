using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class EMEVD {
        /// <summary>
        /// An instruction given to the game which tells it to subsitute arg bytes in a particular instruction with ones defined here.
        /// </summary>
        public class Parameter {
            /// <summary>
            /// The index into the event's instruction list for which to apply the parameter subsitution.
            /// </summary>
            public long InstructionIndex { get; set; }

            /// <summary>
            /// Index of the starting byte in the instruction's arguments.
            /// </summary>
            public long TargetStartByte { get; set; }

            /// <summary>
            /// Index of the starting byte in the event's parameters.
            /// </summary>
            public long SourceStartByte { get; set; }

            /// <summary>
            /// Amount of bytes to copy to the target instruction's arguments.
            /// </summary>
            public int ByteCount { get; set; }

            /// <summary>
            /// Unknown; always 0 before Sekiro, generally counts up from 1 for each parameter in an event. Does not seem to have any effect in-game.
            /// </summary>
            public int UnkID { get; set; }

            /// <summary>
            /// Creates a new Parameter with default values.
            /// </summary>
            public Parameter() { }

            /// <summary>
            /// Creates a Parameter with the specified values.
            /// </summary>
            public Parameter(long instrIndex, long targetStartByte, long srcStartByte, int byteCount) {
                this.InstructionIndex = instrIndex;
                this.TargetStartByte = targetStartByte;
                this.SourceStartByte = srcStartByte;
                this.ByteCount = byteCount;
            }

            internal Parameter(BinaryReaderEx br, Game format) {
                this.InstructionIndex = br.ReadVarint();
                this.TargetStartByte = br.ReadVarint();
                this.SourceStartByte = br.ReadVarint();
                this.ByteCount = br.ReadInt32();
                this.UnkID = br.ReadInt32();
            }

            internal void Write(BinaryWriterEx bw, Game format) {
                bw.WriteVarint(this.InstructionIndex);
                bw.WriteVarint(this.TargetStartByte);
                bw.WriteVarint(this.SourceStartByte);
                bw.WriteInt32(this.ByteCount);
                bw.WriteInt32(this.UnkID);
            }
        }
    }
}
