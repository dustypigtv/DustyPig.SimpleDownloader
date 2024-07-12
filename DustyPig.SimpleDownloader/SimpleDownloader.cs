using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DustyPig.Utils;

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


    static FileStream CreateFile(FileInfo fileInfo)
    {
        fileInfo.Directory.Create();
        return new FileStream(fileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.Read, FILE_BUFFER_SIZE, true);
    }



    static async Task DownloadFileAsync(HttpResponseMessage response, FileInfo fileInfo, IProgress<DownloadProgress> progress, CancellationToken cancellationToken)
    {
        long started = DateTime.Now.Ticks;

        using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        long totalBytes = response.Content.Headers.ContentLength ?? -1;
        long totalDownloaded = 0;

        byte[] buffer = ArrayPool<byte>.Shared.Rent(COPYTO_BUFFER_SIZE);
        using var fileStream = CreateFile(fileInfo);

        try
        {
            while (true)
            {
                var read = await contentStream.ReadAsync(new Memory<byte>(buffer), cancellationToken).ConfigureAwait(false);
                if (read > 0)
                    await fileStream.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, read), cancellationToken).ConfigureAwait(false);
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
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }



    public static async Task DownloadFileAsync(Uri uri, FileInfo fileInfo, IDictionary<string, string> headers = null, IProgress<DownloadProgress> progress = null, CancellationToken cancellationToken = default)
    {
        using var response = await GetResponseAsync(uri, headers, cancellationToken).ConfigureAwait(false);
        await DownloadFileAsync(response, fileInfo, progress, cancellationToken).ConfigureAwait(false);
        fileInfo.Refresh();
    }

    public static Task DownloadFileAsync(Uri uri, string filename, IDictionary<string, string> headers = null, IProgress<DownloadProgress> progress = null, CancellationToken cancellationToken = default) =>
        DownloadFileAsync(uri, new FileInfo(filename), headers, progress, cancellationToken);



    public static async Task DownloadFileAsync(string url, FileInfo fileInfo, IDictionary<string, string> headers = null, IProgress<DownloadProgress> progress = null, CancellationToken cancellationToken = default)
    {
        using var response = await GetResponseAsync(url, headers, cancellationToken).ConfigureAwait(false);
        await DownloadFileAsync(response, fileInfo, progress, cancellationToken).ConfigureAwait(false);
        fileInfo.Refresh();
    }

    public static Task DownloadFileAsync(string url, string filename, IDictionary<string, string> headers = null, IProgress<DownloadProgress> progress = null, CancellationToken cancellationToken = default) =>
        DownloadFileAsync(url, new FileInfo(filename), headers, progress, cancellationToken);










    /// <summary>
    /// Uses a HEAD request to get the content size, returns -1 if not specified by the server
    /// </summary>
    public static async Task<long> GetDownloadSizeAsync(string url, IDictionary<string, string> headers = null, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, url);
        SetHeaders(request, headers);
        using var response = await GetResponseAsync(request, cancellationToken).ConfigureAwait(false);
        return response.Content.Headers.ContentLength ?? -1;
    }



    /// <summary>
    /// Uses a HEAD request to get the content size, returns -1 if not specified by the server
    /// </summary>
    public static async Task<long> GetDownloadSizeAsync(Uri uri, IDictionary<string, string> headers = null, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, uri);
        SetHeaders(request, headers);
        using var response = await GetResponseAsync(request, cancellationToken).ConfigureAwait(false);
        return response.Content.Headers.ContentLength ?? -1;
    }

    






    public static async Task<string> DownloadStringAsync(string url, IDictionary<string, string> headers = null, CancellationToken cancellationToken = default)
    {
        using var response = await GetResponseAsync(url, headers, cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }



    public static async Task<string> DownloadStringAsync(Uri uri, IDictionary<string, string> headers = null, CancellationToken cancellationToken = default)
    {
        using var response = await GetResponseAsync(uri, headers, cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }






    public static async Task<byte[]> DownloadDataAsync(string url, IDictionary<string, string> headers = null, CancellationToken cancellationToken = default)
    {
        using var response = await GetResponseAsync(url, headers, cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
    }




    public static async Task<byte[]> DownloadDataAsync(Uri uri, IDictionary<string, string> headers = null, CancellationToken cancellationToken = default)
    {
        using var response = await GetResponseAsync(uri, headers, cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
    }

}
