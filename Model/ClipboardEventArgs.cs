using System;
using System.Windows.Media.Imaging;

namespace YouTubeThumbnailGrabber.Model
{
    class ClipboardEventArgs : EventArgs
    {
        /// <summary>
        /// The text currently in the clipboard
        /// </summary>
        public string ClipboardText { get; private set; }
        public BitmapFrame ClipboardImage { get; private set; }
        /// <summary>
        /// Initializes an instance of a ClipboardEventArgs object to pass along text from the clipboard. (All other properties are null)
        /// </summary>
        /// <param name="clipboardText">Text from the clipboard</param>
        public ClipboardEventArgs(string clipboardText)
        {
            ClipboardText = clipboardText;
            ClipboardImage = null;
        }
        /// <summary>
        /// Initializes an instance of a ClipboardEventArgs object to pass along an image from the clipboard. (All other properties are null)
        /// </summary>
        /// <param name="clipboardImage">Image from the clipboard</param>
        public ClipboardEventArgs(BitmapFrame clipboardImage)
        {
            ClipboardText = null;
            ClipboardImage = clipboardImage;
        }
    }
}
