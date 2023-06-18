using System;

namespace DustyPig.SimpleDownloader
{
    public class DownloadProgress
    {
        internal DownloadProgress(long downloaded, long total)
        {
            DownloadedBytes = Math.Max(0, downloaded);
            TotalBytes = Math.Max(0, total);
            Progress = TotalBytes == 0 ? -1 : downloaded / (double)total * 100;
        }

        public long DownloadedBytes { get; }
        public long TotalBytes { get; }
        public double Progress { get; }
    }
}
