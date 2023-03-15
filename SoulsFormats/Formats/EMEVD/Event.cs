using System.Collections.Generic;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class EMEVD {
        /// <summary>
        /// An event containing instructions to be executed.
        /// </summary>
        public class Event {
            /// <summary>
            /// The ID of the event.
            /// </summary>
            public long ID { get; set; }

            /// <summary>
            /// Instructions to execute for this event.
            /// </summary>
            public List<Instruction> Instructions { get; set; }

            /// <summary>
            /// Parameters to be passed to this event.
            /// </summary>
            public List<Parameter> Parameters { get; set; }

            /// <summary>
            /// Behavior of this event when resting.
            /// </summary>
            public RestBehaviorType RestBehavior { get; set; }

            /// <summary>
            /// Optional name for the event, stored separately in an EMELD file.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Creates an empty Event with the given values.
            /// </summary>
            public Event(long id = 0, RestBehaviorType restBehavior = RestBehaviorType.Default) {
                this.ID = id;
                this.Instructions = new List<Instruction>();
                this.Parameters = new List<Parameter>();
                this.RestBehavior = restBehavior;
            }

            internal Event(BinaryReaderEx br, Game format, Offsets offsets) {
                this.ID = br.ReadVarint();
                long instructionCount = br.ReadVarint();
                long instructionsOffset = br.ReadVarint();
                long parameterCount = br.ReadVarint();
                long parametersOffset = br.ReadVarint();
                this.RestBehavior = br.ReadEnum32<RestBehaviorType>();
                _ = br.AssertInt32(0);

                this.Instructions = new List<Instruction>((int)instructionCount);
                if (instructionCount > 0) {
                    br.StepIn(offsets.Instructions + instructionsOffset);
                    {
                        for (int i = 0; i < instructionCount; i++) {
                            this.Instructions.Add(new Instruction(br, format, offsets));
                        }
                    }
                    br.StepOut();
                }

                this.Parameters = new List<Parameter>((int)parameterCount);
                if (parameterCount > 0) {
                    br.StepIn(offsets.Parameters + parametersOffset);
                    {
                        for (int i = 0; i < parameterCount; i++) {
                            this.Parameters.Add(new Parameter(br, format));
                        }
                    }
                    br.StepOut();
                }
            }

            internal void Write(BinaryWriterEx bw, Game format, int eventIndex) {
                bw.WriteVarint(this.ID);
                bw.WriteVarint(this.Instructions.Count);
                bw.ReserveVarint($"Event{eventIndex}InstrsOffset");
                bw.WriteVarint(this.Parameters.Count);
                if (format < Game.Bloodborne) {
                    bw.ReserveInt32($"Event{eventIndex}ParamsOffset");
                } else if (format < Game.DarkSouls3) {
                    bw.ReserveInt32($"Event{eventIndex}ParamsOffset");
                    bw.WriteInt32(0);
                } else {
                    bw.ReserveInt64($"Event{eventIndex}ParamsOffset");
                }
                bw.WriteUInt32((uint)this.RestBehavior);
                bw.WriteInt32(0);
            }

            internal void WriteInstructions(BinaryWriterEx bw, Game format, Offsets offsets, int eventIndex) {
                long instrsOffset = this.Instructions.Count > 0 ? bw.Position - offsets.Instructions : -1;
                bw.FillVarint($"Event{eventIndex}InstrsOffset", instrsOffset);

                for (int i = 0; i < this.Instructions.Count; i++) {
                    this.Instructions[i].Write(bw, format, eventIndex, i);
                }
            }

            internal void WriteParameters(BinaryWriterEx bw, Game format, Offsets offsets, int eventIndex) {
                long paramsOffset = this.Parameters.Count > 0 ? bw.Position - offsets.Parameters : -1;
                if (format < Game.DarkSouls3) {
                    bw.FillInt32($"Event{eventIndex}ParamsOffset", (int)paramsOffset);
                } else {
                    bw.FillInt64($"Event{eventIndex}ParamsOffset", paramsOffset);
                }

                for (int i = 0; i < this.Parameters.Count; i++) {
                    this.Parameters[i].Write(bw, format);
                }
            }

            /// <summary>
            /// Defines the behavior of the event when resting.
            /// </summary>
            public enum RestBehaviorType : uint {
                /// <summary>
                /// No effect upon resting.
                /// </summary>
                Default = 0,

                /// <summary>
                /// Event restarts upon resting.
                /// </summary>
                Restart = 1,

                /// <summary>
                /// Event is terminated upon resting.
                /// </summary>
                End = 2,
            }
        }
    }
}
