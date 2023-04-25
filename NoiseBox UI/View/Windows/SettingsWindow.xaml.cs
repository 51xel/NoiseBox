using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using WinForms = System.Windows.Forms;
using NoiseBox;
using NoiseBox_UI.View.UserControls;
using NoiseBox_UI.Utils;
using MaterialDesignThemes.Wpf;
using System.Text.RegularExpressions;

namespace NoiseBox_UI.View.Windows {
    public partial class SettingsWindow : Window {
        public SettingsWindow() {
            InitializeComponent();
            DataContext = this;
            WinMax.DoSourceInitialized(this);

            PreviewKeyDown += Window_PreviewKeyDown;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            var win = (Owner as MainWindow);

            foreach (var device in DeviceControll.GetOutputDevicesList()) {
                MainOutputDevicesList.Items.Add(device);
                AdditionalOutputDevicesList.Items.Add(device);
                MicOutputDevicesList.Items.Add(device);
            }

            foreach (var device in DeviceControll.GetInputDevicesList()) {
                InputDevicesList.Items.Add(device);
            }

            if (MainOutputDevicesList.Items.Contains(Properties.Settings.Default.MainOutputDevice)) {
                MainOutputDevicesList.SelectedItem = Properties.Settings.Default.MainOutputDevice;
            }
            else {
                MainOutputDevicesList.SelectedItem = DeviceControll.GetOutputDeviceNameById(win.AudioStreamControl.MainMusic.GetOutputDeviceId());
            }

            if (AdditionalOutputDevicesList.Items.Contains(Properties.Settings.Default.AdditionalOutputDevice)) {
                AdditionalOutputDevicesList.SelectedItem = Properties.Settings.Default.AdditionalOutputDevice;
            }
            else {
                AdditionalOutputDevicesList.SelectedItem = DeviceControll.GetOutputDeviceNameById(0);
            }

            MicOutputDevicesList.SelectedItem = Properties.Settings.Default.MicOutputDevice;
            InputDevicesList.SelectedItem = Properties.Settings.Default.InputDevice;

            MicOutputEnabled.IsChecked = Properties.Settings.Default.MicOutputEnabled;
            AdditionalOutputEnabled.IsChecked = Properties.Settings.Default.AdditionalOutputEnabled;
            EqualizerOnStartEnabled.IsChecked = Properties.Settings.Default.EqualizerOnStartEnabled;

            if (string.IsNullOrEmpty(Properties.Settings.Default.DownloadsFolder)) {
                string downloadsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                DownloadsFolder.Text = downloadsFolderPath;
            }
            else {
                DownloadsFolder.Text = Properties.Settings.Default.DownloadsFolder;
            }
            
            VisualizationEnabled.IsChecked = Properties.Settings.Default.VisualizationEnabled;
            MinimizeToTrayEnabled.IsChecked = Properties.Settings.Default.MinimizeToTrayEnabled;

            PopulateHotkeyStackPanel();
        }

