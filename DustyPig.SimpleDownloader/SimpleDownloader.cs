using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1 || NET5_0_OR_GREATER
using System.Buffers;
#endif

namespace DustyPig.Utils
{
    public static class SimpleDownloader
    {
        const int FILE_BUFFER_SIZE = 4096;
        const int COPYTO_BUFFER_SIZE = 81920;

        static readonly HttpClient _httpClient = new HttpClient();


        static void SetHeaders(HttpRequestMessage request, IDictionary<string, string> headers)
        {
            if (headers == null)
                return;

            foreach (var header in headers)
                request.Headers.Add(header.Key, header.Value);
        }

        static HttpRequestMessage CreateRequest(string url, IDictionary<string, string> headers)
        {
            var ret = new HttpRequestMessage(HttpMethod.Get, url);
            SetHeaders(ret, headers);
            return ret;
        }

        static HttpRequestMessage CreateRequest(Uri uri, IDictionary<string, string> headers)
        {
            var ret = new HttpRequestMessage(HttpMethod.Get, uri);
            SetHeaders(ret, headers);
            return ret;
        }

        static async Task<HttpResponseMessage> GetResponseAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return response;
        }

        static async Task<HttpResponseMessage> GetResponseAsync(string url, IDictionary<string, string> headers, CancellationToken cancellationToken)
        {
            using var request = CreateRequest(url, headers);
            return await GetResponseAsync(request, cancellationToken).ConfigureAwait(false);
        }

        static async Task<HttpResponseMessage> GetResponseAsync(Uri uri, IDictionary<string, string> headers, CancellationToken cancellationToken)
        {
            using var request = CreateRequest(uri, headers);
            return await GetResponseAsync(request, cancellationToken).ConfigureAwait(false);
        }


