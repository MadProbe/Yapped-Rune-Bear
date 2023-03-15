﻿using DefType = SoulsFormats.PARAMDEF.DefType;
using EditFlags = SoulsFormats.PARAMDEF.EditFlags;

namespace SoulsFormats.Formats.PARAM {
    internal static class ParamUtil {
        public static string GetDefaultFormat(DefType type) => defaultFormatsArray[(int)type];
            //type switch {
            //    DefType.s8 => "%d",
            //    DefType.u8 => "%d",
            //    DefType.s16 => "%d",
            //    DefType.u16 => "%d",
            //    DefType.s32 => "%d",
            //    DefType.u32 => "%d",
            //    DefType.b32 => "%d",
            //    DefType.f32 => "%f",
            //    DefType.angle32 => "%f",
            //    DefType.f64 => "%f",
            //    DefType.dummy8 => "",
            //    DefType.fixstr => "%d",
            //    DefType.fixstrW => "%d",
            //    _ => throw new NotImplementedException($"No default format specified for {nameof(DefType)}.{type}"),
            //};

        private static readonly string[] defaultFormatsArray = new string[] {
            /* DefType.s8 */ "%d",
            /* DefType.u8 */ "%d",
            /* DefType.s16 */ "%d",
            /* DefType.u16 */ "%d",
            /* DefType.s32 */ "%d",
            /* DefType.u32 */ "%d",
            /* DefType.b32 */ "%d",
            /* DefType.f32 */ "%f",
            /* DefType.angle32 */ "%f",
            /* DefType.f64 */ "%f",
            /* DefType.dummy8 */ "",
            /* DefType.fixstr */ "%d",
            /* DefType.fixstrW */ "%d",
        };

        private static readonly Dictionary<DefType, float> fixedDefaults = new() {
            [DefType.s8] = 0,
            [DefType.u8] = 0,
            [DefType.s16] = 0,
            [DefType.u16] = 0,
            [DefType.s32] = 0,
            [DefType.u32] = 0,
            [DefType.b32] = 0,
            [DefType.f32] = 0,
            [DefType.angle32] = 0,
            [DefType.f64] = 0,
            [DefType.dummy8] = 0,
            [DefType.fixstr] = 0,
            [DefType.fixstrW] = 0,
        };

        private static readonly object[] fixedDefaultsArray = new object[] {
            /* DefType.s8 */ 0f,
            /* DefType.u8 */ 0f,
            /* DefType.s16 */ 0f,
            /* DefType.u16 */ 0f,
            /* DefType.s32 */ 0f,
            /* DefType.u32 */ 0f,
            /* DefType.b32 */ 0f,
            /* DefType.f32 */ 0f,
            /* DefType.angle32 */ 0f,
            /* DefType.f64 */ 0f,
            /* DefType.dummy8 */ 0f,
            /* DefType.fixstr */ 0f,
            /* DefType.fixstrW */ 0f,
        };

        private static readonly Dictionary<DefType, object> variableDefaults = new() {
            [DefType.s8] = 0,
            [DefType.u8] = 0,
            [DefType.s16] = 0,
            [DefType.u16] = 0,
            [DefType.s32] = 0,
            [DefType.u32] = 0,
            [DefType.b32] = 0,
            [DefType.f32] = 0f,
            [DefType.angle32] = 0f,
            [DefType.f64] = 0d,
            [DefType.dummy8] = null,
            [DefType.fixstr] = null,
            [DefType.fixstrW] = null,
        };

        private static readonly object[] variableDefaultsArray = new object[] {
            /* DefType.s8 */ 0,
            /* DefType.u8 */ 0,
            /* DefType.s16 */ 0,
            /* DefType.u16 */ 0,
            /* DefType.s32 */ 0,
            /* DefType.u32 */ 0,
            /* DefType.b32 */ 0,
            /* DefType.f32 */ 0f,
            /* DefType.angle32 */ 0f,
            /* DefType.f64 */ 0d,
            /* DefType.dummy8 */ null,
            /* DefType.fixstr */ null,
            /* DefType.fixstrW */ null,
        };

        public static object GetDefaultDefault(PARAMDEF def, DefType type) => def?.VariableEditorValueTypes == true ? variableDefaultsArray[(int)type] : fixedDefaultsArray[(int)type];

