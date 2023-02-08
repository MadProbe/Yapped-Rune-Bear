using SoulsFormats;
using SoulsFormats.Binder.BND4;
using SoulsFormats.Util;

namespace Chomp.Util {
    // Token: 0x0200000A RID: 10
    internal static class Utility {
        // Token: 0x0600010E RID: 270 RVA: 0x0000F320 File Offset: 0x0000D520
        public static BND4 DecryptDS2Regulation(string path) {
            byte[] bytes = File.ReadAllBytes(path);
            byte[] iv = new byte[16];
            iv[0] = 128;
            Array.Copy(bytes, 0, iv, 1, 11);
            iv[15] = 1;
            using var ms = new MemoryStream(bytes, 32, bytes.Length);
            byte[] decrypted = CryptographyUtility.DecryptAesCtr(ms, ds2RegulationKey, iv);
            File.WriteAllBytes("ffff.bnd", decrypted);
            return SoulsFile<BND4>.Read(decrypted);
        }

        // Token: 0x0600010F RID: 271 RVA: 0x0000F3B8 File Offset: 0x0000D5B8
        public static void EncryptDS2Regulation(string path, BND4 bnd) {
            _ = Directory.CreateDirectory(Path.GetDirectoryName(path));
            bnd.Write(path);
        }

        // Token: 0x06000110 RID: 272 RVA: 0x0000F3CD File Offset: 0x0000D5CD
        public static void ShowError(string message) => MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);

        // Token: 0x06000111 RID: 273 RVA: 0x0000F3DE File Offset: 0x0000D5DE
        public static void DebugPrint(string message) => Console.WriteLine(message);

        // Token: 0x04000090 RID: 144
        private static readonly byte[] ds2RegulationKey = new byte[] { 64, 23, 129, 48, 223, 10, 148, 84, 51, 9, 225, 113, 236, 191, 37, 76 };
    }
}
