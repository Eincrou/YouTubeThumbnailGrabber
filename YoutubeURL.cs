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
        private const string _pattern = @"(?:\w*://).*/(?:watch\?v=)?(.{11})";
        private string _inputURL;
        public string VideoID { get { return GetVideoID(_inputURL); } }
        public string LongYTURL { get { return @"https://www.youtube.com/watch?v=" + VideoID; } }
        public string ShortYTURL { get { return @"http://youtu.be/" + VideoID; } }

        public YouTubeURL(string inputURL)
        {
            _inputURL = inputURL;
        }

        public static string GetVideoID(string url)
        {
            Regex RX = new Regex(_pattern);
            Match m = RX.Match(url);
            if (m.Success && m.Groups[1].Success)
                return m.Groups[1].Value;
            else
                throw new ArgumentException("Invalid YouTube video URL.");
        }

        public static bool ValidateYTURL(string URLToCheck)
        {
            Regex RX = new Regex(_pattern);
            Match m = RX.Match(URLToCheck);
            if (m.Success && m.Groups[1].Success)
                return true;
            else
                return false;
        }
    }
}
