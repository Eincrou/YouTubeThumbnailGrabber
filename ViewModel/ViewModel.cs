using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using YouTubeThumbnailGrabber.Model;
using YouTubeThumbnailGrabber.View;
using DialogBoxResult = System.Windows.Forms.DialogResult;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

namespace YouTubeThumbnailGrabber.ViewModel
{
    public class ViewModel
    {
        private string _channelUrl;
        private readonly string _configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xml");
        private string SaveImageFilename { get { return System.IO.Path.Combine(_options.SaveImagePath, _thumbnail.VideoURL.VideoID) + ".jpg"; } }

        private readonly BitmapImage _defaultThumbnail = new BitmapImage(new Uri(@"\Resources\YouTubeThumbnailUnavailable.jpg", UriKind.Relative));
        private readonly ClipboardMonitor _clipboardMonitor;
        private readonly FolderBrowserDialog _folderDialog = new FolderBrowserDialog();
        private Options _options;
        private YouTubeVideoThumbnail _thumbnail;
        private YouTubePage _youtubePage;

        /* 
         * These status bar fields will be made into a class.... later.
         */
        public string StsBrURL { get; set; }
        public string StsBrTitle { get; set; }
        public BitmapImage StsBrChanImage { get; set; }
        public string StsBrChanName { get; set; }

        public ViewModel(Window mainWindow)
        {
            _clipboardMonitor = new ClipboardMonitor(mainWindow);
            _clipboardMonitor.ClipboardTextChanged += clipboardMonitor_ClipboardTextChanged;
            mainWindow.Closed += mainWindow_Closed;
            ReadSettings();
        }


