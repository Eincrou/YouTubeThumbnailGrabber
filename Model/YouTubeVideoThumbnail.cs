using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YouTubeThumbnailGrabber.Model
{
    /// <summary>
    /// Properties and events related to grabbing and storing YouTube thumbnail images.
    /// </summary>
    public class YouTubeVideoThumbnail
    {
        /// <summary>
        /// Instance of a YouTube video URL for this instance of a YouTubeVideoThumbnail
        /// </summary>
        public YouTubeURL VideoURL { get; private set; }
        /// <summary>
        /// URI for full resolution thumbnail image
        /// </summary>
        public Uri ImageMaxResURI { get; private set; }
        /// <summary>
        /// URI for lower-quality thumbnail image
        /// </summary>
        public Uri ImageAlternateURI { get; private set; }
        private BitmapImage _thumbnailImage;
        /// <summary>
        /// Downloaded thumbnail for this YouTubeURL
        /// </summary>
        public BitmapImage ThumbnailImage { get { return _thumbnailImage; } }
        /// <summary>
        /// Gets whether this instance's thumbnail image is the maximum resolution version, or the lower resolution alternative
        /// </summary>
        public bool UsingMaxResImage { get; private set; }
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
            ImageMaxResURI = new Uri(String.Format("http://i.ytimg.com/vi/{0:ID}/maxresdefault.jpg", VideoURL), UriKind.Absolute);
            ImageAlternateURI = new Uri(String.Format("http://i.ytimg.com/vi/{0:ID}/0.jpg", VideoURL), UriKind.Absolute);
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
            UsingMaxResImage = true;
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
