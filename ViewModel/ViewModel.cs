using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class ViewModel : INotifyPropertyChanged
    {
        private string _channelUrl;
        private bool _isUpdatingThumbnail = false;
        private readonly string _configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xml");

        private string SaveImageFilename
        {
            get
            {
                string imageJpgFileName;
                switch (_options.ImageFileNamingMode)
                {
                    case FileNamingMode.ChannelTitle:
                        string workingFileName = String.Format("{0} - {1}", _youtubePage.ChannelName,_youtubePage.VideoTitle);
                        foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                        {
                            workingFileName = workingFileName.Replace(c, '-');
                        }
                        imageJpgFileName =  workingFileName;
                        break;
                    case FileNamingMode.VideoID:
                        imageJpgFileName = _thumbnail.VideoUrl.VideoID;
                        break;
                    default:
                        throw new InvalidEnumArgumentException("An invalid FileNamingMode was found.");
                }
                return System.IO.Path.Combine(_options.SaveImagePath, imageJpgFileName) + ".jpg";
            }
        }

        private readonly BitmapImage _defaultThumbnail = new BitmapImage(new Uri(@"\Resources\YouTubeThumbnailUnavailable.jpg", UriKind.Relative));
        private ClipboardMonitor _clipboardMonitor;
        private readonly FolderBrowserDialog _folderDialog = new FolderBrowserDialog();
        private Options _options;
        private YouTubeVideoThumbnail _thumbnail;
        private YouTubePage _youtubePage;

        public bool IsThumbnailDisplayed { get { return !(_thumbnail == null); } }
        public BitmapImage ThumbnailBitmapImage
        {
            get
            {
                if (_isUpdatingThumbnail) return null;
                return (_thumbnail != null) ? _thumbnail.ThumbnailImage : _defaultThumbnail;
            }
        }

        /* 
         * These status bar fields will be made into a class.... later.
         */

        public string StsBrUrl
        {
            get { return _stsBrUrl; }
            set
            {
                _stsBrUrl = value;
                OnPropertyChanged("StsBrUrl");
            }
        }
        private string _stsBrUrl;
        public string StsBrTitle
        {
            get { return _stsBrTitle; }
            set
            {
                _stsBrTitle = value;
                OnPropertyChanged("StsBrTitle");
            }
        }
        private string _stsBrTitle;
        public BitmapImage StsBrChanImage
        {
            get { return _stsBrChanImage; }
            private set
            {
                _stsBrChanImage = value;
                OnPropertyChanged("StsBrChanImage");
            }
        }
        private BitmapImage _stsBrChanImage;
        public string StsBrChanName
        {
            get { return _stsBrChanName; }
            private set
            {
                _stsBrChanName = value;
                OnPropertyChanged("StsBrChanName");
            }
        }
        private string _stsBrChanName;

        public ViewModel()
        {
            
        }

        public void InitializeViewModel(Window mainWindow)
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
            // Where to instantiate new object so it doesn't block?
            if (_youtubePage == null || !(_youtubePage.VideoUrl).Equals(_thumbnail.VideoUrl))
                _youtubePage = new YouTubePage(_thumbnail.VideoUrl);
            if (_youtubePage.ChannelIcon == null)
                _youtubePage.ChanImageDownloaded += youtubePage_ChanImageDownloaded;
            else
                StsBrChanImage = _youtubePage.ChannelIcon;

            StsBrUrl = _youtubePage.VideoUrl.ShortYTURL;
            StringBuilder sb = new StringBuilder(_youtubePage.VideoTitle);
            if (_options.PublishedDateTitle)
                sb.Insert(0, String.Format("[{0:yy.MM.dd}] ", _youtubePage.Published));
            if (_options.VideoViews)
                sb.Append(String.Format(" ({0})", _youtubePage.VideoViewCount));
            StsBrTitle = sb.ToString();
            StsBrChanName = _youtubePage.ChannelName;
            _channelUrl = _youtubePage.ChannelUri.OriginalString;
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
                if (_thumbnail != null && (YouTubeURL.GetVideoID(url) == _thumbnail.VideoUrl.VideoID))
                {
                    return false;
                    //    MessageBox.Show("This video's thumbnail is already being displayed.", "Duplicate URL");
                }
                _isUpdatingThumbnail = true;
                OnPropertyChanged("ThumbnailBitmapImage");
                _thumbnail = new YouTubeVideoThumbnail(url);
                _thumbnail.GetThumbnailSuccess += Image_DownloadCompleted;
                _thumbnail.GetThumbailFailure += Image_DownloadFailed;
                _thumbnail.ThumbnailImage.DownloadProgress += Image_DownloadProgress;

                _youtubePage = new YouTubePage(_thumbnail.VideoUrl);
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
                MessageBox.Show("Video Thumbnail Saved to\n" + fileName, "Image Successfully Saved",
                    MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return true;
            }
        }

        public void OpenImageInViewer() {
            string tempDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Temp\" + _thumbnail.VideoUrl.VideoID + ".jpg";
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
                encoder.Frames.Add(BitmapFrame.Create(_thumbnail.ThumbnailImage as BitmapImage));
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
            System.Diagnostics.Process.Start(_thumbnail.VideoUrl.LongYTURL);
        }

        public void SetImageToClipboard()
        {
            Clipboard.SetImage(_thumbnail.ThumbnailImage);
        }
        public void SetVideoUrlToClipboard()
        {
            if (_thumbnail == null) return; 
            Clipboard.SetText(_thumbnail.VideoUrl.ShortYTURL);
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
            var opMenu = new OptionsMenu(_options);
            opMenu.Owner = Application.Current.MainWindow;
            opMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            opMenu.Closed += opMenu_Closed;
            opMenu.ShowDialog();
        }
        #endregion


        #region Event Handlers
        private void clipboardMonitor_ClipboardTextChanged(object sender, ClipboardEventArgs e)
        {
            string clipText = e.ClipboardText;
            if (!YouTubeURL.ValidateYTURL(clipText)) return;
            if (_thumbnail != null && (YouTubeURL.GetVideoID(clipText) == _thumbnail.VideoUrl.VideoID)) return;
            GrabThumbnail(clipText);
        }
        private void Image_DownloadCompleted(object sender, EventArgs e)
        {
            //SaveImage.IsEnabled = true;  
            _isUpdatingThumbnail = false;
            _thumbnail.GetThumbnailSuccess -= Image_DownloadCompleted;
            OnPropertyChanged("ThumbnailBitmapImage");
            UpdateStatusBar();
            if (_options.AutoSaveImages)
                SaveThumbnailImageToFile();
            OnThumbnailImageCompleted(sender, e);
        }
        private void Image_DownloadFailed(object sender, ExceptionEventArgs e)
        {
            //SaveImage.IsEnabled = false;
            _isUpdatingThumbnail = false;
            _thumbnail.GetThumbailFailure -= Image_DownloadFailed;
            OnPropertyChanged("ThumbnailBitmapImage");
            OnThumbnailImageFailed(sender, e);
        }
        private void Image_DownloadProgress(object sender, DownloadProgressEventArgs e)
        {
            OnThumbnailImageProgress(sender, e);
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
        public event EventHandler ThumbnailImageDownloadCompletedRouted;
        private void OnThumbnailImageCompleted(object sender, EventArgs e)
        {
            EventHandler thumbnailImageDownloadCompleted = ThumbnailImageDownloadCompletedRouted;
            if (thumbnailImageDownloadCompleted != null)
                ThumbnailImageDownloadCompletedRouted(sender, e);
        }
        /// <summary>
        /// Notifies upon failure of thumbnail image download
        /// </summary>
        public event EventHandler<ExceptionEventArgs> ThumbnailImageDownloadFailedRouted;
        private void OnThumbnailImageFailed(object sender, ExceptionEventArgs e)
        {
            var thumbnailImageDownloadFailed = ThumbnailImageDownloadFailedRouted;
            if (thumbnailImageDownloadFailed != null)
                ThumbnailImageDownloadFailedRouted(sender, e);
        }
        /// <summary>
        /// Notifies on the progress of thumbnail image downloads.
        /// </summary>
        public event EventHandler<DownloadProgressEventArgs> ThumbnailImageProgressRouted;
        private void OnThumbnailImageProgress(object sender, DownloadProgressEventArgs e)
        {
            EventHandler<DownloadProgressEventArgs> thumbnailImageProgress = ThumbnailImageProgressRouted;
            if (thumbnailImageProgress != null)
                ThumbnailImageProgressRouted(sender, e);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName = null)
        {
            var onPropertyChanged = PropertyChanged;
            if (onPropertyChanged != null) onPropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        #endregion
    }
}