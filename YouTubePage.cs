using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace YouTubeThumbnailGrabber
{
    class YouTubePage
    {
        public YouTubeURL YTURL {get; private set;}
        public string VideoTitle { get; private set; }
        public string ChannelName { get; private set; }
        public Uri ChannelURL { get; private set; }
        public BitmapImage ChannelIcon { get; private set; }

        private string _page { get; set; }

        public YouTubePage(YouTubeURL yturl)
        {
            YTURL = yturl;
            WebClient wc = new WebClient();
            string page = wc.DownloadString(YTURL.LongYTURL);

            GetVideoTitle(page);
            GetChannelInfo(page);
            GetChannelIcon(page);
        }

        private void GetVideoTitle(string page)
        {
            Match titleMatch = Regex.Match(page, @"<h1\sclass=""yt\swatch-title-container""\s>[^<]*<.*title=\""(.*)\"">");
            string title = titleMatch.Groups[1].Value;
            VideoTitle = WebUtility.HtmlDecode(title);
        }
        private void GetChannelInfo(string page)
        {
            Match channelMatch = Regex.Match(page, @"<div\sclass=""yt-user-info"">[^<]*<.*href=""([^""]*)"".*>(.*)</a>");
            ChannelURL = new Uri(@"http://www.youtube.com" + channelMatch.Groups[1].Value, UriKind.Absolute);
            string channelName = channelMatch.Groups[2].Value;
            ChannelName = WebUtility.HtmlDecode(channelName);
        }
        private void GetChannelIcon(string page)
        {
            Match chanImageMatch = Regex.Match(page, @"(?im)^\s+<img.*data-thumb=""([^""]*)""");
            BitmapImage chanImageIcon = new BitmapImage(new Uri(chanImageMatch.Groups[1].Value, UriKind.Absolute));
            chanImageIcon.DownloadCompleted += chanImageIcon_DownloadCompleted;
        }
        void chanImageIcon_DownloadCompleted(object sender, EventArgs e)
        {
            ChannelIcon = (BitmapImage)sender;
            OnChanImageDownloaded(e);
        }
        public event EventHandler ChanImageDownloaded;
        private void OnChanImageDownloaded(EventArgs e)
        {
            EventHandler chanImageDownloaded = ChanImageDownloaded;
            if (chanImageDownloaded != null)
                chanImageDownloaded(this, e);
        }
    }
}
