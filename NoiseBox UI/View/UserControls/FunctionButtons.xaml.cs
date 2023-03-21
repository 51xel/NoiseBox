using Microsoft.Win32;
using NoiseBox;
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
using System.Windows.Navigation;
using System.IO;

namespace NoiseBox_UI.View.UserControls {
    public partial class FunctionButtons : UserControl {
        public FunctionButtons() {
            InitializeComponent();
        }

        private bool _isDownloadsWindowOpen = false;
        private DownloadsWindow _downloadsWin;

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
            _isDownloadsWindowOpen = true;

            _downloadsWin.Show();
        }

        private async void ConvertButton_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All Supported Formats (*.wav;*.aiff;*.flac;*.ogg;*.aac;*.wma;*.m4a;*.ac3;*.amr;*.mp2;*.avi;*.mpeg;*.wmv;*.mp4;*.mov;*.flv;*.mkv;*.3gp;*.asf;*.gxf;*.m2ts;*.ts;*.mxf;*.ogv)|*.wav;*.aiff;*.flac;*.ogg;*.aac;*.wma;*.m4a;*.ac3;*.amr;*.mp2;*.avi;*.mpeg;*.wmv;*.mp4;*.mov;*.flv;*.mkv;*.3gp;*.asf;*.gxf;*.m2ts;*.ts;*.mxf;*.ogv";

            if (openFileDialog.ShowDialog() == true) {
                ConvertingProgress.Visibility = Visibility.Visible;

                var fileName = openFileDialog.FileNames[0];

                var directory = System.AppContext.BaseDirectory.Split(Path.DirectorySeparatorChar);
                var slice = new ArraySegment<string>(directory, 0, directory.Length - 4);
                var BinariesDirPath = Path.Combine(Path.Combine(slice.ToArray()), "Binaries");
                var ffmpegLocation = Path.Combine(BinariesDirPath, @"ffmpeg\bin");

                await MusicLibrary.ConvertToMp3(fileName, ffmpegLocation);

                fileName = Path.ChangeExtension(fileName, ".mp3");

                if (File.Exists(fileName)) {
                    Song song = new Song { Path = fileName };

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
                    MessageBox.Show("Error while converting");
                }

                ConvertingProgress.Visibility = Visibility.Collapsed;
            }
        }
    }
}
