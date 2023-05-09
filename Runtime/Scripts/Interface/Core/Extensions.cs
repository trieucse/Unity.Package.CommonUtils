using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

#pragma warning disable IDE0060, RCS1175

namespace Trackman
{
    [DebuggerStepThrough]
    public static class Extensions
    {
        #region Object Methods
        public static bool ValidIndex(this int value) => value != -1;
        public static bool NotMaxValue(this uint value) => value != uint.MaxValue;
        public static bool Valid(this float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        public static bool NotNullOrEmpty(this string value) => !string.IsNullOrEmpty(value);
        public static bool NotZero(this Vector3 value) => value != Vector3.zero;
        public static bool NotZero(this Vector2 value) => value != Vector2.zero;
        public static bool NotZero(this IntPtr value) => value != IntPtr.Zero;

        public static bool InvalidIndex(this int value) => value == -1;
        public static bool MaxValue(this uint value) => value == uint.MaxValue;
        public static bool Invalid(this float value) => float.IsNaN(value) || float.IsInfinity(value);
        public static bool NullOrEmpty(this string value) => string.IsNullOrEmpty(value);
        public static bool Zero(this Vector3 value) => value == Vector3.zero;
        public static bool Zero(this Vector2 value) => value == Vector2.zero;
        public static bool Zero(this IntPtr value) => value == IntPtr.Zero;

        public static T As<T>(this object value)
        {
            return (T)value;
        }
        public static int AsInt<T>(this T value) where T : struct, Enum
        {
            return (int)(object)value;
        }
        public static T AsEnum<T>(this object value) where T : struct, Enum
        {
            if (value is T enumValue) return enumValue;
            if (value is string stringValue) return (T)Enum.Parse(typeof(T), stringValue);
            throw new ArgumentException();
        }

        public static bool ANY<T>(this T value, T arg) where T : Enum
        {
            return value.GetHashCode().ANY(arg.GetHashCode());
        }
        public static bool ANY(this int value, int arg)
        {
            return (value & arg) != 0;
        }
        public static bool AND<T>(this T value, T arg) where T : Enum
        {
            return value.GetHashCode().AND(arg.GetHashCode());
        }
        public static bool AND(this int value, int arg)
        {
            return (value & arg) == arg;
        }
        public static T AddFlag<T>(this T value, T arg) where T : Enum
        {
            if (value.AND(arg)) return value;
            return (T)Convert.ChangeType(Convert.ToInt32(value) + Convert.ToInt32(arg), typeof(T));
        }
        public static T RemoveFlag<T>(this T value, T arg) where T : Enum
        {
            if (value.AND(arg)) return (T)Convert.ChangeType(Convert.ToInt32(value) - Convert.ToInt32(arg), typeof(T));
            return value;
        }

        public static IEnumerable<(T item, int index)> Indexed<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Select((item, index) => (item, index));
        }
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T element in enumerable) action(element);
        }
        public static int IndexOf<T>(this IEnumerable<T> enumerable, T value)
        {
            int index = 0;
            foreach (T item in enumerable)
            {
                if (item.Equals(value)) return index;
                index++;
            }

            return -1;
        }
        public static int FindIndex<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            int index = 0;
            foreach (T item in enumerable)
            {
                if (predicate(item)) return index;
                index++;
            }

            return -1;
        }
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> enumerable, T item)
        {
            IEnumerable<T> GetItem()
            {
                yield return item;
            }

            return enumerable.Union(GetItem());
        }
        public static IEnumerable<T> Except<T>(this IEnumerable<T> enumerable, T item)
        {
            IEnumerable<T> GetItem()
            {
                yield return item;
            }

            return enumerable.Except(GetItem());
        }
        public static IEnumerable<T> Once<T>(this T value)
        {
            yield return value;
        }

        public static string PrettyTypeName(this Type type) => $"{(type.IsGenericType ? type.Name.Substring(0, type.Name.IndexOf('`')) : type.Name)}{(type.IsGenericType ? $"<{string.Join(",", type.GenericTypeArguments.Select(x => x.PrettyTypeName()))}>" : "")}";
        #endregion

        #region Math Methods
        public static Vector3 ToXZ(this Vector3 value, float y = 0.0f)
        {
            return new Vector3(value.x, y, value.z);
        }
        public static Vector3 ToDirectionXZ(this Vector3 value)
        {
            return value.ToXZ().normalized;
        }
        public static Vector3 ToDirectionXZ(this Vector3 value, Vector3 target)
        {
            Vector3 temp = value;
            temp.y = 0;
            target.y = 0;
            return (target - temp).normalized;
        }
        public static bool IsInsideXZ(this Vector3 point, Vector3[] points)
        {
            bool inside = false;
            for (int i = 0, length = points.Length, j = length - 1; i < length; j = i++)
            {
                Vector3 a = points[i];
                Vector3 b = points[j];
                if (((a.z <= point.z && point.z < b.z) || (b.z <= point.z && point.z < a.z)) && (point.x < (b.x - a.x) * (point.z - a.z) / (b.z - a.z) + a.x)) inside = !inside;
            }

            return inside;
        }
        public static bool IsInsideXZ(this Vector3 point, List<Vector3> points)
        {
            bool inside = false;
            for (int i = 0, length = points.Count, j = length - 1; i < length; j = i++)
            {
                Vector3 a = points[i];
                Vector3 b = points[j];
                if (((a.z <= point.z && point.z < b.z) || (b.z <= point.z && point.z < a.z)) && (point.x < (b.x - a.x) * (point.z - a.z) / (b.z - a.z) + a.x)) inside = !inside;
            }

            return inside;
        }
        public static bool IsClockwise(this Vector3[] points)
        {
            float sum = 0;
            for (int i = 0, count = points.Length; i < count; i++)
            {
                Vector3 v1 = points[i];
                Vector3 v2 = points[(i + 1) % points.Length];
                sum += (v2.x - v1.x) * (v2.z + v1.z);
            }

            return sum > 0;
        }
        public static bool IsClockwise(this List<Vector3> points)
        {
            float sum = 0;
            for (int i = 0, count = points.Count; i < count; i++)
            {
                Vector3 v1 = points[i];
                Vector3 v2 = points[(i + 1) % points.Count];
                sum += (v2.x - v1.x) * (v2.z + v1.z);
            }

            return sum > 0;
        }
        public static int ToDigit(this bool value)
        {
            return value ? 1 : 0;
        }
        public static int ToSign(this bool value)
        {
            return value ? 1 : -1;
        }
        #endregion

        #region List Methods
        public static bool RemoveFast<T>(this IList<T> list, T value)
        {
            int index = list.IndexOf(value);
            if (index < 0) return false;
            list.RemoveFast(index);
            return true;
        }
        public static void RemoveFast<T>(this IList<T> list, int index)
        {
            int last = list.Count - 1;
            list[index] = list[last];
            list.RemoveAt(last);
        }
        public static T PopLast<T>(this IList<T> list)
        {
            int lastIndex = list.Count - 1;
            T value = list[lastIndex];
            list.RemoveAt(lastIndex);
            return value;
        }
        #endregion
    }
}