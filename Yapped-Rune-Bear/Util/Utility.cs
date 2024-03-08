using System.Diagnostics.CodeAnalysis;
using SoulsFormats.Binder.BND4;
using SoulsFormats.Util;

namespace Chomp.Util {
    // Token: 0x0200000A RID: 10
    public unsafe static partial class Utility {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ref T GetReference<T>(this T[] array) => ref MemoryMarshal.GetArrayDataReference(array);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ref T GetReference<T>(this T[] array, int offset) => ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(array), offset);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ref T GetReference<T>(this scoped ReadOnlySpan<T> span) => ref MemoryMarshal.GetReference(span);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ref T GetReference<T>(this scoped ReadOnlySpan<T> span, int offset) => ref Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(span), offset);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ref T GetReference<T>(this scoped Span<T> span) => ref MemoryMarshal.GetReference(span);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ref T GetReference<T>(this scoped Span<T> span, int byteOffset) => ref Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(span), byteOffset);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AssignAnyAt<T, I>(this scoped Span<T> span, int offset, I value) => Unsafe.Add(ref MemoryMarshal.GetReference(span), offset) = CastTo<T, I>(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this scoped ReadOnlySpan<T> input, scoped Span<T> output, int outputByteOffset, nint length) =>
            Memmove(ref input.GetReference(), ref output.GetReference(outputByteOffset), length);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this scoped ReadOnlySpan<T> input, T* dest) => Memmove(ref input.GetReference(), dest, input.Length);
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
        public static void ShowError(string message) {
            if (MessageBox.Show(message, "Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Hand) == DialogResult.Abort) {
                _exit(-1);
            }
        }
        [DoesNotReturn]
        [LibraryImport("ucrtbase.dll")]
        internal static partial void _exit(int exitCode);

        // Token: 0x06000111 RID: 273 RVA: 0x0000F3DE File Offset: 0x0000D5DE
        public static void DebugPrint(string message) => Console.WriteLine(message);

        // Token: 0x04000090 RID: 144
        private static readonly byte[] ds2RegulationKey = new byte[] { 0x40, 0x17, 0x81, 0x30, 0xdf, 0x0a, 0x94, 0x54, 0x33, 0x09, 0xe1, 0x71, 0xec, 0xbf, 0x25, 0x4c };
    }
}
