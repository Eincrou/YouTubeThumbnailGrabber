using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using DialogBoxResult = System.Windows.Forms.DialogResult;
using System.Xml.Serialization;

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

        private bool ctrlKeyDown = false;
        string configPath;

        public MainWindow()
        {
            InitializeComponent();

            configPath = AppDomain.CurrentDomain.BaseDirectory + "config.xml";
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
                        options = new Options();
                    }                    
                }
            }
            else
                options = new Options();

            folderDialog.SelectedPath = options.SaveImagePath;
            AutoSaveCheckBox.IsChecked = options.AutoSaveImages;
        }

        private void GetImage_Click(object sender, RoutedEventArgs e)
        {
            if (YouTubeURL.ValidateYTURL(InputVideoURL.Text))
            {
                ThumbnailImage.Source = new BitmapImage();
                Thumbnail = new YouTubeVideoThumbnail(InputVideoURL.Text);
                Thumbnail.GetThumbnailSuccess += Image_DownloadCompleted;
                Thumbnail.GetThumbailFailure += Image_DownloadFailed;
                Thumbnail.ThumbnailImage.DownloadProgress += ImageMaxRes_DownloadProgress;
                if (Thumbnail.ThumbnailImage.IsDownloading)
                {
                    DownloadProgress.Visibility = Visibility.Visible;
                }
                else
                    ThumbnailImage.Source = (sender as BitmapImage);
            }
            else
                MessageBox.Show("Incorrect YouTube URL", "Invalid URL", MessageBoxButton.OK, MessageBoxImage.Error);
        }


        void Image_DownloadCompleted(object sender, EventArgs e)
        {
            ThumbnailImage.Source = (sender as BitmapImage);
            SaveImage.IsEnabled = true;
            DownloadProgress.Visibility = Visibility.Collapsed;
            ImageResolution.Text = Thumbnail.ThumbnailImage.PixelWidth + " x " + Thumbnail.ThumbnailImage.PixelHeight;
            OpenVideo.IsEnabled = true;

            if (options.AutoSaveImages)
                SaveThumbnailImage();
        }
        void Image_DownloadFailed(object sender, ExceptionEventArgs e)
        {
            MessageBox.Show("The video thumbnail has failed to download.", "Download failed", MessageBoxButton.OK, MessageBoxImage.Error);
            SaveImage.IsEnabled = false;
            DownloadProgress.Visibility = Visibility.Collapsed;
            OpenVideo.IsEnabled = false;
        }
        void ImageMaxRes_DownloadProgress(object sender, DownloadProgressEventArgs e)
        {
            DownloadProgress.Value = e.Progress;
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            SaveThumbnailImage();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key ==  Key.RightCtrl)
                ctrlKeyDown = true;            
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                ctrlKeyDown = false;
        }

        private void ThumbnailImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ctrlKeyDown)
            {
                Clipboard.SetImage(Thumbnail.ThumbnailImage);
                MessageBox.Show("Image copied to clipboard", "Copy thumbnail");
            }
        }
        private void SetSaveFolder_Click(object sender, RoutedEventArgs e)
        {
            if (folderDialog.ShowDialog() == DialogBoxResult.OK)
            {
                if (folderDialog.SelectedPath != folderDialog.SelectedPath + "\\")
                    folderDialog.SelectedPath += "\\";
                options.SaveImagePath = folderDialog.SelectedPath;
            }
            SaveOptions();
        }
        private void AutoSaveCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            options.AutoSaveImages = (sender as CheckBox).IsChecked ?? false;
            SaveOptions();
        }

        private void SaveThumbnailImage()
        {            
            string fileName = options.SaveImagePath + Thumbnail.VideoURL.VideoID + ".jpg";
            if (File.Exists(fileName))
                MessageBox.Show("Image file already exists in this direcotry.", "Image not saved", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(ThumbnailImage.Source as BitmapImage));
                using (Stream output = File.Create(fileName))
                    encoder.Save(output);
                MessageBox.Show("Video Thumbnail Saved to\n" + fileName, "Image Successfully Saved", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }
        private void SaveOptions()
        {
            XmlSerializer xml = new XmlSerializer(typeof(Options));
            using (Stream output = File.Create(configPath))
            {
                xml.Serialize(output, options);
            }
        }

        private void OpenVideo_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Thumbnail.VideoURL.LongYTURL);
        }


    }
}
