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

namespace NoiseBox_UI.View.UserControls {
    public partial class FunctionButtons : UserControl {
        public FunctionButtons() {
            InitializeComponent();
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e) {
            DownloadsWindow win = new DownloadsWindow();
            win.Owner = Window.GetWindow(this);
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            win.Show();
        }
    }
}
