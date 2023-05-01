using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;
using Converter = System.Convert;
using System.Buffers;

namespace Trackman
{
    public class JsonUtility : JsonUtilityShared<Generation>
    {
    }

    public class Generation
    {
    }

    public class JsonUtilitySharedBase<Gen> // NOTE: Gen parameter allows all inheritors have separate static variables
    {
        #region Containers
        protected abstract class VectorConverter<T, U> : JsonConverter<T>
        {
            #region Methods
            public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                Dictionary<string, U> dictionary = serializer.Deserialize<Dictionary<string, U>>(reader);
                if (Schema.All(x => dictionary.ContainsKey(x.Key))) return ReadValue(dictionary);
                return hasExistingValue ? existingValue : default;
            }
            public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
            {
                WriteValue(value, Schema);
                serializer.Serialize(writer, Schema);
            }
            protected abstract T ReadValue(Dictionary<string, U> dictionary);
            protected abstract void WriteValue(T value, Dictionary<string, U> dictionary);
            protected abstract Dictionary<string, U> Schema { get; }
            #endregion
        }

        protected class ColorConverter : VectorConverter<Color, float>
        {
            #region Methods
            protected override Color ReadValue(Dictionary<string, float> dictionary) => new(dictionary["r"], dictionary["g"], dictionary["b"], dictionary["a"]);
            protected override void WriteValue(Color value, Dictionary<string, float> dictionary)
            {
                dictionary["r"] = value.r; dictionary["g"] = value.g; dictionary["b"] = value.b; dictionary["a"] = value.a;
            }
            protected override Dictionary<string, float> Schema { get; } = new() { { "r", 0 }, { "g", 0 }, { "b", 0 }, { "a", 0 } };
            #endregion
        }

        protected class Vector2Converter : VectorConverter<Vector2, float>
        {
            #region Methods
            protected override Vector2 ReadValue(Dictionary<string, float> dictionary) => new(dictionary["x"], dictionary["y"]);
            protected override void WriteValue(Vector2 value, Dictionary<string, float> dictionary)
            {
                dictionary["x"] = value.x; dictionary["y"] = value.y;
            }
            protected override Dictionary<string, float> Schema { get; } = new() { { "x", 0 }, { "y", 0 } };
            #endregion
        }

        protected class Vector2IntConverter : VectorConverter<Vector2Int, int>
        {
            #region Methods
            protected override Vector2Int ReadValue(Dictionary<string, int> dictionary) => new(dictionary["x"], dictionary["y"]);
            protected override void WriteValue(Vector2Int value, Dictionary<string, int> dictionary)
            {
                dictionary["x"] = value.x; dictionary["y"] = value.y;
            }
            protected override Dictionary<string, int> Schema { get; } = new() { { "x", 0 }, { "y", 0 } };
            #endregion
        }

        protected class Vector3IntConverter : VectorConverter<Vector3Int, int>
        {
            #region Methods
            protected override Vector3Int ReadValue(Dictionary<string, int> dictionary) => new(dictionary["x"], dictionary["y"], dictionary["z"]);
            protected override void WriteValue(Vector3Int value, Dictionary<string, int> dictionary)
            {
                dictionary["x"] = value.x; dictionary["y"] = value.y; dictionary["z"] = value.z;
            }
            protected override Dictionary<string, int> Schema { get; } = new() { { "x", 0 }, { "y", 0 }, { "z", 0 } };
            #endregion
        }

        protected class Vector3Converter : VectorConverter<Vector3, float>
        {
            #region Methods
            protected override Vector3 ReadValue(Dictionary<string, float> dictionary) => new(dictionary["x"], dictionary["y"], dictionary["z"]);
            protected override void WriteValue(Vector3 value, Dictionary<string, float> dictionary)
            {
                dictionary["x"] = value.x; dictionary["y"] = value.y; dictionary["z"] = value.z;
            }
            protected override Dictionary<string, float> Schema { get; } = new() { { "x", 0 }, { "y", 0 }, { "z", 0 } };
            #endregion
        }

        protected class Vector4Converter : VectorConverter<Vector4, float>
        {
            #region Methods
            protected override Vector4 ReadValue(Dictionary<string, float> dictionary) => new(dictionary["x"], dictionary["y"], dictionary["z"], dictionary["w"]);
            protected override void WriteValue(Vector4 value, Dictionary<string, float> dictionary)
            {
                dictionary["x"] = value.x; dictionary["y"] = value.y; dictionary["z"] = value.z; dictionary["w"] = value.w;
            }
            protected override Dictionary<string, float> Schema { get; } = new() { { "x", 0 }, { "y", 0 }, { "z", 0 }, { "w", 0 } };
            #endregion
        }

