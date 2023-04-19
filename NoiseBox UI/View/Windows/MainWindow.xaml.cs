using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NoiseBox;
using NoiseBox_UI.View.UserControls;
using System.Windows.Threading;
using System.IO;
using NoiseBox_UI.Utils;
using System.Collections.Generic;

namespace NoiseBox_UI.View.Windows {
    public partial class MainWindow : Window {
        public AudioStreamControl AudioStreamControl;
        private DispatcherTimer SeekBarTimer = new DispatcherTimer();

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

            if (string.IsNullOrEmpty(Properties.Settings.Default.MainOutputDevice)) {
                Properties.Settings.Default.MainOutputDevice = DeviceControll.GetOutputDeviceNameById(0);
            }
            else if (!DeviceControll.GetOutputDevicesList().Contains(Properties.Settings.Default.MainOutputDevice)) {
                Properties.Settings.Default.MainOutputDevice = DeviceControll.GetOutputDeviceNameById(0);
            }

            if (string.IsNullOrEmpty(Properties.Settings.Default.AdditionalOutputDevice)) {
                foreach (string outputDevice in DeviceControll.GetOutputDevicesList()) {
                    if (outputDevice.Contains("virtual", StringComparison.OrdinalIgnoreCase)) {
                        Properties.Settings.Default.AdditionalOutputDevice = outputDevice;
                    }
                }
            }
            else if (!DeviceControll.GetOutputDevicesList().Contains(Properties.Settings.Default.AdditionalOutputDevice)) {
                Properties.Settings.Default.AdditionalOutputDevice = "";

                foreach (string outputDevice in DeviceControll.GetOutputDevicesList()) {
                    if (outputDevice.Contains("virtual", StringComparison.OrdinalIgnoreCase)) {
                        Properties.Settings.Default.AdditionalOutputDevice = outputDevice;
                    }
                }
            }

            AudioStreamControl = new AudioStreamControl(Properties.Settings.Default.MainOutputDevice);

            if (Properties.Settings.Default.AdditionalOutputEnabled && !string.IsNullOrEmpty(Properties.Settings.Default.AdditionalOutputDevice)) {
                AudioStreamControl.ActivateAdditionalMusic(Properties.Settings.Default.AdditionalOutputDevice);
            }

            AudioStreamControl.MainMusic.StoppedEvent += Music_StoppedEvent;

            DisplayPlaylists();

            BottomControlPanel.PlayPauseButton.Click += PlayPauseButton_Click;
            BottomControlPanel.PrevButton.Click += PrevButton_Click;
            BottomControlPanel.NextButton.Click += NextButton_Click;
            BottomControlPanel.SeekBar.PreviewMouseLeftButtonUp += SeekBar_PreviewMouseLeftButtonUp;
            BottomControlPanel.SeekBar.ValueChanged += SeekBar_ValueChanged;

            BottomControlPanel.MicVolumeSlider.IsEnabled = Properties.Settings.Default.MicOutputEnabled;
            BottomControlPanel.MicVolumeButton.IsEnabled = Properties.Settings.Default.MicOutputEnabled;
            BottomControlPanel.AdditionalVolumeSlider.IsEnabled = Properties.Settings.Default.AdditionalOutputEnabled;
            BottomControlPanel.AdditionalVolumeButton.IsEnabled = Properties.Settings.Default.AdditionalOutputEnabled;

            BottomControlPanel.MainVolumeSlider.Value = Properties.Settings.Default.MainVolumeSliderValue;
            BottomControlPanel.MicVolumeSlider.Value = Properties.Settings.Default.MicVolumeSliderValue;
            BottomControlPanel.AdditionalVolumeSlider.Value = Properties.Settings.Default.AdditionalVolumeSliderValue;

            AudioStreamControl.MainMusic.MusicVolume = (float)Properties.Settings.Default.MainVolumeSliderValue / 100;

            if (AudioStreamControl.AdditionalMusic != null) {
                AudioStreamControl.AdditionalMusic.MusicVolume = (float)Properties.Settings.Default.AdditionalVolumeSliderValue / 100;
            }
            if (AudioStreamControl.Microphone != null) {
                AudioStreamControl.Microphone.InputDeviceVolume = (float)Properties.Settings.Default.MicVolumeSliderValue / 100;
            }

            BottomControlPanel.MainVolumeSlider.ValueChanged += MainVolumeSlider_ValueChanged;
            BottomControlPanel.AdditionalVolumeSlider.ValueChanged += AdditionalVolumeSlider_ValueChanged;

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

