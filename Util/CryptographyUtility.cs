using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace Chomp.Util {
    // Token: 0x02000006 RID: 6
    public static class CryptographyUtility {
        // Token: 0x060000F7 RID: 247 RVA: 0x0000EBC4 File Offset: 0x0000CDC4
        public static byte[] DecryptAesEcb(Stream inputStream, byte[] key) {
            BufferedBlockCipher cipher = CreateAesEcbCipher(key);
            return DecryptAes(inputStream, cipher, inputStream.Length);
        }

        // Token: 0x060000F8 RID: 248 RVA: 0x0000EBE8 File Offset: 0x0000CDE8
        public static byte[] DecryptAesCbc(Stream inputStream, byte[] key, byte[] iv) {
            IBlockCipher blockCipher = new AesEngine();
            ICipherParameters parameters = new ParametersWithIV(new KeyParameter(key), iv);
            var cipher = new BufferedBlockCipher(new CbcBlockCipher(blockCipher));
            cipher.Init(false, parameters);
            return DecryptAes(inputStream, cipher, inputStream.Length);
        }

        // Token: 0x060000F9 RID: 249 RVA: 0x0000EC28 File Offset: 0x0000CE28
        public static byte[] DecryptAesCtr(Stream inputStream, byte[] key, byte[] iv) {
            IBlockCipher blockCipher = new AesEngine();
            ICipherParameters parameters = new ParametersWithIV(new KeyParameter(key), iv);
            var cipher = new BufferedBlockCipher(new SicBlockCipher(blockCipher));
            cipher.Init(false, parameters);
            return DecryptAes(inputStream, cipher, inputStream.Length);
        }

        // Token: 0x060000FA RID: 250 RVA: 0x0000EC67 File Offset: 0x0000CE67
        public static byte[] EncryptAesCtr(byte[] input, byte[] key, byte[] iv) {
            IBufferedCipher cipher = CipherUtilities.GetCipher("AES/CTR/NoPadding");
            cipher.Init(true, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), iv));
            return cipher.DoFinal(input);
        }

        // Token: 0x060000FB RID: 251 RVA: 0x0000EC94 File Offset: 0x0000CE94
        private static BufferedBlockCipher CreateAesEcbCipher(byte[] key) {
            IBlockCipher blockCipher = new AesEngine();
            var parameter = new KeyParameter(key);
            var bufferedBlockCipher = new BufferedBlockCipher(blockCipher);
            bufferedBlockCipher.Init(false, parameter);
            return bufferedBlockCipher;
        }

        // Token: 0x060000FC RID: 252 RVA: 0x0000ECBC File Offset: 0x0000CEBC
        private static byte[] DecryptAes(Stream inputStream, BufferedBlockCipher cipher, long length) {
            int blockSize = cipher.GetBlockSize();
            int inputLength = (int)length;
            int paddedLength = inputLength;
            if (paddedLength % blockSize > 0) {
                paddedLength += blockSize - paddedLength % blockSize;
            }
            byte[] input = new byte[paddedLength];
            byte[] output = new byte[cipher.GetOutputSize(paddedLength)];
            _ = inputStream.Read(input, 0, inputLength);
            int len = cipher.ProcessBytes(input, 0, input.Length, output, 0);
            _ = cipher.DoFinal(output, len);
            return output;
        }

        // Token: 0x060000FD RID: 253 RVA: 0x0000ED20 File Offset: 0x0000CF20
        public static MemoryStream DecryptRsa(string filePath, string key) {
            if (filePath == null) {
                throw new ArgumentNullException(nameof(filePath));
            }
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }
            AsymmetricKeyParameter keyParameter = GetKeyOrDefault(key);
            var engine = new RsaEngine();
            engine.Init(false, keyParameter);
            var outputStream = new MemoryStream();
            using (FileStream inputStream = File.OpenRead(filePath)) {
                int inputBlockSize = engine.GetInputBlockSize();
                int outputBlockSize = engine.GetOutputBlockSize();
                byte[] inputBlock = new byte[inputBlockSize];
                while (inputStream.Read(inputBlock, 0, inputBlock.Length) > 0) {
                    byte[] outputBlock = engine.ProcessBlock(inputBlock, 0, inputBlockSize);
                    int requiredPadding = outputBlockSize - outputBlock.Length;
                    if (requiredPadding > 0) {
                        byte[] paddedOutputBlock = new byte[outputBlockSize];
                        outputBlock.CopyTo(paddedOutputBlock, requiredPadding);
                        outputBlock = paddedOutputBlock;
                    }
                    outputStream.Write(outputBlock, 0, outputBlock.Length);
                }
            }
            _ = outputStream.Seek(0L, SeekOrigin.Begin);
            return outputStream;
        }

        // Token: 0x060000FE RID: 254 RVA: 0x0000EE00 File Offset: 0x0000D000
        public static AsymmetricKeyParameter GetKeyOrDefault(string key) {
            AsymmetricKeyParameter asymmetricKeyParameter = null;
            try {
                asymmetricKeyParameter = (AsymmetricKeyParameter)new PemReader(new StringReader(key)).ReadObject();
            } catch { }
            return asymmetricKeyParameter;
        }
    }
}
