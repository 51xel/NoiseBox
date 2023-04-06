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
using NAudio.Wave;
using NAudio.Gui;
using System.Windows.Media.Animation;

namespace NoiseBox_UI {
    public partial class MainWindow : Window {
        public AudioStreamControl AudioStreamControl;
        DispatcherTimer SeekBarTimer = new DispatcherTimer();

        public Playlist? SelectedPlaylist;
        public Song? SelectedSong = null;
        public string? BackgroundPlaylistName = null;

        public bool VisualizationEnabled = Properties.Settings.Default.VisualizationEnabled;
        private string? CurrentlyVisualizedPath = null;

        public BandsSettings SelectedBandsSettings = null;

        public MainWindow() {
            InitializeComponent();
            DataContext = this;
            WinMax.DoSourceInitialized(this);

            AudioStreamControl = new AudioStreamControl(DeviceControll.GetOutputDeviceNameById(0));
            AudioStreamControl.MainMusic.StoppedEvent += Music_StoppedEvent;

            DisplayPlaylists();

            BottomControlPanel.PlayPauseButton.Click += PlayPauseButton_Click;
            BottomControlPanel.PrevButton.Click += PrevButton_Click;
            BottomControlPanel.NextButton.Click += NextButton_Click;
            BottomControlPanel.SeekBar.PreviewMouseLeftButtonUp += SeekBar_PreviewMouseLeftButtonUp;
            BottomControlPanel.SeekBar.ValueChanged += SeekBar_ValueChanged;

            BottomControlPanel.MicVolumeSlider.IsEnabled = Properties.Settings.Default.MicOutputEnabled;
            BottomControlPanel.MicVolumeButton.IsEnabled = Properties.Settings.Default.MicOutputEnabled;
            BottomControlPanel.VCVolumeSlider.IsEnabled = Properties.Settings.Default.VirtualCableOutputEnabled;
            BottomControlPanel.VCVolumeButton.IsEnabled = Properties.Settings.Default.VirtualCableOutputEnabled;

            BottomControlPanel.MainVolumeSlider.Value = Properties.Settings.Default.MainVolumeSliderValue;
            BottomControlPanel.MicVolumeSlider.Value = Properties.Settings.Default.MicVolumeSliderValue;
            BottomControlPanel.VCVolumeSlider.Value = Properties.Settings.Default.VCVolumeSliderValue;

            BottomControlPanel.MainVolumeSlider.ValueChanged += MainVolumeSlider_ValueChanged;

            SongList.ClickRowElement += Song_Click;

            PlaylistList.ClickRowElement += (s, e) => { SelectPlaylistByName((((s as Button).Content as ContentPresenter).Content as Playlist).Name.ToString()); };

            SeekBarTimer.Interval = TimeSpan.FromMilliseconds(50);
            SeekBarTimer.Tick += timer_Tick;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            var lastSelectedPlaylistName = Properties.Settings.Default.LastSelectedPlaylistName;

            if (!string.IsNullOrEmpty(lastSelectedPlaylistName)) {
                SelectedPlaylist = MusicLibrary.GetPlaylists().Find(p => p.Name == lastSelectedPlaylistName);

                if (SelectedPlaylist != null) {
                    SelectPlaylistByName(lastSelectedPlaylistName);
                }
            }

            var lastBackgroundPlaylistName = Properties.Settings.Default.LastBackgroundPlaylistName;
            var lastSelectedSongId = Properties.Settings.Default.LastSelectedSongId;

            if (!string.IsNullOrEmpty(lastBackgroundPlaylistName)) {
                var backgroundPlaylist = MusicLibrary.GetPlaylists().Find(p => p.Name == lastBackgroundPlaylistName);

                if (backgroundPlaylist != null) {
                    BackgroundPlaylistName = lastBackgroundPlaylistName;

                    foreach (var button in Helper.FindVisualChildren<Button>(PlaylistList.List)) {
                        if (((button.Content as ContentPresenter).Content as Playlist).Name == BackgroundPlaylistName) {
                            button.FontWeight = FontWeights.ExtraBold;
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(lastSelectedSongId)) {
                        SelectedSong = MusicLibrary.GetSongsFromPlaylist(BackgroundPlaylistName).Find(s => s.Id == lastSelectedSongId);

                        if (SelectedSong != null) {
                            SelectSong(SelectedSong);

                            PlayPauseButton_Click(null, null);

                            BottomControlPanel.SeekBar.Value = Properties.Settings.Default.LastSeekBarValue;
                        }
                    }
                }
            }

            BottomControlPanel.Mode = (BottomControlPanel.PlaybackMode)Properties.Settings.Default.LastPlaybackMode;
        }

        private void MainVolumeSlider_ValueChanged(object sender, EventArgs e) {
            AudioStreamControl.MainMusic.MusicVolume = (float)BottomControlPanel.MainVolumeSlider.Value / 100;
        }

        private void Music_StoppedEvent(object sender, EventArgs e) {
            if (Math.Abs(AudioStreamControl.MainMusic.CurrentTrackLength - AudioStreamControl.MainMusic.CurrentTrackPosition) <= 0.1) {
                BottomControlPanel.State = BottomControlPanel.ButtonState.Paused;
                SeekBarTimer.Stop();

                NextButton_Click(null, null);
            }
        }

        private void Song_Click(object sender, RoutedEventArgs e) {
            var idBefore = SelectedSong != null ? SelectedSong.Id : "";

            SelectSong(((sender as Button).Content as GridViewRowPresenter).Content as Song);

            var idAfter = SelectedSong != null ? SelectedSong.Id : "";

            if (idBefore != idAfter) { // outline background playlist
                BackgroundPlaylistName = SelectedPlaylist.Name;

                foreach (var button in Helper.FindVisualChildren<Button>(PlaylistList.List)) {
                    if (((button.Content as ContentPresenter).Content as Playlist).Name == SelectedPlaylist.Name) {
                        button.FontWeight = FontWeights.ExtraBold;
                    }
                    else {
                        button.FontWeight = FontWeights.Normal;
                    }
                }
            }
        }

        public void SelectSong(Song song) {
            if (!File.Exists(song.Path)) {
                MessageBox.Show("File does not exist", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else {
                SelectedSong = song.Clone();

                AudioStreamControl.MainMusic.PathToMusic = SelectedSong.Path;

                AudioStreamControl.MainMusic.StopAndPlayFromPosition(0);
                SeekBarTimer.Start();

                AudioStreamControl.MainMusic.MusicVolume = (float)BottomControlPanel.MainVolumeSlider.Value / 100;

                BottomControlPanel.State = BottomControlPanel.ButtonState.Playing;

                BottomControlPanel.CurrentSongName.Text = SelectedSong.Name;
                var ts = SelectedSong.Duration;
                BottomControlPanel.TotalTime.Text = string.Format("{0}:{1}", (int)ts.TotalMinutes, ts.Seconds.ToString("D2"));
                BottomControlPanel.CurrentTime.Text = "0:00";

                foreach (var button in Helper.FindVisualChildren<Button>(SongList.List)) { // outline selected song
                    if (((button.Content as GridViewRowPresenter).Content as Song).Id == SelectedSong.Id) {
                        button.FontWeight = FontWeights.ExtraBold;
                    }
                    else {
                        button.FontWeight = FontWeights.Normal;
                    }
                }

                if (VisualizationEnabled) {
                    if (CurrentlyVisualizedPath != SelectedSong.Path) {
                        CurrentlyVisualizedPath = SelectedSong.Path;

                        BottomControlPanel.VisualizeAudio(SelectedSong.Path);
                    }
                }
            }
        }

        private void SeekBar_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var posInSeekBar = (BottomControlPanel.SeekBar.Value * AudioStreamControl.MainMusic.CurrentTrackLength) / 100;

            if (AudioStreamControl.MainMusic.PathToMusic != null && AudioStreamControl.MainMusic.CurrentTrackPosition != posInSeekBar && !AudioStreamControl.MainMusic.IsPaused) {
                AudioStreamControl.MainMusic.StopAndPlayFromPosition(posInSeekBar);

                BottomControlPanel.State = BottomControlPanel.ButtonState.Playing;
                SeekBarTimer.Start();
            }
        }

        private void SeekBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (SelectedSong != null) {
                var posInSeekBar = (BottomControlPanel.SeekBar.Value * AudioStreamControl.MainMusic.CurrentTrackLength) / 100;
                var ts = TimeSpan.FromSeconds(posInSeekBar); 
                BottomControlPanel.CurrentTime.Text = string.Format("{0}:{1}", (int)ts.TotalMinutes, ts.Seconds.ToString("D2"));
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

        private void NextButton_Click(object sender, RoutedEventArgs e) {
            if (SelectedSong != null) {
                var selectedSongIndex = SongList.List.Items
                                        .Cast<Song>()
                                        .ToList()
                                        .FindIndex(item => item.Id == SelectedSong.Id);

                if (selectedSongIndex == -1) { // changed displayed playlist
                    switch (BottomControlPanel.Mode) {
                        case BottomControlPanel.PlaybackMode.Loop:

                            var backgroundSongs = MusicLibrary.GetSongsFromPlaylist(BackgroundPlaylistName);
                            selectedSongIndex = backgroundSongs.FindIndex(item => item.Id == SelectedSong.Id);

                            if (selectedSongIndex != -1) {
                                if (selectedSongIndex == backgroundSongs.Count - 1) {
                                    SelectSong(backgroundSongs[0] as Song);
                                }
                                else {
                                    SelectSong(backgroundSongs[selectedSongIndex + 1] as Song);
                                }
                            }

                            break;

                        case BottomControlPanel.PlaybackMode.Loop1:
                            SelectSong(SelectedSong);
                            break;

                        case BottomControlPanel.PlaybackMode.NoLoop:
                            break;
                    }

                    return;
                }

                switch (BottomControlPanel.Mode) {
                    case BottomControlPanel.PlaybackMode.Loop:
                        if (selectedSongIndex == SongList.List.Items.Count - 1) {
                            SelectSong(SongList.List.Items[0] as Song);
                        }
                        else {
                            SelectSong(SongList.List.Items[selectedSongIndex + 1] as Song);
                        }
                        break;

                    case BottomControlPanel.PlaybackMode.Loop1:
                        SelectSong(SelectedSong);
                        break;

                    case BottomControlPanel.PlaybackMode.NoLoop:
                        break;
                }
            }
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e) {
            if (SelectedSong != null) {
                var selectedSongIndex = SongList.List.Items
                                        .Cast<Song>()
                                        .ToList()
                                        .FindIndex(item => item.Id == SelectedSong.Id);

                if (selectedSongIndex == -1) { // changed displayed playlist
                    switch (BottomControlPanel.Mode) {
                        case BottomControlPanel.PlaybackMode.Loop:

                            var backgroundSongs = MusicLibrary.GetSongsFromPlaylist(BackgroundPlaylistName);
                            selectedSongIndex = backgroundSongs.FindIndex(item => item.Id == SelectedSong.Id);

                            if (selectedSongIndex != -1) {
                                if (selectedSongIndex == 0) {
                                    SelectSong(backgroundSongs[backgroundSongs.Count - 1] as Song);
                                }
                                else {
                                    SelectSong(backgroundSongs[selectedSongIndex - 1] as Song);
                                }
                            }

                            break;

                        case BottomControlPanel.PlaybackMode.Loop1:
                            SelectSong(SelectedSong);
                            break;

                        case BottomControlPanel.PlaybackMode.NoLoop:
                            SelectSong(SelectedSong);
                            break;
                    }

                    return;
                }

                switch (BottomControlPanel.Mode) {
                    case BottomControlPanel.PlaybackMode.Loop:
                        if (selectedSongIndex == 0) {
                            SelectSong(SongList.List.Items[SongList.List.Items.Count - 1] as Song);
                        }
                        else {
                            SelectSong(SongList.List.Items[selectedSongIndex - 1] as Song);
                        }
                        break;

                    case BottomControlPanel.PlaybackMode.Loop1:
                        SelectSong(SelectedSong);
                        break;

                    case BottomControlPanel.PlaybackMode.NoLoop:
                        SelectSong(SelectedSong);
                        break;
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

        private async Task DisplaySelectedPlaylist() {
            if (SelectedPlaylist != null) {
                PlaylistText.CurrentPlaylistName.Text = SelectedPlaylist.Name;

                var songs = MusicLibrary.GetSongsFromPlaylist(SelectedPlaylist.Name);
                SongList.List.Items.Clear();

                foreach (var song in songs) {
                    SongList.List.Items.Add(song);
                }

                await Task.Delay(10); // waiting till list is loaded and outline selected song
                if (SelectedSong != null) {
                    foreach (var button in Helper.FindVisualChildren<Button>(SongList.List)) {
                        if (((button.Content as GridViewRowPresenter).Content as Song).Id == SelectedSong.Id) {
                            button.FontWeight = FontWeights.ExtraBold;
                            break;
                        }
                    }
                }
            }
        }

        public async void SelectedSongRemoved() {
            if (SelectedSong != null) {
                AudioStreamControl.MainMusic.Stop();
                SelectedSong = null;
                BottomControlPanel.State = BottomControlPanel.ButtonState.Paused;
                SeekBarTimer.Stop();
                BottomControlPanel.CurrentSongName.Text = "Song not selected";
                BottomControlPanel.TotalTime.Text = "0:00";
                BottomControlPanel.CurrentTime.Text = "0:00";
                BottomControlPanel.SeekBar.Value = 0;
                
                foreach (var button in Helper.FindVisualChildren<Button>(PlaylistList.List)) { // remove outlining from playlist
                    if (((button.Content as ContentPresenter).Content as Playlist).Name == BackgroundPlaylistName) {
                        button.FontWeight = FontWeights.Normal;
                        break;
                    }
                }

                BackgroundPlaylistName = null;

                BottomControlPanel.Rendering = false;

                BottomControlPanel.ShowSeekBarHideBorders();

                BottomControlPanel.UniGrid.Children.Clear();
            }
        }

        public void RenameSelectedPlaylist(string newName) {
            if (SelectedPlaylist != null) {
                SelectedPlaylist.Name = newName;
                PlaylistText.CurrentPlaylistName.Text = newName;
            }
        }

        public void RenameSelectedSong(string newName) {
            if (SelectedSong != null) {
                SelectedSong.Name = newName;
                BottomControlPanel.CurrentSongName.Text = newName;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            Helper.FindVisualChildren<Grid>(this).FirstOrDefault().Focus();
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

        private void Window_Closed(object sender, EventArgs e) {
            Properties.Settings.Default.MainVolumeSliderValue = BottomControlPanel.MainVolumeSlider.Value;
            Properties.Settings.Default.MicVolumeSliderValue = BottomControlPanel.MicVolumeSlider.Value;
            Properties.Settings.Default.VCVolumeSliderValue = BottomControlPanel.VCVolumeSlider.Value;

            Properties.Settings.Default.LastSelectedPlaylistName = SelectedPlaylist != null ? SelectedPlaylist.Name : "";
            Properties.Settings.Default.LastBackgroundPlaylistName = BackgroundPlaylistName != null ? BackgroundPlaylistName : "";
            Properties.Settings.Default.LastSelectedSongId = SelectedSong != null ? SelectedSong.Id : "";

            Properties.Settings.Default.LastSeekBarValue = BottomControlPanel.SeekBar.Value;

            Properties.Settings.Default.LastPlaybackMode = (int)BottomControlPanel.Mode;

            Properties.Settings.Default.Save();
        }
    }
}
