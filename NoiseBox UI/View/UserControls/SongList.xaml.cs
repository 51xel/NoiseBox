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

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e) {
            var listView = sender as ListView;
            var gridView = listView.View as GridView;

            var workingWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth; // take into account vertical scrollbar
            workingWidth -= gridView.Columns.Last().Width;

            gridView.Columns[1].Width = workingWidth * 0.4;
            gridView.Columns[2].Width = workingWidth * 0.6;
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var draggedItem = sender as ListViewItem;

            if (draggedItem != null) {
                DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
            }
        }

        private void ListViewItem_Drop(object sender, DragEventArgs e) {
            var droppedData = e.Data.GetData(typeof(Song)) as Song;
            var target = ((ListViewItem)(sender)).DataContext as Song;

            if (droppedData != null && target != null) {
                int removedIdx = List.Items.IndexOf(droppedData);
                int targetIdx = List.Items.IndexOf(target);

                if (removedIdx < targetIdx) {
                    List.Items.Insert(targetIdx + 1, droppedData);
                    List.Items.RemoveAt(removedIdx);

                    MusicLibrary.AddSongToPlaylist(droppedData.Id, ((MainWindow)Window.GetWindow(this)).SelectedPlaylist.Name, targetIdx + 1);
                    MusicLibrary.RemoveSongFromPlaylist(droppedData.Id, ((MainWindow)Window.GetWindow(this)).SelectedPlaylist.Name);
                }
                else if (removedIdx > targetIdx) {
                    int remIdx = removedIdx + 1;
                    if (List.Items.Count + 1 > remIdx) {
                        List.Items.Insert(targetIdx, droppedData);
                        List.Items.RemoveAt(remIdx);

                        MusicLibrary.AddSongToPlaylist(droppedData.Id, ((MainWindow)Window.GetWindow(this)).SelectedPlaylist.Name, targetIdx);
                        MusicLibrary.RemoveSongFromPlaylist(droppedData.Id, ((MainWindow)Window.GetWindow(this)).SelectedPlaylist.Name, remIdx);
                    }
                }
                else {
                    ClickRowElement?.Invoke(sender, null);
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
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null) {
                Button button = ((ContextMenu)menuItem.Parent).PlacementTarget as Button;

                MessageBox.Show($"{menuItem.Header} on {((button.Content as GridViewRowPresenter).Content as Song).Name}");
            }
        }
    }
}
