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
using System.Windows.Shapes;
using TagLib.Id3v2;

namespace NoiseBox_UI.View.UserControls
{
    public partial class PlaylistList : UserControl
    {
        public RoutedEventHandler ClickRowElement;

        public PlaylistList()
        {
            InitializeComponent();
        }

        private void ListViewItem_Drop(object sender, DragEventArgs e) {
            var droppedData = e.Data.GetData(typeof(Song)) as Song;
            var target = ((ListViewItem)(sender)).DataContext;

            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                var songToAdd = new Song() { Path = files[0] };

                var win = ((MainWindow)Window.GetWindow(this));

                if (MusicLibrary.AddSong(songToAdd)) {
                    MusicLibrary.AddSongToPlaylist(songToAdd.Id, target.ToString());

                    if (win.SelectedPlaylist == null) {
                        win.SelectPlaylistByName(target.ToString());
                    }
                    else if (win.SelectedPlaylist.Name == target.ToString()) {
                        win.SongList.List.Items.Add(songToAdd);
                    }
                    else {
                        win.SelectPlaylistByName(target.ToString());
                    }
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null) {
                Button button = ((ContextMenu)menuItem.Parent).PlacementTarget as Button;

                var win = ((MainWindow)Window.GetWindow(this));

                if (Equals(menuItem.Header, "Remove playlist")) {
                    string pName = (button.Content as ContentPresenter).Content.ToString();
                    List.Items.Remove(pName);
                    MusicLibrary.RemovePlaylist(pName);

                    win.PlaylistText.CurrentPlaylistName.Text = "Playlist not selected";
                    win.SongList.List.Items.Clear();
                    win.SelectedPlaylist = null;
                }
            }
        }
    }
}
