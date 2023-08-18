using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Trackman
{
    public static class HttpExtensions
    {
        public const int defaultTimeout = 30;
        public const string requestSucceeded = "ok";
        public const string sentOk = "Sent";
        public const string receivedOk = "Received";

        #region Fields
        static readonly List<UnityWebRequest> webRequests = new(16);
        #endregion

        #region Properties
        public static List<UnityWebRequest> WebRequests => webRequests;
        static readonly Lazy<ITracing> tracing = new(() => UnityEngine.Object.FindObjectsOfType<MonoBehaviour>().OfType<ITracing>().FirstOrDefault());
        #endregion

        #region Methods
        public static Task<byte[]> HttpGetAsync(this string url, IDictionary<string, string> headers = default, IDictionary<string, string> responseHeaders = default, string contentType = default, string type = "Http", int timeout = defaultTimeout, CancellationToken cancellationToken = default)
        {
            return url.HttpMethodAsync("GET", null, headers, responseHeaders, contentType, type, timeout, cancellationToken);
        }
        public static Task<byte[]> HttpPostAsync(this string url, byte[] post, IDictionary<string, string> headers = default, IDictionary<string, string> responseHeaders = default, string contentType = default, string type = "Http", int timeout = defaultTimeout, CancellationToken cancellationToken = default)
        {
            return url.HttpMethodAsync("POST", post, headers, responseHeaders, contentType, type, timeout, cancellationToken);
        }
        public static Task<byte[]> HttpPutAsync(this string url, byte[] post, IDictionary<string, string> headers = default, IDictionary<string, string> responseHeaders = default, string contentType = default, string type = "Http", int timeout = defaultTimeout, CancellationToken cancellationToken = default)
        {
            return url.HttpMethodAsync("PUT", post, headers, responseHeaders, contentType, type, timeout, cancellationToken);
        }
        public static Task HttpDeleteAsync(this string url, IDictionary<string, string> headers = default, IDictionary<string, string> responseHeaders = default, string type = "Http", int timeout = defaultTimeout, CancellationToken cancellationToken = default)
        {
            return url.HttpMethodAsync("DELETE", null, headers, responseHeaders, null, type, timeout, cancellationToken);
        }
        static async Task<byte[]> HttpMethodAsync(this string url, string method, byte[] post = default, IDictionary<string, string> headers = default, IDictionary<string, string> responseHeaders = default, string contentType = default, string type = "Http", int timeout = defaultTimeout, CancellationToken cancellationToken = default)
        {
            string DebugString(byte[] response = default)
            {
                string PrettyPrintBytes(byte[] bytes, IDictionary<string, string> headers)
                {
                    if (bytes is null) return "null";
                    bool textContent = (headers is not null && headers.TryGetValue("Content-Type", out string value) && value.Contains("Application/json")) || (contentType.NotNullOrEmpty() && contentType.Contains("Application/json"));
                    return textContent ? Encoding.UTF8.GetString(bytes) : $"<binary {bytes.Length} bytes>";
                }

                string headersString = "";
                if (headers is not null)
                {
                    headersString = "HEADERS:\n";
                    foreach (KeyValuePair<string, string> header in headers) headersString += $"{header.Key}: {header.Value}\n";
                    if (contentType.NotNullOrEmpty()) headersString += $"Content-Type: {contentType}\n";
                }
                else if (contentType.NotNullOrEmpty())
                {
                    headersString = "HEADERS:\n";
                    if (contentType.NotNullOrEmpty()) headersString += $"Content-Type: {contentType}\n";
                }

                string postString = "";
                if (post is not null && post.Length < 4096) postString = $"POST:\n{PrettyPrintBytes(post, headers)}\n";

                string responseHeadersString = "";
                if (responseHeaders is not null)
                {
                    responseHeadersString = "RESPONSE HEADERS:\n";
                    foreach (KeyValuePair<string, string> header in responseHeaders) responseHeadersString += $"{header.Key}: {header.Value}\n";
                }

                string responseString = "";
                if (response is not null && response.Length < 4096) responseString = $"RESPONSE:\n{PrettyPrintBytes(response, responseHeaders)}\n";

                string text = "";
                if (headers is not null) text += headersString;
                if (post is not null) text += postString;
                if (responseHeaders is not null) text += responseHeadersString;
                if (response is not null) text += responseString;
                return text;
            }
            string ByteSizeString(UnityWebRequest value)
            {
                if (value.uploadedBytes == 0) return $"{value.downloadHandler.nativeData.Length.ToByteSize()}";
                return $"{value.downloadHandler.nativeData.Length.ToByteSize()} {value.uploadedBytes.ToByteSize()}";
            }

            using var scope = tracing.Value?.Scope($"{nameof(HttpExtensions)}.{nameof(HttpMethodAsync)}");
            headers = tracing.Value?.Inject(tracing.Value?.Active, headers ?? new Dictionary<string, string>()) ?? headers;

            UnityWebRequest request = new(url, method, new DownloadHandlerBuffer(), default) { timeout = timeout };

            if (headers is not null) foreach (KeyValuePair<string, string> header in headers) request.SetRequestHeader(header.Key, header.Value);
            if (post is not null) request.uploadHandler = new UploadHandlerRaw(post);
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
                webRequests.Remove(request);
                if (request.uploadHandler is not null) request.uploadHandler.Dispose();
            }

            if (responseHeaders is not null) foreach (KeyValuePair<string, string> keyValue in request.GetResponseHeaders()) responseHeaders.Add(keyValue);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[{type.Nick()}] {method} {request.url} = {request.result} \n{request.responseCode}\n{request.error}\n{DebugString()}");
                throw new Exception(request.responseCode.ToString(), new Exception(request.error));
            }
            else
            {
                Debug.Log($"[{type.Nick()}] {method} {request.url} {ByteSizeString(request)} \n{DebugString(request.downloadHandler.data)}");
                return request.downloadHandler.data;
            }
        }
        #endregion
    }
}