        static FileStream CreateFile(string filename)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
            return new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read, FILE_BUFFER_SIZE, true);
        }



        private static async Task DownloadFileAsync(HttpResponseMessage response, string filename, IProgress<DownloadProgress> progress, CancellationToken cancellationToken)
        {
            long started = DateTime.Now.Ticks;
            
#if NET5_0_OR_GREATER
            using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#else
            using var contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
#endif

            long totalBytes = response.Content.Headers.ContentLength ?? -1;
            long totalDownloaded = 0;

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1 || NET5_0_OR_GREATER
            byte[] buffer = ArrayPool<byte>.Shared.Rent(COPYTO_BUFFER_SIZE);
#else
            byte[] buffer = new byte[COPYTO_BUFFER_SIZE];
#endif

            using var fileStream = CreateFile(filename);

            try
            {
                while (true)
                {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1 || NET5_0_OR_GREATER
                    var read = await contentStream.ReadAsync(new Memory<byte>(buffer), cancellationToken).ConfigureAwait(false);
                    if (read > 0)
                        await fileStream.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, read), cancellationToken).ConfigureAwait(false);
#else
                    var read = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
                    if (read > 0)
                        await fileStream.WriteAsync(buffer, 0, read, cancellationToken).ConfigureAwait(false);
#endif
                    if (progress != null)
                    {
                        totalDownloaded += read;
                        progress.Report(new DownloadProgress(totalDownloaded, totalBytes, started));
                    }

                    if (read <= 0)
                        break;
                }
            }
            finally
            {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1 || NET5_0_OR_GREATER
                ArrayPool<byte>.Shared.Return(buffer);
#endif
            }
        }

        public static async Task DownloadFileAsync(string url, string filename, IDictionary<string, string> headers, IProgress<DownloadProgress> progress, CancellationToken cancellationToken)
        {
            using var response = await GetResponseAsync(url, headers, cancellationToken).ConfigureAwait(false);
            await DownloadFileAsync(response, filename, progress, cancellationToken).ConfigureAwait(false);   
        }

        public static Task DownloadFileAsync(string url, string filename) =>
            DownloadFileAsync(url, filename, null, null, default);

        public static Task DownloadFileAsync(string url, string filename, IDictionary<string, string> headers) =>
            DownloadFileAsync(url, filename, headers, null, default);

        public static Task DownloadFileAsync(string url, string filename, IProgress<DownloadProgress> progress) =>
            DownloadFileAsync(url, filename, null, progress, default);

        public static Task DownloadFileAsync(string url, string filename, CancellationToken cancellationToken) =>
            DownloadFileAsync(url, filename, null, null, cancellationToken);

        public static Task DownloadFileAsync(string url, string filename, IDictionary<string, string> headers, IProgress<DownloadProgress> progress) =>
            DownloadFileAsync(url, filename, headers, progress, default);

        public static Task DownloadFileAsync(string url, string filename, IDictionary<string, string> headers, CancellationToken cancellationToken) =>
            DownloadFileAsync(url, filename, headers, null, cancellationToken);

        public static Task DownloadFileAsync(string url, string filename, IProgress<DownloadProgress> progress, CancellationToken cancellationToken) =>
            DownloadFileAsync(url, filename, null, progress, cancellationToken);









        public static async Task DownloadFileAsync(Uri uri, string filename, IDictionary<string, string> headers, IProgress<DownloadProgress> progress, CancellationToken cancellationToken)
        {
            using var response = await GetResponseAsync(uri, headers, cancellationToken).ConfigureAwait(false);
            await DownloadFileAsync(response, filename, progress, cancellationToken).ConfigureAwait(false);
        }

        public static Task DownloadFileAsync(Uri uri, string filename) =>
            DownloadFileAsync(uri, filename, null, null, default);

        public static Task DownloadFileAsync(Uri uri, string filename, IDictionary<string, string> headers) =>
            DownloadFileAsync(uri, filename, headers, null, default);

        public static Task DownloadFileAsync(Uri uri, string filename, IProgress<DownloadProgress> progress) =>
            DownloadFileAsync(uri, filename, null, progress, default);

        public static Task DownloadFileAsync(Uri uri, string filename, CancellationToken cancellationToken) =>
            DownloadFileAsync(uri, filename, null, null, cancellationToken);

        public static Task DownloadFileAsync(Uri uri, string filename, IDictionary<string, string> headers, IProgress<DownloadProgress> progress) =>
            DownloadFileAsync(uri, filename, headers, progress, default);

        public static Task DownloadFileAsync(Uri uri, string filename, IDictionary<string, string> headers, CancellationToken cancellationToken) =>
            DownloadFileAsync(uri, filename, headers, null, cancellationToken);

        public static Task DownloadFileAsync(Uri uri, string filename, IProgress<DownloadProgress> progress, CancellationToken cancellationToken) =>
            DownloadFileAsync(uri, filename, null, progress, cancellationToken);









        public static async Task<string> DownloadStringAsync(string url, IDictionary<string, string> headers)
        {
            using var response = await GetResponseAsync(url, headers, default).ConfigureAwait(false);
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public static Task<string> DownloadStringAsync(string url) =>
            DownloadStringAsync(url, null);








        public static async Task<string> DownloadStringAsync(Uri uri, IDictionary<string, string> headers)
        {
            using var response = await GetResponseAsync(uri, headers, default).ConfigureAwait(false);
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public static Task<string> DownloadStringAsync(Uri uri) =>
            DownloadStringAsync(uri, null);







        public static async Task<byte[]> DownloadDataAsync(string url, IDictionary<string, string> headers)
        {
            using var response = await GetResponseAsync(url, headers, default).ConfigureAwait(false);
            return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        }

        public static Task<byte[]> DownloadDataAsync(string url) =>
            DownloadDataAsync(url, null);









        public static async Task<byte[]> DownloadDataAsync(Uri uri, IDictionary<string, string> headers)
        {
            using var response = await GetResponseAsync(uri, headers, default).ConfigureAwait(false);
            return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        }

        public static Task<byte[]> DownloadDataAsync(Uri uri) =>
            DownloadDataAsync(uri, null);
    }
}
