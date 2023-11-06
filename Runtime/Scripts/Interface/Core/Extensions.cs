using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

#pragma warning disable IDE0060, RCS1175

namespace Trackman
{
    [DebuggerStepThrough]
    public static class Extensions
    {
        #region Object Methods
        public static void DestroySafe(this Object value)
        {
            if (!value) return;
#if UNITY_EDITOR
            if (Application.isPlaying) Object.Destroy(value);
            else Object.DestroyImmediate(value);
#else
            Object.Destroy(value);
#endif
        }
        public static void ReleaseSafe(this GraphicsBuffer buffer)
        {
            if (buffer is not null && buffer.IsValid()) buffer.Release();
        }
        public static T OrNull<T>(this T unityObject) where T : Object => unityObject ? unityObject : null;

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

        public static T As<T>(this object value) => (T)value;
        public static int AsInt<T>(this T value) where T : struct, Enum => (int)(object)value;
        public static T AsEnum<T>(this object value) where T : struct, Enum =>
            value switch
            {
                T enumValue => enumValue,
                string stringValue => (T)Enum.Parse(typeof(T), stringValue),
                _ => throw new ArgumentException()
            };

        public static bool ANY<T>(this T value, T arg) where T : Enum => Convert.ToInt32(value).ANY(Convert.ToInt32(arg));
        public static bool ANY(this int value, int arg) => (value & arg) != 0;
        public static bool AND<T>(this T value, T arg) where T : Enum => Convert.ToInt32(value).AND(Convert.ToInt32(arg));
        public static bool AND(this int value, int arg) => (value & arg) == arg;
        public static T AddFlag<T>(this T value, T arg) where T : Enum => (T)Enum.ToObject(typeof(T), Convert.ToInt32(value) | Convert.ToInt32(arg));
        public static T RemoveFlag<T>(this T value, T arg) where T : Enum => (T)Enum.ToObject(typeof(T), Convert.ToInt32(value) & ~Convert.ToInt32(arg));

        public static IEnumerable<(T item, int index)> Indexed<T>(this IEnumerable<T> enumerable) => enumerable.Select((item, index) => (item, index));
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T element in enumerable) action(element);
        }
        public static void ForEach<T, U>(this IEnumerable<T> enumerable, Action<T, U> action, U argument)
        {
            foreach (T element in enumerable) action(element, argument);
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
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> enumerable, T item) => enumerable.Union(item.Once());
        public static IEnumerable<T> Except<T>(this IEnumerable<T> enumerable, T item) => enumerable.Except(item.Once());
        public static IEnumerable<T> Once<T>(this T value)
        {
            yield return value;
        }

        public static bool IsRecord(this Type type) => (type?.Name.EndsWith("Record") ?? false) || (type?.Namespace?.EndsWith("DataModel") ?? false);

        public static string PrettyTypeName(this Type type) => $"{(type.IsGenericType ? type.Name.Substring(0, type.Name.IndexOf('`')) : type.Name)}{(type.IsGenericType ? $"<{string.Join(",", type.GenericTypeArguments.Select(x => x.PrettyTypeName()))}>" : "")}";
        #endregion

        #region Math Methods
        public static Vector3 ToXZ(this Vector3 value, float y = 0) => new(value.x, y, value.z);
        public static Vector3 ToDirectionXZ(this Vector3 value) => value.ToXZ().normalized;
        public static Vector3 ToDirectionXZ(this Vector3 value, Vector3 target)
        {
            Vector3 temp = value;
            temp.y = 0;
            target.y = 0;
            return (target - temp).normalized;
        }
        public static bool IsInsideXZ<T>(this Vector3 point, T points) where T : IReadOnlyList<Vector3>
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
        public static bool IsClockwise<T>(this T points) where T : IReadOnlyList<Vector3>
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
        public static int ToDigit(this bool value) => value ? 1 : 0;
        public static int ToSign(this bool value) => value ? 1 : -1;
        public static Vector3 ToVector3(this IReadOnlyList<float> value) => new (value[0], value[1], value[2]);
        #endregion

        #region List Methods
        public static bool TryRemoveFast<T>(this IList<T> list, T value)
        {
            int index = list.IndexOf(value);
            if (index < 0) return false;
            list.RemoveFast(index);
            return true;
        }
        public static void RemoveFast<T>(this IList<T> list, T value)
        {
            int index = list.IndexOf(value);
            list.RemoveFast(index);
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