        private static readonly Dictionary<DefType, float> fixedMinimums = new() {
            [DefType.s8] = sbyte.MinValue,
            [DefType.u8] = byte.MinValue,
            [DefType.s16] = short.MinValue,
            [DefType.u16] = ushort.MinValue,
            [DefType.s32] = -2147483520, // Smallest representable float greater than int.MinValue
            [DefType.u32] = uint.MinValue,
            [DefType.b32] = 0,
            [DefType.f32] = float.MinValue,
            [DefType.angle32] = float.MinValue,
            [DefType.f64] = float.MinValue,
            [DefType.dummy8] = 0,
            [DefType.fixstr] = -1,
            [DefType.fixstrW] = -1,
        };

        private static readonly object[] fixedMinimumsArray = new object[] {
            /* DefType.s8 */ (float)sbyte.MinValue,
            /* DefType.u8 */ (float)byte.MinValue,
            /* DefType.s16 */ (float)short.MinValue,
            /* DefType.u16 */ (float)ushort.MinValue,
            /* DefType.s32 */ -2147483520f, // Smallest representable float greater than int.MinValue
            /* DefType.u32 */ (float)uint.MinValue,
            /* DefType.b32 */ 0f,
            /* DefType.f32 */ float.MinValue,
            /* DefType.angle32 */ float.MinValue,
            /* DefType.f64 */ float.MinValue,
            /* DefType.dummy8 */ 0f,
            /* DefType.fixstr */ -1f,
            /* DefType.fixstrW */ -1f,
        };

        private static readonly Dictionary<DefType, object> variableMinimums = new() {
            [DefType.s8] = (int)sbyte.MinValue,
            [DefType.u8] = (int)byte.MinValue,
            [DefType.s16] = (int)short.MinValue,
            [DefType.u16] = (int)ushort.MinValue,
            [DefType.s32] = int.MinValue,
            [DefType.u32] = (int)uint.MinValue,
            [DefType.b32] = 0,
            [DefType.f32] = float.MinValue,
            [DefType.angle32] = float.MinValue,
            [DefType.f64] = double.MinValue,
            [DefType.dummy8] = null,
            [DefType.fixstr] = null,
            [DefType.fixstrW] = null,
        };

        private static readonly object[] variableMinimumsArray = new object[] {
            /* DefType.s8 */ (int)sbyte.MinValue,
            /* DefType.u8 */ (int)byte.MinValue,
            /* DefType.s16 */ (int)short.MinValue,
            /* DefType.u16 */ (int)ushort.MinValue,
            /* DefType.s32 */ int.MinValue,
            /* DefType.u32 */ (int)uint.MinValue,
            /* DefType.b32 */ 0,
            /* DefType.f32 */ float.MinValue,
            /* DefType.angle32 */ float.MinValue,
            /* DefType.f64 */ double.MinValue,
            /* DefType.dummy8 */ null,
            /* DefType.fixstr */ null,
            /* DefType.fixstrW */ null,
        };

        public static object GetDefaultMinimum(PARAMDEF def, DefType type) => def?.VariableEditorValueTypes == true ? variableMinimumsArray[(int)type] : fixedMinimumsArray[(int)type];

        private static readonly Dictionary<DefType, float> fixedMaximums = new() {
            [DefType.s8] = sbyte.MaxValue,
            [DefType.u8] = byte.MaxValue,
            [DefType.s16] = short.MaxValue,
            [DefType.u16] = ushort.MaxValue,
            [DefType.s32] = 2147483520, // Largest representable float less than int.MaxValue
            [DefType.u32] = 4294967040, // Largest representable float less than uint.MaxValue
            [DefType.b32] = 1,
            [DefType.f32] = float.MaxValue,
            [DefType.angle32] = float.MaxValue,
            [DefType.f64] = float.MaxValue,
            [DefType.dummy8] = 0,
            [DefType.fixstr] = 1e9f,
            [DefType.fixstrW] = 1e9f,
        };

