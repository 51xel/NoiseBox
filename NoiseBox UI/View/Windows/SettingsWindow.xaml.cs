using System;
using System.Collections.Generic;
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
using System.IO;
using WinForms = System.Windows.Forms;
using NoiseBox;
using NoiseBox_UI.View.UserControls;
using NoiseBox_UI.Utils;
using MaterialDesignThemes.Wpf;

namespace NoiseBox_UI.View.Windows {
    public partial class SettingsWindow : Window {
        public SettingsWindow() {
            InitializeComponent();
            DataContext = this;
            WinMax.DoSourceInitialized(this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            foreach (var device in DeviceControll.GetOutputDevicesList()) {
                MainOutputDevicesList.Items.Add(device);
                AdditionalOutputDevicesList.Items.Add(device);
            }

            MainOutputDevicesList.SelectedItem = Properties.Settings.Default.MainOutputDevice;
            AdditionalOutputDevicesList.SelectedItem = Properties.Settings.Default.AdditionalOutputDevice;

            MicOutputEnabled.IsChecked = Properties.Settings.Default.MicOutputEnabled;
            AdditionalOutputEnabled.IsChecked = Properties.Settings.Default.AdditionalOutputEnabled;

            if (string.IsNullOrEmpty(Properties.Settings.Default.DownloadsFolder)) {
                string downloadsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                DownloadsFolder.Text = downloadsFolderPath;
            }
            else {
                DownloadsFolder.Text = Properties.Settings.Default.DownloadsFolder;
            }
            
            VisualizationEnabled.IsChecked = Properties.Settings.Default.VisualizationEnabled;
            MinimizeToTrayEnabled.IsChecked = Properties.Settings.Default.MinimizeToTrayEnabled;
        }

        private void Save_Click(object sender, RoutedEventArgs e) {
            var win = (Owner as MainWindow); 

            if (MainOutputDevicesList.SelectedItem.ToString() != Properties.Settings.Default.MainOutputDevice) {
                Properties.Settings.Default.MainOutputDevice = MainOutputDevicesList.SelectedItem.ToString();
                win.AudioStreamControl.MainMusic.ReselectOutputDevice(Properties.Settings.Default.MainOutputDevice);
            }

            Properties.Settings.Default.AdditionalOutputDevice = AdditionalOutputDevicesList.SelectedItem.ToString();

            if (AdditionalOutputEnabled.IsChecked.GetValueOrDefault() != Properties.Settings.Default.AdditionalOutputEnabled) {
                Properties.Settings.Default.AdditionalOutputEnabled = AdditionalOutputEnabled.IsChecked.GetValueOrDefault();

                if (Properties.Settings.Default.AdditionalOutputEnabled) {
                    if (!string.IsNullOrEmpty(Properties.Settings.Default.AdditionalOutputDevice)) {
                        win.AudioStreamControl.ActivateAdditionalMusic(Properties.Settings.Default.AdditionalOutputDevice);
                        win.AudioStreamControl.AdditionalMusic.MusicVolume = (float)win.BottomControlPanel.AdditionalVolumeSlider.Value / 100;
                    }
                }
                else if (win.AudioStreamControl.AdditionalMusic != null) {
                    win.AudioStreamControl.AdditionalMusic.CloseStream();
                    win.AudioStreamControl.AdditionalMusic = null;
                }
            }

            Properties.Settings.Default.MicOutputEnabled = MicOutputEnabled.IsChecked.GetValueOrDefault();

            win.BottomControlPanel.MicVolumeSlider.IsEnabled = Properties.Settings.Default.MicOutputEnabled;
            win.BottomControlPanel.MicVolumeButton.IsEnabled = Properties.Settings.Default.MicOutputEnabled;
            win.BottomControlPanel.AdditionalVolumeSlider.IsEnabled = Properties.Settings.Default.AdditionalOutputEnabled;
            win.BottomControlPanel.AdditionalVolumeButton.IsEnabled = Properties.Settings.Default.AdditionalOutputEnabled;

            Properties.Settings.Default.DownloadsFolder = DownloadsFolder.Text;
            Properties.Settings.Default.MinimizeToTrayEnabled = MinimizeToTrayEnabled.IsChecked.GetValueOrDefault();

            if (VisualizationEnabled.IsChecked.GetValueOrDefault() != Properties.Settings.Default.VisualizationEnabled) {
                Properties.Settings.Default.VisualizationEnabled = VisualizationEnabled.IsChecked.GetValueOrDefault();
                win.VisualizationEnabled = Properties.Settings.Default.VisualizationEnabled;

                if (Properties.Settings.Default.VisualizationEnabled) {
                    win.StartVisualization();
                }
                else {
                    win.StopVisualization();
                }
            }

            Properties.Settings.Default.Save();

            SavedSnackbar.MessageQueue?.Enqueue("Saved!", null, null, null, false, true, TimeSpan.FromSeconds(1));
        }

        private void EditDownloadsFolder(object sender, RoutedEventArgs e) {
            var dialog = new WinForms.FolderBrowserDialog();
            dialog.InitialDirectory = DownloadsFolder.Text;

            var res = dialog.ShowDialog();

            if (res == WinForms.DialogResult.OK) {
                DownloadsFolder.Text = dialog.SelectedPath;
            }
        }

        private void Window_Closed(object sender, EventArgs e) {

        }

        private void Window_StateChanged(object sender, EventArgs e) {
            if (WindowState == WindowState.Maximized) {
                Uri uri = new Uri("/Images/Icons/restore.png", UriKind.Relative);
                ImageSource imgSource = new BitmapImage(uri);
                TitlebarButtons.MaximizeButtonImage.Source = imgSource;
            }
            else if (WindowState == WindowState.Normal) {
                Uri uri = new Uri("/Images/Icons/maximize.png", UriKind.Relative);
                ImageSource imgSource = new BitmapImage(uri);
                TitlebarButtons.MaximizeButtonImage.Source = imgSource;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            Helper.FindVisualChildren<Grid>(this).FirstOrDefault().Focus();
        }
    }
}
