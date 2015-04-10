using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using YouTubeThumbnailGrabber.Model;
using DialogBoxResult = System.Windows.Forms.DialogResult;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

namespace YouTubeThumbnailGrabber.ViewModel
{
    public class ViewModel
    {
        private string channelURL;
        private string configPath;
        private string SaveImageFilename { get { return System.IO.Path.Combine(options.SaveImagePath, thumbnail.VideoURL.VideoID) + ".jpg"; } }

        private BitmapImage defaultThumbnail;
        private ClipboardMonitor clipboardMonitor;
        private FolderBrowserDialog folderDialog = new FolderBrowserDialog();
        private Options options;
        private YouTubeVideoThumbnail thumbnail;
        private YouTubePage youtubePage;

        /* 
         * These status bar fields will be made into a class.... later.
         */
        public string StsBrURL { get; set; }
        public string StsBrTitle { get; set; }
        public BitmapImage StsBrChanImage { get; set; }
        public string StsBrChanName { get; set; }

        public ViewModel(Window mainWindow)
        {
            clipboardMonitor = new ClipboardMonitor(mainWindow);
            clipboardMonitor.ClipboardTextChanged += clipboardMonitor_ClipboardTextChanged;

            defaultThumbnail = new BitmapImage(new Uri(@"\Resources\YouTubeThumbnailUnavailable.jpg", UriKind.Relative));
            configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xml");
            ReadSettings();
        }
        #region private Methods		
        private void ReadSettings()
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
        private void UpdateStatusBar()
        {
            if (youtubePage.ChannelIcon == null)
                youtubePage.ChanImageDownloaded += youtubePage_ChanImageDownloaded;
            else
                StsBrChanImage = youtubePage.ChannelIcon;

            StsBrURL = youtubePage.YTURL.ShortYTURL;
            StringBuilder sb = new StringBuilder(youtubePage.VideoTitle);
            if (options.PublishedDateTitle)
                sb.Insert(0, String.Format("[{0:yy.MM.dd}] ", youtubePage.Published));
            if (options.VideoViews)
                sb.Append(String.Format(" ({0})", youtubePage.VideoViewCount));
            StsBrTitle = sb.ToString();
            StsBrChanName = youtubePage.ChannelName;
            channelURL = youtubePage.ChannelURL.OriginalString;

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
                if (thumbnail != null && (YouTubeURL.GetVideoID(url) == thumbnail.VideoURL.VideoID))
                {
                    MessageBox.Show("This video's thumbnail is already being displayed.", "Duplicate URL");
                    return false;
                }
                thumbnail = new YouTubeVideoThumbnail(url);
                thumbnail.GetThumbnailSuccess += Image_DownloadCompleted;
                thumbnail.GetThumbailFailure += Image_DownloadFailed;
                thumbnail.ThumbnailImage.DownloadProgress += Image_DownloadProgress;

                youtubePage = new YouTubePage(thumbnail.VideoURL);
                return true;
            }
            else
                return false;               
        }
        /// <summary>
        /// Saves the currently displayed thumbnail image to a .jpg file
        /// </summary>
        /// <returns>Whether the save operation was successful</returns>
        public bool SaveThumbnailImageFile()
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
        #endregion


        #region Event Handlers
        private void clipboardMonitor_ClipboardTextChanged(object sender, ClipboardEventArgs e)
        {
            string clipText = e.ClipboardText;
            if (YouTubeURL.ValidateYTURL(clipText))
            {
                if (thumbnail != null && (YouTubeURL.GetVideoID(clipText) == thumbnail.VideoURL.VideoID)) return;
                GrabThumbnail(clipText);
            }
        }
        private void Image_DownloadCompleted(object sender, EventArgs e)
        {
            //SaveImage.IsEnabled = true;            
            thumbnail.GetThumbnailSuccess -= Image_DownloadCompleted;
            UpdateStatusBar();
            if (options.AutoSaveImages)
                SaveThumbnailImageFile();
            OnThumbnailImageCompleted();
        }
        private void Image_DownloadFailed(object sender, ExceptionEventArgs e)
        {
            //SaveImage.IsEnabled = false;
            thumbnail.GetThumbailFailure -= Image_DownloadFailed;
            OnThumbnailImageFailed();
        }
        private void Image_DownloadProgress(object sender, DownloadProgressEventArgs e)
        {
            OnThumbnailImageProgress(e);
        }

        void youtubePage_ChanImageDownloaded(object sender, EventArgs e)
        {
            YouTubePage youtubePage = sender as YouTubePage;
            StsBrChanImage = youtubePage.ChannelIcon;
            youtubePage.ChanImageDownloaded -= youtubePage_ChanImageDownloaded;
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