        private void Save_Click(object sender, RoutedEventArgs e) {
            var win = (Owner as MainWindow); 

            if (MainOutputDevicesList.SelectedItem.ToString() != Properties.Settings.Default.MainOutputDevice || MainOutputDevicesList.SelectedItem.ToString() != DeviceControll.GetOutputDeviceNameById(win.AudioStreamControl.MainMusic.GetOutputDeviceId())) {
                Properties.Settings.Default.MainOutputDevice = MainOutputDevicesList.SelectedItem.ToString();
                win.AudioStreamControl.MainMusic.ReselectOutputDevice(Properties.Settings.Default.MainOutputDevice);
            }

            if (MicOutputDevicesList.SelectedItem != null) {
                Properties.Settings.Default.MicOutputDevice = MicOutputDevicesList.SelectedItem.ToString();
            }

            if (InputDevicesList.SelectedItem != null) {
                Properties.Settings.Default.InputDevice = InputDevicesList.SelectedItem.ToString();
            }

            Properties.Settings.Default.MicOutputEnabled = MicOutputEnabled.IsChecked.GetValueOrDefault();

            if (Properties.Settings.Default.MicOutputEnabled &&
                !string.IsNullOrEmpty(Properties.Settings.Default.MicOutputDevice) &&
                !string.IsNullOrEmpty(Properties.Settings.Default.InputDevice)) {

                if (win.AudioStreamControl.Microphone != null) {
                    win.AudioStreamControl.Microphone.CloseStream();
                    win.AudioStreamControl.Microphone = null;
                }

                win.AudioStreamControl.ActivateMic(Properties.Settings.Default.InputDevice, Properties.Settings.Default.MicOutputDevice);
                win.AudioStreamControl.Microphone.InputDeviceVolume = (float)Properties.Settings.Default.MicVolumeSliderValue / 100;
            }
            else {
                if (win.AudioStreamControl.Microphone != null) {
                    win.AudioStreamControl.Microphone.CloseStream();
                    win.AudioStreamControl.Microphone = null;
                }

                Properties.Settings.Default.MicOutputEnabled = false;
                MicOutputEnabled.IsChecked = false;
            }

            win.BottomControlPanel.MicVolumeSlider.IsEnabled = Properties.Settings.Default.MicOutputEnabled;
            win.BottomControlPanel.MicVolumeButton.IsEnabled = Properties.Settings.Default.MicOutputEnabled;

            bool changedAdditionalDevice = false;
            bool changedAdditionalEnabled = false;

            if (AdditionalOutputDevicesList.SelectedItem != null) {
                changedAdditionalDevice = (AdditionalOutputDevicesList.SelectedItem.ToString() != Properties.Settings.Default.AdditionalOutputDevice) 
                    || (AdditionalOutputDevicesList.SelectedItem.ToString() != DeviceControll.GetOutputDeviceNameById(win.AudioStreamControl.AdditionalMusic.GetOutputDeviceId()));
                Properties.Settings.Default.AdditionalOutputDevice = AdditionalOutputDevicesList.SelectedItem.ToString();
            }

            changedAdditionalEnabled = AdditionalOutputEnabled.IsChecked.GetValueOrDefault() != Properties.Settings.Default.AdditionalOutputEnabled;

            if ((changedAdditionalDevice && AdditionalOutputEnabled.IsChecked.GetValueOrDefault()) || 
                (changedAdditionalEnabled && AdditionalOutputEnabled.IsChecked.GetValueOrDefault())) {

                if (AdditionalOutputDevicesList.SelectedItem != null) {
                    Properties.Settings.Default.AdditionalOutputEnabled = true;

                    win.AudioStreamControl.ActivateAdditionalMusic(Properties.Settings.Default.AdditionalOutputDevice);
                    win.AudioStreamControl.AdditionalMusic.MusicVolume = (float)win.BottomControlPanel.AdditionalVolumeSlider.Value / 100;
                }
                else {
                    Properties.Settings.Default.AdditionalOutputEnabled = false;
                    AdditionalOutputEnabled.IsChecked = false;
                }
            }
            else if (changedAdditionalEnabled && !AdditionalOutputEnabled.IsChecked.GetValueOrDefault()) {
                Properties.Settings.Default.AdditionalOutputEnabled = false;

                if (win.AudioStreamControl.AdditionalMusic != null) {
                    win.AudioStreamControl.AdditionalMusic.CloseStream();
                    win.AudioStreamControl.AdditionalMusic = null;
                }
            }

            win.BottomControlPanel.AdditionalVolumeSlider.IsEnabled = Properties.Settings.Default.AdditionalOutputEnabled;
            win.BottomControlPanel.AdditionalVolumeButton.IsEnabled = Properties.Settings.Default.AdditionalOutputEnabled;

            Properties.Settings.Default.DownloadsFolder = DownloadsFolder.Text;
            Properties.Settings.Default.MinimizeToTrayEnabled = MinimizeToTrayEnabled.IsChecked.GetValueOrDefault();
            Properties.Settings.Default.EqualizerOnStartEnabled = EqualizerOnStartEnabled.IsChecked.GetValueOrDefault();

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

            foreach (KeyValuePair<string, string> prop in tempHotkeys) {
                if (prop.Key.EndsWith("Hotkey")) {
                    Properties.Settings.Default[prop.Key] = tempHotkeys[prop.Key];
                }
            }

            Properties.Settings.Default.Save();

            InfoSnackbar.MessageQueue?.Clear();
            InfoSnackbar.MessageQueue?.Enqueue("Saved!", null, null, null, false, true, TimeSpan.FromSeconds(1));
        }

