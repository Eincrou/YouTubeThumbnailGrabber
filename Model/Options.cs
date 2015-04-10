using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeThumbnailGrabber.Model
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
        /// <summary>
        /// Whether to automatically grab video thumbnail when a valid YouTube link is detected in the clipboard
        /// </summary>
        public bool AutoLoadURLs { get; set; }
        /// <summary>
        /// Whether to add the date of upload/publish for the current video
        /// </summary>
        public bool PublishedDateTitle { get; set; }
        /// <summary>
        /// Whether to append the number of views a video has received to the end of the video title
        /// </summary>
        public bool VideoViews { get; set; }
        /// <summary>
        /// The filenaming method to use for saving thumbnail images
        /// </summary>
        public FileNamingMode ImageFileNamingMode { get; set; }
    }
}
