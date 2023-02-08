﻿using static SoulsFormats.PARAM;

namespace Chomp.Tools {
    internal static class Globals {
        public static Color FromARGB(uint k) => Color.FromArgb(unchecked((int)k));
        public static unsafe void printPointer<T>(T value) => Console.WriteLine(toHex(Pointers.AsPointer(value)));

        public static unsafe void printContents<T>(T value) {
            FieldInfo[] infos = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            nuint size = (nuint)infos.Length + 2;
            nuint p = (nuint)Pointers.AsPointer(value);
            if (value is Array array) {
                size += (nuint)array.LongLength;
            }
            Console.WriteLine($"Contents of {typeof(T)}({toHex(p)}) and it is size {size}, sizeof({sizeof(T)}) and fields {infos.Length}");
            for (nuint i = p; i < p + size * 8;) {
                Console.Write(toHex(*(void**)i));
                Console.Write((i += 8) < p + size * 8 ? '|' : '\n');
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static unsafe void ForEachNoChecks<T>(this List<T> values, Action<T> action) {
            int offset = sizeof(T);
            for (T* arrayp = ListAsPointer(values),
                endp = PointerOffset<T, T>(arrayp, *Pointers.AsPointer<List<T>, int>(values, 20UL) * offset);
                arrayp != endp; arrayp = Pointers.PointerOffset<T, T>(arrayp, offset)) {
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
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static U Do<T, U>(T _, U value) => value;
        public static T AppendValue<T>(this List<T> list, T value) {
            list.Add(value);
            return value;
        }
        public static long MeasureTimeSpent(Action action) {
            var sw = new Stopwatch();
            sw.Start();
            action();
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void SetBackAndForeColors(this ToolStripSeparator strip, Color foreColor, Color backColor) {
            strip.BackColor = backColor;
            strip.ForeColor = foreColor;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void OPWrite(this StreamWriter stream, string text, string text2) {
            stream.Write(text);
            stream.Write(text2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void OPWrite(this StreamWriter stream, string text, string text2, string text3) {
            stream.Write(text);
            stream.Write(text2);
            stream.Write(text3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void OPWrite(this StreamWriter stream, string text, string text2, string text3, string text4) {
            stream.Write(text);
            stream.Write(text2);
            stream.Write(text3);
            stream.Write(text4);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void OPWrite(this StreamWriter stream, string text, string text2, string text3, string text4, string text5) {
            stream.Write(text);
            stream.Write(text2);
            stream.Write(text3);
            stream.Write(text4);
            stream.Write(text5);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void OPWrite(this StreamWriter stream, string text, string text2, string text3, string text4, string text5, string text6) {
            stream.Write(text);
            stream.Write(text2);
            stream.Write(text3);
            stream.Write(text4);
            stream.Write(text5);
            stream.Write(text6);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void OPWrite(this StreamWriter stream, string text, string text2, string text3, string text4, string text5, string text6, string text7) {
            stream.Write(text);
            stream.Write(text2);
            stream.Write(text3);
            stream.Write(text4);
            stream.Write(text5);
            stream.Write(text6);
            stream.Write(text7);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void OPWrite(this StreamWriter stream, string text, string text2, string text3, string text4, string text5, string text6, string text7, string text8) {
            stream.Write(text);
            stream.Write(text2);
            stream.Write(text3);
            stream.Write(text4);
            stream.Write(text5);
            stream.Write(text6);
            stream.Write(text7);
            stream.Write(text8);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void WriteLine(this StreamWriter stream, string text, string text2) {
            stream.Write(text);
            stream.WriteLine(text2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void WriteLine(this StreamWriter stream, string text, string text2, string text3) {
            stream.Write(text);
            stream.Write(text2);
            stream.WriteLine(text3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void WriteLine(this StreamWriter stream, string text, string text2, string text3, string text4) {
            stream.Write(text);
            stream.Write(text2);
            stream.Write(text3);
            stream.WriteLine(text4);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void WriteLine(this StreamWriter stream, string text, string text2, string text3, string text4, string text5) {
            stream.Write(text);
            stream.Write(text2);
            stream.Write(text3);
            stream.Write(text4);
            stream.WriteLine(text5);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void WriteLine(this StreamWriter stream, string text, string text2, string text3, string text4, string text5, string text6) {
            stream.Write(text);
            stream.Write(text2);
            stream.Write(text3);
            stream.Write(text4);
            stream.Write(text5);
            stream.WriteLine(text6);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void WriteLine(this StreamWriter stream, string text, string text2, string text3, string text4, string text5, string text6, string text7) {
            stream.Write(text);
            stream.Write(text2);
            stream.Write(text3);
            stream.Write(text4);
            stream.Write(text5);
            stream.Write(text6);
            stream.WriteLine(text7);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void WriteLine(this StreamWriter stream, string text, string text2, string text3, string text4, string text5, string text6, string text7, string text8) {
            stream.Write(text);
            stream.Write(text2);
            stream.Write(text3);
            stream.Write(text4);
            stream.Write(text5);
            stream.Write(text6);
            stream.Write(text7);
            stream.WriteLine(text8);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string[]? GetCellReferences(this Cell cell) => Main.refs_dict[cell.Def.Def.ParamType]?[cell.Def.InternalName];
        public static bool AddNoReplacement<T>(this List<T> list, T item) {
            if (list.Contains(item)) return false;
            list.Add(item);
            return true;
        }
    }
}
