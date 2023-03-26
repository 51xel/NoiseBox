using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WinForms = System.Windows.Forms;
using NoiseBox;
using System.Windows.Controls;

namespace NoiseBox_UI{
    public partial class CustomEqualizer : Window, INotifyPropertyChanged {
        public CustomEqualizer(){
            InitializeComponent();
            WinMax.DoSourceInitialized(this);
            DataContext = this;
        }

        public float Maximum {
            get { return (Owner as MainWindow).AudioStreamControl.MainMusic.MaximumGain; }
        }

        public float Minimum {
            get { return (Owner as MainWindow).AudioStreamControl.MainMusic.MinimumGain; }
        }

        public float Band1 {
            get { return (Owner as MainWindow).AudioStreamControl.MainMusic.Band1; }
            set { (Owner as MainWindow).AudioStreamControl.MainMusic.Band1 = value; OnPropertyChanged(); }
        }

        public float Band2 {
            get { return (Owner as MainWindow).AudioStreamControl.MainMusic.Band2; }
            set { (Owner as MainWindow).AudioStreamControl.MainMusic.Band2 = value; OnPropertyChanged(); }
        }

        public float Band3 {
            get { return (Owner as MainWindow).AudioStreamControl.MainMusic.Band3; }
            set { (Owner as MainWindow).AudioStreamControl.MainMusic.Band3 = value; OnPropertyChanged(); }
        }

        public float Band4 {
            get { return (Owner as MainWindow).AudioStreamControl.MainMusic.Band4; }
            set { (Owner as MainWindow).AudioStreamControl.MainMusic.Band4 = value; OnPropertyChanged(); }
        }

        public float Band5 {
            get { return (Owner as MainWindow).AudioStreamControl.MainMusic.Band5; }
            set { (Owner as MainWindow).AudioStreamControl.MainMusic.Band5 = value; OnPropertyChanged(); }
        }

        public float Band6 {
            get { return (Owner as MainWindow).AudioStreamControl.MainMusic.Band6; }
            set { (Owner as MainWindow).AudioStreamControl.MainMusic.Band6 = value; OnPropertyChanged(); }
        }

        public float Band7 {
            get { return (Owner as MainWindow).AudioStreamControl.MainMusic.Band7; }
            set { (Owner as MainWindow).AudioStreamControl.MainMusic.Band7 = value; OnPropertyChanged(); }
        }

        public float Band8 {
            get { return (Owner as MainWindow).AudioStreamControl.MainMusic.Band8; }
            set { (Owner as MainWindow).AudioStreamControl.MainMusic.Band8 = value;}
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

        private void StartStopButton_Click(object sender, RoutedEventArgs e) {
            if (StartStopText.Text == "Start") {
                var win = Owner as MainWindow;

                if (win.AudioStreamControl.MainMusic.PathToMusic != null) {
                    win.AudioStreamControl.MainMusic.InitializeEqualizer();
                    win.AudioStreamControl.MainMusic.StopAndPlayFromPosition(win.AudioStreamControl.MainMusic.CurrentTrackPosition);
                    StartStopText.Text = "Stop";
                }
            }else if (StartStopText.Text == "Stop"){
                var win = Owner as MainWindow;

                win.AudioStreamControl.MainMusic.StopEqualizer();
                win.AudioStreamControl.MainMusic.StopAndPlayFromPosition(win.AudioStreamControl.MainMusic.CurrentTrackPosition);

                StartStopText.Text = "Start";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

         private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e) {
            Band1 = 0f;
            Band2 = 0f;
            Band3 = 0f;
            Band4 = 0f;
            Band5 = 0f;
            Band6 = 0f;
            Band7 = 0f;
            Band8 = 0f;
        }

    }
}
