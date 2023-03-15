using System.Collections.Generic;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class FFXDLSE {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class State : FXSerializable {
            internal override string ClassName => "FXSerializableState";

            internal override int Version => 1;

            public List<Action> Actions { get; set; }

            public List<Trigger> Triggers { get; set; }

            public State() {
                this.Actions = new List<Action>();
                this.Triggers = new List<Trigger>();
            }

            internal State(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames) {
                int actionCount = br.ReadInt32();
                int triggerCount = br.ReadInt32();
                this.Actions = new List<Action>(actionCount);
                for (int i = 0; i < actionCount; i++) {
                    this.Actions.Add(new Action(br, classNames));
                }

                this.Triggers = new List<Trigger>(triggerCount);
                for (int i = 0; i < triggerCount; i++) {
                    this.Triggers.Add(new Trigger(br, classNames));
                }
            }

            internal override void AddClassNames(List<string> classNames) {
                base.AddClassNames(classNames);
                foreach (Action action in this.Actions) {
                    action.AddClassNames(classNames);
                }

                foreach (Trigger trigger in this.Triggers) {
                    trigger.AddClassNames(classNames);
                }
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames) {
                bw.WriteInt32(this.Actions.Count);
                bw.WriteInt32(this.Triggers.Count);
                foreach (Action action in this.Actions) {
                    action.Write(bw, classNames);
                }

                foreach (Trigger trigger in this.Triggers) {
                    trigger.Write(bw, classNames);
                }
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
