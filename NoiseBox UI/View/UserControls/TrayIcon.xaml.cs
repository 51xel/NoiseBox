using NoiseBox_UI.View.Windows;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NoiseBox_UI.View.UserControls {
    public partial class TrayIcon : UserControl {
        public TrayIcon() {
            InitializeComponent();
        }

        private void TaskbarIcon_Loaded(object sender, RoutedEventArgs e) {
            var win = Window.GetWindow(this);
            win.IsVisibleChanged += (_, _) => {
                TaskbarIconOpenButton.Content = win.Visibility != Visibility.Hidden ? "Minimize to Tray" : "Open NoiseBox";
            };
        }

        private void TaskbarIcon_Click(object sender, RoutedEventArgs e) {
            var win = Window.GetWindow(this);

            if (win.Visibility == Visibility.Hidden) {
                win.Visibility = Visibility.Visible;
                win.Activate();
            }
            else {
                if (win.WindowState == WindowState.Minimized) {
                    win.WindowState = WindowState.Normal;
                    win.Activate();
                }
                else {
                    win.Hide();
                }
            }
        }

        private void TaskbarIconCloseButton_Click(object sender, RoutedEventArgs e) {
            TaskbarIcon.Dispose();

            var win = Window.GetWindow(this) as MainWindow;
            win.Window_Closed(null, null); // saves settings
            win.Close();

            Environment.Exit(0);
        }
    }
}
