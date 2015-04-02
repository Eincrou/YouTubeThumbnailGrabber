using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using DialogBoxResult = System.Windows.Forms.DialogResult;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

namespace YoutubeThumbnailGrabber
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Options options;
        YouTubeVideoThumbnail Thumbnail;
        FolderBrowserDialog folderDialog = new FolderBrowserDialog();
        BitmapImage defaultThumbnail;
        ContextMenu ThumbnailImageCM;

        string SaveImageFilename { get { return System.IO.Path.Combine(options.SaveImagePath, Thumbnail.VideoURL.VideoID) + ".jpg"; } }

        string configPath;

        public MainWindow()
        {
            InitializeComponent();

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

            folderDialog.Description = "Select a folder to save thumbnail images.";
            folderDialog.SelectedPath = options.SaveImagePath;
            AutoSaveCheckBox.IsChecked = options.AutoSaveImages;
        }

        private void LoadDefaultSettings()
        {
            options = new Options();
            options.AutoSaveImages = false;
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
                if (Thumbnail != null && (YouTubeURL.GetVideoID(url) == Thumbnail.VideoURL.VideoID))
                {
                    MessageBox.Show("This video's thumbnail is already being displayed.", "Duplicate URL");
                    return;
                }
                ThumbnailImage.Source = new BitmapImage();
                if (ThumbnailImage.ContextMenu == null)
                    ThumbnailImage.ContextMenu = ThumbnailImageCM;
                Thumbnail = new YouTubeVideoThumbnail(url);
                Thumbnail.GetThumbnailSuccess += Image_DownloadCompleted;
                Thumbnail.GetThumbailFailure += Image_DownloadFailed;
                Thumbnail.ThumbnailImage.DownloadProgress += ImageMaxRes_DownloadProgress;

                if (Thumbnail.ThumbnailImage.IsDownloading)
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
            ImageResolution.Text = Thumbnail.ThumbnailImage.PixelWidth + " x " + Thumbnail.ThumbnailImage.PixelHeight;
            OpenVideo.IsEnabled = true;
            Thumbnail.GetThumbnailSuccess -= Image_DownloadCompleted;

            if (options.AutoSaveImages)
                SaveThumbnailImage();
        }
        void Image_DownloadFailed(object sender, ExceptionEventArgs e)
        {           
            SaveImage.IsEnabled = false;
            DownloadProgress.Visibility = Visibility.Collapsed;
            OpenVideo.IsEnabled = false;
            ThumbnailImage.Source = defaultThumbnail;
            ThumbnailImage.ContextMenu = null;
            Thumbnail.GetThumbailFailure -= Image_DownloadFailed;
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
            Clipboard.SetImage(Thumbnail.ThumbnailImage);
            MessageBox.Show("Image copied to clipboard", "Image Copied", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void SetSaveFolder_Click(object sender, RoutedEventArgs e)
        {
            if (folderDialog.ShowDialog() == DialogBoxResult.OK)
                options.SaveImagePath = folderDialog.SelectedPath;
            SaveOptions();
        }
        private void AutoSaveCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            options.AutoSaveImages = (sender as CheckBox).IsChecked ?? false;
            SaveOptions();
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
                encoder.Frames.Add(BitmapFrame.Create(Thumbnail.ThumbnailImage as BitmapImage));
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
                    MessageBox.Show("The image could not be saved.", 
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
            System.Diagnostics.Process.Start(Thumbnail.VideoURL.LongYTURL);
        }

        private void OpenImageInViewer(object sender, RoutedEventArgs e)
        {
            string temp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Temp\" + Thumbnail.VideoURL.VideoID + ".jpg";
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
                encoder.Frames.Add(BitmapFrame.Create(Thumbnail.ThumbnailImage as BitmapImage));
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
    }
}
