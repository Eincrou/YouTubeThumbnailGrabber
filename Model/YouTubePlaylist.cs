using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YouTubeThumbnailGrabber.Model
{
    public class YouTubePlaylist
    {
        private readonly string _playlistId;
        private readonly string _playlistPage;
        private readonly List<YouTubeURL> _pageUrls = new List<YouTubeURL>();
        /// <summary>
        /// URL to this playlist's main page
        /// </summary>
        public string PlaylistUrl
        {
            get { return @"https://www.youtube.com/playlist?list=" + _playlistId; } 
        }
        /// <summary>
        /// Title of this playlist
        /// </summary>
        public string Title { get; private set; }
        /// <summary>
        /// Name of the channel that created and owns this playlist
        /// </summary>
        public string Owner { get; private set; }
        /// <summary>
        /// Number of videos in this playlist
        /// </summary>
        public string NumVideos { get; private set; }
        /// <summary>
        /// Number of page views for this playlist
        /// </summary>
        public string Views { get; private set; }
        /// <summary>
        /// Date of the last time the playlist owner changed this playlist
        /// </summary>
        public DateTime LastUpdated { get; private set; }
        /// <summary>
        /// All of the videos in this playlist
        /// </summary>
        public List<YouTubeURL> VideoUrlsList { get { return _pageUrls; } }
        /// <summary>
        /// Instantiates a new YouTubePlaylist object, containing information about a playlist on YouTube.
        /// </summary>
        /// <param name="playlistUrl">String representing a valid YouTube playlist URL</param>
        public YouTubePlaylist(string playlistUrl)
        {
            if (!ValidatePlaylist(playlistUrl))
                throw new ArgumentException("Invalid YouTube playlist URL", "playlistUrl");
            _playlistId = GetPlaylistId(playlistUrl);
            var downloader = new HttpDownloader(PlaylistUrl, String.Empty, String.Empty);
            _playlistPage = downloader.GetPage();

            GetPlaylistInformation();
            GetAllYouTubeUrls();
        }
        /// <summary>
        /// Checks if a string has YouTube playlist information
        /// </summary>
        /// <param name="url">String to validate as continaing a YouTube playlist</param>
        /// <returns>Whether the input string is a URL with a valid YouTube playlist</returns>
        public static bool ValidatePlaylist(string url)
        {
            if ((url.Contains("youtube.com") || url.Contains("youtu.be")) && url.Contains("list="))
                return true;
            return false;
        }
        private string GetPlaylistId(string playlistUrl)
        {
            var plIdMatch = Regex.Match(playlistUrl, @"list=(?<list>[^&]*)");
            return plIdMatch.Groups["list"].Value;
        } 
        private void GetPlaylistInformation()
        {
            GetTitle();
            var detailsMatch = Regex.Match(_playlistPage, 
                @"<ul\sclass=""pl-header-details""><li>by\s<a\shref=[^>]*>(?<owner>[^<]*)</a></li><li>(?<numvideos>\S*)[^<]*<\/li><li>(?<views>\S*)[^<]*<\/li><li>Last\supdated\son\s(?<updated>[^<]*)");
            Owner = WebUtility.HtmlDecode(detailsMatch.Groups["owner"].Value);
            NumVideos = detailsMatch.Groups["numvideos"].Value;
            Views = detailsMatch.Groups["views"].Value;
            LastUpdated = DateTime.Parse(detailsMatch.Groups["updated"].Value);
        }
        private void GetTitle()
        {
            var titleMatch = Regex.Match(_playlistPage, @"<h1\sclass=""pl-header-title"">\s*(?<title>[^\r\n]*)");
            string title = titleMatch.Groups["title"].Value;
            Title = WebUtility.HtmlDecode(title);
        }

        private void GetAllYouTubeUrls()
        {
            var urlMatches = Regex.Matches(_playlistPage,
                @"<td\sclass=""pl-video-title"">\s*<a\s([\w-]*=""[^""]*""\s?)*");
            foreach (Match match in urlMatches) // This hardcoding should be improved.
            {
                string href = match.Groups[1].Captures[2].Value;
                string vidId = href.Substring(15, 11);
                _pageUrls.Add(new YouTubeURL(@"https://www.youtube.com/watch?v=" + vidId));
            }
        }

    }
}
