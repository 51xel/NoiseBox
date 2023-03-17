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

        private bool _isDownloadsWindowOpen = false;
        private DownloadsWindow _downloadsWin;

        private void DownloadButton_Click(object sender, RoutedEventArgs e) {
            if (_isDownloadsWindowOpen) {
                if (_downloadsWin.WindowState == WindowState.Minimized) {
                    _downloadsWin.WindowState = WindowState.Normal;
                }
                return;
            }

            _downloadsWin = new DownloadsWindow();
            _downloadsWin.Owner = Window.GetWindow(this);
            _downloadsWin.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            _downloadsWin.Closed += (_, _) => { _isDownloadsWindowOpen = false; };
            _isDownloadsWindowOpen = true;

            _downloadsWin.Show();
        }
    }
}