        private static readonly object[] fixedMaximumsArray = new object[] {
            (float)sbyte.MaxValue,
            (float)byte.MaxValue,
            (float)short.MaxValue,
            (float)ushort.MaxValue,
            2147483520f, // Largest representable float less than int.MaxValue
            4294967040f, // Largest representable float less than uint.MaxValue
            1f,
            float.MaxValue,
            float.MaxValue,
            float.MaxValue,
            0f,
            1e9f,
            1e9f,
        };

        private static readonly Dictionary<DefType, object> variableMaximums = new() {
            [DefType.s8] = (int)sbyte.MaxValue,
            [DefType.u8] = (int)byte.MaxValue,
            [DefType.s16] = (int)short.MaxValue,
            [DefType.u16] = (int)ushort.MaxValue,
            [DefType.s32] = int.MaxValue,
            [DefType.u32] = int.MaxValue, // Yes, u32 uses signed int too (usually)
            [DefType.b32] = 1,
            [DefType.f32] = float.MaxValue,
            [DefType.angle32] = float.MaxValue,
            [DefType.f64] = double.MaxValue,
            [DefType.dummy8] = null,
            [DefType.fixstr] = null,
            [DefType.fixstrW] = null,
        };

        private static readonly object[] variableMaximumsArray = new object[] {
            (int)sbyte.MaxValue,
            (int)byte.MaxValue,
            (int)short.MaxValue,
            (int)ushort.MaxValue,
            int.MaxValue,
            int.MaxValue, // Yes, u32 uses signed int too (usually)
            1,
            float.MaxValue,
            float.MaxValue,
            double.MaxValue,
            null,
            null,
            null,
        };

        public static object GetDefaultMaximum(PARAMDEF def, DefType type) => def?.VariableEditorValueTypes == true ? variableMaximumsArray[(int)type] : fixedMaximumsArray[(int)type];

        private static readonly Dictionary<DefType, float> fixedIncrements = new() {
            [DefType.s8] = 1,
            [DefType.u8] = 1,
            [DefType.s16] = 1,
            [DefType.u16] = 1,
            [DefType.s32] = 1,
            [DefType.u32] = 1,
            [DefType.b32] = 1,
            [DefType.f32] = 0.01f,
            [DefType.angle32] = 0.01f,
            [DefType.f64] = 0.01f,
            [DefType.dummy8] = 0,
            [DefType.fixstr] = 1,
            [DefType.fixstrW] = 1,
        };

        private static readonly object[] fixedIncrementsArray = new object[] {
            /* DefType.s8 */ 1f,
            /* DefType.u8 */ 1f,
            /* DefType.s16 */ 1f,
            /* DefType.u16 */ 1f,
            /* DefType.s32 */ 1f,
            /* DefType.u32 */ 1f,
            /* DefType.b32 */ 1f,
            /* DefType.f32 */ 0.01f,
            /* DefType.angle32 */ 0.01f,
            /* DefType.f64 */ 0.01f,
            /* DefType.dummy8 */ 0f,
            /* DefType.fixstr */ 1f,
            /* DefType.fixstrW */ 1f,
        };

        private static readonly Dictionary<DefType, object> variableIncrements = new() {
            [DefType.s8] = 1,
            [DefType.u8] = 1,
            [DefType.s16] = 1,
            [DefType.u16] = 1,
            [DefType.s32] = 1,
            [DefType.u32] = 1,
            [DefType.b32] = 1,
            [DefType.f32] = 0.01f,
            [DefType.angle32] = 0.01f,
            [DefType.f64] = 0.01d,
            [DefType.dummy8] = null,
            [DefType.fixstr] = null,
            [DefType.fixstrW] = null,
        };

        private static readonly object[] variableIncrementsArray = new object[] {
            /* DefType.s8 */ 1,
            /* DefType.u8 */ 1,
            /* DefType.s16 */ 1,
            /* DefType.u16 */ 1,
            /* DefType.s32 */ 1,
            /* DefType.u32 */ 1,
            /* DefType.b32 */ 1,
            /* DefType.f32 */ 0.01f,
            /* DefType.angle32 */ 0.01f,
            /* DefType.f64 */ 0.01d,
            /* DefType.dummy8 */ null,
            /* DefType.fixstr */ null,
            /* DefType.fixstrW */ null,
        };

