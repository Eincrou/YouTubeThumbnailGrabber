using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YoutubeThumbnailGrabber
{
    public class YouTubeVideoThumbnail
    {
        public YouTubeURL VideoURL { get; private set; }
        public string ImageMaxResString { get { return @"http://img.youtube.com/vi/" + VideoURL.VideoID + @"/maxresdefault.jpg"; } }
        public Uri ImageMaxResURI { get { return new Uri(ImageMaxResString, UriKind.Absolute); } }
        public string ImageAlternateString { get { return @"http://i.ytimg.com/vi/" + VideoURL.VideoID + @"/0.jpg"; } }
        public Uri ImageAlternateURI { get { return new Uri(ImageAlternateString, UriKind.Absolute); } }
        private BitmapImage _thumbnailImage;
        public BitmapImage ThumbnailImage { get { return _thumbnailImage; } }

        public YouTubeVideoThumbnail(string url)
        {
            VideoURL = new YouTubeURL(url);
            GetThumbnail();
        }

        private void GetThumbnail()
        {
                _thumbnailImage = new BitmapImage(ImageMaxResURI);
                _thumbnailImage.DownloadCompleted += OnThumbnailSuccess;
                _thumbnailImage.DownloadFailed += _thumbnailImage_DownloadFailed;
        }

        void _thumbnailImage_DownloadFailed(object sender, ExceptionEventArgs e)
        {
            _thumbnailImage = new BitmapImage(ImageAlternateURI);
            _thumbnailImage.DownloadCompleted += OnThumbnailSuccess;
            _thumbnailImage.DownloadFailed += OnThumbnailFailure;
        }

        public event EventHandler GetThumbnailSuccess;
        private void OnThumbnailSuccess(object sender, EventArgs e)
        {
            EventHandler getThumbnailSuccess = GetThumbnailSuccess;
            if (getThumbnailSuccess != null)
            {
                GetThumbnailSuccess(sender, e);
            }
        }

        public event EventHandler<ExceptionEventArgs> GetThumbailFailure;
        private void OnThumbnailFailure(object sender, ExceptionEventArgs exception)
        {
            EventHandler<ExceptionEventArgs> getThumbailFailure = GetThumbailFailure;
            if (getThumbailFailure != null)
            {
                GetThumbailFailure(sender, exception);
            }
        }

    }
}
