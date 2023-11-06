using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Trackman.Globalization
{
    public static class GlobalizationExtensions
    {
        static readonly CultureInfo current = CultureInfo.CurrentCulture;
        static readonly CultureInfo invariant = CultureInfo.InvariantCulture;

        #region Methods
        public static bool ToBool(this string text) => bool.Parse(text);
        public static float ToFloat(this string text) => float.Parse(text, NumberStyles.Float, invariant);
        public static int ToInt32(this string text) => int.Parse(text, NumberStyles.Integer, invariant);
        public static Vector2 ToVector2(this IReadOnlyList<string> text) => new(text[0].ToFloat(), text[1].ToFloat());
        public static Vector3 ToVector3(this IReadOnlyList<string> text) => new(text[0].ToFloat(), text[1].ToFloat(), text[2].ToFloat());
        public static Vector4 ToVector4(this IReadOnlyList<string> text) => new(text[0].ToFloat(), text[1].ToFloat(), text[2].ToFloat(), text[3].ToFloat());
        public static Vector2Int ToVector2Int(this IReadOnlyList<string> text) => new(text[0].ToInt32(), text[1].ToInt32());
        public static Vector3Int ToVector3Int(this IReadOnlyList<string> text) => new(text[0].ToInt32(), text[1].ToInt32(), text[2].ToInt32());

        public static int ConvertInt32(this string text) => Convert.ToInt32(text, invariant);
        public static float ConvertFloat(this string text) => Convert.ToSingle(text, invariant);

        public static string ToText(this bool value) => value.ToString(current);
        public static string ToText(this int value, string format = default) => value.ToString(format, current);
        public static string ToText(this float value, string format = default) => value.ToString(format, current);
        public static string ToText(this TimeSpan value, string format = default) => value.ToString(format);
        #endregion
    }
}