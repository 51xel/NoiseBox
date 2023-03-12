using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
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
    public partial class TitlebarButtons : UserControl {
        public TitlebarButtons() {
            InitializeComponent();
        }

        private void ButtonMouseEnter(object sender, MouseEventArgs e) {
            ((sender as Button).Content as Image).Opacity = 1;
        }

        private void ButtonMouseLeave(object sender, MouseEventArgs e) {
            ((sender as Button).Content as Image).Opacity = 0.6;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) {
            MainWindow win = (MainWindow)Window.GetWindow(this);
            win.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e) {
            MainWindow win = (MainWindow)Window.GetWindow(this);
            win.WindowState = win.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) {
            MainWindow win = (MainWindow)Window.GetWindow(this);
            win.Close();
        }
    }
}