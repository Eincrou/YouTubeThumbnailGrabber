using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace YouTubeThumbnailGrabber.Model
{
    public class YouTubePage
    {
        private string _page;
        /// <summary>
        /// Contains numerous fields with various YouTube URLs
        /// </summary>
        public YouTubeURL YTURL {get; private set;}
        /// <summary>
        /// The title of this YouTube video
        /// </summary>
        public string VideoTitle
        {
            get
            {
                if (_videoTitle == null)
                    GetVideoTitle();
                return _videoTitle;
            }
        }
        private string _videoTitle;
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
        private string _videoViewCount;
        /// <summary>
        /// Gets the name of the video uploader's channel
        /// </summary>
        public string ChannelName
        {
            get
            {
                if (_channelName == null)
                    GetChannelInfo();
                return _channelName;
            }
        }
        private string _channelName;
        /// <summary>
        /// Gets the URL of the video uploader's channel page
        /// </summary>
        public Uri ChannelUri
        {
            get
            {
                if (_channelUrl == null)
                    GetChannelInfo();
                return _channelUrl;
            }
        }
        private Uri _channelUrl;
        /// <summary>
        /// Gets the video uploader's channel icon image. If null is returned, listen to the ChanImageDownloaded event to be notified when the download is complete
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
        private BitmapImage _channelIcon;        
        /// <summary>
        /// Gets the date the video was uploaded or published
        /// </summary>
        public DateTime Published
        {
            get
            {
                if (!_published.HasValue)
                    GetPublished();
                if (_published != null) return _published.Value;
            }
        }
        private DateTime? _published;
        private string _videoDescription;
        /// <summary>
        /// The uploader's description of this video
        /// </summary>
        public string VideoDescription
        {
            get
            {
                if (_videoDescription == null)
                    GetVideoDescription();
                return _videoDescription;
            }
        }
        /// <summary>
        /// Instantiates a new object YouTubePage class that can parse information about YouTube videos from their HTML pages
        /// </summary>
        /// <param name="yturl"></param>
        public YouTubePage(YouTubeURL yturl)
        {
            YTURL = yturl;
            var downloader = new HttpDownloader(YTURL.LongYTURL, String.Empty, String.Empty);
            _page = downloader.GetPage();
        }

        private void GetVideoTitle()
        {
            var titleMatch = Regex.Match(_page, @"<h1\sclass=""yt\swatch-title-container""\s>[^<]*<.*title=\""(?<title>.*)\"">");
            string title = titleMatch.Groups["title"].Value;
            _videoTitle = WebUtility.HtmlDecode(title);
        }
        private void GetVideoViewCount()
        {
            var viewCountMatch = Regex.Match(_page, @"watch-view-count[^>]+>(?<views>[^<]*)", RegexOptions.IgnoreCase);
            string views;
            if (viewCountMatch.Groups["views"].Value != String.Empty)
            {
                views = viewCountMatch.Groups["views"].Value;
                if (!views.Contains(" views"))
                    views += " views";
            }
            else
                views = "LIVE NOW";
            _videoViewCount = views;
        }
        private void GetChannelInfo()
        {
            var channelMatch = Regex.Match(_page, @"<div\sclass=""yt-user-info"">[^<]*<.*href=""(?<chanUrl>[^""]*)"".*>(?<chanName>.*)</a>");
            _channelUrl = new Uri(@"http://www.youtube.com" + channelMatch.Groups["chanUrl"].Value, UriKind.Absolute);
            _channelName = WebUtility.HtmlDecode(channelMatch.Groups["chanName"].Value);
        }
        private void GetChannelIcon()
        {
            var chanImageMatch = Regex.Match(_page, @"(?im)^\s+<img.*data-thumb=""(?<imageUrl>[^""]*)""");
            var chanImageIcon = new BitmapImage(
                new Uri(chanImageMatch.Groups["imageUrl"].Value, UriKind.Absolute));
            chanImageIcon.DownloadCompleted += chanImageIcon_DownloadCompleted;
        }
        void chanImageIcon_DownloadCompleted(object sender, EventArgs e)
        {
            _channelIcon = (BitmapImage)sender;
            OnChanImageDownloaded(e);
        }

        private void GetPublished()
        {
            var pubMatch = Regex.Match(_page, @"(?:Published|Uploaded|Started)\son\s(?<date>[^<]*)");
            _published = pubMatch.Groups["date"].Success ? DateTime.Parse(pubMatch.Groups["date"].Value) : new DateTime();
        }

        private void GetVideoDescription()
        {
            var vidDescMatch = Regex.Match(_page, @"(?i)eow-description""\s>(?<desc>[^<]*)");
            string description = vidDescMatch.Groups["desc"].Value;
            _videoDescription = WebUtility.HtmlDecode(description);
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