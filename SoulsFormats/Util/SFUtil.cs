using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using SoulsFormats.Binder.BND4;
using SoulsFormats.Formats;

namespace SoulsFormats.Util {
    /// <summary>
    /// Miscellaneous utility functions for SoulsFormats, mostly for internal use.
    /// </summary>
    public static partial class SFUtil {
        /// <summary>
        /// Guesses the extension of a file based on its contents.
        /// </summary>
        public static string GuessExtension(byte[] bytes, bool bigEndian = false) {
            bool dcx = false;
            if (DCX.Is(bytes)) {
                dcx = true;
                bytes = DCX.Decompress(bytes);
            }

            static bool checkMsb(BinaryReaderEx br) {
                if (br.Length < 8) {
                    return false;
                }

                int offset = br.GetInt32(4);
                if (offset < 0 || offset >= br.Length - 1) {
                    return false;
                }

                try {
                    return br.GetASCII(offset) == "MODEL_PARAM_ST";
                } catch {
                    return false;
                }
            }

            static bool checkParam(BinaryReaderEx br) => br.Length >= 0x2C && MyRegex().IsMatch(br.GetASCII(0xC, 0x20));

            static bool checkTdf(BinaryReaderEx br) {
                if (br.Length < 4) {
                    return false;
                }

                if (br.GetASCII(0, 1) != "\"") {
                    return false;
                }

                for (int i = 1; i < br.Length; i++) {
                    if (br.GetASCII(i, 1) == "\"") {
                        return i < br.Length - 2 && br.GetASCII(i + 1, 2) == "\r\n";
                    }
                }
                return false;
            }

            string ext = "";
            using (var ms = new MemoryStream(bytes)) {
                var br = new BinaryReaderEx(bigEndian, ms);
                string magic = null;
                if (br.Length >= 4) {
                    magic = br.ReadASCII(4);
                }

                if (magic == "AISD") {
                    ext = ".aisd";
                } else if (magic is "BDF3" or "BDF4") {
                    ext = ".bdt";
                } else if (magic is "BHF3" or "BHF4") {
                    ext = ".bhd";
                } else if (magic is "BND3" or "BND4") {
                    ext = ".bnd";
                } else if (magic == "DDS ") {
                    ext = ".dds";
                }
                // ESD or FFX
                else if (magic?.ToUpper() == "DLSE") {
                    ext = ".dlse";
                } else if (bigEndian && magic == "\0BRD" || !bigEndian && magic == "DRB\0") {
                    ext = ".drb";
                } else if (magic == "EDF\0") {
                    ext = ".edf";
                } else if (magic == "ELD\0") {
                    ext = ".eld";
                } else if (magic == "ENFL") {
                    ext = ".entryfilelist";
                } else if (magic?.ToUpper() == "FSSL") {
                    ext = ".esd";
                } else if (magic == "EVD\0") {
                    ext = ".evd";
                } else if (br.Length >= 3 && br.GetASCII(0, 3) == "FEV" || br.Length >= 0x10 && br.GetASCII(8, 8) == "FEV FMT ") {
                    ext = ".fev";
                } else if (br.Length >= 6 && br.GetASCII(0, 6) == "FLVER\0") {
                    ext = ".flver";
                } else if (br.Length >= 3 && br.GetASCII(0, 3) == "FSB") {
                    ext = ".fsb";
                } else if (br.Length >= 3 && br.GetASCII(0, 3) == "GFX") {
                    ext = ".gfx";
                } else if (br.Length >= 0x19 && br.GetASCII(0xC, 0xE) == "ITLIMITER_INFO") {
                    ext = ".itl";
                } else if (br.Length >= 4 && br.GetASCII(1, 3) == "Lua") {
                    ext = ".lua";
                } else if (checkMsb(br)) {
                    ext = ".msb";
                } else if (br.Length >= 0x30 && br.GetASCII(0x2C, 4) == "MTD ") {
                    ext = ".mtd";
                } else if (magic == "DFPN") {
                    ext = ".nfd";
                } else if (checkParam(br)) {
                    ext = ".param";
                } else if (br.Length >= 4 && br.GetASCII(1, 3) == "PNG") {
                    ext = ".png";
                } else if (br.Length >= 0x2C && br.GetASCII(0x28, 4) == "SIB ") {
                    ext = ".sib";
                } else if (magic == "TAE ") {
                    ext = ".tae";
                } else if (checkTdf(br)) {
                    ext = ".tdf";
                } else if (magic == "TPF\0") {
                    ext = ".tpf";
                } else if (magic == "#BOM") {
                    ext = ".txt";
                } else if (br.Length >= 5 && br.GetASCII(0, 5) == "<?xml") {
                    ext = ".xml";
                }
                // This is pretty sketchy
                else if (br.Length >= 0xC && br.GetByte(0) == 0 && br.GetByte(3) == 0 && br.GetInt32(4) == br.Length && br.GetInt16(0xA) == 0) {
                    ext = ".fmg";
                }
            }

            return dcx ? $"{ext}.dcx" : ext;
        }