        #region private Methods		
        private void ReadSettings()
        {
            if (File.Exists(_configPath))
            {
                var xml = new XmlSerializer(typeof(Options));
                using (Stream input = File.OpenRead(_configPath))
                {
                    try
                    {
                        _options = (Options)xml.Deserialize(input);
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
            if (_options.AutoLoadURLs && !_clipboardMonitor.IsMonitoringClipboard)
                _clipboardMonitor.InitCBViewer();
            else if (_clipboardMonitor.IsMonitoringClipboard)
                _clipboardMonitor.CloseCBViewer();

            _folderDialog.Description = "Select a folder to save thumbnail images.";
            _folderDialog.SelectedPath = _options.SaveImagePath;
        }
        private void LoadDefaultSettings()
        {
            _options = new Options
            {
                ImageFileNamingMode = FileNamingMode.VideoID,
                AutoSaveImages = false,
                AutoLoadURLs = false,
                PublishedDateTitle = false,
                VideoViews = false,
                SaveImagePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };
        }
        private void UpdateStatusBar()
        {
            if (_youtubePage.ChannelIcon == null)
                _youtubePage.ChanImageDownloaded += youtubePage_ChanImageDownloaded;
            else
                StsBrChanImage = _youtubePage.ChannelIcon;

            StsBrURL = _youtubePage.YTURL.ShortYTURL;
            StringBuilder sb = new StringBuilder(_youtubePage.VideoTitle);
            if (_options.PublishedDateTitle)
                sb.Insert(0, String.Format("[{0:yy.MM.dd}] ", _youtubePage.Published));
            if (_options.VideoViews)
                sb.Append(String.Format(" ({0})", _youtubePage.VideoViewCount));
            StsBrTitle = sb.ToString();
            StsBrChanName = _youtubePage.ChannelName;
            _channelUrl = _youtubePage.ChannelUri.OriginalString;

            OnStatusBarUpdated();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Attempts to use the input string to grab a YouTube video thumbnail image. Returns false if an invalid YouTube URL, or if the thumbnail image is already being displayed
        /// </summary>
        /// <param name="url">URL to a YouTube video page</param>
        /// <returns>Whether the URL could be successfully parsed and is a new URL</returns>
        public bool GrabThumbnail(string url)
        {
            if (YouTubeURL.ValidateYTURL(url))
            {
                if (_thumbnail != null && (YouTubeURL.GetVideoID(url) == _thumbnail.VideoURL.VideoID))
                {
                    MessageBox.Show("This video's thumbnail is already being displayed.", "Duplicate URL");
                    return false;
                }
                _thumbnail = new YouTubeVideoThumbnail(url);
                _thumbnail.GetThumbnailSuccess += Image_DownloadCompleted;
                _thumbnail.GetThumbailFailure += Image_DownloadFailed;
                _thumbnail.ThumbnailImage.DownloadProgress += Image_DownloadProgress;

                _youtubePage = new YouTubePage(_thumbnail.VideoURL);
                return true;
            }
            else
                return false;               
        }
        /// <summary>
        /// Saves the currently displayed thumbnail image to a .jpg file
        /// </summary>
        /// <returns>Whether the save operation was successful</returns>
        public bool SaveThumbnailImageToFile()
        {
            string fileName = SaveImageFilename;
            if (File.Exists(fileName))
            {
                MessageBox.Show("Image file already exists in this direcotry.", "Image not saved",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else
            {
                if (!Directory.Exists(_options.SaveImagePath))
                    Directory.CreateDirectory(_options.SaveImagePath);
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(_thumbnail.ThumbnailImage as BitmapImage));
                try
                {
                    using (Stream output = File.Create(fileName))
                        encoder.Save(output);
                }
                catch (System.UnauthorizedAccessException)
                {
                    MessageBox.Show("Image could not be saved. Please run this application as an Administrator.",
                        "Write Permissions Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                catch (Exception)
                {
                    MessageBox.Show("Image could not be saved.",
                        "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                return true;
            }
        }

        public void OpenImageInViewer() {
            string tempDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Temp\" + thumbnail.VideoURL.VideoID + ".jpg";
            string[] checkLocations = { SaveImageFilename, tempDirectory };
            bool fileExists = false;
            string existingFile = String.Empty;
            foreach (var check in checkLocations)
                if (File.Exists(check)) {
                    fileExists = true;
                    existingFile = check;
                    break;
                }
            if (fileExists)
                System.Diagnostics.Process.Start(existingFile);
            else {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(thumbnail.ThumbnailImage as BitmapImage));
                try {
                    using (Stream output = File.Create(tempDirectory))
                        encoder.Save(output);
                }
                catch (Exception) {
                    MessageBox.Show("The image could not be saved.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                System.Diagnostics.Process.Start(tempDirectory);
            }
        }
        public void OpenVideoInBrowser() {
            if (_thumbnail == null) return;
            System.Diagnostics.Process.Start(_thumbnail.VideoURL.LongYTURL);
        }

        public void SetImageToClipboard()
        {
            Clipboard.SetImage(_thumbnail.ThumbnailImage);
        }
        public void SetVideoUrlToClipboard()
        {
            if (_thumbnail == null) return; 
            Clipboard.SetText(_thumbnail.VideoURL.ShortYTURL);
        }
        public void SetChannelUrlToClipboard()
        {
            if (_thumbnail == null) return;
            Clipboard.SetText(_channelUrl);
        }
        public void OpenChannelInBrowser()
        {
            if (_thumbnail == null) return;
            System.Diagnostics.Process.Start(_channelUrl);
        }

        public void OpenOptionsMenu()
        {
            var opMenu = new OptionsMenu();
            opMenu.Closed += opMenu_Closed;
        }
        #endregion


        #region Event Handlers
        private void clipboardMonitor_ClipboardTextChanged(object sender, ClipboardEventArgs e)
        {
            string clipText = e.ClipboardText;
            if (!YouTubeURL.ValidateYTURL(clipText)) return;
            if (_thumbnail != null && (YouTubeURL.GetVideoID(clipText) == _thumbnail.VideoURL.VideoID)) return;
            GrabThumbnail(clipText);
        }
        private void Image_DownloadCompleted(object sender, EventArgs e)
        {
            //SaveImage.IsEnabled = true;            
            _thumbnail.GetThumbnailSuccess -= Image_DownloadCompleted;
            UpdateStatusBar();
            if (_options.AutoSaveImages)
                SaveThumbnailImageToFile();
            OnThumbnailImageCompleted();
        }
        private void Image_DownloadFailed(object sender, ExceptionEventArgs e)
        {
            //SaveImage.IsEnabled = false;
            _thumbnail.GetThumbailFailure -= Image_DownloadFailed;
            OnThumbnailImageFailed();
        }
        private void Image_DownloadProgress(object sender, DownloadProgressEventArgs e)
        {
            OnThumbnailImageProgress(e);
        }

        void youtubePage_ChanImageDownloaded(object sender, EventArgs e)
        {
            var youtubePage = sender as YouTubePage;
            if (youtubePage == null) return;
            StsBrChanImage = youtubePage.ChannelIcon;
            youtubePage.ChanImageDownloaded -= youtubePage_ChanImageDownloaded;
        }
        void mainWindow_Closed(object sender, EventArgs e) {
            _clipboardMonitor.CloseCBViewer();
        }
        void opMenu_Closed(object sender, EventArgs e) {
            ((OptionsMenu)sender).Closed -= opMenu_Closed;
            ReadSettings();
            UpdateStatusBar();
        }
        #endregion

        #region Public Events
        /// <summary>
        /// Notifies upon succes of thumbnail image download
        /// </summary>
        public event EventHandler ThumbnailImageDownloadCompleted;
        private void OnThumbnailImageCompleted()
        {
            EventHandler thumbnailImageDownloadCompleted = ThumbnailImageDownloadCompleted;
            if (thumbnailImageDownloadCompleted != null)
                ThumbnailImageDownloadCompleted(this, new EventArgs());
        }
        /// <summary>
        /// Notifies upon failure of thumbnail image download
        /// </summary>
        public event EventHandler ThumbnailImageDownloadFailed;
        private void OnThumbnailImageFailed()
        {
            EventHandler thumbnailImageDownloadFailed = ThumbnailImageDownloadFailed;
            if (thumbnailImageDownloadFailed != null)
                ThumbnailImageDownloadFailed(this, new EventArgs());
        }
        /// <summary>
        /// Notifies on the progress of thumbnail image downloads
        /// </summary>
        public event EventHandler<DownloadProgressEventArgs> ThumbnailImageProgress;
        private void OnThumbnailImageProgress(DownloadProgressEventArgs e)
        {
            EventHandler<DownloadProgressEventArgs> thumbnailImageProgress = ThumbnailImageProgress;
            if (thumbnailImageProgress != null)
                ThumbnailImageProgress(this, e);
        }
        /// <summary>
        /// Notifies upon status bar information changes
        /// </summary>
        public event EventHandler StatusBarUpdated;
        private void OnStatusBarUpdated()
        {
            EventHandler statusBarUpdated = StatusBarUpdated;
            if (statusBarUpdated != null)
                StatusBarUpdated(this, new EventArgs());
        }
        #endregion
    }
}