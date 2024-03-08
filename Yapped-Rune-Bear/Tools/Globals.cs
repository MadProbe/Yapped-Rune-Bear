using System.Diagnostics.CodeAnalysis;
using Chomp.Util;
using static SoulsFormats.PARAM;

namespace Chomp.Tools {
    internal static class Globals {
        public static Stopwatch stopwatch = new ();
        public static Color FromARGB(uint k) => Color.FromArgb(unchecked((int)k));
        public static unsafe void printPointer<T>(T value) => Console.WriteLine(toHex(AsPointer(value)));

        public static unsafe void printContents<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(T value) {
            FieldInfo[] infos = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            nuint size = (nuint)infos.Length + 2;
            nuint p = (nuint)AsPointer(value);
            if (value is Array array) {
                size += (nuint)array.LongLength;
            }
            Console.WriteLine($"Contents of {typeof(T)}({toHex(p)}) and it is size {size}, sizeof({sizeof(T)}) and fields {infos.Length}");
            for (nuint i = p; i < p + size * 8;) {
                Console.Write(toHex(*(void**)i));
                Console.Write((i += 8) < p + size * 8 ? '|' : '\n');
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ForEachNoChecks<T>(this List<T> values, Action<T> action) {
            int offset = sizeof(T);
            for (T* arrayp = ListAsPointer(values),
                endp = PointerOffset<T, T>(arrayp, *AsPointer<List<T>, int>(values, 20UL) * offset);
                arrayp != endp; arrayp = PointerOffset<T, T>(arrayp, offset)) {
                action(*arrayp);
            }
        }
        public static string toHex(UInt128 value) => value.ToString("X").PadLeft(32, '0');
        public static string toHex(Int128 value) => value.ToString("X").PadLeft(32, '0');
        public static string toHex(int value) => value.ToString("X").PadLeft(8, '0');
        public static string toHex(uint value) => value.ToString("X").PadLeft(8, '0');
        public static string toHex(long value) => value.ToString("X").PadLeft(16, '0');
        public static string toHex(ulong value) => value.ToString("X").PadLeft(16, '0');
        public static string toHex(nuint value) => value.ToString("X").PadLeft(16, '0');
        public static unsafe string toHex(void* value) => toHex((nuint)value);
        public static U CallFuncWithVariable<T, U>(T v, Func<T, U> f) => f(v);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U Do<T, U>(T _, U value) => value;
        public static T EchoAdd<T>(this List<T> list, T value) {
            list.Add(value);
            return value;
        }
        public static long MeasureTimeSpent(Action action) => MeasureTimeSpent(action, stopwatch);
        public static long MeasureTimeSpent(Action action, Stopwatch stopwatch) {
            stopwatch.Reset();
            stopwatch.Start();
            action();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }
        public static Task<long> MeasureTimeSpent(Func<Task> action) => MeasureTimeSpent(action, stopwatch);
        public static async Task<long> MeasureTimeSpent(Func<Task> action, Stopwatch stopwatch) {
            stopwatch.Start();
            await action();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBackAndForeColors(this ToolStripMenuItem strip, Color foreColor, Color backColor) {
            strip.BackColor = backColor;
            strip.ForeColor = foreColor;
            strip.DisplayStyle = ToolStripItemDisplayStyle.Text;
            ToolStripItemCollection items = strip.DropDownItems;
            int count = items.Count;
            for (int i = 0; i < count; i++) {
                ToolStripItem item = items[i];
                item.BackColor = backColor;
                item.ForeColor = foreColor;
            }
        }
        internal class ToolStripCustomRenderer : ToolStripProfessionalRenderer {
            protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e) {
                if (!e.Item.Selected) {
                    base.OnRenderButtonBackground(e);
                } else {
                    var rectangle = new Rectangle(0, 0, e.Item.Size.Width - 1, e.Item.Size.Height - 1);
                    e.Graphics.FillRectangle(new SolidBrush(Main.GridSelectionBackColor), rectangle);
                    e.Graphics.DrawRectangle(new Pen(Main.GridSelectionBackColor), rectangle);
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBackAndForeColors(this ToolStripSeparator strip, Color foreColor, Color backColor) {
            strip.BackColor = backColor;
            strip.ForeColor = foreColor;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteAllocfree(this StreamWriter stream, string text, string text2) {
            stream.WriteAllocfree(text);
            stream.WriteAllocfree(text2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteAllocfree(this StreamWriter stream, string text, string text2, string text3) {
            stream.WriteAllocfree(text);
            stream.WriteAllocfree(text2);
            stream.WriteAllocfree(text3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteAllocfree(this StreamWriter stream, string text, string text2, string text3, string text4) {
            stream.WriteAllocfree(text);
            stream.WriteAllocfree(text2);
            stream.WriteAllocfree(text3);
            stream.WriteAllocfree(text4);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OPWrite(this StreamWriter stream, string text, string text2, string text3, string text4, string text5) {
            stream.WriteAllocfree(text);
            stream.WriteAllocfree(text2);
            stream.WriteAllocfree(text3);
            stream.WriteAllocfree(text4);
            stream.WriteAllocfree(text5);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OPWrite(this StreamWriter stream, string text, string text2, string text3, string text4, string text5, string text6) {
            stream.WriteAllocfree(text);
            stream.WriteAllocfree(text2);
            stream.WriteAllocfree(text3);
            stream.WriteAllocfree(text4);
            stream.WriteAllocfree(text5);
            stream.WriteAllocfree(text6);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OPWrite(this StreamWriter stream, string text, string text2, string text3, string text4, string text5, string text6, string text7) {
            stream.WriteAllocfree(text);
            stream.WriteAllocfree(text2);
            stream.WriteAllocfree(text3);
            stream.WriteAllocfree(text4);
            stream.WriteAllocfree(text5);
            stream.WriteAllocfree(text6);
            stream.WriteAllocfree(text7);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OPWrite(this StreamWriter stream, string text, string text2, string text3, string text4, string text5, string text6, string text7, string text8) {
            stream.WriteAllocfree(text);
            stream.WriteAllocfree(text2);
            stream.WriteAllocfree(text3);
            stream.WriteAllocfree(text4);
            stream.WriteAllocfree(text5);
            stream.WriteAllocfree(text6);
            stream.WriteAllocfree(text7);
            stream.WriteAllocfree(text8);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteLine(this StreamWriter stream, string text, string text2) {
            stream.WriteAllocfree(text);
            stream.WriteLineAllocfree(text2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteLine(this StreamWriter stream, string text, string text2, string text3) {
            stream.WriteAllocfree(text);
            stream.WriteAllocfree(text2);
            stream.WriteLineAllocfree(text3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteLine(this StreamWriter stream, string text, string text2, string text3, string text4) {
            stream.WriteAllocfree(text);
            stream.WriteAllocfree(text2);
            stream.WriteAllocfree(text3);
            stream.WriteLineAllocfree(text4);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteLine(this StreamWriter stream, string text, string text2, string text3, string text4, string text5) {
            stream.WriteAllocfree(text);
            stream.WriteAllocfree(text2);
            stream.WriteAllocfree(text3);
            stream.WriteAllocfree(text4);
            stream.WriteLineAllocfree(text5);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteLine(this StreamWriter stream, string text, string text2, string text3, string text4, string text5, string text6) {
            stream.WriteAllocfree(text);
            stream.WriteAllocfree(text2);
            stream.WriteAllocfree(text3);
            stream.WriteAllocfree(text4);
            stream.WriteAllocfree(text5);
            stream.WriteLineAllocfree(text6);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteLine(this StreamWriter stream, string text, string text2, string text3, string text4, string text5, string text6, string text7) {
            stream.WriteAllocfree(text);
            stream.WriteAllocfree(text2);
            stream.WriteAllocfree(text3);
            stream.WriteAllocfree(text4);
            stream.WriteAllocfree(text5);
            stream.WriteAllocfree(text6);
            stream.WriteLineAllocfree(text7);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteLine(this StreamWriter stream, string text, string text2, string text3, string text4, string text5, string text6, string text7, string text8) {
            stream.WriteAllocfree(text);
            stream.WriteAllocfree(text2);
            stream.WriteAllocfree(text3);
            stream.WriteAllocfree(text4);
            stream.WriteAllocfree(text5);
            stream.WriteAllocfree(text6);
            stream.WriteAllocfree(text7);
            stream.WriteLineAllocfree(text8);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string[]? GetCellReferences(this Cell cell) => Main.refs_dict[cell.Def.Def.ParamType]?[cell.Def.InternalName];
        public static bool AddNoReplacement<T>(this List<T> list, T item) {
            if (list.Contains(item)) {
                return false;
            }

            list.Add(item);
            return true;
        }
    }
}