        /// <summary>
        /// Reverses the order of bits in a byte, probably very inefficiently.
        /// </summary>
        public static byte ReverseBits(byte value) => (byte)(
                (value & 0b00000001) << 7 |
                (value & 0b00000010) << 5 |
                (value & 0b00000100) << 3 |
                (value & 0b00001000) << 1 |
                (value & 0b00010000) >> 1 |
                (value & 0b00100000) >> 3 |
                (value & 0b01000000) >> 5 |
                (value & 0b10000000) >> 7
                );

        /// <summary>
        /// Makes a backup of a file if not already found, and returns the backed-up path.
        /// </summary>
        public static string Backup(string file, bool overwrite = false) {
            string bak = file + ".bak";
            if (overwrite || !File.Exists(bak)) {
                File.Copy(file, bak, overwrite);
            }

            return bak;
        }

        /// <summary>
        /// Returns the extension of the specified file path, removing .dcx if present.
        /// </summary>
        public static string GetRealExtension(string path) {
            string extension = Path.GetExtension(path);
            if (extension == ".dcx") {
                extension = Path.GetExtension(Path.GetFileNameWithoutExtension(path));
            }

            return extension;
        }

        /// <summary>
        /// Returns the file name of the specified path, removing both .dcx if present and the actual extension.
        /// </summary>
        public static string GetRealFileName(string path) {
            string name = Path.GetFileNameWithoutExtension(path);
            if (Path.GetExtension(path) == ".dcx") {
                name = Path.GetFileNameWithoutExtension(name);
            }

            return name;
        }

        /// <summary>
        /// Decompresses data and returns a new BinaryReaderEx if necessary.
        /// </summary>
        public static BinaryReaderEx GetDecompressedBR(BinaryReaderEx br, out DCX.Type compression) {
            if (DCX.Is(br)) {
                byte[] bytes = DCX.Decompress(br, out compression);
                return new BinaryReaderEx(false, bytes);
            } else {
                compression = DCX.Type.None;
                return br;
            }
        }

        /// <summary>
        /// FromSoft's basic filename hashing algorithm, used in some BND and BXF formats.
        /// </summary>
        public static uint FromPathHash(string text) {
            string hashable = text.ToLowerInvariant().Replace('\\', '/');
            if (!hashable.StartsWith("/")) {
                hashable = '/' + hashable;
            }

            return hashable.Aggregate(0u, (i, c) => i * 37u + c);
        }

        /// <summary>
        /// Determines whether a number is prime or not.
        /// </summary>
        public static bool IsPrime(uint candidate) {
            if ((candidate & 1) == 0 || candidate == 1) {
                return candidate == 2;
            }

            for (int i = 3; i * i <= candidate; i += 2) {
                if (candidate % i == 0) {
                    return false;
                }
            }

            return true;
        }

        private static readonly Regex timestampRx = MyRegex1();

        /// <summary>
        /// Converts a BND/BXF timestamp string to a DateTime object.
        /// </summary>
        public static DateTime BinderTimestampToDate(string timestamp) {
            Match match = timestampRx.Match(timestamp);
            if (!match.Success) {
                throw new InvalidDataException("Unrecognized timestamp format.");
            }

            int year = int.Parse(match.Groups[1].Value) + 2000;
            int month = match.Groups[2].Value[0] - 'A';
            int day = int.Parse(match.Groups[3].Value);
            int hour = match.Groups[4].Value[0] - 'A';
            int minute = int.Parse(match.Groups[5].Value);

            return new DateTime(year, month, day, hour, minute, 0);
        }

