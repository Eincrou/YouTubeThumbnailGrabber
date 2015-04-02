using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeThumbnailGrabber
{
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
    }
}