                            AudioStreamControl.CurrentTrackPosition = AudioStreamControl.CurrentTrackLength * Properties.Settings.Default.LastSeekBarValue / 100;

                            BottomControlPanel.SeekBar.Value = Properties.Settings.Default.LastSeekBarValue;
                        }
                    }
                }
            }

            BottomControlPanel.Mode = (BottomControlPanel.PlaybackMode)Properties.Settings.Default.LastPlaybackMode;

            PreviewKeyDown += MainWindow_PreviewKeyDown;
        }

        private void MainVolumeSlider_ValueChanged(object sender, EventArgs e) {
            AudioStreamControl.MainMusic.MusicVolume = (float)BottomControlPanel.MainVolumeSlider.Value / 100;
        }

        private void AdditionalVolumeSlider_ValueChanged(object sender, EventArgs e) {
            AudioStreamControl.AdditionalMusic.MusicVolume = (float)BottomControlPanel.AdditionalVolumeSlider.Value / 100;
        }

        private void Music_StoppedEvent(object sender, EventArgs e) {
            if (Math.Abs(AudioStreamControl.CurrentTrackLength - AudioStreamControl.CurrentTrackPosition) <= 0.1) {
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
                InfoSnackbar.MessageQueue?.Clear();
                InfoSnackbar.MessageQueue?.Enqueue($"Song \"{song.Name}\" could not be found", null, null, null, false, true, TimeSpan.FromSeconds(2));
            }
            else {
                SelectedSong = song.Clone();

                AudioStreamControl.PathToMusic = SelectedSong.Path;

                AudioStreamControl.StopAndPlayFromPosition(0);
                SeekBarTimer.Start();

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

                StartVisualization();
            }
        }

        private void SeekBar_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var posInSeekBar = (BottomControlPanel.SeekBar.Value * AudioStreamControl.CurrentTrackLength) / 100;

            if (AudioStreamControl.PathToMusic != null && AudioStreamControl.CurrentTrackPosition != posInSeekBar && !AudioStreamControl.MainMusic.IsPaused) {
                AudioStreamControl.StopAndPlayFromPosition(posInSeekBar);

                BottomControlPanel.State = BottomControlPanel.ButtonState.Playing;
                SeekBarTimer.Start();
            }
        }

        private void SeekBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (SelectedSong != null) {
                var posInSeekBar = (BottomControlPanel.SeekBar.Value * AudioStreamControl.CurrentTrackLength) / 100;
                var ts = TimeSpan.FromSeconds(posInSeekBar); 
                BottomControlPanel.CurrentTime.Text = string.Format("{0}:{1}", (int)ts.TotalMinutes, ts.Seconds.ToString("D2"));
            }
        }

        private void timer_Tick(object sender, EventArgs e) {
            if (!(BottomControlPanel.SeekBar.IsMouseOver && Mouse.LeftButton == MouseButtonState.Pressed)) {
                BottomControlPanel.SeekBar.Value = (AudioStreamControl.CurrentTrackPosition * 100) / AudioStreamControl.CurrentTrackLength;
            }
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e) {
            if (SelectedSong != null) {
                if (BottomControlPanel.State == BottomControlPanel.ButtonState.Paused) {
                    BottomControlPanel.State = BottomControlPanel.ButtonState.Playing;

                    AudioStreamControl.StopAndPlayFromPosition((BottomControlPanel.SeekBar.Value * AudioStreamControl.CurrentTrackLength) / 100);

                    SeekBarTimer.Start();
                }
                else {
                    BottomControlPanel.State = BottomControlPanel.ButtonState.Paused;

                    AudioStreamControl.Pause();
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
                                    SelectWithSkipping(backgroundSongs[0] as Song, NextButton_Click);
                                }
                                else {
                                    SelectWithSkipping(backgroundSongs[selectedSongIndex + 1] as Song, NextButton_Click);
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
                            SelectWithSkipping(SongList.List.Items[0] as Song, NextButton_Click);
                        }
                        else {
                            SelectWithSkipping(SongList.List.Items[selectedSongIndex + 1] as Song, NextButton_Click);
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
                                    SelectWithSkipping(backgroundSongs[backgroundSongs.Count - 1] as Song, PrevButton_Click);
                                }
                                else {
                                    SelectWithSkipping(backgroundSongs[selectedSongIndex - 1] as Song, PrevButton_Click);
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
                            SelectWithSkipping(SongList.List.Items[SongList.List.Items.Count - 1] as Song, PrevButton_Click);
                        }
                        else {
                            SelectWithSkipping(SongList.List.Items[selectedSongIndex - 1] as Song, PrevButton_Click);
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

        private void SelectWithSkipping(Song song, Action<object, RoutedEventArgs> NextPrevButton_Click) { // skips if song doesn't exist
            if (!File.Exists(song.Path)) {
                InfoSnackbar.MessageQueue?.Clear();
                InfoSnackbar.MessageQueue?.Enqueue($"Song \"{song.Name}\" could not be found", null, null, null, false, true, TimeSpan.FromSeconds(2));
                SelectedSong = song.Clone();
                NextPrevButton_Click(null, null);
            }
            else {
                SelectSong(song);
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

        public void SelectedSongRemoved() {
            if (SelectedSong != null) {
                AudioStreamControl.Stop();
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

                SelectedSong = null;

                StopVisualization();
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

        public void StartVisualization() {
            if (VisualizationEnabled && SelectedSong != null) {
                if (CurrentlyVisualizedPath != SelectedSong.Path) {
                    CurrentlyVisualizedPath = SelectedSong.Path;

                    BottomControlPanel.VisualizeAudio(SelectedSong.Path);
                }
            }
        }

        public void StopVisualization() {
            BottomControlPanel.Rendering = false;
            BottomControlPanel.ShowSeekBarHideBorders();
            BottomControlPanel.UniGrid.Children.Clear();

            CurrentlyVisualizedPath = null;
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

        public void Window_Closed(object sender, EventArgs e) {
            Properties.Settings.Default.MainVolumeSliderValue = BottomControlPanel.MainVolumeSlider.Value;
            Properties.Settings.Default.MicVolumeSliderValue = BottomControlPanel.MicVolumeSlider.Value;
            Properties.Settings.Default.AdditionalVolumeSliderValue = BottomControlPanel.AdditionalVolumeSlider.Value;

            Properties.Settings.Default.LastSelectedPlaylistName = SelectedPlaylist != null ? SelectedPlaylist.Name : "";
            Properties.Settings.Default.LastBackgroundPlaylistName = BackgroundPlaylistName != null ? BackgroundPlaylistName : "";
            Properties.Settings.Default.LastSelectedSongId = SelectedSong != null ? SelectedSong.Id : "";

            Properties.Settings.Default.LastSeekBarValue = BottomControlPanel.SeekBar.Value;

            Properties.Settings.Default.LastPlaybackMode = (int)BottomControlPanel.Mode;

            Properties.Settings.Default.Save();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (Properties.Settings.Default.MinimizeToTrayEnabled) {
                e.Cancel = true;
                Hide();
            }
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            bool isVisible = (bool)e.NewValue;
            UpdateChildWindowVisibility(this, isVisible);
        }

        private void UpdateChildWindowVisibility(Window parentWindow, bool isVisible) {
            var childWindows = new List<Window>();

            foreach (Window window in Application.Current.Windows) {
                if (window != parentWindow && window.Owner == parentWindow) {
                    childWindows.Add(window);
                }
            }

            foreach (Window childWindow in childWindows) {
                childWindow.Visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
                UpdateChildWindowVisibility(childWindow, isVisible);
            }
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (!(Keyboard.FocusedElement is TextBox)) {
                var enteredHotkey = "";

                if (e.KeyboardDevice.Modifiers != ModifierKeys.None) {
                    enteredHotkey = e.KeyboardDevice.Modifiers + " + " + e.Key;
                }
                else {
                    enteredHotkey = e.Key.ToString();
                }

                if (enteredHotkey == Properties.Settings.Default["PlayPauseHotkey"].ToString()) {
                    PlayPauseButton_Click(null, null);
                    e.Handled = true;
                }
                else if (enteredHotkey == Properties.Settings.Default["NextSongHotkey"].ToString()) {
                    NextButton_Click(null, null);
                    e.Handled = true;
                }
                else if (enteredHotkey == Properties.Settings.Default["PreviousSongHotkey"].ToString()) {
                    PrevButton_Click(null, null);
                    e.Handled = true;
                }
                else if (enteredHotkey == Properties.Settings.Default["IncreaseMainVolumeHotkey"].ToString()) {
                    var val = BottomControlPanel.MainVolumeSlider.Value;
                    BottomControlPanel.MainVolumeSlider.Value = val + 5 > 100 ? 100 : val + 5;
                    e.Handled = true;
                }
                else if (enteredHotkey == Properties.Settings.Default["DecreaseMainVolumeHotkey"].ToString()) {
                    var val = BottomControlPanel.MainVolumeSlider.Value;
                    BottomControlPanel.MainVolumeSlider.Value = val - 5 < 0 ? 0 : val - 5;
                    e.Handled = true;
                }                
            }
        }
    }
}
