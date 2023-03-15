using System;
using System.Collections.Generic;
using SoulsFormats.Util;

namespace SoulsFormats.Formats {
    /// <summary>
    /// A collection of controller rumble effects used in all games. Extension: .rmb
    /// </summary>
    public class RMB : SoulsFile<RMB> {
        /// <summary>
        /// Whether the file is big-endian. True for PS3 and X360, false otherwise.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// Available effects; always 256 in vanilla files.
        /// </summary>
        public List<Rumble> Rumbles { get; private set; }

        /// <summary>
        /// Creates an empty RMB.
        /// </summary>
        public RMB() => this.Rumbles = new List<Rumble>();

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            br.BigEndian = false;
            br.BigEndian = this.BigEndian = br.GetInt32(4) == 0x10000000;

            short rumbleCount = br.ReadInt16();
            _ = br.AssertInt16(0);
            _ = br.AssertInt32(0x10);
            _ = br.AssertInt32(0);
            _ = br.AssertInt32(0);

            this.Rumbles = new List<Rumble>(rumbleCount);
            for (int i = 0; i < rumbleCount; i++) {
                this.Rumbles.Add(new Rumble(br));
            }
        }

        /// <summary>
        /// Verifies that there are no null references.
        /// </summary>
        public override bool Validate(out Exception ex) {
            if (!ValidateNull(this.Rumbles, $"{nameof(this.Rumbles)} may not be null.", out ex)) {
                return false;
            }

            for (int i = 0; i < this.Rumbles.Count; i++) {
                Rumble rumble = this.Rumbles[i];
                if (!ValidateNull(rumble, $"{nameof(this.Rumbles)}[{i}]: {nameof(Rumble)} may not be null.", out ex)
                    || !ValidateNull(rumble.HeavyStates, $"{nameof(this.Rumbles)}[{i}]: {nameof(Rumble.HeavyStates)} may not be null.", out ex)
                    || !ValidateNull(rumble.LightStates, $"{nameof(this.Rumbles)}[{i}]: {nameof(Rumble.LightStates)} may not be null.", out ex)) {
                    return false;
                }
            }

            ex = null;
            return true;
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected internal override void Write(BinaryWriterEx bw) {
            bw.BigEndian = this.BigEndian;

            bw.WriteInt16((short)this.Rumbles.Count);
            bw.WriteInt16(0);
            bw.WriteInt32(0x10);
            bw.WriteInt32(0);
            bw.WriteInt32(0);

            for (int i = 0; i < this.Rumbles.Count; i++) {
                this.Rumbles[i].Write(bw, i);
            }

            for (int i = 0; i < this.Rumbles.Count; i++) {
                this.Rumbles[i].WriteStates(bw, i);
            }
        }

        /// <summary>
        /// A controller rumble effect.
        /// </summary>
        public class Rumble {
            /// <summary>
            /// A sequence of states for the heavy rumble motor.
            /// </summary>
            public List<State> HeavyStates { get; set; }

            /// <summary>
            /// A sequence of states for the light rumble motor.
            /// </summary>
            public List<State> LightStates { get; set; }

            /// <summary>
            /// Creates an empty Rumble.
            /// </summary>
            public Rumble() {
                this.HeavyStates = new List<State>();
                this.LightStates = new List<State>();
            }

            internal Rumble(BinaryReaderEx br) {
                short heavyCount = br.ReadInt16();
                short lightCount = br.ReadInt16();
                _ = br.AssertInt32(0);
                int heavyOffset = br.ReadInt32();
                int lightOffset = br.ReadInt32();

                List<State> readStates(short count, int offset) {
                    var states = new List<State>(count);
                    if (count > 0) {
                        br.Position = offset;
                        for (int i = 0; i < count; i++) {
                            states.Add(new State(br));
                        }
                    }
                    return states;
                }

                long pos = br.Position;
                this.HeavyStates = readStates(heavyCount, heavyOffset);
                this.LightStates = readStates(lightCount, lightOffset);
                br.Position = pos;
            }

            internal void Write(BinaryWriterEx bw, int index) {
                bw.WriteInt16((short)this.HeavyStates.Count);
                bw.WriteInt16((short)this.LightStates.Count);
                bw.WriteInt32(0);
                bw.ReserveInt32($"HeavyOffset[{index}]");
                bw.ReserveInt32($"LightOffset[{index}]");
            }

            internal void WriteStates(BinaryWriterEx bw, int index) {
                int writeStates(List<State> states) {
                    if (states.Count == 0) {
                        return 0;
                    } else {
                        int offset = (int)bw.Position;
                        foreach (State state in states) {
                            state.Write(bw);
                        }

                        return offset;
                    }
                }

                bw.FillInt32($"HeavyOffset[{index}]", writeStates(this.HeavyStates));
                bw.FillInt32($"LightOffset[{index}]", writeStates(this.LightStates));
            }
        }

        /// <summary>
        /// Defines a sweep of rumble motor strength over a given period of time.
        /// </summary>
        public class State {
            /// <summary>
            /// The time the state begins, in 30 fps frames.
            /// </summary>
            public short Start { get; set; }

            /// <summary>
            /// The duration of the state, in 30 fps frames.
            /// </summary>
            public short Duration { get; set; }

            /// <summary>
            /// The strength of the motor at the beginning of the state.
            /// </summary>
            public byte BeginStrength { get; set; }

            /// <summary>
            /// The strength of the motor at the end of the state.
            /// </summary>
            public byte EndStrength { get; set; }

            /// <summary>
            /// Creates a State with no rumble.
            /// </summary>
            public State() { }

            /// <summary>
            /// Creates a State with the given values.
            /// </summary>
            public State(short start, short duration, byte beginStrength, byte endStrength) {
                this.Start = start;
                this.Duration = duration;
                this.BeginStrength = beginStrength;
                this.EndStrength = endStrength;
            }

            internal State(BinaryReaderEx br) {
                this.Start = br.ReadInt16();
                this.Duration = br.ReadInt16();
                this.BeginStrength = br.ReadByte();
                this.EndStrength = br.ReadByte();
                _ = br.AssertInt16(0);
            }

            internal void Write(BinaryWriterEx bw) {
                bw.WriteInt16(this.Start);
                bw.WriteInt16(this.Duration);
                bw.WriteByte(this.BeginStrength);
                bw.WriteByte(this.EndStrength);
                bw.WriteInt16(0);
            }
        }
    }
}
