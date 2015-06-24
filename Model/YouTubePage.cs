﻿using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;

namespace YouTubeThumbnailGrabber.Model
{
    public enum YouTubeVideoPrivacy
    {
        Public, Unlisted, Private
    }
    public class YouTubePage
    {
        private string _page;

        #region Public Properties
        /// <summary>
        /// Contains fields with various YouTube URLs
        /// </summary>
        public YouTubeURL VideoUrl { get; private set; }
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
        /// The uploader's description of this video
        /// </summary>
        public string VideoDescription
        {
            get
            {
                if (_videoDescription == null)
                    GetDescription();
                return _videoDescription;
            }
        }
        private string _videoDescription;
        /// <summary>
        /// Whether the video is premium content
        /// </summary>
        public bool Paid
        {
            get
            {
                if (!_paid.HasValue)
                    GetPaid();
                return _paid.Value;
            }
        }
        private bool? _paid;
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
        /// The length of the video.
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
                if (!_duration.HasValue)
                    GetDuration();
                return _duration.Value;
            }
        }
        private TimeSpan? _duration;

        /// <summary>
        /// Gets the video's visibility (Public, Unlisted or Private)
        /// </summary>
        public YouTubeVideoPrivacy VideoPrivacy
        {
            get
            {
                if (!_videoPrivacy.HasValue)
                    GetVideoPrivacy();
                return _videoPrivacy.Value;
            }
        }
        private YouTubeVideoPrivacy? _videoPrivacy;
        /// <summary>
        /// Whether the video has an age-restricted content warning
        /// </summary>
        private bool IsFamilyFriendly
        {
            get
            {
                if (!_isFamilyFriendly.HasValue)
                    GetIsFamilyFriendly();
                return _isFamilyFriendly.Value;
            }
        }
        private bool? _isFamilyFriendly;
        /// <summary>
        /// Two-character country codes for the countries in which the video is viewable
        /// </summary>
        public string[] RegionsAllowed
        {
            get
            {
                if(_regionsAllowed == null)
                    GetRegionsAllowed();
                return _regionsAllowed;
            }
        }
        private string[] _regionsAllowed;
        /// <summary>
        /// Gets the number of views for this video page
        /// </summary>
        public int ViewCount
        {
            get
            {
                if (!_viewCount.HasValue)
                    GetViewCount();
                return _viewCount.Value;
            }
        }
        private int? _viewCount;
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
        private DateTime? _published;
        /// <summary>
        /// The video's genre
        /// </summary>
        public string Genre
        {
            get
            {
                if (_genre == null)
                    GetGenre();
                return _genre;
            }
        }
        private string _genre;
        #endregion


        /// <summary>
        /// Instantiates a new YouTubePage instance that can parse information about YouTube videos from their HTML pages
        /// </summary>
        /// <param name="yturl"></param>
        public YouTubePage(YouTubeURL yturl)
        {
            VideoUrl = yturl;
            var downloader = new HttpDownloader(VideoUrl.LongYTURL, String.Empty, String.Empty);
            _page = downloader.GetPage();
        }

        private void GetVideoTitle()
        {
            var titleMatch = Regex.Match(_page, @"<meta\sitemprop=""name""\scontent=""(?<name>[^""]*)"">");
            string title = titleMatch.Groups["name"].Value;
            _videoTitle = WebUtility.HtmlDecode(title);
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
            var iconUrl = chanImageMatch.Groups["imageUrl"].Value.StartsWith(@"//")
                ? "http:" + chanImageMatch.Groups["imageUrl"].Value
                : chanImageMatch.Groups["imageUrl"].Value;
            
            var chanImageIcon = new BitmapImage(
                new Uri(iconUrl, UriKind.Absolute));
            chanImageIcon.DownloadCompleted += chanImageIcon_DownloadCompleted;
            chanImageIcon.DownloadFailed += chanImageIcon_DownloadFailed;

        }
        void chanImageIcon_DownloadCompleted(object sender, EventArgs e)
        {
            _channelIcon = (BitmapImage)sender;
            _channelIcon.DownloadCompleted -= chanImageIcon_DownloadCompleted;
            OnChanImageDownloaded(e);
        }
        void chanImageIcon_DownloadFailed(object sender, System.Windows.Media.ExceptionEventArgs e)
        {
            ((BitmapImage) sender).DownloadFailed -= chanImageIcon_DownloadFailed;
            MessageBox.Show("The channel image icon has failed to download.");
        }
        private void GetPaid()
        {
            var paidMatch = Regex.Match(_page, @"<meta\sitemprop=""paid""\scontent=""(?<paid>\w*)"">");
            _paid = bool.Parse(paidMatch.Groups["paid"].Value);
        }
        private void GetDescription()
        {
            var descMatch = Regex.Match(_page, @"<meta\sitemprop=""description""\scontent=""(?<desc>[^""]*)"">");
            string description = descMatch.Groups["desc"].Value;
            _videoDescription = WebUtility.HtmlDecode(description);
        }
        private void GetDuration()
        {
            var durationMatch = Regex.Match(_page, @"<meta\sitemprop=""duration""\scontent=""(?<duration>\w*)"">");
            var minutesSecondsMatch = Regex.Match(durationMatch.Groups["duration"].Value, @"PT(?<M>\d*)M(?<S>\d*)S");
            int totalMinutes = int.Parse(minutesSecondsMatch.Groups["M"].Value);
            int totalSeconds = int.Parse(minutesSecondsMatch.Groups["S"].Value);
            _duration = new TimeSpan(0, 0, totalMinutes, totalSeconds);
        }
        private void GetVideoPrivacy()
        {
            var privacyMatch = Regex.Match(_page, @"<meta\sitemprop=""unlisted""\scontent=""(?<unlisted>\w*)"">");
            if (privacyMatch.Groups["unlisted"].Success)
            {
                _videoPrivacy =
                    (YouTubeVideoPrivacy)
                        Enum.Parse(typeof (YouTubeVideoPrivacy), privacyMatch.Groups["unlisted"].Value);
            }
            else
                _videoPrivacy = YouTubeVideoPrivacy.Private;
        }
        private void GetIsFamilyFriendly()
        {
            var famFriendMatch = Regex.Match(_page, @"<meta\sitemprop=""isFamilyFriendly""\scontent=""(?<famFriend>\w*)"">");
            _isFamilyFriendly = bool.Parse(famFriendMatch.Groups["famFriend"].Value);
        }
        private void GetRegionsAllowed()
        {
            var regionsMatch = Regex.Match(_page, @"<meta\sitemprop=""regionsAllowed""\scontent=""(?<regions>[^""]*)"">");
            _regionsAllowed = regionsMatch.Groups["regions"].Value.Split(',');
        }
        private void GetViewCount()
        {
            var viewCountMatch = Regex.Match(_page, @"<meta\sitemprop=""interactionCount""\scontent=""(?<views>\d*)"">");
            //string views;
            //if (viewCountMatch.Groups["views"].Value != String.Empty)
            //{
            //    views = viewCountMatch.Groups["views"].Value;
            //    if (!views.Contains(" views"))
            //        views += " views";
            //}
            //else
            //    views = "LIVE NOW";
            //_viewCount = views;
            _viewCount = int.Parse(viewCountMatch.Groups["views"].Value);
        }
        private void GetPublished()
        {
            //var pubMatch = Regex.Match(_page, @"(?:Published|Uploaded|Started|Streamed live)\son\s(?<date>[^<]*)");
            //_published = pubMatch.Groups["date"].Success ? DateTime.Parse(pubMatch.Groups["date"].Value) : new DateTime();
            var publishMatch = Regex.Match(_page, @"<meta\sitemprop=""datePublished""\scontent=""(?<published>[^""]*)"">");
            _published = DateTime.Parse(publishMatch.Groups["published"].Value);
        }
        private void GetGenre()
        {
            var genreMatch = Regex.Match(_page, @"<meta\sitemprop=""genre""\scontent=""(?<genre>[^""]*)"">");
            _genre = WebUtility.HtmlDecode(genreMatch.Groups["genre"].Value);
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