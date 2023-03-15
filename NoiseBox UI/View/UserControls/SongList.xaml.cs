using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
using NoiseBox;

namespace NoiseBox_UI.View.UserControls {

    public partial class SongList : UserControl {

        public SongList() {
            DataContext = this;
            InitializeComponent();
        }

        public RoutedEventHandler ClickRowElement;
        private bool _isDragging = false;
        private Point _startPoint;
        private string _oldTextBoxText;

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e) {
            var listView = sender as ListView;
            var gridView = listView.View as GridView;

            var workingWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth; // take into account vertical scrollbar
            workingWidth -= gridView.Columns.Last().Width;

            gridView.Columns[1].Width = workingWidth * 0.4;
            gridView.Columns[2].Width = workingWidth * 0.6;
        }

        private void StartDrag(object sender, MouseEventArgs e) {
            _isDragging = true;

            var draggedItem = sender as ListViewItem;
            if (draggedItem != null) {
                DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
            }

            _isDragging = false;
        }

        private void ListViewItem_PreviewMouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed && !_isDragging) {
                Point position = e.GetPosition(null);

                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                        Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance) {
                    StartDrag(sender, e);
                }
            }
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            _startPoint = e.GetPosition(null);
        }

        private void ListViewItem_Drop(object sender, DragEventArgs e) {
            var droppedData = e.Data.GetData(typeof(Song)) as Song;
            var target = ((ListViewItem)(sender)).DataContext as Song;

            if (droppedData != null && target != null) {
                int removedIdx = List.Items.IndexOf(droppedData);
                int targetIdx = List.Items.IndexOf(target);

                if (removedIdx != targetIdx) {
                    List.Items.RemoveAt(removedIdx);
                    List.Items.Insert(targetIdx, droppedData);

                    MusicLibrary.RemoveSongFromPlaylist(droppedData.Id, ((MainWindow)Window.GetWindow(this)).SelectedPlaylist.Name);
                    MusicLibrary.AddSongToPlaylist(droppedData.Id, ((MainWindow)Window.GetWindow(this)).SelectedPlaylist.Name, targetIdx);
                }
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                int targetIdx = List.Items.IndexOf(target);

                var songToAdd = new Song() {Path = files[0]};

                if (MusicLibrary.AddSong(songToAdd)) {
                    MusicLibrary.AddSongToPlaylist(songToAdd.Id, ((MainWindow)Window.GetWindow(this)).SelectedPlaylist.Name, targetIdx);
                    List.Items.Insert(targetIdx, songToAdd);
                }
            }

            e.Handled = true; // prevents ListView_Drop from being raised
        }

        private void TextBox_PreviewDragOver(object sender, DragEventArgs e) {
            e.Handled = true; // allows objects to be dropped on TextBox
        }

        private void ListView_Drop(object sender, DragEventArgs e) {
            Playlist selectedPlaylist = ((MainWindow)Window.GetWindow(this)).SelectedPlaylist;

            if (e.Data.GetDataPresent(DataFormats.FileDrop) && selectedPlaylist != null) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                var songToAdd = new Song() { Path = files[0] };

                if (MusicLibrary.AddSong(songToAdd)) {
                    MusicLibrary.AddSongToPlaylist(songToAdd.Id, selectedPlaylist.Name);
                    List.Items.Add(songToAdd);
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null) {
                var win = (MainWindow)Window.GetWindow(this);

                if (Equals(menuItem.Header, "Remove from playlist")) {
                    MusicLibrary.RemoveSongFromPlaylist((menuItem.DataContext as Song).Id, win.SelectedPlaylist.Name);
                    MusicLibrary.RemoveSong((menuItem.DataContext as Song).Id);

                    List.Items.Remove(menuItem.DataContext as Song);
                }
                else if (Equals(menuItem.Header, "Rename")) {
                    var button = ((ContextMenu)menuItem.Parent).PlacementTarget as Button;
                    var buttonGVRP = button.Content as GridViewRowPresenter;

                    var textBox = FindVisualChildren<TextBox>(buttonGVRP).First(); // we only have 1 textbox in button

                    textBox.IsReadOnly = false;
                    textBox.Cursor = Cursors.IBeam;
                    textBox.SelectAll();

                    textBox.Focusable = true;
                    textBox.Focus();

                    textBox.FontWeight = FontWeights.Bold;

                    _oldTextBoxText = textBox.Text;

                    _isDragging = true; // prevents dragging while entering and allows text selection with mouse
                }
            }
        }

        private void SetTextBoxToDefaultAndSaveText(object sender) {
            var textBox = sender as TextBox;

            textBox.IsReadOnly = true;
            textBox.Cursor = Cursors.Hand;

            textBox.Focusable = false;

            textBox.FontWeight = FontWeights.Normal;

            var textBoxText = textBox.Text.Trim();

            if (textBoxText != _oldTextBoxText) {
                if (!MusicLibrary.RenameSong((textBox.DataContext as Song).Id, textBoxText)) {
                    textBox.Text = _oldTextBoxText;
                }
                else {
                    textBox.Text = textBoxText;
                    (textBox.DataContext as Song).Name = textBoxText;
                }
            }
            else {
                textBox.Text = _oldTextBoxText;
            }

            _isDragging = false;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                SetTextBoxToDefaultAndSaveText(sender);
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e) {
            SetTextBoxToDefaultAndSaveText(sender);
        }

        private IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject {
            if (depObj != null) {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++) {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                    if (child != null && child is T)
                        yield return (T)child;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }
    }

    public class DurationConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var ts = (TimeSpan)value;

            var output = string.Format("{0}:{1}", (int)ts.TotalMinutes, ts.Seconds.ToString("D2"));

            return output;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return Binding.DoNothing;
        }
    }
}
