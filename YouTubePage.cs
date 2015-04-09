using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace YouTubeThumbnailGrabber
{
    public class YouTubePage
    {
        public YouTubeURL YTURL {get; private set;}
        public string VideoTitle { get; private set; }
        private string _videoViewCount;
        public string VideoViewCount
        {
            get
            {
                if (_videoViewCount == null)
                    GetVideoViewCount();
                return _videoViewCount;
            }
        }
        public string ChannelName { get; private set; }
        public Uri ChannelURL { get; private set; }
        private BitmapImage _channelIcon;
        public BitmapImage ChannelIcon
        {
            get
            {
                if (_channelIcon == null)
                {
                    GetChannelIcon();
                    return null;
                }
                return _channelIcon;
            }
        }

        private DateTime? _published;
        public DateTime Published
        {
            get
            {
                if (!_published.HasValue)
                    GetPublished();
                return _published.Value;
            }
        }

        private string _page { get; set; }

        public YouTubePage(YouTubeURL yturl)
        {
            YTURL = yturl;
            WebClient wc = new WebClient();
            _page = wc.DownloadString(YTURL.LongYTURL);

            GetVideoTitle();
            GetChannelInfo();
            
        }

        private void GetVideoTitle()
        {
            Match titleMatch = Regex.Match(_page, @"<h1\sclass=""yt\swatch-title-container""\s>[^<]*<.*title=\""(.*)\"">");
            string title = titleMatch.Groups[1].Value;
            VideoTitle = WebUtility.HtmlDecode(title);
        }
        private void GetVideoViewCount()
        {
            Match viewCountMatch = Regex.Match(_page, @"watch-view-count[^>]+>([^<]*)", RegexOptions.IgnoreCase);
            _videoViewCount = viewCountMatch.Groups[1].Value;
        }
        private void GetChannelInfo()
        {
            Match channelMatch = Regex.Match(_page, @"<div\sclass=""yt-user-info"">[^<]*<.*href=""([^""]*)"".*>(.*)</a>");
            ChannelURL = new Uri(@"http://www.youtube.com" + channelMatch.Groups[1].Value, UriKind.Absolute);
            string channelName = channelMatch.Groups[2].Value;
            ChannelName = WebUtility.HtmlDecode(channelName);
        }
        private void GetChannelIcon()
        {
            Match chanImageMatch = Regex.Match(_page, @"(?im)^\s+<img.*data-thumb=""([^""]*)""");
            BitmapImage chanImageIcon = new BitmapImage(new Uri(chanImageMatch.Groups[1].Value, UriKind.Absolute));
            chanImageIcon.DownloadCompleted += chanImageIcon_DownloadCompleted;
        }
        void chanImageIcon_DownloadCompleted(object sender, EventArgs e)
        {
            _channelIcon = (BitmapImage)sender;
            OnChanImageDownloaded(e);
        }

        private void GetPublished()
        {
            Match pubMatch = Regex.Match(_page, @"(?:Published|Uploaded)\son\s([^<]*)");
            if (pubMatch.Groups[1].Success)
                _published = DateTime.Parse(pubMatch.Groups[1].Value);
            else
                _published = new DateTime();
        }
        /// <summary>
        /// Notifies when the channel icon image has completed downloading.
        /// </summary>
        public event EventHandler ChanImageDownloaded;
        private void OnChanImageDownloaded(EventArgs e)
        {
            EventHandler chanImageDownloaded = ChanImageDownloaded;
            if (chanImageDownloaded != null)
                chanImageDownloaded(this, e);
        }        
    }
}
