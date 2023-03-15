using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using SoulsFormats.Util;

namespace SoulsFormats {
    /// <summary>
    /// An SFX configuration format used in DeS and DS2; only DS2 is supported. Extension: .ffx
    /// </summary>
    public partial class FFXDLSE : SoulsFile<FFXDLSE> {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public FXEffect Effect { get; set; }

        public FFXDLSE() => this.Effect = new FXEffect();

        protected internal override bool Is(BinaryReaderEx br) {
            if (br.Length < 4) {
                return false;
            }

            string magic = br.GetASCII(0, 4);
            return magic == "DLsE";
        }

        protected internal override void Read(BinaryReaderEx br) {
            br.BigEndian = false;
            _ = br.AssertASCII("DLsE");
            _ = br.AssertByte(1);
            _ = br.AssertByte(3);
            _ = br.AssertByte(0);
            _ = br.AssertByte(0);
            _ = br.AssertInt32(0);
            _ = br.AssertInt32(0);
            _ = br.AssertByte(0);
            _ = br.AssertInt32(1);
            short classNameCount = br.ReadInt16();

            var classNames = new List<string>(classNameCount);
            for (int i = 0; i < classNameCount; i++) {
                int length = br.ReadInt32();
                classNames.Add(br.ReadASCII(length));
            }

            this.Effect = new FXEffect(br, classNames);
        }

        protected internal override void Write(BinaryWriterEx bw) {
            var classNames = new List<string>();
            this.Effect.AddClassNames(classNames);

            bw.BigEndian = false;
            bw.WriteASCII("DLsE");
            bw.WriteByte(1);
            bw.WriteByte(3);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteByte(0);
            bw.WriteInt32(1);
            bw.WriteInt16((short)classNames.Count);

            foreach (string className in classNames) {
                bw.WriteInt32(className.Length);
                bw.WriteASCII(className);
            }

            this.Effect.Write(bw, classNames);
        }

        #region XML Serialization
        private static XmlSerializer _ffxSerializer;
        private static XmlSerializer _stateSerializer;
        private static XmlSerializer _paramSerializer;

        private static XmlSerializer MakeSerializers(int returnIndex) {
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            XmlSerializer[] serializers = XmlSerializer.FromTypes(new Type[] { typeof(FFXDLSE), typeof(State), typeof(Param) });
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code

            _ffxSerializer = serializers[0];
            _stateSerializer = serializers[1];
            _paramSerializer = serializers[2];
            return serializers[returnIndex];
        }

        private static XmlSerializer FFXSerializer => _ffxSerializer ?? MakeSerializers(0);
        private static XmlSerializer StateSerializer => _stateSerializer ?? MakeSerializers(1);
        private static XmlSerializer ParamSerializer => _paramSerializer ?? MakeSerializers(2);

        public static FFXDLSE XmlDeserialize(Stream stream)
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            => (FFXDLSE)FFXSerializer.Deserialize(stream);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code

        public static FFXDLSE XmlDeserialize(TextReader textReader)
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            => (FFXDLSE)FFXSerializer.Deserialize(textReader);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code

        public static FFXDLSE XmlDeserialize(XmlReader xmlReader)
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            => (FFXDLSE)FFXSerializer.Deserialize(xmlReader);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code

        public void XmlSerialize(Stream stream)
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            => FFXSerializer.Serialize(stream, this);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code

        public void XmlSerialize(TextWriter textWriter)
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            => FFXSerializer.Serialize(textWriter, this);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code

        public void XmlSerialize(XmlWriter xmlWriter)
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            => FFXSerializer.Serialize(xmlWriter, this);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
        #endregion

        private static class DLVector {
            public static List<int> Read(BinaryReaderEx br, List<string> classNames) {
                _ = br.AssertInt16((short)(classNames.IndexOf("DLVector") + 1));
                int count = br.ReadInt32();
                return new List<int>(br.ReadInt32s(count));
            }

            public static void AddClassNames(List<string> classNames) {
                if (!classNames.Contains("DLVector")) {
                    classNames.Add("DLVector");
                }
            }

            public static void Write(BinaryWriterEx bw, List<string> classNames, List<int> vector) {
                bw.WriteInt16((short)(classNames.IndexOf("DLVector") + 1));
                bw.WriteInt32(vector.Count);
                bw.WriteInt32s(vector);
            }
        }

        public abstract class FXSerializable {
            internal abstract string ClassName { get; }

            internal abstract int Version { get; }

            internal FXSerializable() { }

            internal FXSerializable(BinaryReaderEx br, List<string> classNames) {
                long start = br.Position;
                _ = br.AssertInt16((short)(classNames.IndexOf(this.ClassName) + 1));
                _ = br.AssertInt32(this.Version);
                int length = br.ReadInt32();
                this.Deserialize(br, classNames);
                if (br.Position != start + length) {
                    throw new InvalidDataException("Failed to read all object data (or read too much of it).");
                }
            }

            protected internal abstract void Deserialize(BinaryReaderEx br, List<string> classNames);

            internal virtual void AddClassNames(List<string> classNames) {
                if (!classNames.Contains(this.ClassName)) {
                    classNames.Add(this.ClassName);
                }
            }

            internal void Write(BinaryWriterEx bw, List<string> classNames) {
                long start = bw.Position;
                bw.WriteInt16((short)(classNames.IndexOf(this.ClassName) + 1));
                bw.WriteInt32(this.Version);
                bw.ReserveInt32($"{start:X}Length");
                this.Serialize(bw, classNames);
                bw.FillInt32($"{start:X}Length", (int)(bw.Position - start));
            }

            protected internal abstract void Serialize(BinaryWriterEx bw, List<string> classNames);
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
