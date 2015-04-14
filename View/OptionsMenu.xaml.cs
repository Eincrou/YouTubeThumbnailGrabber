using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using YouTubeThumbnailGrabber.Model;
using DialogBoxResult = System.Windows.Forms.DialogResult;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

namespace YouTubeThumbnailGrabber.View
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class OptionsMenu : Window
    {
        private readonly FolderBrowserDialog _folderDialog = new FolderBrowserDialog();
        private Options _options;
        private readonly string _configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xml");
        public OptionsMenu(Options options)
        {
            InitializeComponent();
            _options = options;
            LoadSettings();
        }
        private void BrowseForDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(TBSaveDirectory.Text) && !String.IsNullOrWhiteSpace(TBSaveDirectory.Text))
                _folderDialog.SelectedPath = TBSaveDirectory.Text;
            else
                _folderDialog.SelectedPath = _options.SaveImagePath;
                if (_folderDialog.ShowDialog() == DialogBoxResult.OK)
                {
                    _options.SaveImagePath = _folderDialog.SelectedPath;
                    TBSaveDirectory.Text = _options.SaveImagePath;
                }
        }
        private void CloseDialog_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LoadSettings()
        {
            CBNamingMode.SelectedIndex = (int)_options.ImageFileNamingMode;
            CKBAutoSave.IsChecked = _options.AutoSaveImages;
            CKBAutoLoad.IsChecked = _options.AutoLoadURLs;
            CKBAddPublished.IsChecked = _options.PublishedDateTitle;
            CKBVideoViews.IsChecked = _options.VideoViews;
            _folderDialog.Description = "Select a folder to save thumbnail images.";
            TBSaveDirectory.Text = _options.SaveImagePath;
        }
        private void SaveOptions()
        {
            try
            {
                XmlSerializer xml = new XmlSerializer(typeof(Options));
                using (Stream output = File.Create(_configPath))
                    xml.Serialize(output, _options);
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

        private void CBNamingMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            _options.ImageFileNamingMode = (FileNamingMode)cb.SelectedIndex;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveOptions();
        }

        private void CKBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            CheckBox ckbx = e.Source as CheckBox;
            if (ckbx == null) return;
            switch (ckbx.Name)
            {
                case "CKBAutoSave":
                    _options.AutoSaveImages = ckbx.IsChecked.Value;
                    break;
                case "CKBAutoLoad":
                    _options.AutoLoadURLs = ckbx.IsChecked.Value;
                    break;
                case "CKBAddPublished":
                    _options.PublishedDateTitle = ckbx.IsChecked.Value;
                    break;
                case "CKBVideoViews":
                    _options.VideoViews = ckbx.IsChecked.Value;
                    break;
                default:
                    break;
            }
        }
    }
}
