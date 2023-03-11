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

namespace NoiseBox_UI.View.UserControls
{
    public partial class PlaylistList : UserControl
    {
        public RoutedEventHandler ClickRowElement;

        public PlaylistList()
        {
            InitializeComponent();
        }

        private void RowElement_Drop(object sender, DragEventArgs e) {
            var droppedData = e.Data.GetData(typeof(MainWindow.Song)) as MainWindow.Song;
            var target = ((ListViewItem)(sender)).DataContext;

            MessageBox.Show(droppedData.Name + " to " + target);
        }
    }
}
