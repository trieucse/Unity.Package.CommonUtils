using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Trackman
{
#if UNITY_EDITOR
    using UnityPrefs = UnityEditor.EditorPrefs;
#else
    using UnityPrefs = UnityEngine.PlayerPrefs;
#endif
    public abstract class Prefs : IPrefs
    {
        #region Properties
        protected abstract string ScopePrefix { get; }
        public string Prefix { get; set; } = string.Empty;

        ICollection<string> IDictionary<string, object>.Keys => throw new NotImplementedException();
        ICollection<object> IDictionary<string, object>.Values => throw new NotImplementedException();
        int ICollection<KeyValuePair<string, object>>.Count => throw new NotImplementedException();
        bool ICollection<KeyValuePair<string, object>>.IsReadOnly => false;
        public object this[string key] { get => Read<object>(key, null); set => Write(key, value); }
        #endregion

        #region Methods
        public bool Contains(string path) => UnityPrefs.HasKey(GetFullPath(path));
        public void Write(string path, object value)
        {
            if (value is bool boolValue) UnityPrefs.SetInt(GetFullPath(path), boolValue ? 1 : 0);
            else if (value is float floatValue) UnityPrefs.SetFloat(GetFullPath(path), floatValue);
            else if (value is int intValue) UnityPrefs.SetInt(GetFullPath(path), intValue);
            else if (value is string stringValue) UnityPrefs.SetString(GetFullPath(path), stringValue);
            else UnityPrefs.SetString(GetFullPath(path), DebugUtility.GetString(value));
        }
        public T Read<T>(string path, T defaultValue)
        {
            try
            {
                if (defaultValue is bool boolValue) return (T)(object)(UnityPrefs.GetInt(GetFullPath(path), boolValue ? 1 : 0) == 1);
                if (defaultValue is float floatValue) return (T)(object)UnityPrefs.GetFloat(GetFullPath(path), floatValue);
                if (defaultValue is int intValue) return (T)(object)UnityPrefs.GetInt(GetFullPath(path), intValue);
                if (defaultValue is string stringValue) return (T)(object)UnityPrefs.GetString(GetFullPath(path), stringValue);
                if (defaultValue == null)
                {
                    string stringVal = UnityPrefs.GetString(GetFullPath(path), "");
                    if (stringVal.NullOrEmpty()) return default;
                    if (bool.TryParse(stringVal, out boolValue)) return (T)(object)boolValue;
                    if (int.TryParse(stringVal, out intValue)) return (T)(object)intValue;
                    if (float.TryParse(stringVal, out floatValue)) return (T)(object)floatValue;
                    return (T)(object)stringVal;
                }
                return DebugUtility.FromString<T>(UnityPrefs.GetString(GetFullPath(path), DebugUtility.GetString(defaultValue)));
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            return defaultValue;
        }
        #endregion

        #region Dicitonary Methods
        public void Add(string key, object value) => Write(key, value);
        public bool ContainsKey(string key) => Contains(key);
        public bool Remove(string key)
        {
            bool contains = Contains(key);
            if (contains) UnityPrefs.DeleteKey(GetFullPath(key));
            return contains;
        }
        bool IDictionary<string, object>.TryGetValue(string key, out object value)
        {
            bool contains = Contains(key);
            if (contains) value = this[key];
            else value = default;
            return contains;
        }
        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item) => Add(item.Key, item.Value);
        void ICollection<KeyValuePair<string, object>>.Clear() => throw new NotSupportedException();
        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item) => Contains(item.Key);
        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => throw new NotSupportedException();
        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item) => Remove(item.Key);
        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() => throw new NotSupportedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();
        #endregion

        #region Support Methods
        string GetFullPath(string path) => $"{ScopePrefix}.Prefs.{(Prefix.NotNullOrEmpty() ? Path.Combine(Prefix, path) : path)}";
        #endregion
    }

    public class ProductPrefs : Prefs
    {
        #region Properties
        protected override string ScopePrefix { get; }
        #endregion

        #region Constructors
        public ProductPrefs(Scene scene)
        {
#if UNITY_EDITOR
            if (!scene.IsValid()) throw new ArgumentException();
            UnityEditor.PackageManager.PackageInfo scenePackageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(scene.path);
            ScopePrefix = scenePackageInfo?.displayName.Split('.')[^1] ?? Application.productName.Replace(" ", "");
#else
            ScopePrefix = Application.productName;
#endif
        }
        #endregion
    }

    public class ProjectPrefs : Prefs
    {
        #region Fields
        static readonly Lazy<string> productName = new(() => Application.productName.Replace(" ", ""));
        #endregion

        #region Properties
        protected override string ScopePrefix => productName.Value;
        #endregion
    }

    public class GlobalPrefs : Prefs
    {
        #region Properties
        protected override string ScopePrefix => "Shared";
        #endregion
    }
}