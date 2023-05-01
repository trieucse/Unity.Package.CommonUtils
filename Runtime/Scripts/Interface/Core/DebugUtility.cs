#pragma warning disable RCS1102

namespace Trackman
{
    public class DebugUtility
    {
        #region Methods
        public static string GetString(object value)
        {
            return JsonUtility.ToJson(value, false);
        }
        public static T FromString<T>(string value)
        {
            return JsonUtility.FromJson<T>(value);
        }
        #endregion
    }
}