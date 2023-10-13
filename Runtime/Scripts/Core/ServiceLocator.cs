using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Trackman
{
    public static class ServiceLocator
    {
        #region Fields
        static readonly Dictionary<Type, Array> interfaceCache = new();
        static MonoBehaviour[] monoCache;
        #endregion

        #region Methods
        public static void ClearInterfaces()
        {
            monoCache = default;
            interfaceCache.Clear();
        }
        public static void InitializeInterfaces()
        {
            Validate();
            InitializeInterfaces(Object.FindObjectsOfType<MonoBehaviour>(true));
        }
        public static void InitializeInterfaces(params MonoBehaviour[] behaviours)
        {
            monoCache = behaviours;
            interfaceCache.Clear();
        }
        public static T[] FindInterfaces<T>(this MonoBehaviour _) => FindInterfaces<T>();
        public static T[] FindInterfaces<T>()
        {
            Validate();

            Type type = typeof(T);
            if (interfaceCache.TryGetValue(type, out Array monoBehaviours)) return (T[])monoBehaviours;
            interfaceCache.Add(type, monoCache.OfType<T>().ToArray());

            return (T[])interfaceCache[type];
        }
        public static T FindInterface<T>(this MonoBehaviour _) where T : class => FindInterface<T>();
        public static T FindInterface<T>() where T : class
        {
            Validate();

            Type type = typeof(T);
            if (interfaceCache.TryGetValue(type, out Array monoBehaviours)) return ((T[])monoBehaviours)[0];

            T[] results = FindInterfaces<T>();
            if (results.Length == 1) return results[0];
            throw new NotSupportedException($"{nameof(FindInterfaces)} found {results.Length} interfaces of type {type.FullName}");
        }
        #endregion

        #region Support Methods
        [Conditional("UNITY_EDITOR")]
        static void Validate()
        {
            if (!Application.isPlaying && !Environment.StackTrace.Contains("UnityEngine.TestRunner")) // NOTE: there is no better way to detect we are called from test - Unity :(
            {
                throw new NotSupportedException("You're not in playmode! Supported only in playmode!");
            }
        }
        #endregion
    }
}