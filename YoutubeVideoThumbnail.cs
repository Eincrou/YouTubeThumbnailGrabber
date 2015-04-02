using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YouTubeThumbnailGrabber
{
    /// <summary>
    /// Properties and events related to grabbing and storing YouTube thumbnail images.
    /// </summary>
    public class YouTubeVideoThumbnail
    {
        public YouTubeURL VideoURL { get; private set; }
        public string ImageMaxResString { get; private set; }
        public Uri ImageMaxResURI { get; private set; }
        public string ImageAlternateString { get; private set; }
        public Uri ImageAlternateURI { get; private set; }
        private BitmapImage _thumbnailImage;
        public BitmapImage ThumbnailImage { get { return _thumbnailImage; } }
        /// <summary>
        /// Initializes a new instance of the YouTubeVideoThumbnail class. Provides members related to storing a YouTubeURL, URLs to video thumbnail images, and events related to downloading them.
        /// </summary>
        /// <param name="youtubeuUrl">A validated YouTube URL. (Use YouTubeURL.ValidateYTURL)</param>
        public YouTubeVideoThumbnail(string youtubeuUrl)
        {
            VideoURL = new YouTubeURL(youtubeuUrl);
            CreateURLs();
            GetThumbnail();
        }
        /// <summary>
        /// Generates the URLs and URIs needed to access thumbnail images.
        /// </summary>
        private void CreateURLs()
        {
            ImageMaxResString = String.Format("http://i.ytimg.com/vi/{0:ID}/maxresdefault.jpg", VideoURL);
            ImageMaxResURI = new Uri(ImageMaxResString, UriKind.Absolute);
            ImageAlternateString = String.Format("http://i.ytimg.com/vi/{0:ID}/0.jpg", VideoURL);
            ImageAlternateURI = new Uri(ImageAlternateString, UriKind.Absolute);
        }
        /// <summary>
        /// Initiates attempts to download the thumbnail image for this VideoURL. If the first attempt fails, a second attempt is made with an alternate URL.
        /// </summary>
        private void GetThumbnail()
        {
            _thumbnailImage = new BitmapImage(ImageMaxResURI);
            _thumbnailImage.DownloadCompleted += OnThumbnailSuccess;
            _thumbnailImage.DownloadFailed += _thumbnailImage_DownloadFailed;
        }
        /// <summary>
        /// Attempts to download lower-quality thumbnail if the maxresdefault attempt fails.
        /// </summary>
        /// <param name="sender">Passthrough for the object from the first attempt.</param>
        /// <param name="e">Passthrough for the EventArgs from the first attempt.</param>
        void _thumbnailImage_DownloadFailed(object sender, ExceptionEventArgs e)
        {
            _thumbnailImage = new BitmapImage(ImageAlternateURI);
            _thumbnailImage.DownloadCompleted += OnThumbnailSuccess;
            _thumbnailImage.DownloadFailed += OnThumbnailFailure;
        }
        /// <summary>
        /// Occurs when the YouTube thumbnail image has been completely downloaded.
        /// </summary>
        public event EventHandler GetThumbnailSuccess;
        private void OnThumbnailSuccess(object sender, EventArgs e)
        {
            EventHandler getThumbnailSuccess = GetThumbnailSuccess;
            if (getThumbnailSuccess != null)
                GetThumbnailSuccess(sender, e);
        }
        /// <summary>
        /// Occurs when the YouTube thumbnail image has failed to download.
        /// </summary>
        public event EventHandler<ExceptionEventArgs> GetThumbailFailure;
        private void OnThumbnailFailure(object sender, ExceptionEventArgs exception)
        {
            EventHandler<ExceptionEventArgs> getThumbailFailure = GetThumbailFailure;
            if (getThumbailFailure != null)
                GetThumbailFailure(sender, exception);
        }
    }
}
