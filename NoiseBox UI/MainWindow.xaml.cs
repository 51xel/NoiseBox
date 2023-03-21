using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NoiseBox.Log;
using NoiseBox;
using NoiseBox_UI.View.UserControls;
using System.Windows.Threading;
using System.IO;
using System.Threading;

namespace NoiseBox_UI {
    public partial class MainWindow : Window {
        public AudioStreamControl AudioStreamControl;
        DispatcherTimer SeekBarTimer = new DispatcherTimer();

        public Playlist? SelectedPlaylist = MusicLibrary.GetPlaylists().FirstOrDefault();

        public MainWindow() {
            InitializeComponent();
            DataContext = this;
            WinMax.DoSourceInitialized(this);

            AudioStreamControl = new AudioStreamControl(DeviceControll.GetOutputDeviceNameById(0));

            DisplayPlaylists();
            DisplaySelectedPlaylist();

            BottomControlPanel.PlayPauseButton.Click += PlayPauseButton_Click;
            BottomControlPanel.SeekBar.PreviewMouseLeftButtonUp += SeekBar_PreviewMouseLeftButtonUp;


            SongList.ClickRowElement += (s, e) => {
                var path = (((s as Button).Content as GridViewRowPresenter).Content as Song).Path;

                if (!File.Exists(path)) {
                    MessageBox.Show("File does not exist", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else {
                    AudioStreamControl.MainMusic.PathToMusic = path;

                    AudioStreamControl.MainMusic.StopAndPlayFromPosition(0);
                    SeekBarTimer.Start();

                    AudioStreamControl.MainMusic.MusicVolume = 0.1f; 

                    BottomControlPanel.State = BottomControlPanel.ButtonState.Playing;
                }
            };

            PlaylistList.ClickRowElement += (s, e) => { SelectPlaylistByName((((s as Button).Content as ContentPresenter).Content as Playlist).Name.ToString()); };

            SeekBarTimer.Interval = TimeSpan.FromMilliseconds(50);
            SeekBarTimer.Tick += timer_Tick;
        }

        private void SeekBar_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var posInSeekBar = (BottomControlPanel.SeekBar.Value * AudioStreamControl.MainMusic.CurrentTrackLength) / 100;

            if (AudioStreamControl.MainMusic.PathToMusic != null && AudioStreamControl.MainMusic.CurrentTrackPosition != posInSeekBar && !AudioStreamControl.MainMusic.IsPaused) {
                AudioStreamControl.MainMusic.StopAndPlayFromPosition(posInSeekBar);
            }
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

        private void timer_Tick(object sender, EventArgs e) {
            if (!(BottomControlPanel.SeekBar.IsMouseOver && Mouse.LeftButton == MouseButtonState.Pressed)) {
                BottomControlPanel.SeekBar.Value = (AudioStreamControl.MainMusic.CurrentTrackPosition * 100) / AudioStreamControl.MainMusic.CurrentTrackLength;
            }
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e) {
            if (AudioStreamControl.MainMusic.PathToMusic != null) {
                if (BottomControlPanel.State == BottomControlPanel.ButtonState.Paused) {
                    BottomControlPanel.State = BottomControlPanel.ButtonState.Playing;

                    AudioStreamControl.MainMusic.StopAndPlayFromPosition((BottomControlPanel.SeekBar.Value * AudioStreamControl.MainMusic.CurrentTrackLength) / 100);

                    SeekBarTimer.Start();
                }
                else {
                    BottomControlPanel.State = BottomControlPanel.ButtonState.Paused;

                    AudioStreamControl.MainMusic.Pause();
                    SeekBarTimer.Stop();
                }
            }
        }

        private void DisplayPlaylists() {
            var playlists = MusicLibrary.GetPlaylists();

            foreach (var p in playlists) {
                PlaylistList.List.Items.Add(p);
            }
        }

        public void SelectPlaylistByName(string name) {
            foreach (var playlist in MusicLibrary.GetPlaylists()) {
                if (playlist.Name == name) {
                    SelectedPlaylist = playlist;

                    DisplaySelectedPlaylist();

                    break;
                }
            }
        }

        private void DisplaySelectedPlaylist() {
            if (SelectedPlaylist != null) {
                PlaylistText.CurrentPlaylistName.Text = SelectedPlaylist.Name;

                var songs = MusicLibrary.GetSongsFromPlaylist(SelectedPlaylist.Name);
                SongList.List.Items.Clear();

                foreach (var song in songs) {
                    SongList.List.Items.Add(song);
                }
            }
        }

        public void RenameSelectedPlaylist(string newName) {
            if (SelectedPlaylist != null) {
                SelectedPlaylist.Name = newName;
                PlaylistText.CurrentPlaylistName.Text = newName;
            }
        }
    }
}
