using System;
using System.Collections.Generic;
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
        public RoutedEventHandler ClickRowElement;

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e) {
            ListView listView = sender as ListView;
            GridView gridView = listView.View as GridView;

            var workingWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth; // take into account vertical scrollbar
            workingWidth -= (gridView.Columns.First().Width + gridView.Columns.Last().Width);

            double ratio = 1.0 / (gridView.Columns.Count() - 2); // even width between all cols except first and last

            for (int i = 1; i < gridView.Columns.Count() - 1; i++) {
                gridView.Columns[i].Width = workingWidth * ratio;
            }
        }

        public SongList() {
            InitializeComponent();
        }
    }
}
