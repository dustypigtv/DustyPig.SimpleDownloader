using System;

namespace DustyPig.Utils
{
    public class DownloadProgress
    {
        internal DownloadProgress(long downloaded, long total, long startedTicks)
        {
            DownloadedBytes = downloaded;
            TotalBytes = total;
            BytesRemaining = downloaded > total ? -1 : total - downloaded;
            Percent = downloaded > total ? -1 : downloaded / (double)total;

            long ticks = DateTime.Now.Ticks - startedTicks;
            BytesPerSecond = ticks <= 0 ? double.NaN : DownloadedBytes / TimeSpan.FromTicks(ticks).TotalSeconds;
          
            RemainingSeconds = BytesRemaining < 0 || double.IsNaN(BytesPerSecond) ? -1 : TimeSpan.FromSeconds(BytesRemaining / BytesPerSecond).TotalSeconds;
        }

        /// <summary>
        /// Total bytes that have been downloaded
        /// </summary>
        public long DownloadedBytes { get; }

        /// <summary>
        /// Total byte size of the file. -1 means the content headers didn't include the size
        /// </summary>
        public long TotalBytes { get; }

        /// <summary>
        /// -1 means <see cref="DownloadedBytes"/> &gt; <see cref="TotalBytes"/>
        /// </summary>
        public long BytesRemaining { get; }

        /// <summary>
        /// Percent complete. -1 means <see cref="DownloadedBytes"/> &gt; <see cref="TotalBytes"/>
        /// </summary>
        public double Percent { get; }

        /// <summary>
        /// <see cref="double.NaN"/> means the speed could not be accurately calculated
        /// </summary>
        public double BytesPerSecond { get; }

        /// <summary>
        /// -1 means <see cref="TotalBytes"/> == -1 or <see cref="BytesPerSecond"/> == <see cref="double.NaN"/>
        /// </summary>
        public double RemainingSeconds { get; }
    }
}
