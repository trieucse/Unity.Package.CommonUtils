namespace Trackman
{
    public static class FormatExtensions
    {
        #region Methods
        public static string Space(this string value, int indent, bool left = true)
        {
            if (value.Length > indent) value = value.Substring(0, indent);
            return string.Format(left ? $"{{0,-{indent}}}" : $"{{0,{indent}}}", value);
        }
        public static string ToByteSize(this int value) =>
            value switch
            {
                > 1024 * 1024 => $"{value / (float)(1024 * 1024):F1}mb",
                > 1024 => $"{value / (float)1024:F1}kb",
                _ => $"{value}b"
            };
        public static string ToByteSize(this long value) =>
            value switch
            {
                > 1024 * 1024 => $"{value / (float)(1024 * 1024):F1}mb",
                > 1024 => $"{value / (float)1024:F1}kb",
                _ => $"{value}b"
            };
        public static string ToByteSize(this ulong value) =>
            value switch
            {
                > 1024 * 1024 => $"{value / (float)(1024 * 1024):F1}mb",
                > 1024 => $"{value / (float)1024:F1}kb",
                _ => $"{value}b"
            };
        public static string Nick(this string value) => $"[{value}]";
        public static string Nick(this IClassName value, string execution = default) => $"[{(execution.NotNullOrEmpty() ? $"{execution} => " : "")}{value.ClassName}]";
        #endregion
    }
}