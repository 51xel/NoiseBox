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

            if (droppedData != null) {
                MessageBox.Show(droppedData.Name + " to " + target);
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                MessageBox.Show(String.Join(" ", files) + $" dropped into {target} playlist");
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null) {
                Button button = ((ContextMenu)menuItem.Parent).PlacementTarget as Button;

                if (Equals(menuItem.Header, "Remove playlist")) {
                    string pName = (button.Content as ContentPresenter).Content.ToString();
                    List.Items.Remove(pName);
                    MusicLibrary.RemovePlaylist(pName);

                    ((MainWindow)Window.GetWindow(this)).PlaylistText.CurrentPlaylistName.Text = "Playlist not selected";
                    ((MainWindow)Window.GetWindow(this)).SongList.List.Items.Clear();
                }
            }
        }
    }
}
