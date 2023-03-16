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

namespace NoiseBox_UI {
    public partial class MainWindow : Window {
        public Playlist? SelectedPlaylist = MusicLibrary.GetPlaylists().FirstOrDefault();

        public MainWindow() {
            InitializeComponent();
            DataContext = this;
            WinMax.DoSourceInitialized(this);

            DisplayPlaylists();
            DisplaySelectedPlaylist();

            BottomControlPanel.PlayPauseButton.Click += PlayPauseButton_Click;

            SongList.ClickRowElement += (s, e) => MessageBox.Show((((s as Button).Content as GridViewRowPresenter).Content as Song).Duration.ToString());

            PlaylistList.ClickRowElement += (s, e) => { SelectPlaylistByName((((s as Button).Content as ContentPresenter).Content as Playlist).Name.ToString()); };
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e) {
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

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e) {

            //SongList.List.Items.Add(new Song() { Id = "0", Name = "In The End " + SongList.List.Items.Count, Path = @"D:\Music\Linkin Park", Duration = TimeSpan.FromSeconds(123) });

            //if (BottomControlPanel.State == View.UserControls.BottomControlPanel.ButtonState.Paused) {
            //    BottomControlPanel.State = View.UserControls.BottomControlPanel.ButtonState.Playing;
            //}
            //else {
            //    BottomControlPanel.State = View.UserControls.BottomControlPanel.ButtonState.Paused;
            //}
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
