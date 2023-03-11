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

namespace NoiseBox_UI.View.UserControls {

    public partial class SongList : UserControl {

        public SongList() {
            DataContext = this;
            InitializeComponent();
        }

        public RoutedEventHandler ClickRowElement;

        public ObservableCollection<MainWindow.Song> SongsOC { get; } = new ObservableCollection<MainWindow.Song>();

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
            var droppedData = e.Data.GetData(typeof(MainWindow.Song)) as MainWindow.Song;
            var target = ((ListViewItem)(sender)).DataContext as MainWindow.Song;

            if (droppedData != null && target != null) {
                int removedIdx = List.Items.IndexOf(droppedData);
                int targetIdx = List.Items.IndexOf(target);

                if (removedIdx < targetIdx) {
                    SongsOC.Insert(targetIdx + 1, droppedData);
                    SongsOC.RemoveAt(removedIdx);
                }
                else if (removedIdx > targetIdx) {
                    int remIdx = removedIdx + 1;
                    if (List.Items.Count + 1 > remIdx) {
                        SongsOC.Insert(targetIdx, droppedData);
                        SongsOC.RemoveAt(remIdx);
                    }
                }
                else {
                    ClickRowElement?.Invoke(sender, null);
                }
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                int targetIdx = List.Items.IndexOf(target);

                SongsOC.Insert(targetIdx, new MainWindow.Song() { Id = 0, Name = "Dropped file", PathToFile = files[0], Duration = "00:00" });
            }
        }
    }
}
