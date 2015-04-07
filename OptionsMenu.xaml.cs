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
using System.Windows.Shapes;
using System.Xml.Serialization;
using DialogBoxResult = System.Windows.Forms.DialogResult;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

namespace YouTubeThumbnailGrabber
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class OptionsMenu : Window
    {
        private FolderBrowserDialog folderDialog = new FolderBrowserDialog();
        private Options options;
        private string configPath;
        public OptionsMenu()
        {
            InitializeComponent();

            configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xml");
            LoadSettings();
        }
        private void BrowseForDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(TBSaveDirectory.Text) && !String.IsNullOrWhiteSpace(TBSaveDirectory.Text))
                folderDialog.SelectedPath = TBSaveDirectory.Text;
            else
                folderDialog.SelectedPath = options.SaveImagePath;
                if (folderDialog.ShowDialog() == DialogBoxResult.OK)
                {
                    options.SaveImagePath = folderDialog.SelectedPath;
                    TBSaveDirectory.Text = options.SaveImagePath;
                }
        }
        private void CloseDialog_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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

            CBNamingMode.SelectedIndex = (int)options.ImageFileNamingMode;
            CKBAutoSave.IsChecked = options.AutoSaveImages;
            CKBAutoLoad.IsChecked = options.AutoLoadURLs;
            CKBAddPublished.IsChecked = options.PublishedDateTitle;
            folderDialog.Description = "Select a folder to save thumbnail images.";
            TBSaveDirectory.Text = options.SaveImagePath;
        }
        private void LoadDefaultSettings()
        {
            options = new Options();
            options.AutoSaveImages = false;
            options.SaveImagePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
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

        private void CBNamingMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            options.ImageFileNamingMode = (FileNamingMode)cb.SelectedIndex;
        }

        private void CKBAutoSave_Checked(object sender, RoutedEventArgs e)
        {
            options.AutoSaveImages = (sender as CheckBox).IsChecked.Value;
        }

        private void CKBAutoLoad_Checked(object sender, RoutedEventArgs e)
        {
            options.AutoLoadURLs = (sender as CheckBox).IsChecked.Value;
        }

        private void CKBAddPublished_Checked(object sender, RoutedEventArgs e)
        {
            options.PublishedDateTitle = (sender as CheckBox).IsChecked.Value;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveOptions();
        }

    }
}