        private void EditDownloadsFolder(object sender, RoutedEventArgs e) {
            var dialog = new WinForms.FolderBrowserDialog();
            dialog.InitialDirectory = DownloadsFolder.Text;

            var res = dialog.ShowDialog();

            if (res == WinForms.DialogResult.OK) {
                DownloadsFolder.Text = dialog.SelectedPath;
            }
        }

        string editedHotkey = "";
        Dictionary<string, string> tempHotkeys = new Dictionary<string, string>();

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) { 
            if (string.IsNullOrEmpty(editedHotkey)) {
                return;
            }

            if (!IsModifierKey(e.Key)) {
                var newHotkey = "";

                if (e.KeyboardDevice.Modifiers != ModifierKeys.None) {
                    newHotkey = e.KeyboardDevice.Modifiers + " + " + e.Key;
                }
                else {
                    newHotkey = e.Key.ToString();
                }

                if (tempHotkeys[editedHotkey] == newHotkey) {
                    editedHotkey = "";
                    e.Handled = true;
                    return;
                }

                bool hotkeyIsUsed = false;

                foreach (KeyValuePair<string, string> prop in tempHotkeys) {
                    if (prop.Key.EndsWith("Hotkey")) {
                        if (prop.Value == newHotkey) {
                            hotkeyIsUsed = true;

                            InfoSnackbar.MessageQueue?.Clear();
                            InfoSnackbar.MessageQueue?.Enqueue($"This one is already used by {prop.Key}, try another one", null, null, null, false, true, TimeSpan.FromSeconds(1));

                            break;
                        }
                    }
                }

                if (!hotkeyIsUsed) {
                    (FindName(editedHotkey) as TextBlock).Text = newHotkey;
                    tempHotkeys[editedHotkey] = newHotkey;
                    editedHotkey = "";
                }
            }

            e.Handled = true;
        }

        private void EditHotkey(object sender, RoutedEventArgs e) {
            string name = (sender as Button).Name;
            editedHotkey = name.Substring(0, name.Length - 3); // e.g. PlayPauseHotkeyBtn -> PlayPauseHotkey

            InfoSnackbar.MessageQueue?.Clear();
            InfoSnackbar.MessageQueue?.Enqueue("Press new key combination", null, null, null, false, true, TimeSpan.FromSeconds(1));
        }

        private bool IsModifierKey(Key key) {
            List<Key> modifierKeys = new List<Key> {
                Key.LeftCtrl, Key.RightCtrl,
                Key.LeftAlt, Key.RightAlt,
                Key.LeftShift, Key.RightShift,
                Key.LWin, Key.RWin,
                Key.System
            };

            return modifierKeys.Contains(key);
        }

        void PopulateHotkeyStackPanel() {
            string[] hotkeys = { // names of hotkeys
                "Play/Pause",
                "Next Song",
                "Previous Song",
                "Increase Main Volume",
                "Decrease Main Volume"
            };

            foreach (var hk in hotkeys) {
                string name = Regex.Replace(hk, @"[^a-zA-Z0-9_]", ""); // e.g. Play/Pause -> PlayPause

                Grid grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(200) });
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(40) });

                TextBlock tb1 = new TextBlock() { Text = hk, Style = (Style)TabControl.FindResource(typeof(TextBlock)) };
                Grid.SetColumn(tb1, 0);
                grid.Children.Add(tb1);

                TextBlock tb2 = new TextBlock() { Name = name + "Hotkey", Style = (Style)TabControl.FindResource(typeof(TextBlock)), TextAlignment = TextAlignment.Center };
                tb2.Text = Properties.Settings.Default[name + "Hotkey"].ToString();
                tempHotkeys[name + "Hotkey"] = tb2.Text;
                Grid.SetColumn(tb2, 1);
                grid.Children.Add(tb2);
                HotkeyStackPanel.RegisterName(tb2.Name, tb2);

                Button btn = new Button() { Name = name + "HotkeyBtn", Style = (Style)FindResource("NoStylingButton") };
                btn.Click += EditHotkey;

                PackIcon packIcon = new PackIcon() { Kind = PackIconKind.PencilOutline, Foreground = Brushes.White};
                btn.Content = packIcon;

                Grid.SetColumn(btn, 2);
                grid.Children.Add(btn);
                HotkeyStackPanel.RegisterName(btn.Name, btn);

                HotkeyStackPanel.Children.Add(grid);
            }
        }

        private void Window_Closing(object sender, EventArgs e) {

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
