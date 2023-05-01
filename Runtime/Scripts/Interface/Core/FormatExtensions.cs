namespace Trackman
{
    public static class FormatExtensions
    {
        #region Methods
        public static string Space(this string value, int indent, bool left = true)
        {
            if (value.Length > indent) value = value.Substring(0, indent);
            if (left) return string.Format($"{{0,-{indent}}}", value);
            else return string.Format($"{{0,{indent}}}", value);
        }

        public static string ToByteSize(this int value)
        {
            if (value > 1024 * 1024) return $"{value / (float)(1024 * 1024):F1}mb";
            else if (value > 1024) return $"{value / (float)1024:F1}kb";
            else return $"{value}b";
        }
        public static string ToByteSize(this long value)
        {
            if (value > 1024 * 1024) return $"{value / (float)(1024 * 1024):F1}mb";
            else if (value > 1024) return $"{value / (float)1024:F1}kb";
            else return $"{value}b";
        }
        public static string ToByteSize(this ulong value)
        {
            if (value > 1024 * 1024) return $"{value / (float)(1024 * 1024):F1}mb";
            else if (value > 1024) return $"{value / (float)1024:F1}kb";
            else return $"{value}b";
        }

        public static string Nick(this string value)
        {
            return $"[{value}]";
        }
        public static string Nick(this IClassName value, string execution = default)
        {
            return $"[{(execution.NotNullOrEmpty() ? $"{execution} => " : "")}{value.ClassName}]";
        }
        #endregion
    }
}