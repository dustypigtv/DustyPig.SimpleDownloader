using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DustyPig.Utils
{
    public static class SimpleDownloader
    {
        static readonly HttpClient _httpClient = new HttpClient();

        static HttpRequestMessage CreateRequest(string url, IDictionary<string, string> headers)
        {
            var ret = new HttpRequestMessage(HttpMethod.Get, url);
            if (headers != null)
                foreach (var header in headers)
                    ret.Headers.Add(header.Key, header.Value);
            return ret;
        }

        static HttpRequestMessage CreateRequest(Uri uri, IDictionary<string, string> headers)
        {
            var ret = new HttpRequestMessage(HttpMethod.Get, uri);
            if (headers != null)
                foreach (var header in headers)
                    ret.Headers.Add(header.Key, header.Value);
            return ret;
        }

        static async Task<HttpResponseMessage> GetResponseAsync(string url, IDictionary<string, string> headers, CancellationToken cancellationToken)
        {
            using var request = CreateRequest(url, headers);
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return response;
        }

        static async Task<HttpResponseMessage> GetResponseAsync(Uri uri, IDictionary<string, string> headers, CancellationToken cancellationToken)
        {
            using var request = CreateRequest(uri, headers);
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return response;
        }


        static FileStream CreateFile(string filename)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
            return new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, true);
        }








        public static async Task DownloadFileAsync(string url, string filename, IDictionary<string, string> headers, CancellationToken cancellationToken)
        {
            using var response = await GetResponseAsync(url, headers, cancellationToken).ConfigureAwait(false);

#if NET5_0_OR_GREATER
            using var content = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#else
            using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
#endif

            using var fileStream = CreateFile(filename);

#if NET5_0_OR_GREATER
            await content.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
#else
            await content.CopyToAsync(fileStream).ConfigureAwait(false);
#endif
        }

        public static Task DownloadFileAsync(string url, string filename) =>
            DownloadFileAsync(url, filename, null, default);

        public static Task DownloadFileAsync(string url, string filename, IDictionary<string, string> headers) =>
            DownloadFileAsync(url, filename, headers, default);

        public static Task DownloadFileAsync(string url, string filename, CancellationToken cancellationToken) =>
            DownloadFileAsync(url, filename, null, cancellationToken);










        public static async Task DownloadFileAsync(Uri uri, string filename, IDictionary<string, string> headers, CancellationToken cancellationToken)
        {
            using var response = await GetResponseAsync(uri, headers, cancellationToken).ConfigureAwait(false);

#if NET5_0_OR_GREATER
            using var content = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#else
            using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
#endif

            using var fileStream = CreateFile(filename);

#if NET5_0_OR_GREATER
            await content.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
#else
            await content.CopyToAsync(fileStream).ConfigureAwait(false);
#endif
        }

        public static Task DownloadFileAsync(Uri uri, string filename) =>
            DownloadFileAsync(uri, filename, null, default);

        public static Task DownloadFileAsync(Uri uri, string filename, IDictionary<string, string> headers) =>
            DownloadFileAsync(uri, filename, headers, default);

        public static Task DownloadFileAsync(Uri uri, string filename, CancellationToken cancellationToken) =>
            DownloadFileAsync(uri, filename, null, cancellationToken);





        public static async Task<string> DownloadStringAsync(string url, IDictionary<string, string> headers, CancellationToken cancellationToken)
        {
            using var response = await GetResponseAsync(url, headers, cancellationToken).ConfigureAwait(false);

#if NET5_0_OR_GREATER
            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif
        }

        public static Task<string> DownloadStringAsync(string url) =>
            DownloadStringAsync(url, null, default);

        public static Task<string> DownloadStringAsync(string url, IDictionary<string, string> headers) =>
            DownloadStringAsync(url, headers, default);

        public static Task<string> DownloadStringAsync(string url, CancellationToken cancellationToken) =>
            DownloadStringAsync(url, null, cancellationToken);











        public static async Task<string> DownloadStringAsync(Uri uri, IDictionary<string, string> headers, CancellationToken cancellationToken)
        {
            using var response = await GetResponseAsync(uri, headers, cancellationToken).ConfigureAwait(false);
#if NET5_0_OR_GREATER
            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif
        }

        public static Task<string> DownloadStringAsync(Uri uri) =>
            DownloadStringAsync(uri, null, default);

        public static Task<string> DownloadStringAsync(Uri uri, IDictionary<string, string> headers) =>
            DownloadStringAsync(uri, headers, default);

        public static Task<string> DownloadStringAsync(Uri uri, CancellationToken cancellationToken) =>
            DownloadStringAsync(uri, null, cancellationToken);









        public static async Task<byte[]> DownloadDataAsync(string url, IDictionary<string, string> headers, CancellationToken cancellationToken)
        {
            using var response = await GetResponseAsync(url, headers, cancellationToken).ConfigureAwait(false);
#if NET5_0_OR_GREATER
            return await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
#else
            return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
#endif
        }

        public static Task<byte[]> DownloadDataAsync(string url) =>
            DownloadDataAsync(url, null, default);

        public static Task<byte[]> DownloadDataAsync(string url, IDictionary<string, string> headers) =>
            DownloadDataAsync(url, headers, default);

        public static Task<byte[]> DownloadDataAsync(string url, CancellationToken cancellationToken) =>
            DownloadDataAsync(url, null, cancellationToken);









        public static async Task<byte[]> DownloadDataAsync(Uri uri, IDictionary<string, string> headers, CancellationToken cancellationToken)
        {
            using var response = await GetResponseAsync(uri, headers, cancellationToken).ConfigureAwait(false);
#if NET5_0_OR_GREATER
            return await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
#else
            return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
#endif
        }

        public static Task<byte[]> DownloadDataAsync(Uri uri) =>
            DownloadDataAsync(uri, null, default);

        public static Task<byte[]> DownloadDataAsync(Uri uri, IDictionary<string, string> headers) =>
            DownloadDataAsync(uri, headers, default);

        public static Task<byte[]> DownloadDataAsync(Uri uri, CancellationToken cancellationToken) =>
            DownloadDataAsync(uri, null, cancellationToken);


    }
}