        /// <summary>
        /// Converts a DateTime object to a BND/BXF timestamp string.
        /// </summary>
        public static string DateToBinderTimestamp(DateTime dateTime) {
            int year = dateTime.Year - 2000;
            if (year is < 0 or > 99) {
                throw new InvalidDataException("BND timestamp year must be between 2000 and 2099 inclusive.");
            }

            char month = (char)(dateTime.Month + 'A');
            int day = dateTime.Day;
            char hour = (char)(dateTime.Hour + 'A');
            int minute = dateTime.Minute;

            return $"{year:D2}{month}{day}{hour}{minute}".PadRight(8, '\0');
        }

        /// <summary>
        /// Compresses data and writes it to a BinaryWriterEx with Zlib wrapper.
        /// </summary>
        public static int WriteZlib(BinaryWriterEx bw, byte formatByte, byte[] input) {
            long start = bw.Position;
            bw.WriteByte(0x78);
            bw.WriteByte(formatByte);

            using (var deflateStream = new DeflateStream(bw.Stream, CompressionMode.Compress, true)) {
                deflateStream.Write(input, 0, input.Length);
            }

            bw.WriteUInt32(Adler32(input));
            return (int)(bw.Position - start);
        }

        /// <summary>
        /// Reads a Zlib block from a BinaryReaderEx and returns the uncompressed data.
        /// </summary>
        public static byte[] ReadZlib(BinaryReaderEx br, int compressedSize) {
            _ = br.AssertByte(0x78);
            _ = br.AssertByte(0x01, 0x5E, 0x9C, 0xDA);
            byte[] compressed = br.ReadBytes(compressedSize - 2);

            using var decompressedStream = new MemoryStream(compressedSize * 2); // in most cases compression ratio is ~= 50%
            using var compressedStream = new MemoryStream(compressed);
            using var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress, true);
            deflateStream.CopyTo(decompressedStream);
            return decompressedStream.ToArray();
        }

        /// <summary>
        /// Computes an Adler32 checksum used by Zlib.
        /// </summary>
        public static uint Adler32(byte[] data) {
            uint adlerA = 1;
            uint adlerB = 0;

            for (int i = 0, length = data.Length; i < length; i++) {
                adlerA = (adlerA + data[i]) % 0xFFF1;
                adlerB = (adlerB + adlerA) % 0xFFF1;
            }

            return adlerB << 16 | adlerA;
        }

        /// <summary>
        /// Concatenates multiple collections into one list.
        /// </summary>
        public static List<T> ConcatAll<T>(params IEnumerable<T>[] lists) {
            IEnumerable<T> all = Array.Empty<T>();
            foreach (IEnumerable<T> list in lists) {
                all = all.Concat(list);
            }

            return all.ToList();
        }

        /// <summary>
        /// Convert a list to a dictionary with indices as keys.
        /// </summary>
        public static Dictionary<int, T> Dictionize<T>(List<T> items) {
            var dict = new Dictionary<int, T>(items.Count);
            for (int i = 0; i < items.Count; i++) {
                dict[i] = items[i];
            }

            return dict;
        }

        /// <summary>
        /// Converts a hex string in format "AA BB CC DD" to a byte array.
        /// </summary>
        public static byte[] ParseHexString(string str) {
            string[] strings = str.Split(' ');
            byte[] bytes = new byte[strings.Length];
            for (int i = 0; i < strings.Length; i++) {
                bytes[i] = Convert.ToByte(strings[i], 16);
            }

            return bytes;
        }

        /// <summary>
        /// Returns a copy of the key used for encrypting original DS2 save files on PC.
        /// </summary>
        public static byte[] GetDS2SaveKey() => (byte[])ds2SaveKey.Clone();
        private static readonly byte[] ds2SaveKey = ParseHexString("B7 FD 46 3E 4A 9C 11 02 DF 17 39 E5 F3 B2 A5 0F");

        /// <summary>
        /// Returns a copy of the key used for encrypting DS2 SotFS save files on PC.
        /// </summary>
        public static byte[] GetScholarSaveKey() => (byte[])scholarSaveKey.Clone();
        private static readonly byte[] scholarSaveKey = ParseHexString("59 9F 9B 69 96 40 A5 52 36 EE 2D 70 83 5E C7 44");

