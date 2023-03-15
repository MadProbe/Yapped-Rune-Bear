using SoulsFormats.Util;

namespace SoulsFormats.Binder {
    internal static class BinderHashTable {
        public static void Assert(BinaryReaderEx br) {
            _ = br.ReadInt64(); // Hashes offset
            _ = br.ReadInt32(); // Bucket count
            _ = br.AssertByte(0x10); // Hash table header size?
            _ = br.AssertByte(8); // Bucket size?
            _ = br.AssertByte(8); // Hash size?
            _ = br.AssertByte(0);
            // Don't actually care about the hashes, I just like asserting
        }

        public static void Write(BinaryWriterEx bw, List<BinderFileHeader> files) {
            uint groupCount = 0;
            for (uint p = (uint)files.Count / 7; p <= 100000; p++) {
                if (SFUtil.IsPrime(p)) {
                    groupCount = p;
                    break;
                }
            }

            if (groupCount == 0) {
                throw new InvalidOperationException("Could not determine hash group count.");
            }

            var hashLists = new List<PathHash>[groupCount];
            var pathHashes = new PathHash[files.Count];
            for (int i = 0; i < groupCount; i++) {
                hashLists[i] = new List<PathHash>();
            }

            for (int i = 0; i < files.Count; i++) {
                var pathHash = new PathHash(i, files[i].Name);
                hashLists[pathHash.Hash % groupCount].Add(pathHash);
                pathHashes[i] = pathHash;
            }

            var hashGroupsArray = new HashGroup[groupCount];

            for (int i = 0, count = 0; i < groupCount; i++) {
                hashLists[i].Sort((ph1, ph2) => ph1.Hash.CompareTo(ph2.Hash));
                hashGroupsArray[i] = new HashGroup(count, -count + (count += hashLists[i].Count));
            }

            bw.ReserveInt64("HashesOffset");
            bw.WriteUInt32(groupCount);

            bw.WriteByte(0x10);
            bw.WriteByte(8);
            bw.WriteByte(8);
            bw.WriteByte(0);

            foreach (HashGroup hashGroup in hashGroupsArray) {
                hashGroup.Write(bw);
            }

            bw.FillInt64("HashesOffset", bw.Position);
            foreach (PathHash pathHash in pathHashes) {
                pathHash.Write(bw);
            }
        }

        private class PathHash {
            public uint Hash;
            public int Index;

            public PathHash(BinaryReaderEx br) {
                this.Hash = br.ReadUInt32();
                this.Index = br.ReadInt32();
            }

            public PathHash(int index, string path) {
                this.Hash = SFUtil.FromPathHash(path);
                this.Index = index;
            }

            public void Write(BinaryWriterEx bw) {
                bw.WriteUInt32(this.Hash);
                bw.WriteInt32(this.Index);
            }
        }

        private class HashGroup {
            public int Index, Length;

            public HashGroup(BinaryReaderEx br) {
                this.Index = br.ReadInt32();
                this.Length = br.ReadInt32();
            }

            public HashGroup(int index, int length) {
                this.Index = index;
                this.Length = length;
            }

            public void Write(BinaryWriterEx bw) {
                bw.WriteInt32(this.Length);
                bw.WriteInt32(this.Index);
            }
        }
    }
}
