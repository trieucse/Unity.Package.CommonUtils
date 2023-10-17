using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Trackman
{
    public static class HttpExtensions
    {
        public const int defaultTimeout = 30;
        public const int saneDumpLength = 4096;
        public const string requestSucceeded = "ok";
        public const string sentOk = "Sent";
        public const string receivedOk = "Received";

        #region Fields
        static readonly List<UnityWebRequest> webRequests = new(16);
        static readonly StringBuilder debugString = new(1024);
        #endregion

        #region Properties
        public static List<UnityWebRequest> WebRequests => webRequests;
        static readonly Lazy<ITracing> tracing = new(() => UnityEngine.Object.FindObjectsOfType<MonoBehaviour>().OfType<ITracing>().FirstOrDefault());
        #endregion

        #region Methods
        public static async Task<byte[]> HttpGetAsync(this string url, IDictionary<string, string> headers = default, IDictionary<string, string> responseHeaders = default, string contentType = default, string type = "Http", int timeout = defaultTimeout, CancellationToken cancellationToken = default)
        {
            UnityWebRequest request = await url.HttpMethodAsync("GET", null, null, headers, responseHeaders, contentType, type, timeout, cancellationToken);
            return request.downloadHandler.data;
        }
        public static async Task<byte[]> HttpPostAsync(this string url, byte[] post, IDictionary<string, string> headers = default, IDictionary<string, string> responseHeaders = default, string contentType = default, string type = "Http", int timeout = defaultTimeout, CancellationToken cancellationToken = default)
        {
            UnityWebRequest request = await url.HttpMethodAsync("POST", post, null, headers, responseHeaders, contentType, type, timeout, cancellationToken);
            return request.downloadHandler.data;
        }
        public static async Task<byte[]> HttpPostAsync(this string url, NativeArray<byte> post, IDictionary<string, string> headers = default, IDictionary<string, string> responseHeaders = default, string contentType = default, string type = "Http", int timeout = defaultTimeout, CancellationToken cancellationToken = default)
        {
            UnityWebRequest request = await url.HttpMethodAsync("POST", null, post, headers, responseHeaders, contentType, type, timeout, cancellationToken);
            return request.downloadHandler.data;
        }
        public static async Task<byte[]> HttpPutAsync(this string url, byte[] post, IDictionary<string, string> headers = default, IDictionary<string, string> responseHeaders = default, string contentType = default, string type = "Http", int timeout = defaultTimeout, CancellationToken cancellationToken = default)
        {
            UnityWebRequest request = await url.HttpMethodAsync("PUT", post, null, headers, responseHeaders, contentType, type, timeout, cancellationToken);
            return request.downloadHandler.data;
        }
        public static async Task<byte[]> HttpPutAsync(this string url, NativeArray<byte> post, IDictionary<string, string> headers = default, IDictionary<string, string> responseHeaders = default, string contentType = default, string type = "Http", int timeout = defaultTimeout, CancellationToken cancellationToken = default)
        {
            UnityWebRequest request = await url.HttpMethodAsync("PUT", null, post, headers, responseHeaders, contentType, type, timeout, cancellationToken);
            return request.downloadHandler.data;
        }
        public static Task HttpDeleteAsync(this string url, IDictionary<string, string> headers = default, IDictionary<string, string> responseHeaders = default, string type = "Http", int timeout = defaultTimeout, CancellationToken cancellationToken = default)
        {
            return url.HttpMethodAsync("DELETE", null, null, headers, responseHeaders, null, type, timeout, cancellationToken);
        }
        public static Task<UnityWebRequest> HttpGetRequestAsync(this string url, IDictionary<string, string> headers = default, IDictionary<string, string> responseHeaders = default, string contentType = default, string type = "Http", int timeout = defaultTimeout, CancellationToken cancellationToken = default)
        {
            return url.HttpMethodAsync("GET", null, null, headers, responseHeaders, contentType, type, timeout, cancellationToken);
        }

        static async Task<UnityWebRequest> HttpMethodAsync(this string url, string method, byte[] bytes = default, NativeArray<byte>? nativeArray = default, IDictionary<string, string> headers = default, IDictionary<string, string> responseHeaders = default, string contentType = default, string type = "Http", int timeout = defaultTimeout, CancellationToken cancellationToken = default)
        {
            string DebugString(byte[] response = default)
            {
                string PrettyPrintBytes(byte[] bytes, IDictionary<string, string> headers)
                {
                    if (bytes is null) return "null";
                    bool textContent = (headers is not null && headers.TryGetValue("Content-Type", out string value) && value.Contains("Application/json")) || (contentType.NotNullOrEmpty() && contentType.Contains("Application/json"));
                    return textContent && bytes.Length < saneDumpLength ? Encoding.UTF8.GetString(bytes) : $"<binary {bytes.Length} bytes>";
                }

                debugString.Clear();

                if (headers is not null || contentType.NotNullOrEmpty()) debugString.AppendLine("HEADERS:");

                headers?.ForEach((header, debugString) => debugString.AppendLine($"{header.Key}: {header.Value}"), debugString);
                if (contentType.NotNullOrEmpty()) debugString.AppendLine($"Content-Type: {contentType}");

                if (bytes is not null) debugString.AppendLine($"POST:\n{PrettyPrintBytes(bytes, headers)}");
                else if (nativeArray is not null) debugString.AppendLine($"POST:\n<binary {nativeArray.Value.Length} bytes>");

                if (responseHeaders is not null)
                {
                    debugString.AppendLine("RESPONSE HEADERS:");
                    responseHeaders.ForEach((header, debugString) => debugString.AppendLine($"{header.Key}: {header.Value}"), debugString);
                }

                if (response is not null) debugString.AppendLine($"RESPONSE:\n{PrettyPrintBytes(response, responseHeaders)}");

                return debugString.ToString();
            }
            string ByteSizeString(UnityWebRequest value)
            {
                if (value.uploadedBytes == 0) return $"{value.downloadHandler.nativeData.Length.ToByteSize()}";
                return $"{value.downloadHandler.nativeData.Length.ToByteSize()} {value.uploadedBytes.ToByteSize()}";
            }

            using IDisposable scope = tracing.Value?.Scope($"{nameof(HttpExtensions)}.{nameof(HttpMethodAsync)}");
            headers = tracing.Value?.Inject(tracing.Value?.Active, headers ?? new Dictionary<string, string>()) ?? headers;

            UnityWebRequest request = new(url, method, new DownloadHandlerBuffer(), default) { timeout = timeout };

            headers?.ForEach((header, request) => request.SetRequestHeader(header.Key, header.Value), request);

            if (bytes is not null) request.uploadHandler = new UploadHandlerRaw(bytes);
            else if (nativeArray is not null) request.uploadHandler = new UploadHandlerRaw(nativeArray.Value, false);

            if (contentType.NotNullOrEmpty()) request.SetRequestHeader("Content-Type", contentType);

            webRequests.Add(request);
            try
            {
                UnityWebRequestAsyncOperation operation = request.SendWebRequest();
                // ReSharper disable once UseAwaitUsing, because of Sentry injection limitation
                using CancellationTokenRegistration registration = cancellationToken.Register(request.Abort);
                await operation;
                cancellationToken.ThrowIfCancellationRequested();
            }
            finally
            {
                webRequests.RemoveFast(request);
                request.uploadHandler?.Dispose();
            }

            responseHeaders?.ForEach((header, responseHeaders) => responseHeaders.Add(header), responseHeaders);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[{type.Nick()}] {method} {request.url} = {request.result} \n{request.responseCode}\n{request.error}\n{DebugString()}");
                throw new Exception(request.responseCode.ToString(), new Exception(request.error));
            }

            Debug.Log($"[{type.Nick()}] {method} {request.url} {ByteSizeString(request)} \n{DebugString(request.downloadHandler.data)}");
            return request;
        }
        #endregion
    }
}