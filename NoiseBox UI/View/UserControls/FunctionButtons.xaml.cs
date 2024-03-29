﻿using Microsoft.Win32;
using NoiseBox;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using NoiseBox_UI.View.Windows;

namespace NoiseBox_UI.View.UserControls {
    public partial class FunctionButtons : UserControl {
        public FunctionButtons() {
            InitializeComponent();
        }

        private bool _isSettingsWindowOpen = false;
        private SettingsWindow _settingsWin;

        private bool _isDownloadsWindowOpen = false;
        private DownloadsWindow _downloadsWin;
        
        private bool _isEqualizerWindowOpen = false;
        private CustomEqualizer _equalizerWin;

        private void SettingsButton_Click(object sender, RoutedEventArgs e) {
            if (_isSettingsWindowOpen) {
                if (_settingsWin.WindowState == WindowState.Minimized) {
                    _settingsWin.WindowState = WindowState.Normal;
                }
                return;
            }

            _settingsWin = new SettingsWindow();
            _settingsWin.Owner = Window.GetWindow(this);
            _settingsWin.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            _settingsWin.Closed += (_, _) => { _isSettingsWindowOpen = false; };
            _settingsWin.Closing += (_, _) => { _settingsWin.Owner = null; };
            _isSettingsWindowOpen = true;

            _settingsWin.Show();
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e) {
            if (_isDownloadsWindowOpen) {
                if (_downloadsWin.WindowState == WindowState.Minimized) {
                    _downloadsWin.WindowState = WindowState.Normal;
                }
                return;
            }

            _downloadsWin = new DownloadsWindow();
            _downloadsWin.Owner = Window.GetWindow(this);
            _downloadsWin.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            _downloadsWin.Closed += (_, _) => { _isDownloadsWindowOpen = false; };
            _downloadsWin.Closing += (_, _) => { _downloadsWin.Owner = null; };
            _isDownloadsWindowOpen = true;

            _downloadsWin.Show();
        }

        private async void ConvertButton_Click(object sender, RoutedEventArgs e) {

            OpenFileDialog openFileDialog = null;
            bool? fileDialogRes = null;

            ConvertButton.IsHitTestVisible = false;

            await Task.Run(() => { // because ShowDialog blocks animations
                openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "All Supported Formats (*.wav;*.aiff;*.flac;*.ogg;*.aac;*.wma;*.m4a;*.ac3;*.amr;*.mp2;*.avi;*.mpeg;*.wmv;*.mp4;*.mov;*.flv;*.mkv;*.3gp;*.asf;*.gxf;*.m2ts;*.ts;*.mxf;*.ogv)|*.wav;*.aiff;*.flac;*.ogg;*.aac;*.wma;*.m4a;*.ac3;*.amr;*.mp2;*.avi;*.mpeg;*.wmv;*.mp4;*.mov;*.flv;*.mkv;*.3gp;*.asf;*.gxf;*.m2ts;*.ts;*.mxf;*.ogv|All files (*.*)|*.*";
                openFileDialog.Multiselect = true;
                openFileDialog.Title = "Select file(s)";

                fileDialogRes = openFileDialog.ShowDialog();
            });

            ConvertButton.IsHitTestVisible = true;

            var BinariesDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries");
            var ffmpegLocation = Path.Combine(BinariesDirPath, @"ffmpeg\bin");

            if (fileDialogRes == true) {
                foreach (var fileName in openFileDialog.FileNames) {
                    ConvertingProgress.Visibility = Visibility.Visible;

                    await MusicLibrary.ConvertToMp3(fileName, ffmpegLocation);

                    var newFileName = Path.ChangeExtension(fileName, ".mp3");

                    if (File.Exists(newFileName)) {
                        Song song = new Song { Path = newFileName };

                        if (MusicLibrary.AddSong(song)) {
                            var win = (MainWindow)Window.GetWindow(this);

                            Playlist selectedPlaylist = win.SelectedPlaylist;

                            if (selectedPlaylist == null) {
                                selectedPlaylist = MusicLibrary.GetPlaylists().FirstOrDefault();

                                if (selectedPlaylist != null) {
                                    win.SelectPlaylistByName(selectedPlaylist.Name);

                                    MusicLibrary.AddSongToPlaylist(song.Id, selectedPlaylist.Name);
                                    win.SongList.List.Items.Add(song);
                                }
                            }
                            else {
                                MusicLibrary.AddSongToPlaylist(song.Id, selectedPlaylist.Name);
                                win.SongList.List.Items.Add(song);
                            }
                        }
                    }
                    else {
                        var win = (MainWindow)Window.GetWindow(this);
                        win.InfoSnackbar.MessageQueue?.Clear();
                        win.InfoSnackbar.MessageQueue?.Enqueue($"Error while converting {fileName}", null, null, null, false, true, TimeSpan.FromSeconds(2));
                    }

                    ConvertingProgress.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void EqualizerButton_Click(object sender, RoutedEventArgs e){
            if (_isEqualizerWindowOpen){
                if (_equalizerWin.WindowState == WindowState.Minimized){
                    _equalizerWin.WindowState = WindowState.Normal;
                }
                return;
            }

            _equalizerWin = new CustomEqualizer();
            _equalizerWin.Owner = Window.GetWindow(this);
            _equalizerWin.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            _equalizerWin.Closed += (_, _) => { _isEqualizerWindowOpen = false; };
            _equalizerWin.Closing += (_, _) => { _equalizerWin.Owner = null; };
            _isEqualizerWindowOpen = true;

            _equalizerWin.Show();

            var win = (MainWindow)Window.GetWindow(this);

            if (win.AudioStreamControl.MainMusic.IsEqualizerWorking) {
                _equalizerWin.StartStopText.Text = "Stop";
            }
            else {
                _equalizerWin.StartStopText.Text = "Start";
            }

            _equalizerWin.LoadSelectedBand(win.SelectedBandsSettings);

            if(_equalizerWin.StartStopText.Text == "Start") {
                _equalizerWin.ButtonsSetEnabledState(false);
                _equalizerWin.SliderSetEnabledState(false);
            }
        }
    }
}
