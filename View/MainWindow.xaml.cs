namespace YouTubeThumbnailGrabber.View
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using DialogBoxResult = System.Windows.Forms.DialogResult;
    using ViewModel;
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly ContextMenu ThumbnailImageCM;
        private ViewModel viewModel;
        public MainWindow()
        {
            InitializeComponent();

            viewModel = FindResource("ViewModel") as ViewModel;
// ReSharper disable PossibleNullReferenceException
            viewModel.InitializeViewModel(this);
// ReSharper restore PossibleNullReferenceException
            viewModel.ThumbnailImageDownloadCompletedRouted += Image_DownloadCompleted;
            viewModel.ThumbnailImageDownloadFailedRouted += Image_DownloadFailed;
            viewModel.ThumbnailImageProgressRouted += ImageMaxRes_DownloadProgress;

            ThumbnailImageCM = ThumbnailImage.ContextMenu;
            ThumbnailImage.ContextMenu = null;      
        }


        private void GetImage_Click(object sender, RoutedEventArgs e)
        {
            viewModel.GrabThumbnail(InputVideoURL.Text);
        }

        void Image_DownloadCompleted(object sender, EventArgs e)
        {
            var readyImage = (BitmapImage) sender;
            DownloadProgress.Visibility = Visibility.Collapsed;
            SaveImage.IsEnabled = true;
            ThumbnailImage.ContextMenu = ThumbnailImageCM;
            ImageResolution.Text = readyImage.PixelWidth + " x " + readyImage.PixelHeight;
            this.Cursor = Cursors.Arrow;
        }
        void Image_DownloadFailed(object sender, ExceptionEventArgs e)
        {           
            SaveImage.IsEnabled = false;
            DownloadProgress.Visibility = Visibility.Collapsed;
            ThumbnailImage.ContextMenu = null;
            MessageBox.Show("The video thumbnail has failed to download.", "Download failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        void ImageMaxRes_DownloadProgress(object sender, DownloadProgressEventArgs e)
        {
            if (DownloadProgress.Visibility == Visibility.Collapsed)
                DownloadProgress.Visibility = Visibility.Visible;
            DownloadProgress.Value = e.Progress;
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
           viewModel.SaveThumbnailImageToFile();
        }

        private void OpenVideo_Click(object sender, RoutedEventArgs e)
        {
            viewModel.OpenVideoInBrowser();
        }

        private void OpenImageInViewerCtxtMen(object sender, RoutedEventArgs e)
        {
           viewModel.OpenImageInViewer();
        }
        private void OpenImageInViewerDblClk(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                viewModel.OpenImageInViewer();
        }

        private void ImageToClipboardHandler(object sender, RoutedEventArgs e)
        {
            viewModel.SetImageToClipboard();
        }

        private void StatusBarURL_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            viewModel.OpenVideoInBrowser();
        }

        private void SBChannel_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            viewModel.OpenChannelInBrowser();
        }

        private void OpenOptions_Click(object sender, RoutedEventArgs e)
        {
            viewModel.OpenOptionsMenu();
        }


        private void SBTextCopy_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock tb = e.Source as TextBlock;
            if (tb != null && tb.Text != String.Empty)
                Clipboard.SetText(tb.Text);
        }

        private void MenuCopyAddress_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SetVideoUrlToClipboard();
        }
        private void ThumbnailImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!viewModel.IsThumbnailDisplayed || viewModel.ThumbnailBitmapImage == null) return;
            var tbi = viewModel.ThumbnailBitmapImage;
            double imageDisplayArea = e.NewSize.Width * e.NewSize.Height;
            int imageResolutionArea = (tbi.PixelWidth * tbi.PixelHeight);
            double zoomPercent = imageDisplayArea / imageResolutionArea;
            this.Title = String.Format("YouTube Thumbnail Grabber ({0:p})", zoomPercent);
        }
    }
}
