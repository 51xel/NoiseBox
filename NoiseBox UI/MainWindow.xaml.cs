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

        public MainWindow() {
            InitializeComponent();
            DataContext = this;
            WinMax.DoSourceInitialized(this);

            DisplayPlaylists();

            BottomControlPanel.PlayPauseButton.Click += PlayPauseButton_Click;

            SongList.ClickRowElement += (s, e) => MessageBox.Show(((s as ListViewItem).Content as Song).Name.ToString());
            PlaylistList.ClickRowElement += (s, e) => MessageBox.Show(((s as Button).Content as ContentPresenter).Content.ToString());
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

        public class Song {
            public int Id { get; set; }
            public string Name { get; set; }
            public string PathToFile { get; set; }
            public string Duration { get; set; }
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e) {
            SongList.SongsOC.Add(new Song() { Id = SongList.SongsOC.Count * 100, Name = "In The End " + SongList.SongsOC.Count, PathToFile = @"D:\Music\Linkin Park", Duration = "3:36" });

            if (BottomControlPanel.State == View.UserControls.BottomControlPanel.ButtonState.Paused) {
                BottomControlPanel.State = View.UserControls.BottomControlPanel.ButtonState.Playing;
            }
            else {
                BottomControlPanel.State = View.UserControls.BottomControlPanel.ButtonState.Paused;
            }
        }

        private void DisplayPlaylists() {
            IEnumerable<Playlist> playlists = MusicLibrary.GetPlaylists();

            foreach (var p in playlists) {
                PlaylistList.List.Items.Add(p.Name);
            }
        }
    }
}
