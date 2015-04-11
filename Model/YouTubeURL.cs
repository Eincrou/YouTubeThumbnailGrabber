using System;
using System.Text.RegularExpressions;

namespace YouTubeThumbnailGrabber.Model
{
    /// <summary>
    /// Parses and stores YouTube URLs and VideoIDs.
    /// </summary>
    public class YouTubeURL : IFormattable
    {
        private static readonly string[] _idPatterns =
            { 
                @"youtube.com/watch\?([^/]+\&)?(v|src_vid)=(?<v>[^\&\?\/]{11})", 
                @"youtube.com/\w*/(?<v>[^\&\?\/]{11})",
                @"youtu.be/(?<v>[^\&\?\/]{11})",
                @"youtube.com/verify_age\?next_url=watch%3Fv%3D(?<v>[^\&\?\/]{11})",
                @"interleave-vr.com/youtube-proper-player.php\?v=(?<v>[^\&\?\/]{11})"
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
        public string LongYTURL { get { return @"https://www.youtube.com/watch?v=" + _videoID; } }
        /// <summary>
        /// An abbreviated URL for this instance's VideoID.
        /// </summary>
        public string ShortYTURL { get { return @"http://youtu.be/" + _videoID; } }
        /// <summary>
        /// A URL to force the video to play with the Flash Video Player.
        /// </summary>
        public string EnforcerURL { get { return @"http://www.interleave-vr.com/youtube-proper-player.php?v=" + _videoID; } }
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
        /// Gets the unique identifier of a YouTube video from a valid YouTube video URL.
        /// </summary>
        /// <param name="url">A validated URL for a YouTube video.</param>
        /// <returns>Eleven-character unique identifier for the video.</returns>
        public static string GetVideoID(string url)
        {
            foreach (var pattern in _idPatterns)
            {
                Match match = Regex.Match(url, pattern, RegexOptions.ExplicitCapture);
                if (match.Groups["v"].Success)
                    return match.Groups["v"].Value;
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
            try
            {
                GetVideoID(URLToCheck);
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        }
        public override string ToString()
        {
            return ShortYTURL;
        }
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null) format = "S";
            switch (format.ToUpperInvariant())
            {
                case null:
                case "ID":
                    return VideoID;
                case "S":
                    return ShortYTURL;
                case "L":
                    return LongYTURL;
                case "E":
                    return EnforcerURL;
                default:
                    throw new FormatException(String.Format(formatProvider,
                          "Format {0} is not supported", format));
            }
        }
    }
}
