using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using DialogBoxResult = System.Windows.Forms.DialogResult;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

namespace YouTubeThumbnailGrabber
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Options options;
        YouTubeVideoThumbnail thumbnail;
        YouTubePage youtubePage;
        FolderBrowserDialog folderDialog = new FolderBrowserDialog();
        BitmapImage defaultThumbnail;
        ContextMenu ThumbnailImageCM;
        ClipboardMonitor clipboardMonitor;

        string channelURL;

        string SaveImageFilename { get { return System.IO.Path.Combine(options.SaveImagePath, thumbnail.VideoURL.VideoID) + ".jpg"; } }

        string configPath;

        public MainWindow()
        {
            InitializeComponent();

            clipboardMonitor = new ClipboardMonitor(this);
            clipboardMonitor.ClipboardTextChanged += clipboardMonitor_ClipboardTextChanged;
            defaultThumbnail = new BitmapImage(new Uri(@"\Resources\YouTubeThumbnailUnavailable.jpg", UriKind.Relative));
            ThumbnailImageCM = ThumbnailImage.ContextMenu;
            ThumbnailImage.Source = defaultThumbnail;
            ThumbnailImage.ContextMenu = null;
            configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xml");
            LoadSettings();          
        }

        private void LoadSettings()
        {             
            if (File.Exists(configPath))
            {
                XmlSerializer xml = new XmlSerializer(typeof(Options));
                using (Stream input = File.OpenRead(configPath))
                {
                    try
                    {
                        options = (Options)xml.Deserialize(input);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Configuration failed to load.  Using default settings.", "Load settings failed.", MessageBoxButton.OK, MessageBoxImage.Error);
                        LoadDefaultSettings();
                    }                    
                }
            }
            else
                LoadDefaultSettings();
            if (options.AutoLoadURLs && !clipboardMonitor.IsMonitoringClipboard)
                clipboardMonitor.InitCBViewer();
            else if (clipboardMonitor.IsMonitoringClipboard)
                clipboardMonitor.CloseCBViewer();

            folderDialog.Description = "Select a folder to save thumbnail images.";
            folderDialog.SelectedPath = options.SaveImagePath;
        }

        private void LoadDefaultSettings()
        {
            options = new Options();
            options.ImageFileNamingMode = FileNamingMode.VideoID;
            options.AutoSaveImages = false;
            options.AutoLoadURLs = false;
            options.PublishedDateTitle = false;
            options.VideoViews = false;
            options.SaveImagePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        }

        private void GetImage_Click(object sender, RoutedEventArgs e)
        {
            GrabThumbnail(InputVideoURL.Text);
        }

        private void GrabThumbnail(string url)
        {
            if (YouTubeURL.ValidateYTURL(url))
            {
                if (thumbnail != null && (YouTubeURL.GetVideoID(url) == thumbnail.VideoURL.VideoID))
                {
                    MessageBox.Show("This video's thumbnail is already being displayed.", "Duplicate URL");
                    return;
                }
                this.Cursor = Cursors.AppStarting;
                ThumbnailImage.Source = new BitmapImage();
                if (ThumbnailImage.ContextMenu == null)
                    ThumbnailImage.ContextMenu = ThumbnailImageCM;
                thumbnail = new YouTubeVideoThumbnail(url);
                thumbnail.GetThumbnailSuccess += Image_DownloadCompleted;
                thumbnail.GetThumbailFailure += Image_DownloadFailed;
                thumbnail.ThumbnailImage.DownloadProgress += ImageMaxRes_DownloadProgress;

                youtubePage = new YouTubePage(thumbnail.VideoURL);

                if (thumbnail.ThumbnailImage.IsDownloading)
                    DownloadProgress.Visibility = Visibility.Visible;
            }
            else
                MessageBox.Show("Invalid YouTube video URL", "Invalid URL", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        void Image_DownloadCompleted(object sender, EventArgs e)
        {
            ThumbnailImage.Source = (BitmapImage)sender;
            SaveImage.IsEnabled = true;
            DownloadProgress.Visibility = Visibility.Collapsed;
            ImageResolution.Text = thumbnail.ThumbnailImage.PixelWidth + " x " + thumbnail.ThumbnailImage.PixelHeight;
            UpdateStatusBar();
            thumbnail.GetThumbnailSuccess -= Image_DownloadCompleted;
            this.Cursor = Cursors.Arrow;

            if (options.AutoSaveImages)
                SaveThumbnailImage();
        }
        void Image_DownloadFailed(object sender, ExceptionEventArgs e)
        {           
            SaveImage.IsEnabled = false;
            DownloadProgress.Visibility = Visibility.Collapsed;
            ThumbnailImage.Source = defaultThumbnail;
            ThumbnailImage.ContextMenu = null;
            thumbnail.GetThumbailFailure -= Image_DownloadFailed;
            MessageBox.Show("The video thumbnail has failed to download.", "Download failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        void ImageMaxRes_DownloadProgress(object sender, DownloadProgressEventArgs e)
        {
            DownloadProgress.Value = e.Progress;
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            SaveThumbnailImage();
        }

        private void ImageToClipboard()
        {
            Clipboard.SetImage(thumbnail.ThumbnailImage);
            MessageBox.Show("Image copied to clipboard", "Image Copied", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveThumbnailImage()
        {
            string fileName = SaveImageFilename;
            if (File.Exists(fileName))
                MessageBox.Show("Image file already exists in this direcotry.", "Image not saved", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {
                if (!Directory.Exists(options.SaveImagePath))
                    Directory.CreateDirectory(options.SaveImagePath);
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(thumbnail.ThumbnailImage as BitmapImage));
                try
                {
                    using (Stream output = File.Create(fileName))
                        encoder.Save(output);
                }
                catch (System.UnauthorizedAccessException)
                {
                    MessageBox.Show("Image could not be saved. Please run this application as an Administrator.",
                        "Write Permissions Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                catch (Exception)
                {
                    MessageBox.Show("Image could not be saved.", 
                        "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MessageBox.Show("Video Thumbnail Saved to\n" + fileName, "Image Successfully Saved", 
                    MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }
        private void SaveOptions()
        {
            try
            {
                XmlSerializer xml = new XmlSerializer(typeof(Options));
                using (Stream output = File.Create(configPath))
                    xml.Serialize(output, options);
            }
            catch (System.UnauthorizedAccessException)
            {
                MessageBox.Show("Configuration settings not saved. Please run this application as an Administrator.",
                    "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                MessageBox.Show("Your configuration could not be saved.",
                    "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenVideo_Click(object sender, RoutedEventArgs e)
        {
            OpenVideoInBrowser();
        }

        private void OpenVideoInBrowser()
        {
            System.Diagnostics.Process.Start(thumbnail.VideoURL.LongYTURL);
        }

        private void OpenImageInViewerCtxtMen(object sender, RoutedEventArgs e)
        {
            OpenImageInViewer();
        }
        private void OpenImageInViewerDblClk(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && thumbnail != null)
                OpenImageInViewer();
        }
        private void OpenImageInViewer()
        {
            string temp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Temp\" + thumbnail.VideoURL.VideoID + ".jpg";
            string[] checkLocations = { SaveImageFilename, temp };
            bool fileExists = false;
            string existingFile = String.Empty;
            foreach (var check in checkLocations)
                if (File.Exists(check))
                {
                    fileExists = true;
                    existingFile = check;
                    break;
                }
            if (fileExists)
            {
                System.Diagnostics.Process.Start(existingFile);
            }
            else
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(thumbnail.ThumbnailImage as BitmapImage));
                try
                {
                    using (Stream output = File.Create(temp))
                        encoder.Save(output);
                }
                catch (Exception)
                {
                    MessageBox.Show("The image could not be saved.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                System.Diagnostics.Process.Start(temp);
            }
        }

        private void ImageToClipboardHandler(object sender, RoutedEventArgs e)
        {
            ImageToClipboard();
        }

        private void StatusBarURL_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (thumbnail != null)
                System.Diagnostics.Process.Start(thumbnail.VideoURL.LongYTURL);
        }
        private void StatusBarURL_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (thumbnail != null)
            {
                Clipboard.SetText(thumbnail.VideoURL.ShortYTURL);
                MessageBox.Show("URL copied to clipboard", "URL Copied", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void SBChannel_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (channelURL != null)
                System.Diagnostics.Process.Start(channelURL);
        }

        private void UpdateStatusBar()
        {
            if (youtubePage.ChannelIcon == null)
                youtubePage.ChanImageDownloaded += ytp_ChanImageDownloaded;
            else
                SBChanImage.Source = youtubePage.ChannelIcon;
            
            SBURL.Text = youtubePage.YTURL.ShortYTURL;
            StringBuilder sb = new StringBuilder(youtubePage.VideoTitle);
            if (options.PublishedDateTitle)
                sb.Insert(0, String.Format("[{0:yy.MM.dd}] ", youtubePage.Published));
            if (options.VideoViews)
                sb.Append(String.Format( " ({0})", youtubePage.VideoViewCount));
            SBTitle.Text = sb.ToString();
            SBChannel.Text = youtubePage.ChannelName;           
            channelURL = youtubePage.ChannelURL.OriginalString;
            
        }
        void ytp_ChanImageDownloaded(object sender, EventArgs e)
        {
            YouTubePage ytp = sender as YouTubePage;
            SBChanImage.Source = ytp.ChannelIcon;
            ytp.ChanImageDownloaded -= ytp_ChanImageDownloaded;
        }

        private void OpenOptions_Click(object sender, RoutedEventArgs e)
        {
            OptionsMenu optionsMenu = new OptionsMenu();
            optionsMenu.Closed += optionsMenu_Closed;
            optionsMenu.ShowDialog();
        }

        void optionsMenu_Closed(object sender, EventArgs e)
        {
            ((OptionsMenu)sender).Closed -= optionsMenu_Closed;
            LoadSettings();
            UpdateStatusBar();
        }

        private void SBTextCopy_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock tb = e.Source as TextBlock;
            if (tb != null && tb.Text != String.Empty)
                Clipboard.SetText(tb.Text);
        }

        private void MenuCopyAddress_Click(object sender, RoutedEventArgs e)
        {
            if (thumbnail != null)
            {
                Clipboard.SetText(thumbnail.VideoURL.ShortYTURL);
                MessageBox.Show("URL copied to clipboard", "URL Copied", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        void clipboardMonitor_ClipboardTextChanged(object sender, ClipboardEventArgs e)
        {
            string clipText = e.ClipboardText;
            if (YouTubeURL.ValidateYTURL(clipText))
            {
                if (thumbnail != null && (YouTubeURL.GetVideoID(clipText) == thumbnail.VideoURL.VideoID)) return;
                InputVideoURL.Text = clipText;
                GrabThumbnail(clipText);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            clipboardMonitor.CloseCBViewer();
        }

        private void ThumbnailImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (thumbnail == null)return;
            double imageDisplayArea = e.NewSize.Width * e.NewSize.Height;
            int imageResolutionArea = thumbnail.ThumbnailImage.PixelWidth * thumbnail.ThumbnailImage.PixelHeight;
            double zoomPercent = imageDisplayArea / imageResolutionArea;
            this.Title = String.Format("YouTube Thumbnail Generator ({0:p})", zoomPercent);
        }
    }
}