        public static object GetDefaultIncrement(PARAMDEF def, DefType type) => def?.VariableEditorValueTypes == true ? variableIncrementsArray[(int)type] : fixedIncrementsArray[(int)type];

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter")]
        public static EditFlags GetDefaultEditFlags(DefType _type) => EditFlags.Wrap;
        //type switch {
        //    DefType.s8 => EditFlags.Wrap,
        //    DefType.u8 => EditFlags.Wrap,
        //    DefType.s16 => EditFlags.Wrap,
        //    DefType.u16 => EditFlags.Wrap,
        //    DefType.s32 => EditFlags.Wrap,
        //    DefType.u32 => EditFlags.Wrap,
        //    DefType.b32 => EditFlags.Wrap,
        //    DefType.f32 => EditFlags.Wrap,
        //    DefType.angle32 => EditFlags.Wrap,
        //    DefType.f64 => EditFlags.Wrap,
        //    DefType.dummy8 => EditFlags.None,
        //    DefType.fixstr => EditFlags.Wrap,
        //    DefType.fixstrW => EditFlags.Wrap,
        //    _ => throw new NotImplementedException($"No default edit flags specified for {nameof(DefType)}.{type}"),
        //};

        public static bool IsArrayType(DefType type) => type is DefType.dummy8 or DefType.fixstr or DefType.fixstrW;

        public static bool IsBitType(DefType type) => type is DefType.u8 or DefType.u16 or DefType.u32 or DefType.dummy8;

        public static int GetValueSize(DefType type) => type switch {
            DefType.s8 => 1,
            DefType.u8 => 1,
            DefType.s16 => 2,
            DefType.u16 => 2,
            DefType.s32 => 4,
            DefType.u32 => 4,
            DefType.b32 => 4,
            DefType.f32 => 4,
            DefType.angle32 => 4,
            DefType.f64 => 8,
            DefType.dummy8 => 1,
            DefType.fixstr => 1,
            DefType.fixstrW => 2,
            _ => throw new NotImplementedException($"No value size specified for {nameof(DefType)}.{type}"),
        };

        public static object ConvertDefaultValue(PARAMDEF.Field field) => field.DisplayType switch {
            DefType.s8 => Convert.ToSByte(field.Default),
            DefType.u8 => Convert.ToByte(field.Default),
            DefType.s16 => Convert.ToInt16(field.Default),
            DefType.u16 => Convert.ToUInt16(field.Default),
            DefType.s32 => Convert.ToInt32(field.Default),
            DefType.u32 => Convert.ToUInt32(field.Default),
            DefType.b32 => Convert.ToInt32(field.Default),
            DefType.f32 => Convert.ToSingle(field.Default),
            DefType.angle32 => Convert.ToSingle(field.Default),
            DefType.f64 => Convert.ToDouble(field.Default),
            DefType.fixstr => "",
            DefType.fixstrW => "",
            DefType.dummy8 => field.BitSize == -1 ? (new byte[field.ArrayLength]) : (byte)0,
            _ => throw new NotImplementedException($"Default not implemented for type {field.DisplayType}"),
        };

        public static int GetBitLimit(DefType type) => bitLimits[(int)type];
            //type == DefType.u8
            //    ? 8
            //    : type == DefType.u16
            //        ? 16
            //        : type == DefType.u32 ? 32 : throw new InvalidOperationException($"Bit type may only be u8, u16, or u32.");

        private static readonly int[] bitLimits = new int[] {
            /* DefType.s8 */ sizeof(sbyte) << 3,
            /* DefType.u8 */ sizeof(byte) << 3,
            /* DefType.s16 */ sizeof(short) << 3,
            /* DefType.u16 */ sizeof(ushort) << 3,
            /* DefType.s32 */ sizeof(int) << 3,
            /* DefType.u32 */ sizeof(uint) << 3,
            /* DefType.b32 */ sizeof(int) << 3,
            /* DefType.f32 */ sizeof(float) << 3,
            /* DefType.angle32 */ sizeof(float) << 3,
            /* DefType.f64 */ sizeof(double) << 3,
            /* DefType.dummy8 */ 0x40000000,
            /* DefType.fixstr */ 0x40000000,
            /* DefType.fixstrW */ 0x40000000,
        };
    }
}
