using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeThumbnailGrabber
{
    public enum FileNamingMode
    {
        ChannelTitle = 0,
        VideoID = 1
    }
    /// <summary>
    /// User options to be serialized.
    /// </summary>
    public struct Options
    {
        /// <summary>
        /// Directory to save thumbnail images.
        /// </summary>
        public string SaveImagePath { get; set; }
        /// <summary>
        /// Whether to save images to the save directory as soon as they're grabbed.
        /// </summary>
        public bool AutoSaveImages { get; set; }
        public bool AutoLoadURLs { get; set; }
        public bool PublishedDateTitle { get; set; }
        public FileNamingMode ImageFileNamingMode { get; set; }
    }
}
