﻿using System;
using System.Text.RegularExpressions;

namespace YouTubeThumbnailGrabber.Model
{
    /// <summary>
    /// Parses and stores YouTube URLs and VideoIDs.
    /// </summary>
    public class YouTubeURL : IFormattable, IEquatable<YouTubeURL>
    {
        private static readonly string[] _idPatterns =
            { 
                @"youtube.com/watch\?([^/]+\&)?(v|src_vid|%3D)=(?<v>[^\&\?\/]{11})", 
                @"youtube.com/\w*/(?<v>[^\&\?\/]{11})",
                @"youtu.be/(?<v>[^\&\?\/]{11})",
                @"youtube.com/verify_age\?next_url=watch%3Fv%3D(?<v>[^\&\?\/]{11})",
                @"interleave-vr.com/youtube-proper-player.php\?v=(?<v>[^\&\?\/]{11})"
            };
        private string _inputUrl;
        private readonly string _videoId;
        /// <summary>
        /// This instance's unique video indentifier.
        /// </summary>
        public string VideoID { get { return _videoId; } }
        /// <summary>
        /// A full-length URL for this instance's VideoID.
        /// </summary>
        public string LongYTURL { get { return @"https://www.youtube.com/watch?v=" + _videoId; } }
        /// <summary>
        /// An abbreviated URL for this instance's VideoID.
        /// </summary>
        public string ShortYTURL { get { return @"http://youtu.be/" + _videoId; } }
        /// <summary>
        /// A URL to force the video to play with the Flash Video Player.
        /// </summary>
        public string EnforcerURL { get { return @"http://www.interleave-vr.com/youtube-proper-player.php?v=" + _videoId; } }
        /// <summary>
        /// Initializes an instance of the YouTubeURL class.
        /// </summary>
        /// <param name="inputUrl">A valid URL for a YouTube video.</param>
        public YouTubeURL(string inputUrl)
        {
            _inputUrl = inputUrl;
            _videoId = GetVideoID(inputUrl);
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
        /// <param name="urlToCheck">A URL to check.</param>
        /// <returns>Whether the URL matches a supported YouTube URL pattern.</returns>
        public static bool ValidateUrl(string urlToCheck)
        {
            try
            {
                GetVideoID(urlToCheck);
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        }

        #region Interface/Override
        public bool Equals(YouTubeURL other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(_videoId, other._videoId);
        }
        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((YouTubeURL)obj);
        }
        public override int GetHashCode() {
            return (_videoId != null ? _videoId.GetHashCode() : 0);
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
        #endregion

    }
}


