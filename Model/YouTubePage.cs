using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace YouTubeThumbnailGrabber.Model
{
    public class YouTubePage
    {
        private string _page { get; set; }

        public YouTubeURL YTURL {get; private set;}
        public string VideoTitle { get; private set; }
        private string _videoViewCount;
        /// <summary>
        /// Gets the number of views for this video page
        /// </summary>
        public string VideoViewCount
        {
            get
            {
                if (_videoViewCount == null)
                    GetVideoViewCount();
                return _videoViewCount;
            }
        }
        /// <summary>
        /// Gets the name of the video uploader's channel
        /// </summary>
        public string ChannelName { get; private set; }
        /// <summary>
        /// Gets the URL of the video uploader's channel page
        /// </summary>
        public Uri ChannelURL { get; private set; }
        private BitmapImage _channelIcon;
        /// <summary>
        /// Gets the video uploader's channel icon image
        /// </summary>
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
        /// <summary>
        /// Gets the date the video was uploaded or published
        /// </summary>
        public DateTime Published
        {
            get
            {
                if (!_published.HasValue)
                    GetPublished();
                return _published.Value;
            }
        }
        /// <summary>
        /// Instantiates a new object YouTubePage class that can parse information about YouTube videos from their HTML pages
        /// </summary>
        /// <param name="yturl"></param>
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
            string views = viewCountMatch.Groups[1].Value;
            if (!views.Contains(" views"))
                views += " views";
            _videoViewCount = views;
        }
        private void GetChannelInfo()
        {
            Match channelMatch = Regex.Match(_page, @"<div\sclass=""yt-user-info"">[^<]*<.*href=""([^""]*)"".*>(.*)</a>");
            ChannelURL = new Uri(@"http://www.youtube.com" + channelMatch.Groups[1].Value, UriKind.Absolute);
            ChannelName = WebUtility.HtmlDecode(channelMatch.Groups[2].Value);
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
