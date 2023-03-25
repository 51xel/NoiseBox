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

                            foreach (var btn in Helper.FindVisualChildren<Button>(List)) {
                                if (((btn.Content as ContentPresenter).Content as Playlist).Name == target) {
                                    btn.FontWeight = FontWeights.ExtraBold;
                                }
                                else {
                                    btn.FontWeight = FontWeights.DemiBold;
                                }
                            }
                        }
                    }
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                var songToAdd = new Song() { Path = files[0] };

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

            if (!MusicLibrary.RenamePlaylist(_oldTextBoxText, textBoxText)) {
                textBox.Text = _oldTextBoxText;
            }
            else {
                textBox.Text = textBoxText;
                (textBox.DataContext as Playlist).Name = textBoxText;

                var win = (MainWindow)Window.GetWindow(this);

                if (win.SelectedPlaylist != null) {
                    if (win.SelectedPlaylist.Name == _oldTextBoxText) {
                        win.RenameSelectedPlaylist(textBoxText);
                    }
                }
            }

            List.Items.Refresh(); // list item goes to state before renaming without this line :)
        }
    }
}
