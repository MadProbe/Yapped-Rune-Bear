using System;
using System.Collections.Generic;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class TAE3 {
        /// <summary>
        /// Determines the behavior of an event and what data it contains.
        /// </summary>
        public enum EventType : ulong {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            JumpTable = 000,
            Unk001 = 001,
            Unk002 = 002,
            Unk005 = 005,
            Unk016 = 016,
            Unk017 = 017,
            Unk024 = 024,
            SwitchWeapon1 = 032,
            SwitchWeapon2 = 033,
            Unk034 = 034,
            Unk035 = 035,
            Unk064 = 064,
            Unk065 = 065,
            CreateSpEffect1 = 066,
            CreateSpEffect2 = 067,
            PlayFFX = 096,
            Unk110 = 110,
            HitEffect = 112,
            Unk113 = 113,
            Unk114 = 114,
            Unk115 = 115,
            Unk116 = 116,
            Unk117 = 117,
            Unk118 = 118,
            Unk119 = 119,
            Unk120 = 120,
            Unk121 = 121,
            PlaySound1 = 128,
            PlaySound2 = 129,
            PlaySound3 = 130,
            PlaySound4 = 131,
            PlaySound5 = 132,
            Unk136 = 136,
            Unk137 = 137,
            CreateDecal = 138,
            Unk144 = 144,
            Unk145 = 145,
            Unk150 = 150,
            Unk151 = 151,
            Unk161 = 161,
            Unk192 = 192,
            FadeOut = 193,
            Unk194 = 194,
            Unk224 = 224,
            DisableStaminaRegen = 225,
            Unk226 = 226,
            Unk227 = 227,
            RagdollReviveTime = 228,
            Unk229 = 229,
            SetEventMessageID = 231,
            Unk232 = 232,
            ChangeDrawMask = 233,
            RollDistanceReduction = 236,
            CreateAISound = 237,
            Unk300 = 300,
            Unk301 = 301,
            AddSpEffectDragonForm = 302,
            PlayAnimation = 303,
            BehaviorThing = 304,
            Unk306 = 306,
            CreateBehaviorPC = 307,
            Unk308 = 308,
            Unk310 = 310,
            Unk311 = 311,
            Unk312 = 312,
            Unk317 = 317,
            Unk320 = 320,
            Unk330 = 330,
            EffectDuringThrow = 331,
            Unk332 = 332,
            CreateSpEffect = 401,
            Unk500 = 500,
            Unk510 = 510,
            Unk520 = 520,
            KingOfTheStorm = 522,
            Unk600 = 600,
            Unk601 = 601,
            DebugAnimSpeed = 603,
            Unk605 = 605,
            Unk606 = 606,
            Unk700 = 700,
            EnableTurningDirection = 703,
            FacingAngleCorrection = 705,
            Unk707 = 707,
            HideWeapon = 710,
            HideModelMask = 711,
            DamageLevelModule = 712,
            ModelMask = 713,
            DamageLevelFunction = 714,
            Unk715 = 715,
            CultStart = 720,
            Unk730 = 730,
            Unk740 = 740,
            IFrameState = 760,
            BonePos = 770,
            BoneFixOn1 = 771,
            BoneFixOn2 = 772,
            TurnLowerBody = 781,
            Unk782 = 782,
            SpawnBulletByCultSacrifice1 = 785,
            Unk786 = 786,
            Unk790 = 790,
            Unk791 = 791,
            HitEffect2 = 792,
            CultSacrifice1 = 793,
            SacrificeEmpty = 794,
            Toughness = 795,
            BringCultMenu = 796,
            CeremonyParamID = 797,
            CultSingle = 798,
            CultEmpty2 = 799,
            Unk800 = 800,
            Unk900 = 900,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }

        /// <summary>
        /// An action or effect triggered at a certain time during an animation.
        /// </summary>
        public abstract class Event {
            /// <summary>
            /// The type of this event.
            /// </summary>
            public abstract EventType Type { get; }

            /// <summary>
            /// When the event begins.
            /// </summary>
            public float StartTime;

            /// <summary>
            /// When the event ends.
            /// </summary>
            public float EndTime;

            internal Event(float startTime, float endTime) {
                this.StartTime = startTime;
                this.EndTime = endTime;
            }

            internal void WriteHeader(BinaryWriterEx bw, int animIndex, int eventIndex, Dictionary<float, long> timeOffsets) {
                bw.WriteInt64(timeOffsets[this.StartTime]);
                bw.WriteInt64(timeOffsets[this.EndTime]);
                bw.ReserveInt64($"EventDataOffset{animIndex}:{eventIndex}");
            }

            internal void WriteData(BinaryWriterEx bw, int animIndex, int eventIndex) {
                bw.FillInt64($"EventDataOffset{animIndex}:{eventIndex}", bw.Position);
                bw.WriteUInt64((ulong)this.Type);
                bw.WriteInt64(bw.Position + 8);
                this.WriteSpecific(bw);
                bw.Pad(0x10);
            }

            internal abstract void WriteSpecific(BinaryWriterEx bw);

            /// <summary>
            /// Returns the start time, end time, and type of the event.
            /// </summary>
            public override string ToString() => $"{(int)Math.Round(this.StartTime * 30):D3} - {(int)Math.Round(this.EndTime * 30):D3} {this.Type}";

            internal static Event Read(BinaryReaderEx br) {
                long startTimeOffset = br.ReadInt64();
                long endTimeOffset = br.ReadInt64();
                long eventDataOffset = br.ReadInt64();
                float startTime = br.GetSingle(startTimeOffset);
                float endTime = br.GetSingle(endTimeOffset);

                Event result;
                br.StepIn(eventDataOffset);
                {
                    EventType type = br.ReadEnum64<EventType>();
                    _ = br.AssertInt64(br.Position + 8);
                    result = type switch {
                        EventType.JumpTable => new JumpTable(startTime, endTime, br),
                        EventType.Unk001 => new Unk001(startTime, endTime, br),
                        EventType.Unk002 => new Unk002(startTime, endTime, br),
                        EventType.Unk005 => new Unk005(startTime, endTime, br),
                        EventType.Unk016 => new Unk016(startTime, endTime, br),
                        EventType.Unk017 => new Unk017(startTime, endTime, br),
                        EventType.Unk024 => new Unk024(startTime, endTime, br),
                        EventType.SwitchWeapon1 => new SwitchWeapon1(startTime, endTime, br),
                        EventType.SwitchWeapon2 => new SwitchWeapon2(startTime, endTime, br),
                        EventType.Unk034 => new Unk034(startTime, endTime, br),
                        EventType.Unk035 => new Unk035(startTime, endTime, br),
                        EventType.Unk064 => new Unk064(startTime, endTime, br),
                        EventType.Unk065 => new Unk065(startTime, endTime, br),
                        EventType.CreateSpEffect1 => new CreateSpEffect1(startTime, endTime, br),
                        EventType.CreateSpEffect2 => new CreateSpEffect2(startTime, endTime, br),
                        EventType.PlayFFX => new PlayFFX(startTime, endTime, br),
                        EventType.Unk110 => new Unk110(startTime, endTime, br),
                        EventType.HitEffect => new HitEffect(startTime, endTime, br),
                        EventType.Unk113 => new Unk113(startTime, endTime, br),
                        EventType.Unk114 => new Unk114(startTime, endTime, br),
                        EventType.Unk115 => new Unk115(startTime, endTime, br),
                        EventType.Unk116 => new Unk116(startTime, endTime, br),
                        EventType.Unk117 => new Unk117(startTime, endTime, br),
                        EventType.Unk118 => new Unk118(startTime, endTime, br),
                        EventType.Unk119 => new Unk119(startTime, endTime, br),
                        EventType.Unk120 => new Unk120(startTime, endTime, br),
                        EventType.Unk121 => new Unk121(startTime, endTime, br),
                        EventType.PlaySound1 => new PlaySound1(startTime, endTime, br),
                        EventType.PlaySound2 => new PlaySound2(startTime, endTime, br),
                        EventType.PlaySound3 => new PlaySound3(startTime, endTime, br),
                        EventType.PlaySound4 => new PlaySound4(startTime, endTime, br),
                        EventType.PlaySound5 => new PlaySound5(startTime, endTime, br),
                        EventType.Unk137 => new Unk137(startTime, endTime, br),
                        EventType.CreateDecal => new CreateDecal(startTime, endTime, br),
                        EventType.Unk144 => new Unk144(startTime, endTime, br),
                        EventType.Unk145 => new Unk145(startTime, endTime, br),
                        EventType.Unk150 => new Unk150(startTime, endTime, br),
                        EventType.Unk151 => new Unk151(startTime, endTime, br),
                        EventType.Unk161 => new Unk161(startTime, endTime, br),
                        EventType.FadeOut => new FadeOut(startTime, endTime, br),
                        EventType.Unk194 => new Unk194(startTime, endTime, br),
                        EventType.Unk224 => new Unk224(startTime, endTime, br),
                        EventType.DisableStaminaRegen => new DisableStaminaRegen(startTime, endTime, br),
                        EventType.Unk226 => new Unk226(startTime, endTime, br),
                        EventType.Unk227 => new Unk227(startTime, endTime, br),
                        EventType.RagdollReviveTime => new RagdollReviveTime(startTime, endTime, br),
                        EventType.Unk229 => new Unk229(startTime, endTime, br),
                        EventType.SetEventMessageID => new SetEventMessageID(startTime, endTime, br),
                        EventType.Unk232 => new Unk232(startTime, endTime, br),
                        EventType.ChangeDrawMask => new ChangeDrawMask(startTime, endTime, br),
                        EventType.RollDistanceReduction => new RollDistanceReduction(startTime, endTime, br),
                        EventType.CreateAISound => new CreateAISound(startTime, endTime, br),
                        EventType.Unk300 => new Unk300(startTime, endTime, br),
                        EventType.Unk301 => new Unk301(startTime, endTime, br),
                        EventType.AddSpEffectDragonForm => new AddSpEffectDragonForm(startTime, endTime, br),
                        EventType.PlayAnimation => new PlayAnimation(startTime, endTime, br),
                        EventType.BehaviorThing => new BehaviorThing(startTime, endTime, br),
                        EventType.CreateBehaviorPC => new CreateBehaviorPC(startTime, endTime, br),
                        EventType.Unk308 => new Unk308(startTime, endTime, br),
                        EventType.Unk310 => new Unk310(startTime, endTime, br),
                        EventType.Unk311 => new Unk311(startTime, endTime, br),
                        EventType.Unk312 => new Unk312(startTime, endTime, br),
                        EventType.Unk320 => new Unk320(startTime, endTime, br),
                        EventType.Unk330 => new Unk330(startTime, endTime, br),
                        EventType.EffectDuringThrow => new EffectDuringThrow(startTime, endTime, br),
                        EventType.Unk332 => new Unk332(startTime, endTime, br),
                        EventType.CreateSpEffect => new CreateSpEffect(startTime, endTime, br),
                        EventType.Unk500 => new Unk500(startTime, endTime, br),
                        EventType.Unk510 => new Unk510(startTime, endTime, br),
                        EventType.Unk520 => new Unk520(startTime, endTime, br),
                        EventType.KingOfTheStorm => new KingOfTheStorm(startTime, endTime, br),
                        EventType.Unk600 => new Unk600(startTime, endTime, br),
                        EventType.Unk601 => new Unk601(startTime, endTime, br),
                        EventType.DebugAnimSpeed => new DebugAnimSpeed(startTime, endTime, br),
                        EventType.Unk605 => new Unk605(startTime, endTime, br),
                        EventType.Unk606 => new Unk606(startTime, endTime, br),
                        EventType.Unk700 => new Unk700(startTime, endTime, br),
                        EventType.EnableTurningDirection => new EnableTurningDirection(startTime, endTime, br),
                        EventType.FacingAngleCorrection => new FacingAngleCorrection(startTime, endTime, br),
                        EventType.Unk707 => new Unk707(startTime, endTime, br),
                        EventType.HideWeapon => new HideWeapon(startTime, endTime, br),
                        EventType.HideModelMask => new HideModelMask(startTime, endTime, br),
                        EventType.DamageLevelModule => new DamageLevelModule(startTime, endTime, br),
                        EventType.ModelMask => new ModelMask(startTime, endTime, br),
                        EventType.DamageLevelFunction => new DamageLevelFunction(startTime, endTime, br),
                        EventType.Unk715 => new Unk715(startTime, endTime, br),
                        EventType.CultStart => new CultStart(startTime, endTime, br),
                        EventType.Unk730 => new Unk730(startTime, endTime, br),
                        EventType.Unk740 => new Unk740(startTime, endTime, br),
                        EventType.IFrameState => new IFrameState(startTime, endTime, br),
                        EventType.BonePos => new BonePos(startTime, endTime, br),
                        EventType.BoneFixOn1 => new BoneFixOn1(startTime, endTime, br),
                        EventType.BoneFixOn2 => new BoneFixOn2(startTime, endTime, br),
                        EventType.TurnLowerBody => new TurnLowerBody(startTime, endTime, br),
                        EventType.Unk782 => new Unk782(startTime, endTime, br),
                        EventType.SpawnBulletByCultSacrifice1 => new SpawnBulletByCultSacrifice1(startTime, endTime, br),
                        EventType.Unk786 => new Unk786(startTime, endTime, br),
                        EventType.Unk790 => new Unk790(startTime, endTime, br),
                        EventType.Unk791 => new Unk791(startTime, endTime, br),
                        EventType.HitEffect2 => new HitEffect2(startTime, endTime, br),
                        EventType.CultSacrifice1 => new CultSacrifice1(startTime, endTime, br),
                        EventType.SacrificeEmpty => new SacrificeEmpty(startTime, endTime, br),
                        EventType.Toughness => new Toughness(startTime, endTime, br),
                        EventType.BringCultMenu => new BringCultMenu(startTime, endTime, br),
                        EventType.CeremonyParamID => new CeremonyParamID(startTime, endTime, br),
                        EventType.CultSingle => new CultSingle(startTime, endTime, br),
                        EventType.CultEmpty2 => new CultEmpty2(startTime, endTime, br),
                        EventType.Unk800 => new Unk800(startTime, endTime, br),
                        _ => throw new NotImplementedException(),
                    };
                    if (result.Type != type) {
                        throw new InvalidProgramException("There is a typo in TAE3.Event.cs. Please bully me.");
                    }
                }
                br.StepOut();

                return result;
            }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            /// <summary>
            /// General-purpose event that calls different functions based on the first field.
            /// </summary>
            public class JumpTable : Event // 000
            {
                public override EventType Type => EventType.JumpTable;

                public int JumpTableID { get; set; }
                public int Unk04 { get; set; }
                // Used for jump table ID 3
                public int Unk08 { get; set; }
                public short Unk0C { get; set; }
                public short Unk0E { get; set; }

                internal JumpTable(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.JumpTableID = br.ReadInt32();
                    this.Unk04 = br.ReadInt32();
                    this.Unk08 = br.ReadInt32();
                    this.Unk0C = br.ReadInt16();
                    this.Unk0E = br.ReadInt16();
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.JumpTableID);
                    bw.WriteInt32(this.Unk04);
                    bw.WriteInt32(this.Unk08);
                    bw.WriteInt16(this.Unk0C);
                    bw.WriteInt16(this.Unk0E);
                }

                public override string ToString() => $"{base.ToString()} : {this.JumpTableID}";
            }

            public class Unk001 : Event // 001
            {
                public override EventType Type => EventType.Unk001;

                public int Unk00 { get; set; }
                public int Unk04 { get; set; }
                public int Condition { get; set; }
                public byte Unk0C { get; set; }
                public byte Unk0D { get; set; }
                public short StateInfo { get; set; }

                public Unk001(float startTime, float endTime, int unk00, int unk04, int condition, byte unk0C, byte unk0D, short stateInfo) : base(startTime, endTime) {
                    this.Unk00 = unk00;
                    this.Unk04 = unk04;
                    this.Condition = condition;
                    this.Unk0C = unk0C;
                    this.Unk0D = unk0D;
                    this.StateInfo = stateInfo;
                }

                internal Unk001(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadInt32();
                    this.Unk04 = br.ReadInt32();
                    this.Condition = br.ReadInt32();
                    this.Unk0C = br.ReadByte();
                    this.Unk0D = br.ReadByte();
                    this.StateInfo = br.ReadInt16();
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.Unk00);
                    bw.WriteInt32(this.Unk04);
                    bw.WriteInt32(this.Condition);
                    bw.WriteByte(this.Unk0C);
                    bw.WriteByte(this.Unk0D);
                    bw.WriteInt16(this.StateInfo);
                }
            }

            public class Unk002 : Event // 002
            {
                public override EventType Type => EventType.Unk002;

                public int Unk00;
                public int Unk04;
                public int ChrAsmStyle;
                public byte Unk0C;
                public byte Unk0D;
                public ushort Unk0E;
                public ushort Unk10;

                internal Unk002(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadInt32();
                    this.Unk04 = br.ReadInt32();
                    this.ChrAsmStyle = br.ReadInt32();
                    this.Unk0C = br.ReadByte();
                    this.Unk0D = br.ReadByte();
                    this.Unk0E = br.ReadUInt16();
                    this.Unk10 = br.ReadUInt16();
                    _ = br.AssertInt16(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.Unk00);
                    bw.WriteInt32(this.Unk04);
                    bw.WriteInt32(this.ChrAsmStyle);
                    bw.WriteByte(this.Unk0C);
                    bw.WriteByte(this.Unk0D);
                    bw.WriteUInt16(this.Unk0E);
                    bw.WriteUInt16(this.Unk10);
                    bw.WriteInt16(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk005 : Event // 005
            {
                public override EventType Type => EventType.Unk005;

                public int Unk00;
                public int Unk04;

                internal Unk005(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadInt32();
                    this.Unk04 = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.Unk00);
                    bw.WriteInt32(this.Unk04);
                }
            }

            public class Unk016 : Event // 016
            {
                public override EventType Type => EventType.Unk016;

                internal Unk016(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) { }

                internal override void WriteSpecific(BinaryWriterEx bw) { }
            }

            public class Unk017 : Event // 017
            {
                public override EventType Type => EventType.Unk017;

                internal Unk017(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk024 : Event // 024
            {
                public override EventType Type => EventType.Unk024;

                public int Unk00;
                public int Unk04;
                public int Unk08;
                public int Unk0C;

                internal Unk024(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadInt32();
                    this.Unk04 = br.ReadInt32();
                    this.Unk08 = br.ReadInt32();
                    this.Unk0C = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.Unk00);
                    bw.WriteInt32(this.Unk04);
                    bw.WriteInt32(this.Unk08);
                    bw.WriteInt32(this.Unk0C);
                }
            }

            public class SwitchWeapon1 : Event // 032
            {
                public override EventType Type => EventType.SwitchWeapon1;

                public int SwitchState;

                internal SwitchWeapon1(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.SwitchState = br.ReadInt32();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.SwitchState);
                    bw.WriteInt32(0);
                }
            }

            public class SwitchWeapon2 : Event // 033
            {
                public override EventType Type => EventType.SwitchWeapon2;

                public int SwitchState;

                internal SwitchWeapon2(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.SwitchState = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.SwitchState);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk034 : Event // 034
            {
                public override EventType Type => EventType.Unk034;

                public int State;

                internal Unk034(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.State = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.State);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk035 : Event // 035
            {
                public override EventType Type => EventType.Unk035;

                public int State;

                internal Unk035(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.State = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.State);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk064 : Event // 064
            {
                public override EventType Type => EventType.Unk064;

                public int Unk00;
                public ushort Unk04;
                public ushort Unk06;
                public byte Unk08;
                public byte Unk09;
                public byte Unk0A;
                public byte Unk0B;

                internal Unk064(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadInt32();
                    this.Unk04 = br.ReadUInt16();
                    this.Unk06 = br.ReadUInt16();
                    this.Unk08 = br.ReadByte();
                    this.Unk09 = br.ReadByte();
                    this.Unk0A = br.ReadByte();
                    this.Unk0B = br.ReadByte();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.Unk00);
                    bw.WriteUInt16(this.Unk04);
                    bw.WriteUInt16(this.Unk06);
                    bw.WriteByte(this.Unk08);
                    bw.WriteByte(this.Unk09);
                    bw.WriteByte(this.Unk0A);
                    bw.WriteByte(this.Unk0B);
                    bw.WriteInt32(0);
                }
            }

            public class Unk065 : Event // 065
            {
                public override EventType Type => EventType.Unk065;

                public int Unk00;
                public byte Unk04;
                public byte Unk05;
                public ushort Unk06;
                public int Unk08;

                internal Unk065(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadInt32();
                    this.Unk04 = br.ReadByte();
                    this.Unk05 = br.ReadByte();
                    this.Unk06 = br.ReadUInt16();
                    this.Unk08 = br.ReadInt32();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.Unk00);
                    bw.WriteByte(this.Unk04);
                    bw.WriteByte(this.Unk05);
                    bw.WriteUInt16(this.Unk06);
                    bw.WriteInt32(this.Unk08);
                    bw.WriteInt32(0);
                }
            }

            // During attack
            public class CreateSpEffect1 : Event // 066
            {
                public override EventType Type => EventType.CreateSpEffect1;

                public int SpEffectID;

                public CreateSpEffect1(float startTime, float endTime, int speffectID) : base(startTime, endTime) => this.SpEffectID = speffectID;

                internal CreateSpEffect1(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.SpEffectID = br.ReadInt32();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.SpEffectID);
                    bw.WriteInt32(0);
                }
            }

            // During roll
            public class CreateSpEffect2 : Event // 067
            {
                public override EventType Type => EventType.CreateSpEffect2;

                public int SpEffectID;

                internal CreateSpEffect2(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.SpEffectID = br.ReadInt32();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.SpEffectID);
                    bw.WriteInt32(0);
                }
            }

            public class PlayFFX : Event // 096
            {
                public override EventType Type => EventType.PlayFFX;

                public int FFXID;
                public int Unk04;
                public int Unk08;
                public sbyte State0;
                public sbyte State1;
                public sbyte GhostFFXCondition;
                public byte Unk0F;

                internal PlayFFX(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.FFXID = br.ReadInt32();
                    this.Unk04 = br.ReadInt32();
                    this.Unk08 = br.ReadInt32();
                    this.State0 = br.ReadSByte();
                    this.State1 = br.ReadSByte();
                    this.GhostFFXCondition = br.ReadSByte();
                    this.Unk0F = br.ReadByte();
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.FFXID);
                    bw.WriteInt32(this.Unk04);
                    bw.WriteInt32(this.Unk08);
                    bw.WriteSByte(this.State0);
                    bw.WriteSByte(this.State1);
                    bw.WriteSByte(this.GhostFFXCondition);
                    bw.WriteByte(this.Unk0F);
                }
            }

            public class Unk110 : Event // 110
            {
                public override EventType Type => EventType.Unk110;

                public int ID;

                internal Unk110(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.ID = br.ReadInt32();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.ID);
                    bw.WriteInt32(0);
                }
            }

            public class HitEffect : Event // 112
            {
                public override EventType Type => EventType.HitEffect;

                public int Size;
                public int Unk04;
                public int Unk08;

                internal HitEffect(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Size = br.ReadInt32();
                    this.Unk04 = br.ReadInt32();
                    this.Unk08 = br.ReadInt32();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.Size);
                    bw.WriteInt32(this.Unk04);
                    bw.WriteInt32(this.Unk08);
                    bw.WriteInt32(0);
                }
            }

            public class Unk113 : Event // 113
            {
                public override EventType Type => EventType.Unk113;

                internal Unk113(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk114 : Event // 114
            {
                public override EventType Type => EventType.Unk114;

                public int Unk00;
                public ushort Unk04;
                public ushort Unk06;
                public int Unk08;
                public byte Unk0C;
                public sbyte Unk0D;
                public sbyte Unk0E;
                public byte Unk0F;
                public byte Unk10;
                public byte Unk11;
                public short Unk12;

                internal Unk114(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadInt32();
                    this.Unk04 = br.ReadUInt16();
                    this.Unk06 = br.ReadUInt16();
                    this.Unk08 = br.ReadInt32();
                    this.Unk0C = br.ReadByte();
                    this.Unk0D = br.ReadSByte();
                    this.Unk0E = br.ReadSByte();
                    this.Unk0F = br.ReadByte();
                    this.Unk10 = br.ReadByte();
                    this.Unk11 = br.ReadByte();
                    this.Unk12 = br.ReadInt16();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.Unk00);
                    bw.WriteUInt16(this.Unk04);
                    bw.WriteUInt16(this.Unk06);
                    bw.WriteInt32(this.Unk08);
                    bw.WriteByte(this.Unk0C);
                    bw.WriteSByte(this.Unk0D);
                    bw.WriteSByte(this.Unk0E);
                    bw.WriteByte(this.Unk0F);
                    bw.WriteByte(this.Unk10);
                    bw.WriteByte(this.Unk11);
                    bw.WriteInt16(this.Unk12);
                    bw.WriteInt32(0);
                }
            }

            public class Unk115 : Event // 115
            {
                public override EventType Type => EventType.Unk115;

                public int Unk00;
                public ushort Unk04;
                public ushort Unk06;
                public int Unk08;
                public byte Unk0C;
                public byte Unk0D;
                public byte Unk0E;
                public byte Unk0F;
                public byte Unk10;
                public byte Unk11;
                public short Unk12;

                internal Unk115(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadInt32();
                    this.Unk04 = br.ReadUInt16();
                    this.Unk06 = br.ReadUInt16();
                    this.Unk08 = br.ReadInt32();
                    this.Unk0C = br.ReadByte();
                    this.Unk0D = br.ReadByte();
                    this.Unk0E = br.ReadByte();
                    this.Unk0F = br.ReadByte();
                    this.Unk10 = br.ReadByte();
                    this.Unk11 = br.ReadByte();
                    this.Unk12 = br.ReadInt16();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.Unk00);
                    bw.WriteUInt16(this.Unk04);
                    bw.WriteUInt16(this.Unk06);
                    bw.WriteInt32(this.Unk08);
                    bw.WriteByte(this.Unk0C);
                    bw.WriteByte(this.Unk0D);
                    bw.WriteByte(this.Unk0E);
                    bw.WriteByte(this.Unk0F);
                    bw.WriteByte(this.Unk10);
                    bw.WriteByte(this.Unk11);
                    bw.WriteInt16(this.Unk12);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk116 : Event // 116
            {
                public override EventType Type => EventType.Unk116;

                public int Unk00;
                public int Unk04;
                public int Unk08;
                public int Unk0C;

                internal Unk116(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadInt32();
                    this.Unk04 = br.ReadInt32();
                    this.Unk08 = br.ReadInt32();
                    this.Unk0C = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.Unk00);
                    bw.WriteInt32(this.Unk04);
                    bw.WriteInt32(this.Unk08);
                    bw.WriteInt32(this.Unk0C);
                }
            }

            public class Unk117 : Event // 117
            {
                public override EventType Type => EventType.Unk117;

                public int Unk00;
                public int Unk04;
                public int Unk08;
                public byte Unk0C;
                public byte Unk0D;
                public byte Unk0E;
                public byte Unk0F;

                internal Unk117(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadInt32();
                    this.Unk04 = br.ReadInt32();
                    this.Unk08 = br.ReadInt32(); // -1
                    this.Unk0C = br.ReadByte();
                    this.Unk0D = br.ReadByte();
                    this.Unk0E = br.ReadByte();
                    this.Unk0F = br.ReadByte();
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.Unk00);
                    bw.WriteInt32(this.Unk04);
                    bw.WriteInt32(this.Unk08);
                    bw.WriteByte(this.Unk0C);
                    bw.WriteByte(this.Unk0D);
                    bw.WriteByte(this.Unk0E);
                    bw.WriteByte(this.Unk0F);
                }
            }

            public class Unk118 : Event // 118
            {
                public override EventType Type => EventType.Unk118;

                public int Unk00;
                public ushort Unk04;
                public ushort Unk06;
                public ushort Unk08;
                public ushort Unk0A;

                internal Unk118(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadInt32();
                    this.Unk04 = br.ReadUInt16();
                    this.Unk06 = br.ReadUInt16();
                    this.Unk08 = br.ReadUInt16();
                    this.Unk0A = br.ReadUInt16();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.Unk00);
                    bw.WriteUInt16(this.Unk04);
                    bw.WriteUInt16(this.Unk06);
                    bw.WriteUInt16(this.Unk08);
                    bw.WriteUInt16(this.Unk0A);
                    bw.WriteInt32(0);
                }
            }

            public class Unk119 : Event // 119
            {
                public override EventType Type => EventType.Unk119;

                public int Unk00;
                public int Unk04;
                public int Unk08;
                public byte Unk0C;

                internal Unk119(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadInt32();
                    this.Unk04 = br.ReadInt32();
                    this.Unk08 = br.ReadInt32();
                    this.Unk0C = br.ReadByte(); // 0
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.Unk00);
                    bw.WriteInt32(this.Unk04);
                    bw.WriteInt32(this.Unk08);
                    bw.WriteByte(this.Unk0C);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                }
            }

            public class Unk120 : Event // 120
            {
                public override EventType Type => EventType.Unk120;

                public int ChrType;
                public int[] FFXIDs { get; private set; }
                public int Unk30;
                public int Unk34;
                public byte Unk38;

                internal Unk120(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.ChrType = br.ReadInt32();
                    this.FFXIDs = br.ReadInt32s(11);
                    this.Unk30 = br.ReadInt32();
                    this.Unk34 = br.ReadInt32();
                    this.Unk38 = br.ReadByte();
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.ChrType);
                    bw.WriteInt32s(this.FFXIDs);
                    bw.WriteInt32(this.Unk30);
                    bw.WriteInt32(this.Unk34);
                    bw.WriteByte(this.Unk38);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk121 : Event // 121
            {
                public override EventType Type => EventType.Unk121;

                public int Unk00;
                public ushort Unk04;
                public byte Unk06;
                public byte Unk07;

                internal Unk121(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadInt32();
                    this.Unk04 = br.ReadUInt16();
                    this.Unk06 = br.ReadByte();
                    this.Unk07 = br.ReadByte();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.Unk00);
                    bw.WriteUInt16(this.Unk04);
                    bw.WriteByte(this.Unk06);
                    bw.WriteByte(this.Unk07);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class PlaySound1 : Event // 128
            {
                public override EventType Type => EventType.PlaySound1;

                public int SoundType;
                public int SoundID;

                internal PlaySound1(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.SoundType = br.ReadInt32();
                    this.SoundID = br.ReadInt32();
                    // After event version 0x10?
                    //br.AssertInt32(0);
                    //br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.SoundType);
                    bw.WriteInt32(this.SoundID);
                    //bw.WriteInt32(0);
                    //bw.WriteInt32(0);
                }
            }

            public class PlaySound2 : Event // 129
            {
                public override EventType Type => EventType.PlaySound2;

                public int SoundType;
                public int SoundID;
                public int Unk08;
                public int Unk0C;
                public int Unk10;

                internal PlaySound2(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.SoundType = br.ReadInt32();
                    this.SoundID = br.ReadInt32();
                    this.Unk08 = br.ReadInt32();
                    this.Unk0C = br.ReadInt32();
                    // After event version 0x15?
                    this.Unk10 = br.ReadInt32();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.SoundType);
                    bw.WriteInt32(this.SoundID);
                    bw.WriteInt32(this.Unk08);
                    bw.WriteInt32(this.Unk0C);
                    bw.WriteInt32(this.Unk10);
                    bw.WriteInt32(0);
                }
            }

            public class PlaySound3 : Event // 130
            {
                public override EventType Type => EventType.PlaySound3;

                public int SoundType;
                public int SoundID;
                public float Unk08;
                public float Unk0C;

                internal PlaySound3(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.SoundType = br.ReadInt32();
                    this.SoundID = br.ReadInt32();
                    this.Unk08 = br.ReadSingle();
                    this.Unk0C = br.ReadSingle(); // int -1
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.SoundType);
                    bw.WriteInt32(this.SoundID);
                    bw.WriteSingle(this.Unk08);
                    bw.WriteSingle(this.Unk0C);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class PlaySound4 : Event // 131
            {
                public override EventType Type => EventType.PlaySound4;

                public int SoundType;
                public int SoundID;
                public int Unk08;

                internal PlaySound4(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.SoundType = br.ReadInt32();
                    this.SoundID = br.ReadInt32();
                    this.Unk08 = br.ReadInt32();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.SoundType);
                    bw.WriteInt32(this.SoundID);
                    bw.WriteInt32(this.Unk08);
                    bw.WriteInt32(0);
                }
            }

            public class PlaySound5 : Event // 132
            {
                public override EventType Type => EventType.PlaySound5;

                public int SoundType;
                public int SoundID;

                internal PlaySound5(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.SoundType = br.ReadInt32();
                    this.SoundID = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.SoundType);
                    bw.WriteInt32(this.SoundID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk137 : Event // 137
            {
                public override EventType Type => EventType.Unk137;

                public int Unk00;

                internal Unk137(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadInt32();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.Unk00);
                    bw.WriteInt32(0);
                }
            }

            public class CreateDecal : Event // 138
            {
                public override EventType Type => EventType.CreateDecal;

                public int DecalParamID, Unk04;

                internal CreateDecal(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.DecalParamID = br.ReadInt32();
                    this.Unk04 = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.DecalParamID);
                    bw.WriteInt32(this.Unk04);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk144 : Event // 144
            {
                public override EventType Type => EventType.Unk144;

                public ushort Unk00;
                public ushort Unk02;
                public float Unk04;
                public float Unk08;

                internal Unk144(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadUInt16();
                    this.Unk02 = br.ReadUInt16();
                    this.Unk04 = br.ReadSingle();
                    this.Unk08 = br.ReadSingle();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteUInt16(this.Unk00);
                    bw.WriteUInt16(this.Unk02);
                    bw.WriteSingle(this.Unk04);
                    bw.WriteSingle(this.Unk08);
                    bw.WriteInt32(0);
                }
            }

            public class Unk145 : Event // 145
            {
                public override EventType Type => EventType.Unk145;

                public short Unk00;
                public short Condition;

                internal Unk145(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadInt16();
                    this.Condition = br.ReadInt16();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt16(this.Unk00);
                    bw.WriteInt16(this.Condition);
                    bw.WriteInt32(0);
                }
            }

            public class Unk150 : Event // 150
            {
                public override EventType Type => EventType.Unk150;

                public int Unk00;

                internal Unk150(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.Unk00);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk151 : Event // 151
            {
                public override EventType Type => EventType.Unk151;

                public int DummyPointID;

                internal Unk151(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.DummyPointID = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.DummyPointID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk161 : Event // 161
            {
                public override EventType Type => EventType.Unk161;

                internal Unk161(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class FadeOut : Event // 193
            {
                public override EventType Type => EventType.FadeOut;

                public float GhostVal1;
                public float GhostVal2;

                internal FadeOut(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.GhostVal1 = br.ReadSingle();
                    this.GhostVal2 = br.ReadSingle();
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteSingle(this.GhostVal1);
                    bw.WriteSingle(this.GhostVal2);
                }
            }

            public class Unk194 : Event // 194
            {
                public override EventType Type => EventType.Unk194;

                public ushort Unk00;
                public ushort Unk02;
                public ushort Unk04;
                public ushort Unk06;
                public float Unk08;

                internal Unk194(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadUInt16();
                    this.Unk02 = br.ReadUInt16();
                    this.Unk04 = br.ReadUInt16();
                    this.Unk06 = br.ReadUInt16();
                    this.Unk08 = br.ReadSingle();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteUInt16(this.Unk00);
                    bw.WriteUInt16(this.Unk02);
                    bw.WriteUInt16(this.Unk04);
                    bw.WriteUInt16(this.Unk06);
                    bw.WriteSingle(this.Unk08);
                    bw.WriteInt32(0);
                }
            }

            public class Unk224 : Event // 224
            {
                public override EventType Type => EventType.Unk224;

                public float Unk00;
                public int Unk04;

                internal Unk224(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadSingle();
                    this.Unk04 = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteSingle(this.Unk00);
                    bw.WriteInt32(this.Unk04);
                }
            }

            public class DisableStaminaRegen : Event // 225
            {
                public override EventType Type => EventType.DisableStaminaRegen;

                // "0x64 - Enables Regen Back" -Pav
                public byte State;

                internal DisableStaminaRegen(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.State = br.ReadByte();
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteByte(this.State);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk226 : Event // 226
            {
                public override EventType Type => EventType.Unk226;

                // "x/100 Coefficient" -Pav
                public byte State;

                internal Unk226(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.State = br.ReadByte();
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteByte(this.State);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk227 : Event // 227
            {
                public override EventType Type => EventType.Unk227;

                public int Mask;

                internal Unk227(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Mask = br.ReadInt32();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.Mask);
                    bw.WriteInt32(0);
                }
            }

            public class RagdollReviveTime : Event // 228
            {
                public override EventType Type => EventType.RagdollReviveTime;

                public float Unk00;
                public float ReviveTimer;

                internal RagdollReviveTime(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadSingle();
                    this.ReviveTimer = br.ReadSingle();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteSingle(this.Unk00);
                    bw.WriteSingle(this.ReviveTimer);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk229 : Event // 229
            {
                public override EventType Type => EventType.Unk229;

                public int Unk00;

                internal Unk229(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadInt32();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.Unk00);
                    bw.WriteInt32(0);
                }
            }

            public class SetEventMessageID : Event // 231
            {
                public override EventType Type => EventType.SetEventMessageID;

                public int EventMessageID;

                internal SetEventMessageID(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.EventMessageID = br.ReadInt32();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.EventMessageID);
                    bw.WriteInt32(0);
                }
            }

            public class Unk232 : Event // 232
            {
                public override EventType Type => EventType.Unk232;

                public byte Unk00;
                public byte Unk01;
                public byte Unk02;
                public byte Unk03;

                internal Unk232(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadByte();
                    this.Unk01 = br.ReadByte();
                    this.Unk02 = br.ReadByte();
                    this.Unk03 = br.ReadByte();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteByte(this.Unk00);
                    bw.WriteByte(this.Unk01);
                    bw.WriteByte(this.Unk02);
                    bw.WriteByte(this.Unk03);
                    bw.WriteInt32(0);
                }
            }

            public class ChangeDrawMask : Event // 233
            {
                public override EventType Type => EventType.ChangeDrawMask;

                public byte[] DrawMask { get; private set; }

                internal ChangeDrawMask(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) => this.DrawMask = br.ReadBytes(32);

                internal override void WriteSpecific(BinaryWriterEx bw) => bw.WriteBytes(this.DrawMask);
            }

            public class RollDistanceReduction : Event // 236
            {
                public override EventType Type => EventType.RollDistanceReduction;

                public float Unk00;
                public float Unk04;
                public bool RollType;

                internal RollDistanceReduction(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadSingle();
                    this.Unk04 = br.ReadSingle();
                    this.RollType = br.ReadBoolean();
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteSingle(this.Unk00);
                    bw.WriteSingle(this.Unk04);
                    bw.WriteBoolean(this.RollType);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                }
            }

            public class CreateAISound : Event // 237
            {
                public override EventType Type => EventType.CreateAISound;

                public int AISoundID;

                internal CreateAISound(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.AISoundID = br.ReadInt32();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.AISoundID);
                    bw.WriteInt32(0);
                }
            }

            public class Unk300 : Event // 300
            {
                public override EventType Type => EventType.Unk300;

                public short JumpTableID1;
                public short JumpTableID2;
                public float Unk04;
                public float Unk08;
                public int Unk0C;

                internal Unk300(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.JumpTableID1 = br.ReadInt16();
                    this.JumpTableID2 = br.ReadInt16();
                    this.Unk04 = br.ReadSingle();
                    this.Unk08 = br.ReadSingle();
                    this.Unk0C = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt16(this.JumpTableID1);
                    bw.WriteInt16(this.JumpTableID2);
                    bw.WriteSingle(this.Unk04);
                    bw.WriteSingle(this.Unk08);
                    bw.WriteInt32(this.Unk0C);
                }
            }

            public class Unk301 : Event // 301
            {
                public override EventType Type => EventType.Unk301;

                public int Unk00;

                internal Unk301(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadInt32();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.Unk00);
                    bw.WriteInt32(0);
                }
            }

            public class AddSpEffectDragonForm : Event // 302
            {
                public override EventType Type => EventType.AddSpEffectDragonForm;

                public int SpEffectID;

                internal AddSpEffectDragonForm(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.SpEffectID = br.ReadInt32();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.SpEffectID);
                    bw.WriteInt32(0);
                }
            }

            public class PlayAnimation : Event // 303
            {
                public override EventType Type => EventType.PlayAnimation;

                public int AnimationID;

                internal PlayAnimation(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.AnimationID = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.AnimationID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            // "Behavior Thing?" -Pav
            public class BehaviorThing : Event // 304
            {
                public override EventType Type => EventType.BehaviorThing;

                public ushort Unk00;
                public short Unk02;
                public int BehaviorListID;

                internal BehaviorThing(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadUInt16();
                    this.Unk02 = br.ReadInt16();
                    this.BehaviorListID = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteUInt16(this.Unk00);
                    bw.WriteInt16(this.Unk02);
                    bw.WriteInt32(this.BehaviorListID);
                }
            }

            public class CreateBehaviorPC : Event // 307
            {
                public override EventType Type => EventType.CreateBehaviorPC;

                public short Unk00;
                public short Unk02;
                public int Condition;
                public int Unk08;

                internal CreateBehaviorPC(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadInt16();
                    this.Unk02 = br.ReadInt16();
                    this.Condition = br.ReadInt32();
                    this.Unk08 = br.ReadInt32();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt16(this.Unk00);
                    bw.WriteInt16(this.Unk02);
                    bw.WriteInt32(this.Condition);
                    bw.WriteInt32(this.Unk08);
                    bw.WriteInt32(0);
                }
            }

            public class Unk308 : Event // 308
            {
                public override EventType Type => EventType.Unk308;

                public float Unk00;

                internal Unk308(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadSingle();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteSingle(this.Unk00);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            // "Behavior?" -Pav
            public class Unk310 : Event // 310
            {
                public override EventType Type => EventType.Unk310;

                public byte Unk00;
                public byte Unk01;

                internal Unk310(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadByte();
                    this.Unk01 = br.ReadByte();
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteByte(this.Unk00);
                    bw.WriteByte(this.Unk01);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk311 : Event // 311
            {
                public override EventType Type => EventType.Unk311;

                public byte Unk00;
                public byte Unk01;
                public byte Unk02;

                internal Unk311(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadByte();
                    this.Unk01 = br.ReadByte();
                    this.Unk02 = br.ReadByte();
                    _ = br.AssertByte(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteByte(this.Unk00);
                    bw.WriteByte(this.Unk01);
                    bw.WriteByte(this.Unk02);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk312 : Event // 312
            {
                public override EventType Type => EventType.Unk312;

                public byte[] BehaviorMask { get; private set; }

                internal Unk312(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) => this.BehaviorMask = br.ReadBytes(32);

                internal override void WriteSpecific(BinaryWriterEx bw) => bw.WriteBytes(this.BehaviorMask);
            }

            public class Unk320 : Event // 320
            {
                public override EventType Type => EventType.Unk320;

                public bool Unk00;
                public bool Unk01;
                public bool Unk02;
                public bool Unk03;
                public bool Unk04;
                public bool Unk05;
                public bool Unk06;

                internal Unk320(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadBoolean();
                    this.Unk01 = br.ReadBoolean();
                    this.Unk02 = br.ReadBoolean();
                    this.Unk03 = br.ReadBoolean();
                    this.Unk04 = br.ReadBoolean();
                    this.Unk05 = br.ReadBoolean();
                    this.Unk06 = br.ReadBoolean();
                    _ = br.AssertByte(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteBoolean(this.Unk00);
                    bw.WriteBoolean(this.Unk01);
                    bw.WriteBoolean(this.Unk02);
                    bw.WriteBoolean(this.Unk03);
                    bw.WriteBoolean(this.Unk04);
                    bw.WriteBoolean(this.Unk05);
                    bw.WriteBoolean(this.Unk06);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk330 : Event // 330
            {
                public override EventType Type => EventType.Unk330;

                internal Unk330(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class EffectDuringThrow : Event // 331
            {
                public override EventType Type => EventType.EffectDuringThrow;

                public int SpEffectID1;
                public int SpEffectID2;

                internal EffectDuringThrow(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.SpEffectID1 = br.ReadInt32();
                    this.SpEffectID2 = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.SpEffectID1);
                    bw.WriteInt32(this.SpEffectID2);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk332 : Event // 332
            {
                public override EventType Type => EventType.Unk332;

                internal Unk332(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            // "When Landing" -Pav
            public class CreateSpEffect : Event // 401
            {
                public override EventType Type => EventType.CreateSpEffect;

                public int SpEffectID;

                public CreateSpEffect(float startTime, float endTime, int effectId) : base(startTime, endTime) => this.SpEffectID = effectId;

                internal CreateSpEffect(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.SpEffectID = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.SpEffectID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk500 : Event // 500
            {
                public override EventType Type => EventType.Unk500;

                public byte Unk00;
                public byte Unk01;

                internal Unk500(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadByte();
                    this.Unk01 = br.ReadByte();
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteByte(this.Unk00);
                    bw.WriteByte(this.Unk01);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk510 : Event // 510
            {
                public override EventType Type => EventType.Unk510;

                internal Unk510(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk520 : Event // 520
            {
                public override EventType Type => EventType.Unk520;

                internal Unk520(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class KingOfTheStorm : Event // 522
            {
                public override EventType Type => EventType.KingOfTheStorm;

                public float Unk00;

                internal KingOfTheStorm(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadSingle(); // 0
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteSingle(this.Unk00);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk600 : Event // 600
            {
                public override EventType Type => EventType.Unk600;

                public int Mask;

                internal Unk600(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Mask = br.ReadInt32();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.Mask);
                    bw.WriteInt32(0);
                }
            }

            public class Unk601 : Event // 601
            {
                public override EventType Type => EventType.Unk601;

                public int StayAnimType;
                public float Unk04;
                public float Unk08;

                internal Unk601(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.StayAnimType = br.ReadInt32();
                    this.Unk04 = br.ReadSingle();
                    this.Unk08 = br.ReadSingle();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.StayAnimType);
                    bw.WriteSingle(this.Unk04);
                    bw.WriteSingle(this.Unk08);
                    bw.WriteInt32(0);
                }
            }

            // "TAE Debug Anim Speed" -Pav
            public class DebugAnimSpeed : Event // 603
            {
                public override EventType Type => EventType.DebugAnimSpeed;

                public uint AnimSpeed;

                internal DebugAnimSpeed(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.AnimSpeed = br.ReadUInt32();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteUInt32(this.AnimSpeed);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk605 : Event // 605
            {
                public override EventType Type => EventType.Unk605;

                public bool Unk00;
                public byte Unk01;
                public byte Unk02;
                public byte Unk03;
                public int Unk04;
                public float Unk08;
                public float Unk0C;

                internal Unk605(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadBoolean();
                    this.Unk01 = br.ReadByte();
                    this.Unk02 = br.ReadByte();
                    this.Unk03 = br.ReadByte();
                    this.Unk04 = br.ReadInt32();
                    this.Unk08 = br.ReadSingle();
                    this.Unk0C = br.ReadSingle();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteBoolean(this.Unk00);
                    bw.WriteByte(this.Unk01);
                    bw.WriteByte(this.Unk02);
                    bw.WriteByte(this.Unk03);
                    bw.WriteInt32(this.Unk04);
                    bw.WriteSingle(this.Unk08);
                    bw.WriteSingle(this.Unk0C);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk606 : Event // 606
            {
                public override EventType Type => EventType.Unk606;

                public byte Unk00;
                public byte Unk04;
                public byte Unk06;

                internal Unk606(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadByte(); // 0
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    this.Unk04 = br.ReadByte();
                    _ = br.AssertByte(0);
                    this.Unk06 = br.ReadByte();
                    _ = br.AssertByte(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteByte(this.Unk00);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(this.Unk04);
                    bw.WriteByte(0);
                    bw.WriteByte(this.Unk06);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk700 : Event // 700
            {
                public override EventType Type => EventType.Unk700;

                public float Unk00;
                public float Unk04;
                public float Unk08;
                public float Unk0C;
                public int Unk10;
                // 6 - head turn
                public sbyte Unk14;
                public float Unk18;
                public float Unk1C;
                public float Unk20;
                public float Unk24;

                internal Unk700(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadSingle();
                    this.Unk04 = br.ReadSingle();
                    this.Unk08 = br.ReadSingle();
                    this.Unk0C = br.ReadSingle();
                    this.Unk10 = br.ReadInt32();
                    this.Unk14 = br.ReadSByte();
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    this.Unk18 = br.ReadSingle();
                    this.Unk1C = br.ReadSingle();
                    this.Unk20 = br.ReadSingle();
                    this.Unk24 = br.ReadSingle();
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteSingle(this.Unk00);
                    bw.WriteSingle(this.Unk04);
                    bw.WriteSingle(this.Unk08);
                    bw.WriteSingle(this.Unk0C);
                    bw.WriteInt32(this.Unk10);
                    bw.WriteSByte(this.Unk14);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteSingle(this.Unk18);
                    bw.WriteSingle(this.Unk1C);
                    bw.WriteSingle(this.Unk20);
                    bw.WriteSingle(this.Unk24);
                }
            }

            public class EnableTurningDirection : Event // 703
            {
                public override EventType Type => EventType.EnableTurningDirection;

                public byte State;

                internal EnableTurningDirection(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.State = br.ReadByte();
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteByte(this.State);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class FacingAngleCorrection : Event // 705
            {
                public override EventType Type => EventType.FacingAngleCorrection;

                public float CorrectionRate;

                internal FacingAngleCorrection(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.CorrectionRate = br.ReadSingle();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteSingle(this.CorrectionRate);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk707 : Event // 707
            {
                public override EventType Type => EventType.Unk707;

                internal Unk707(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            // Used for Follower's Javelin WA
            // "Ladder State" -Pav
            public class HideWeapon : Event // 710
            {
                public override EventType Type => EventType.HideWeapon;

                public byte Unk00;
                public byte Unk01;
                public byte Unk02;
                public byte Unk03;

                internal HideWeapon(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadByte();
                    this.Unk01 = br.ReadByte();
                    this.Unk02 = br.ReadByte();
                    this.Unk03 = br.ReadByte();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteByte(this.Unk00);
                    bw.WriteByte(this.Unk01);
                    bw.WriteByte(this.Unk02);
                    bw.WriteByte(this.Unk03);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class HideModelMask : Event // 711
            {
                public override EventType Type => EventType.HideModelMask;

                public byte[] Mask { get; private set; }

                internal HideModelMask(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) => this.Mask = br.ReadBytes(32);

                internal override void WriteSpecific(BinaryWriterEx bw) => bw.WriteBytes(this.Mask);
            }

            public class DamageLevelModule : Event // 712
            {
                public override EventType Type => EventType.DamageLevelModule;

                public byte[] Mask { get; private set; }
                public byte Unk10;
                public byte Unk11;
                public byte Unk12;

                internal DamageLevelModule(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Mask = br.ReadBytes(16);
                    this.Unk10 = br.ReadByte();
                    this.Unk11 = br.ReadByte();
                    this.Unk12 = br.ReadByte();
                    _ = br.AssertByte(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteBytes(this.Mask);
                    bw.WriteByte(this.Unk10);
                    bw.WriteByte(this.Unk11);
                    bw.WriteByte(this.Unk12);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class ModelMask : Event // 713
            {
                public override EventType Type => EventType.ModelMask;

                public byte[] Mask { get; private set; }

                internal ModelMask(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) => this.Mask = br.ReadBytes(32);

                internal override void WriteSpecific(BinaryWriterEx bw) => bw.WriteBytes(this.Mask);
            }

            public class DamageLevelFunction : Event // 714
            {
                public override EventType Type => EventType.DamageLevelFunction;

                public byte Unk00;

                internal DamageLevelFunction(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadByte();
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteByte(this.Unk00);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk715 : Event // 715
            {
                public override EventType Type => EventType.Unk715;

                public byte Unk00;
                public byte Unk01;
                public byte Unk02;
                public byte Unk03;
                public byte Unk04;
                public byte Unk05;
                public byte Unk06;
                public byte Unk07;

                internal Unk715(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadByte();
                    this.Unk01 = br.ReadByte();
                    this.Unk02 = br.ReadByte();
                    this.Unk03 = br.ReadByte();
                    this.Unk04 = br.ReadByte();
                    this.Unk05 = br.ReadByte();
                    this.Unk06 = br.ReadByte();
                    this.Unk07 = br.ReadByte();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteByte(this.Unk00);
                    bw.WriteByte(this.Unk01);
                    bw.WriteByte(this.Unk02);
                    bw.WriteByte(this.Unk03);
                    bw.WriteByte(this.Unk04);
                    bw.WriteByte(this.Unk05);
                    bw.WriteByte(this.Unk06);
                    bw.WriteByte(this.Unk07);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class CultStart : Event // 720
            {
                public override EventType Type => EventType.CultStart;

                public byte CultType;

                internal CultStart(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.CultType = br.ReadByte(); // 0
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteByte(this.CultType);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk730 : Event // 730
            {
                public override EventType Type => EventType.Unk730;

                public int Unk00;
                public int Unk04;

                internal Unk730(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadInt32();
                    this.Unk04 = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.Unk00);
                    bw.WriteInt32(this.Unk04);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk740 : Event // 740
            {
                public override EventType Type => EventType.Unk740;

                internal Unk740(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class IFrameState : Event // 760
            {
                public override EventType Type => EventType.IFrameState;

                public byte Unk00;
                public byte Unk01;
                public byte Unk02;
                public byte Unk03;
                public float Unk04;
                public float Unk08;
                public float Unk0C;
                public float Unk10;
                public float Unk14;

                internal IFrameState(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadByte();
                    this.Unk01 = br.ReadByte();
                    this.Unk02 = br.ReadByte();
                    this.Unk03 = br.ReadByte();
                    this.Unk04 = br.ReadSingle();
                    this.Unk08 = br.ReadSingle();
                    this.Unk0C = br.ReadSingle();
                    this.Unk10 = br.ReadSingle();
                    this.Unk14 = br.ReadSingle();
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteByte(this.Unk00);
                    bw.WriteByte(this.Unk01);
                    bw.WriteByte(this.Unk02);
                    bw.WriteByte(this.Unk03);
                    bw.WriteSingle(this.Unk04);
                    bw.WriteSingle(this.Unk08);
                    bw.WriteSingle(this.Unk0C);
                    bw.WriteSingle(this.Unk10);
                    bw.WriteSingle(this.Unk14);
                }
            }

            public class BonePos : Event // 770
            {
                public override EventType Type => EventType.BonePos;

                public int Unk00;
                public float Unk04;
                public byte Unk08;

                internal BonePos(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadInt32();
                    this.Unk04 = br.ReadSingle();
                    this.Unk08 = br.ReadByte();
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.Unk00);
                    bw.WriteSingle(this.Unk04);
                    bw.WriteByte(this.Unk08);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                }
            }

            public class BoneFixOn1 : Event // 771
            {
                public override EventType Type => EventType.BoneFixOn1;

                public byte BoneID;

                internal BoneFixOn1(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.BoneID = br.ReadByte();
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteByte(this.BoneID);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class BoneFixOn2 : Event // 772
            {
                public override EventType Type => EventType.BoneFixOn2;

                public int Unk00;
                public float Unk04;
                public byte Unk08;

                internal BoneFixOn2(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadInt32();
                    this.Unk04 = br.ReadSingle();
                    this.Unk08 = br.ReadByte();
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.Unk00);
                    bw.WriteSingle(this.Unk04);
                    bw.WriteByte(this.Unk08);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                }
            }

            public class TurnLowerBody : Event // 781
            {
                public override EventType Type => EventType.TurnLowerBody;

                public byte TurnState;

                internal TurnLowerBody(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.TurnState = br.ReadByte();
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteByte(this.TurnState);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk782 : Event // 782
            {
                public override EventType Type => EventType.Unk782;

                internal Unk782(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class SpawnBulletByCultSacrifice1 : Event // 785
            {
                public override EventType Type => EventType.SpawnBulletByCultSacrifice1;

                public float Unk00;
                public int DummyPointID;
                public int BulletID;
                public byte Unk0C;
                public byte Unk0D;

                internal SpawnBulletByCultSacrifice1(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadSingle();
                    this.DummyPointID = br.ReadInt32();
                    this.BulletID = br.ReadInt32();
                    this.Unk0C = br.ReadByte();
                    this.Unk0D = br.ReadByte();
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteSingle(this.Unk00);
                    bw.WriteInt32(this.DummyPointID);
                    bw.WriteInt32(this.BulletID);
                    bw.WriteByte(this.Unk0C);
                    bw.WriteByte(this.Unk0D);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                }
            }

            public class Unk786 : Event // 786
            {
                public override EventType Type => EventType.Unk786;

                public float Unk00;

                internal Unk786(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadSingle();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteSingle(this.Unk00);
                    bw.WriteInt32(0);
                }
            }

            public class Unk790 : Event // 790
            {
                public override EventType Type => EventType.Unk790;

                internal Unk790(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk791 : Event // 791
            {
                public override EventType Type => EventType.Unk791;

                internal Unk791(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class HitEffect2 : Event // 792
            {
                public override EventType Type => EventType.HitEffect2;

                public short Unk00;
                public int Unk04;
                public int Unk08;
                public byte Unk0C;
                public byte Unk0D;
                public byte Unk0E;
                public byte Unk0F;

                internal HitEffect2(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadInt16();
                    _ = br.AssertInt16(0);
                    this.Unk04 = br.ReadInt32();
                    this.Unk08 = br.ReadInt32();
                    this.Unk0C = br.ReadByte();
                    this.Unk0D = br.ReadByte();
                    this.Unk0E = br.ReadByte();
                    this.Unk0F = br.ReadByte();
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt16(this.Unk00);
                    bw.WriteInt16(0);
                    bw.WriteInt32(this.Unk04);
                    bw.WriteInt32(this.Unk08);
                    bw.WriteByte(this.Unk0C);
                    bw.WriteByte(this.Unk0D);
                    bw.WriteByte(this.Unk0E);
                    bw.WriteByte(this.Unk0F);
                }
            }

            public class CultSacrifice1 : Event // 793
            {
                public override EventType Type => EventType.CultSacrifice1;

                public int SacrificeValue;

                internal CultSacrifice1(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.SacrificeValue = br.ReadInt32();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.SacrificeValue);
                    bw.WriteInt32(0);
                }
            }

            public class SacrificeEmpty : Event // 794
            {
                public override EventType Type => EventType.SacrificeEmpty;

                internal SacrificeEmpty(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Toughness : Event // 795
            {
                public override EventType Type => EventType.Toughness;

                public byte ToughnessParamID;
                public bool IsToughnessEffective;
                public float ToughnessRate;

                public Toughness(float startTime, float endTime, byte toughnessParamID, bool isToughnessEffective, float toughnessRate) : base(startTime, endTime) {
                    this.ToughnessParamID = toughnessParamID;
                    this.IsToughnessEffective = isToughnessEffective;
                    this.ToughnessRate = toughnessRate;
                }

                internal Toughness(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.ToughnessParamID = br.ReadByte();
                    this.IsToughnessEffective = br.ReadBoolean();
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    this.ToughnessRate = br.ReadSingle();
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteByte(this.ToughnessParamID);
                    bw.WriteBoolean(this.IsToughnessEffective);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteSingle(this.ToughnessRate);
                }
            }

            public class BringCultMenu : Event // 796
            {
                public override EventType Type => EventType.BringCultMenu;

                public byte MenuType;

                internal BringCultMenu(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.MenuType = br.ReadByte();
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteByte(this.MenuType);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class CeremonyParamID : Event // 797
            {
                public override EventType Type => EventType.CeremonyParamID;

                public int ParamID;

                internal CeremonyParamID(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.ParamID = br.ReadInt32();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(this.ParamID);
                    bw.WriteInt32(0);
                }
            }

            public class CultSingle : Event // 798
            {
                public override EventType Type => EventType.CultSingle;

                public float Unk00;

                internal CultSingle(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.Unk00 = br.ReadSingle();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteSingle(this.Unk00);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class CultEmpty2 : Event // 799
            {
                public override EventType Type => EventType.CultEmpty2;

                internal CultEmpty2(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk800 : Event // 800
            {
                public override EventType Type => EventType.Unk800;

                public float MetersPerTick;
                public float MetersOnTurn;
                public float Unk08;

                internal Unk800(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) {
                    this.MetersPerTick = br.ReadSingle();
                    this.MetersOnTurn = br.ReadSingle();
                    this.Unk08 = br.ReadSingle();
                    _ = br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw) {
                    bw.WriteSingle(this.MetersPerTick);
                    bw.WriteSingle(this.MetersOnTurn);
                    bw.WriteSingle(this.Unk08);
                    bw.WriteInt32(0);
                }
            }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }
    }
}
