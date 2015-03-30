using System;
using System.Text.RegularExpressions;

namespace YoutubeThumbnailGrabber
{
    /// <summary>
    /// Parses and stores YouTube URLs and VideoIDs.
    /// </summary>
    public class YouTubeURL
    {
        private static readonly string[] _idPatterns =
            { 
                @"(?:\w*://.*)?youtube.com/watch\?(?:feature=player_embedded&)?v=([^\&\?\/]{11})/?", 
                @"(?:\w*://.*)?youtube.com/embed/([^\&\?\/]{11})/?",
                @"(?:\w*://.*)?youtube.com/v/([^\&\?\/]{11})/?",
                @"(?:\w*://.*)?youtu.be/([^\&\?\/]{11})/?",
                @"(?:\w*://.*)?youtube.com/verify_age\?next_url=watch%3Fv%3D([^\&\?\/]{11})/?",
                @"(?:\w*://.*)?interleave-vr.com/youtube-proper-player.php\?v=([^\&\?\/]{11})/?"
            };
        private string _inputURL;
        private string _videoID;
        /// <summary>
        /// This instance's unique video indentifier.
        /// </summary>
        public string VideoID { get { return _videoID; } }
        /// <summary>
        /// A full-length URL for this instance's VideoID.
        /// </summary>
        public string LongYTURL { get { return @"https://www.youtube.com/watch?v=" + VideoID; } }
        /// <summary>
        /// An abbreviated URL for this instance's VideoID.
        /// </summary>
        public string ShortYTURL { get { return @"http://youtu.be/" + VideoID; } }
        /// <summary>
        /// A URL to force the video to play with the Flash Video Player.
        /// </summary>
        public string EnforcerURL { get { return @"http://www.interleave-vr.com/youtube-proper-player.php?v=" + VideoID; } }
        /// <summary>
        /// Initializes an instance of the YouTubeURL class.
        /// </summary>
        /// <param name="inputURL">A valid URL for a YouTube video.</param>
        public YouTubeURL(string inputURL)
        {
            _inputURL = inputURL;
            _videoID = GetVideoID(inputURL);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url">A validated URL for a YouTube video.</param>
        /// <returns>Eleven-character unique identifier for the video.</returns>
        public static string GetVideoID(string url)
        {
            foreach (var pattern in _idPatterns)
            {
                Match match = Regex.Match(url, pattern);
                if (match.Groups[1].Success)
                    return match.Groups[1].Value;
            }
            throw new ArgumentException("Invalid YouTube video URL", "url");
        }
        /// <summary>
        /// Checks if the input string can be successfully parsed into a YouTube VideoID.
        /// </summary>
        /// <param name="URLToCheck">A URL to check.</param>
        /// <returns>Whether the URL matches a supported YouTube URL pattern.</returns>
        public static bool ValidateYTURL(string URLToCheck)
        {
            foreach (var pattern in _idPatterns)
                if (Regex.IsMatch(URLToCheck, pattern))
                    return true;
            return false;
        }
    }
}
