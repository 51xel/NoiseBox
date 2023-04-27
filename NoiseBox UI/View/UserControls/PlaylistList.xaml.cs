using NoiseBox;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NoiseBox_UI.View.Windows;
using NoiseBox_UI.Utils;
using Microsoft.Win32;

namespace NoiseBox_UI.View.UserControls
{
    public partial class PlaylistList : UserControl
    {
        public RoutedEventHandler ClickRowElement;
        private string _oldTextBoxText;

        public PlaylistList() {
            InitializeComponent();
        }

        private void ListViewItem_Drop(object sender, DragEventArgs e) {
            var droppedData = e.Data.GetData(typeof(Song)) as Song;
            var target = (((ListViewItem)(sender)).DataContext as Playlist).Name;

            var win = (MainWindow)Window.GetWindow(this);

            if (droppedData != null && target != null) {
                if (target != win.SelectedPlaylist.Name) {
                    MusicLibrary.AddSongToPlaylist(droppedData.Id, target.ToString());
                    MusicLibrary.RemoveSongFromPlaylist(droppedData.Id, win.SelectedPlaylist.Name);

                    int removedIdx = win.SongList.List.Items.IndexOf(droppedData);
                    win.SongList.List.Items.RemoveAt(removedIdx);

                    if (win.SelectedSong != null) {
                        if (droppedData.Id == win.SelectedSong.Id) {
                            win.BackgroundPlaylistName = target;

                            foreach (var btn in Helper.FindVisualChildren<Button>(List)) { // outline background playlist
                                if (((btn.Content as ContentPresenter).Content as Playlist).Name == target) {
                                    btn.FontWeight = FontWeights.ExtraBold;
                                }
                                else {
                                    btn.FontWeight = FontWeights.Normal;
                                }
                            }
                        }
                    }
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                List<string> mp3Files = Helper.GetAllMp3Files(files);

                foreach (var mp3File in mp3Files) {
                    var songToAdd = new Song() { Path = mp3File };

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

            var button = Helper.FindVisualChildren<Button>(sender as ListViewItem).First();

            button.BorderBrush = new SolidColorBrush(Colors.Transparent);
            button.BorderThickness = new Thickness(0);
        }

        private void ListViewItem_PreviewDragEnter(object sender, DragEventArgs e) {
            var button = Helper.FindVisualChildren<Button>(sender as ListViewItem).First();

            button.BorderBrush = new SolidColorBrush(Colors.White) { Opacity = 0.4 };
            button.BorderThickness = new Thickness(2);
        }

        private void ListViewItem_PreviewDragLeave(object sender, DragEventArgs e) {
            var button = Helper.FindVisualChildren<Button>(sender as ListViewItem).First();

            button.BorderBrush = new SolidColorBrush(Colors.Transparent);
            button.BorderThickness = new Thickness(0);
        }

        private void TextBox_PreviewDragOver(object sender, DragEventArgs e) {
            e.Effects = DragDropEffects.Move;
            e.Handled = true; // allows objects to be dropped on TextBox
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null) {
                Button button = ((ContextMenu)menuItem.Parent).PlacementTarget as Button;

                var win = (MainWindow)Window.GetWindow(this);

                if (Equals(menuItem.Header, "Remove playlist")) {

                    if (win.SelectedSong != null) {
                        if (MusicLibrary.GetSongsFromPlaylist((menuItem.DataContext as Playlist).Name).FindIndex(item => item.Id == win.SelectedSong.Id) != -1) {
                            win.SelectedSongRemoved();
                        }
                    }

                    MusicLibrary.RemovePlaylist((menuItem.DataContext as Playlist).Name);
                    List.Items.Remove(menuItem.DataContext as Playlist);

                    win.PlaylistText.CurrentPlaylistName.Text = "Playlist not selected";
                    win.SongList.List.Items.Clear();
                    win.SelectedPlaylist = null;
                }
                else if (Equals(menuItem.Header, "Rename")) {
                    var buttonCP = button.Content as ContentPresenter;

                    var textBox = Helper.FindVisualChildren<TextBox>(buttonCP).First(); // we only have 1 textbox in button

                    textBox.IsReadOnly = false;
                    textBox.Cursor = Cursors.IBeam;
                    textBox.SelectAll();

                    textBox.Focusable = true;
                    textBox.Focus();

                    //textBox.FontWeight = FontWeights.ExtraBold;

                    _oldTextBoxText = textBox.Text;
                }
                else if (Equals(menuItem.Header, "Add song(s)")) {
                    var openFileDialog = new OpenFileDialog();
                    openFileDialog.Multiselect = true;
                    openFileDialog.Title = "Select mp3 file(s)";
                    openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3";

                    if (openFileDialog.ShowDialog() == true) {
                        string[] files = openFileDialog.FileNames;

                        List<string> mp3Files = Helper.GetAllMp3Files(files);

                        foreach (var mp3File in mp3Files) {
                            var songToAdd = new Song() { Path = mp3File };

                            string playlistName = (menuItem.DataContext as Playlist).Name;

                            if (MusicLibrary.AddSong(songToAdd)) {
                                MusicLibrary.AddSongToPlaylist(songToAdd.Id, playlistName);

                                if (win.SelectedPlaylist == null) {
                                    win.SelectPlaylistByName(playlistName);
                                }
                                else if (win.SelectedPlaylist.Name == playlistName) {
                                    win.SongList.List.Items.Add(songToAdd);
                                }
                                else {
                                    win.SelectPlaylistByName(playlistName);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                SetTextBoxToDefaultAndSaveText(sender);
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e) {
            SetTextBoxToDefaultAndSaveText(sender);
        }

        private void SetTextBoxToDefaultAndSaveText(object sender) {
            var textBox = sender as TextBox;

            textBox.IsReadOnly = true;
            textBox.Cursor = Cursors.Hand;

            textBox.SelectionLength = 0;

            textBox.Focusable = false;

            //textBox.FontWeight = FontWeights.DemiBold;

            var textBoxText = textBox.Text.Trim();

            var win = (MainWindow)Window.GetWindow(this);

            if (!MusicLibrary.RenamePlaylist(_oldTextBoxText, textBoxText)) {
                textBox.Text = _oldTextBoxText;
            }
            else {
                textBox.Text = textBoxText;
                (textBox.DataContext as Playlist).Name = textBoxText;

                if (win.SelectedPlaylist != null) {
                    if (win.SelectedPlaylist.Name == _oldTextBoxText) {
                        win.RenameSelectedPlaylist(textBoxText);
                    }
                }

                if (win.BackgroundPlaylistName != null) {
                    if (win.BackgroundPlaylistName == _oldTextBoxText) {
                        win.BackgroundPlaylistName = textBoxText;
                    }
                }
            }

            List.Items.Refresh(); // list item goes to state before renaming without this line :)

            OutlineBackgroundPlaylist(textBox.Text);
        }

        private async Task OutlineBackgroundPlaylist(string textBoxText) {
            var win = (MainWindow)Window.GetWindow(this);

            await Task.Delay(10);

            if (win.BackgroundPlaylistName != null) { // background playlist outlining was lost after refresh 
                if (win.BackgroundPlaylistName == textBoxText) {
                    foreach (var btn in Helper.FindVisualChildren<Button>(List)) {
                        if (((btn.Content as ContentPresenter).Content as Playlist).Name == textBoxText) {
                            btn.FontWeight = FontWeights.ExtraBold;
                        }
                    }
                }
            }
        }
    }
}
