using System.Text;

namespace SoulsFormats.Util {
    public static class SFEncoding {
        public static readonly Encoding ASCII = Encoding.ASCII;

        public static readonly Encoding ShiftJIS;

        public static readonly Encoding UTF16 = Encoding.Unicode;

        public static readonly Encoding UTF16BE = Encoding.BigEndianUnicode;

        static SFEncoding() {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ShiftJIS = Encoding.GetEncoding("shift-jis");
        }
    }
}
