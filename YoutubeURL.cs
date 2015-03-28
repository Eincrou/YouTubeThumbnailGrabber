using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YoutubeThumbnailGrabber
{
    public class YouTubeURL
    {
        private static readonly string[] _idPatterns =
            { 
                @"(?:\w*://.*)?youtube.com/watch\?v=([^\&\?\/]{11})/?", 
                @"(?:\w*://.*)?youtube.com/embed/([^\&\?\/]{11})/?",
                @"(?:\w*://.*)?youtube.com/v/([^\&\?\/]{11})/?",
                @"(?:\w*://.*)?youtu.be/([^\&\?\/]{11})/?",
                @"(?:\w*://.*)?youtube.com/verify_age\?next_url=watch%3Fv%3D([^\&\?\/]{11})/?"
            };
        private string _inputURL;
        private string _videoID;
        public string VideoID { get { return _videoID; } }
        public string LongYTURL { get { return @"https://www.youtube.com/watch?v=" + VideoID; } }
        public string ShortYTURL { get { return @"http://youtu.be/" + VideoID; } }

        public YouTubeURL(string inputURL)
        {
            _inputURL = inputURL;
            _videoID = GetVideoID(inputURL);
        }

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

        public static bool ValidateYTURL(string URLToCheck)
        {
            foreach (var pattern in _idPatterns)
                if (Regex.IsMatch(URLToCheck, pattern))
                    return true;
            return false;
        }
    }
}
