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
using System.Windows.Shapes;

namespace NoiseBox_UI {
    public partial class SettingsWindow : Window {
        public SettingsWindow() {
            InitializeComponent();
            DataContext = this;
            WinMax.DoSourceInitialized(this);
        }

        private void Window_StateChanged(object sender, EventArgs e) {
            if (WindowState == WindowState.Maximized) {
                Uri uri = new Uri("/Images/Icons/restore.png", UriKind.Relative);
                ImageSource imgSource = new BitmapImage(uri);
                TitlebarButtons.MaximizeButtonImage.Source = imgSource;
            }
            else if (WindowState == WindowState.Normal) {
                Uri uri = new Uri("/Images/Icons/maximize.png", UriKind.Relative);
                ImageSource imgSource = new BitmapImage(uri);
                TitlebarButtons.MaximizeButtonImage.Source = imgSource;
            }
        }

        private void Window_Closed(object sender, EventArgs e) {
            
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            Helper.FindVisualChildren<Grid>(this).FirstOrDefault().Focus();
        }
    }
}