        /// <summary>
        /// Returns a copy of the key used for encrypting DS3 save files on PC.
        /// </summary>
        public static byte[] GetDS3SaveKey() => (byte[])ds3SaveKey.Clone();
        private static readonly byte[] ds3SaveKey = ParseHexString("FD 46 4D 69 5E 69 A3 9A 10 E3 19 A7 AC E8 B7 FA");

        /// <summary>
        /// Decrypts a file from a DS2/DS3 SL2. Do not remove the hash and IV before calling.
        /// </summary>
        public static byte[] DecryptSL2File(byte[] encrypted, byte[] key) {
            // Just leaving this here for documentation
            //byte[] hash = new byte[16];
            //Buffer.BlockCopy(encrypted, 0, hash, 0, 16);

            byte[] iv = new byte[16];
            Buffer.BlockCopy(encrypted, 16, iv, 0, 16);

            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.BlockSize = 128;
            // PKCS7-style padding is used, but they don't include the minimum padding
            // so it can't be stripped safely
            aes.Padding = PaddingMode.None;
            aes.Key = key;
            aes.IV = iv;

            ICryptoTransform decryptor = aes.CreateDecryptor();
            using var encStream = new MemoryStream(encrypted, 32, encrypted.Length - 32);
            using var cryptoStream = new CryptoStream(encStream, decryptor, CryptoStreamMode.Read);
            using var decStream = new MemoryStream();
            cryptoStream.CopyTo(decStream);
            return decStream.ToArray();
        }

        /// <summary>
        /// Encrypts a file for a DS2/DS3 SL2. Result includes the hash and IV.
        /// </summary>
        public static byte[] EncryptSL2File(byte[] decrypted, byte[] key) {
            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.BlockSize = 128;
            aes.Padding = PaddingMode.None;
            aes.Key = key;
            aes.GenerateIV();

            ICryptoTransform encryptor = aes.CreateEncryptor();
            using var decStream = new MemoryStream(decrypted);
            using var cryptoStream = new CryptoStream(decStream, encryptor, CryptoStreamMode.Read);
            using var encStream = new MemoryStream();
            encStream.Write(aes.IV, 0, 16);
            cryptoStream.CopyTo(encStream);
            byte[] encrypted = new byte[encStream.Length + 16];
            encStream.Position = 0;
            _ = encStream.Read(encrypted, 16, (int)encStream.Length);
            byte[] hash = MD5.HashData(encrypted.AsSpan(16, (int)encStream.Length));
            Buffer.BlockCopy(hash, 0, encrypted, 0, 16);
            return encrypted;
        }

        private static readonly byte[] ds3RegulationKey = SFEncoding.ASCII.GetBytes("ds3#jn/8_7(rsY9pg55GFN7VFL#+3n/)");

        /// <summary>
        /// Decrypts and unpacks DS3's regulation BND4 from the specified path.
        /// </summary>
        public static BND4 DecryptDS3Regulation(string path) => BND4.Read(DecryptByteArray(ds3RegulationKey, File.ReadAllBytes(path)));

