using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeThumbnailGrabber
{
    class ClipboardEventArgs:EventArgs
    {
        public string ClipboardText { get; private set; }
        public ClipboardEventArgs(string clipboardText)
        {
            ClipboardText = clipboardText;
        }
    }
}
