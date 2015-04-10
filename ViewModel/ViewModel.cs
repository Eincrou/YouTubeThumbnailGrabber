//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Xml.Serialization;
//using YouTubeThumbnailGrabber.Model;
//using DialogBoxResult = System.Windows.Forms.DialogResult;
//using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

//namespace YouTubeThumbnailGrabber.ViewModel
//{
//    public class ViewModel
//    {
//        private string channelURL;
//        private string configPath;
//        private string SaveImageFilename { get { return System.IO.Path.Combine(options.SaveImagePath, thumbnail.VideoURL.VideoID) + ".jpg"; } }

//        BitmapImage defaultThumbnail;
//        ClipboardMonitor clipboardMonitor;
//        FolderBrowserDialog folderDialog = new FolderBrowserDialog();
//        Options options;
//        YouTubeVideoThumbnail thumbnail;
//        YouTubePage youtubePage;

//        public ViewModel(Window mainWindow)
//        {
//            clipboardMonitor = new ClipboardMonitor(mainWindow);
//            clipboardMonitor.ClipboardTextChanged += clipboardMonitor_ClipboardTextChanged;

//            defaultThumbnail = new BitmapImage(new Uri(@"\Resources\YouTubeThumbnailUnavailable.jpg", UriKind.Relative));
//            configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xml");
//            LoadSettings();
//        }
//        private void LoadSettings()
//        {
//            if (File.Exists(configPath))
//            {
//                XmlSerializer xml = new XmlSerializer(typeof(Options));
//                using (Stream input = File.OpenRead(configPath))
//                {
//                    try
//                    {
//                        options = (Options)xml.Deserialize(input);
//                    }
//                    catch (Exception)
//                    {
//                        MessageBox.Show("Configuration failed to load.  Using default settings.", "Load settings failed.", MessageBoxButton.OK, MessageBoxImage.Error);
//                        LoadDefaultSettings();
//                    }
//                }
//            }
//            else
//                LoadDefaultSettings();
//            if (options.AutoLoadURLs && !clipboardMonitor.IsMonitoringClipboard)
//                clipboardMonitor.InitCBViewer();
//            else if (clipboardMonitor.IsMonitoringClipboard)
//                clipboardMonitor.CloseCBViewer();

//            folderDialog.Description = "Select a folder to save thumbnail images.";
//            folderDialog.SelectedPath = options.SaveImagePath;
//        }
//        private void LoadDefaultSettings()
//        {
//            options = new Options();
//            options.ImageFileNamingMode = FileNamingMode.VideoID;
//            options.AutoSaveImages = false;
//            options.AutoLoadURLs = false;
//            options.PublishedDateTitle = false;
//            options.VideoViews = false;
//            options.SaveImagePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
//        }
//        private void GrabThumbnail(string url)
//        {
//            if (YouTubeURL.ValidateYTURL(url))
//            {
//                if (thumbnail != null && (YouTubeURL.GetVideoID(url) == thumbnail.VideoURL.VideoID))
//                {
//                    MessageBox.Show("This video's thumbnail is already being displayed.", "Duplicate URL");
//                    return;
//                }
//                thumbnail = new YouTubeVideoThumbnail(url);
//                thumbnail.GetThumbnailSuccess += Image_DownloadCompleted;
//                thumbnail.GetThumbailFailure += Image_DownloadFailed;
//                thumbnail.ThumbnailImage.DownloadProgress += ImageMaxRes_DownloadProgress;

//                youtubePage = new YouTubePage(thumbnail.VideoURL);
//            }
//            else
//                MessageBox.Show("Invalid YouTube video URL", "Invalid URL", MessageBoxButton.OK, MessageBoxImage.Error);
//        }

//        void clipboardMonitor_ClipboardTextChanged(object sender, ClipboardEventArgs e)
//        {
//            string clipText = e.ClipboardText;
//            if (YouTubeURL.ValidateYTURL(clipText))
//            {
//                if (thumbnail != null && (YouTubeURL.GetVideoID(clipText) == thumbnail.VideoURL.VideoID)) return;
//                InputVideoURL.Text = clipText;
//                GrabThumbnail(clipText);
//            }
//        }
//        void Image_DownloadCompleted(object sender, EventArgs e)
//        {
//            ThumbnailImage.Source = (BitmapImage)sender;
//            SaveImage.IsEnabled = true;
//            DownloadProgress.Visibility = Visibility.Collapsed;
//            ImageResolution.Text = thumbnail.ThumbnailImage.PixelWidth + " x " + thumbnail.ThumbnailImage.PixelHeight;
//            UpdateStatusBar();
//            thumbnail.GetThumbnailSuccess -= Image_DownloadCompleted;

//            if (options.AutoSaveImages)
//                SaveThumbnailImage();
//        }
//        void Image_DownloadFailed(object sender, ExceptionEventArgs e)
//        {
//            SaveImage.IsEnabled = false;
//            DownloadProgress.Visibility = Visibility.Collapsed;
//            ThumbnailImage.Source = defaultThumbnail;
//            ThumbnailImage.ContextMenu = null;
//            thumbnail.GetThumbailFailure -= Image_DownloadFailed;
//            MessageBox.Show("The video thumbnail has failed to download.", "Download failed", MessageBoxButton.OK, MessageBoxImage.Error);
//        }
//        void ImageMaxRes_DownloadProgress(object sender, DownloadProgressEventArgs e)
//        {
//            DownloadProgress.Value = e.Progress;
//        }
//    }
//}