        /// <summary>
        /// Repacks and encrypts DS3's regulation BND4 to the specified path.
        /// </summary>
        public static void EncryptDS3Regulation(string path, BND4 bnd) {
            _ = Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, EncryptByteArray(ds3RegulationKey, bnd.Write()));
        }

        private static readonly byte[] erRegulationKey = ParseHexString("99 BF FC 36 6A 6B C8 C6 F5 82 7D 09 36 02 D6 76 C4 28 92 A0 1C 20 7F B0 24 D3 AF 4E 49 3F EF 99");

        /// <summary>
        /// Decrypts and unpacks ER's regulation BND4 from the specified path.
        /// </summary>
        public static BND4 DecryptERRegulation(string path) => BND4.Read(DecryptByteArrayStreaming(erRegulationKey,
            new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 0x400, FileOptions.SequentialScan),
                    (int)new FileInfo(path).Length));

        /// <summary>
        /// Repacks and encrypts ER's regulation BND4 to the specified path.
        /// </summary>
        public static void EncryptERRegulation(string path, BND4 bnd) {
            _ = Directory.CreateDirectory(Path.GetDirectoryName(path));
            EncryptByteArrayStreaming(erRegulationKey, bnd.Write(), path);
        }
        private const int IV_SIZE = 128 / 8; // basically 16

        private static byte[] EncryptByteArray(byte[] key, byte[] secret) {
            // Avoid extraneous allocations and copying
            byte[] bytes = GC.AllocateUninitializedArray<byte>(secret.Length + (0x10 - secret.Length & 0xf) + IV_SIZE);
            using var ms = new MemoryStream(bytes, writable: true);
            using Aes cryptor = CreateAES(CipherMode.CBC, PaddingMode.PKCS7, keySize: 256, blockSize: 128);

            byte[] iv = cryptor.IV;
            ms.Write(iv, 0, IV_SIZE);

            using var cs = new CryptoStream(ms, cryptor.CreateEncryptor(key, iv), CryptoStreamMode.Write, true);

            cs.Write(secret, 0, secret.Length);
            return bytes;
        }

        private static void EncryptByteArrayStreaming(byte[] key, byte[] data, string path) {
            using var fileStream = new FileStream(path, new FileStreamOptions {
                Access = FileAccess.Write,
                BufferSize = 0,
                Options = FileOptions.SequentialScan,
                Share = FileShare.None
            });
            using var ms = new MemoryStream(data);
            using Aes cryptor = CreateAES(CipherMode.CBC, PaddingMode.PKCS7, keySize: 256, blockSize: 128);

            byte[] iv = cryptor.IV;
            fileStream.Write(iv);

            using var cs = new CryptoStream(fileStream, cryptor.CreateEncryptor(key, iv), CryptoStreamMode.Write, true);
            ms.CopyTo(cs, 0x80);
        }

        private static byte[] DecryptByteArray(byte[] key, byte[] secret) {
            byte[] iv = GC.AllocateUninitializedArray<byte>(IV_SIZE);

            Buffer.BlockCopy(secret, 0, iv, 0, IV_SIZE);

            byte[] buffer = GC.AllocateUninitializedArray<byte>(secret.Length - IV_SIZE);
            using var ms = new MemoryStream(buffer, writable: true);
            using Aes cryptor = CreateAES(CipherMode.CBC, PaddingMode.None, keySize: 256, blockSize: 128);
            using var cs = new CryptoStream(ms, cryptor.CreateDecryptor(key, iv), CryptoStreamMode.Write);

            cs.Write(secret, IV_SIZE, secret.Length - IV_SIZE);
            return buffer;
        }

        private static unsafe byte[] DecryptByteArrayStreaming(byte[] key, FileStream secret, int file_size) {
            byte[] iv = GC.AllocateUninitializedArray<byte>(IV_SIZE);
            _ = secret.Read(iv, 0, IV_SIZE);


            byte[] buffer = GC.AllocateUninitializedArray<byte>(file_size - IV_SIZE);
            using var ms = new MemoryStream(buffer, writable: true);
            using Aes cryptor = CreateAES(CipherMode.CBC, PaddingMode.None, keySize: 256, blockSize: 128);
            using var cs = new CryptoStream(ms, cryptor.CreateDecryptor(key, iv), CryptoStreamMode.Write);
            const int BLOCK_SIZE = 128 / 16;
            //byte* bytes = stackalloc byte[BLOCK_SIZE + 16]; // block size span
            //((nint*)bytes)[0] = typeof(byte[]).TypeHandle.Value;
            //((nint*)bytes)[1] = BLOCK_SIZE;
            byte[] bytesArray = GC.AllocateUninitializedArray<byte>(BLOCK_SIZE);
            int bytesWritten;
            while ((bytesWritten = secret.Read(bytesArray)) != 0) {
                cs.Write(bytesArray, offset: 0, bytesWritten);
            }
            return buffer;
        }

        internal static Aes CreateAES(CipherMode cipherMode, PaddingMode paddingMode, int keySize, int blockSize) {
            var aes = Aes.Create();
            aes.Mode = cipherMode;
            aes.Padding = paddingMode;
            aes.KeySize = keySize;
            aes.BlockSize = blockSize;
            return aes;
        }

        [GeneratedRegex("^[^\0]+\0 *$")]
        private static partial Regex MyRegex();
        [GeneratedRegex("(\\d\\d)(\\w)(\\d+)(\\w)(\\d+)")]
        private static partial Regex MyRegex1();
    }
}