        protected class QuaternionConverter : VectorConverter<Quaternion, float>
        {
            #region Methods
            protected override Quaternion ReadValue(Dictionary<string, float> dictionary) => new(dictionary["x"], dictionary["y"], dictionary["z"], dictionary["w"]);
            protected override void WriteValue(Quaternion value, Dictionary<string, float> dictionary)
            {
                dictionary["x"] = value.x; dictionary["y"] = value.y; dictionary["z"] = value.z; dictionary["w"] = value.w;
            }
            protected override Dictionary<string, float> Schema { get; } = new() { { "x", 0 }, { "y", 0 }, { "z", 0 }, { "w", 0 } };
            #endregion
        }

        protected class IgnoreTypesConverter : JsonConverter
        {
            #region Fields
            Type[] types;
            #endregion

            #region Constructors
            public IgnoreTypesConverter(params Type[] types)
            {
                this.types = types;
            }
            #endregion

            #region Methods
            public override bool CanConvert(Type objectType)
            {
                return types.Any(x => x.IsAssignableFrom(objectType));
            }
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                reader.Skip();
                return default;
            }
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteNull();
            }
            #endregion
        }

        protected class AnimationCurveConverter : JsonConverter<AnimationCurve>
        {
            #region Methods
            public override AnimationCurve ReadJson(JsonReader reader, Type objectType, AnimationCurve existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                reader.Skip();
                return hasExistingValue ? existingValue : default;
            }
            public override void WriteJson(JsonWriter writer, AnimationCurve value, JsonSerializer serializer)
            {
                writer.WriteNull();
            }
            #endregion
        }

        protected class IgnorePropertiesContractResolver : DefaultContractResolver
        {
            #region Methods
            protected override List<MemberInfo> GetSerializableMembers(Type objectType)
            {
                void AddMembers(Type type, List<MemberInfo> members)
                {
                    if (type.BaseType is not null && type.BaseType != typeof(MonoBehaviour)) AddMembers(type.BaseType, members);

                    foreach (FieldInfo info in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    {
                        if (info.Name.Contains("<") || info.Name.Contains(">")) continue;
                        if (info.FieldType.IsSubclassOf(typeof(Object)) && !info.FieldType.IsSubclassOf(typeof(MonoBehaviour))) continue;
                        if (info.FieldType.IsInterface) continue;
                        if (info.GetCustomAttribute<NonSerializedAttribute>() is not null) continue;

                        members.Add(info);
                    }
                }

                List<MemberInfo> members = new();
                AddMembers(objectType, members);

                return members;
            }
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                if (member.MemberType == MemberTypes.Property) return null;
                return base.CreateProperty(member, memberSerialization);
            }
            #endregion
        }
        #endregion

        #region Fields
        protected static JsonSerializerSettings settings;
        protected static JsonSerializer serializer;
        #endregion

        #region Constructors
        static JsonUtilitySharedBase()
        {
            settings = new JsonSerializerSettings();
            settings.Converters.Add(new IgnoreTypesConverter(typeof(Object)));
            settings.Converters.Add(new AnimationCurveConverter());
            settings.Converters.Add(new ColorConverter());
            settings.Converters.Add(new Vector2Converter());
            settings.Converters.Add(new Vector2IntConverter());
            settings.Converters.Add(new Vector3Converter());
            settings.Converters.Add(new Vector3IntConverter());
            settings.Converters.Add(new Vector4Converter());
            settings.Converters.Add(new QuaternionConverter());
            settings.Converters.Add(new StringEnumConverter());
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.ContractResolver = new IgnorePropertiesContractResolver() { IgnoreSerializableAttribute = false, IgnoreShouldSerializeMembers = false };
            serializer = JsonSerializer.Create(settings);
        }
        #endregion

        #region Methods
        public static string ToJson<T>(T value, bool prettyPrint)
        {
            using (StringWriter stringWriter = new StringWriter())
            using (JsonTextWriter jsonTextWriter = new JsonTextWriter(stringWriter) { Formatting = prettyPrint ? Formatting.Indented : Formatting.None })
            {
                serializer.Serialize(jsonTextWriter, value);
                return stringWriter.ToString();
            }
        }
        public static T FromJson<T>(string json)
        {
            using (StringReader stringReader = new StringReader(json))
            using (JsonTextReader jsonTextReader = new JsonTextReader(stringReader))
            {
                return serializer.Deserialize<T>(jsonTextReader);
            }
        }
        #endregion
    }

    public class JsonUtilityShared<Gen> : JsonUtilitySharedBase<Gen>
    {
        public const string contentType = "application/json";

        #region Containers
        class ArrayPool : IArrayPool<char>
        {
            #region Methods
            public char[] Rent(int minimumLength) => ArrayPool<char>.Shared.Rent(minimumLength);
            public void Return(char[] array) => ArrayPool<char>.Shared.Return(array);
            #endregion
        }
        #endregion

        #region Fields
        static readonly IArrayPool<char> arrayPool = new ArrayPool();
        #endregion

        #region Methods
        static void ToJson(object value, StreamWriter streamWriter)
        {
            JsonTextWriter jsonTextWriter = new(streamWriter) { ArrayPool = arrayPool, CloseOutput = false };
            serializer.Serialize(jsonTextWriter, value);
            jsonTextWriter.Flush();
        }
        public static byte[] ToJsonAlloc<T>(T value)
        {
            using MemoryStream stream = new();
            using StreamWriter streamWriter = new(stream);
            ToJson(value, streamWriter);
            return stream.ToArray();
        }
        public static (byte[], int) ToJson<T>(T value, byte[] buffer)
        {
            using MemoryStream memoryStream = new(buffer);
            using StreamWriter streamWriter = new(memoryStream);
            JsonUtility.ToJson(value, streamWriter);
            return (buffer, (int)memoryStream.Position);
        }
        public static string ToJson<T>(T value, bool prettyPrint, bool useArrayPool = true)
        {
            using StringWriter stringWriter = new();
            using JsonTextWriter jsonTextWriter = new(stringWriter) { Formatting = prettyPrint ? Formatting.Indented : Formatting.None };
            if (useArrayPool)
            {
                jsonTextWriter.ArrayPool = arrayPool;
            }
            serializer.Serialize(jsonTextWriter, value);
            return stringWriter.ToString();
        }
        public static object FromJson(byte[] bytes, int count)
        {
            using MemoryStream memoryStream = new(bytes, 0, count);
            using StreamReader streamReader = new(memoryStream);
            using JsonTextReader jsonTextReader = new(streamReader) { ArrayPool = arrayPool };
            return serializer.Deserialize(jsonTextReader);
        }
        public static T FromJson<T>(byte[] bytes)
        {
            return FromJson(bytes, typeof(T)).As<T>();
        }
        public static object FromJson(byte[] bytes, Type type = default)
        {
            using MemoryStream memoryStream = new(bytes);
            return FromJson(memoryStream, type);
        }
        public static object FromJson(Stream stream, Type type = default)
        {
            using StreamReader streamReader = new(stream);
            using JsonTextReader jsonTextReader = new(streamReader) { ArrayPool = arrayPool };
            return serializer.Deserialize(jsonTextReader, type);
        }
        public static object FromJson(string json, Type type = default, bool useArrayPool = true)
        {
            using StringReader stringReader = new(json);
            using JsonTextReader jsonTextReader = new(stringReader);
            if (useArrayPool)
            {
                jsonTextReader.ArrayPool = arrayPool;
            }
            return serializer.Deserialize(jsonTextReader, type);
        }
        public static T FromJson<T>(string json, bool useArrayPool = true)
        {
            using StringReader stringReader = new(json);
            using JsonTextReader jsonTextReader = new(stringReader);
            if (useArrayPool)
            {
                jsonTextReader.ArrayPool = arrayPool;
            }
            return serializer.Deserialize<T>(jsonTextReader);
        }

        public static object[] Convert(object[] objects, Type[] types)
        {
            for (int i = 0; i < objects.Length; ++i) objects[i] = Convert(objects[i], types[i]);
            return objects;
        }
        public static object Convert(object obj, Type parameterType)
        {
            if (TryConvert(obj, parameterType, out object result)) return result;
            else throw (Exception)result;
        }
        public static T Convert<T>(object obj) => (T)Convert(obj, typeof(T));
        public static bool TryConvert(object obj, Type parameterType, out object result, bool silent = false, bool ignoreResult = false)
        {
            bool CompareJTokenType(JTokenType type, Type parameterType)
            {
                switch (type)
                {
                    case JTokenType.Integer: return parameterType == typeof(int) || parameterType == typeof(float) || parameterType == typeof(double);
                    case JTokenType.Float: return parameterType == typeof(float) || parameterType == typeof(double);
                    case JTokenType.Boolean: return parameterType == typeof(bool);
                    default: return false;
                }
            }

            if (obj is null)
            {
                result = default;
                return parameterType.IsClass;
            }

            Type objType = obj.GetType();
            if (parameterType.IsAssignableFrom(objType) || parameterType == typeof(object))
            {
                result = obj;
                return true;
            }
            else if (obj is JArray jarray && parameterType.IsArray)
            {
                Type elementType = parameterType.GetElementType();
                Array array = Array.CreateInstance(elementType, jarray.Count);

                for (int i = 0; i < jarray.Count; ++i)
                {
                    if (TryConvert(jarray[i], elementType, out object element))
                    {
                        array.SetValue(element, i);
                    }
                    else
                    {
                        Debug.LogWarning($"Cannot convert {DebugUtility.GetString(obj)} to {parameterType}, element {DebugUtility.GetString(jarray[i])} to {elementType}");
                        return (result = default) is not null;
                    }
                }

                return (result = array) is not null;
            }
            else if (obj is JObject jobject && (!parameterType.IsPrimitive || CompareJTokenType(jobject.Type, parameterType)) && (parameterType.IsValueType || parameterType.IsClass)) return (result = jobject.ToObject(parameterType, serializer)) is not null;
            else if (obj is JValue jvalue && (!parameterType.IsPrimitive || CompareJTokenType(jvalue.Type, parameterType)) && (parameterType.IsValueType || parameterType.IsClass)) return (result = jvalue.ToObject(parameterType, serializer)) is not null;
            else if (parameterType.IsEnum)
            {
                if (obj is string stringEnumValue) return (result = Enum.Parse(parameterType, stringEnumValue)) is not null;
                else if (objType.IsPrimitive) return (result = Enum.ToObject(parameterType, obj)) is not null;
            }
            else if (obj is string stringValue)
            {
                if (parameterType == typeof(byte[])) return (result = Converter.FromBase64String(stringValue)) is not null;
                else if (typeof(Object).IsAssignableFrom(parameterType)) return (result = default) is not null;
                else if (parameterType == typeof(Color)) return (result = FromJson<Color>(stringValue)) is not null;
                else if (parameterType == typeof(Vector2)) return (result = FromJson<Vector2>(stringValue)) is not null;
                else if (parameterType == typeof(Vector3)) return (result = FromJson<Vector3>(stringValue)) is not null;
                else if (parameterType == typeof(Vector4)) return (result = FromJson<Vector4>(stringValue)) is not null;
                else if (parameterType == typeof(Quaternion)) return (result = FromJson<Quaternion>(stringValue)) is not null;
                else if (parameterType.IsValueType || parameterType.IsClass) return (result = FromJson(stringValue, parameterType)) is not null;
            }
            else if (objType.IsPrimitive && parameterType.IsPrimitive) return (result = Converter.ChangeType(obj, parameterType)) is not null;

            if (!silent) Debug.LogWarning($"Cannot convert {DebugUtility.GetString(obj)} of type {obj?.GetType()} to {parameterType}");
            return (result = default) is not null;
        }

        public static IEnumerable<string> Split(string text) => Split(text, ',', ' ', '\n');
        public static IEnumerable<string> Split(string text, params char[] separators)
        {
            bool inSingleQuote = false;
            bool inDoubleQuote = false;
            bool inArray = false;
            bool inParenthesis = false;
            int jsonDepth = 0;

            for (int i = 0, index = 0, length = text.Length; i < length; ++i)
            {
                if (text[i] == '\\' && (inDoubleQuote || inSingleQuote))
                {
                    ++i;
                    continue;
                }

                if (text[i] == '\'' && !inDoubleQuote) inSingleQuote = !inSingleQuote;
                if (text[i] == '"' && !inSingleQuote) inDoubleQuote = !inDoubleQuote;

                if (inSingleQuote || inDoubleQuote) continue;

                if (text[i] == '{') jsonDepth++;
                if (text[i] == '}') jsonDepth--;

                if (jsonDepth == 0)
                {
                    if (text[i] == '(' || text[i] == ')') inParenthesis = !inParenthesis;
                    if (text[i] == '[' || text[i] == ']') inArray = !inArray;
                }

                if (jsonDepth > 0 || inArray || inParenthesis) continue;

                if (Array.Exists(separators, x => x == text[i]))
                {
                    yield return text.Substring(index, i - index);
                    index = i + 1;
                }
                else if (i == length - 1)
                {
                    yield return text.Substring(index, i + 1 - index);
                    break;
                }
            }
        }
        public static bool Contains(string text, params char[] separators)
        {
            bool inQuote = false;
            bool inArray = false;
            int jsonDepth = 0;

            for (int i = 0, length = text.Length; i < length; ++i)
            {
                if (text[i] == '"') inQuote = !inQuote;
                if (!inQuote)
                {
                    if (text[i] == '{') jsonDepth++;
                    if (text[i] == '}') jsonDepth--;
                }
                if (!inQuote && jsonDepth == 0)
                {
                    if (text[i] == '[' || text[i] == ']') inArray = !inArray;
                }
                if (!inQuote && jsonDepth == 0 && !inArray)
                {
                    if (Array.Exists(separators, x => x == text[i]))
                    {
                        return true;
                    }
                    else if (i == length - 1)
                    {
                        break;
                    }
                }
            }

            return false;
        }
        #endregion
    }
}