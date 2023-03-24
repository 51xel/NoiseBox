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
    public partial class CustomEqualizer : Window{
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
            set { (Owner as MainWindow).AudioStreamControl.MainMusic.Band1 = value; }
        }

        public float Band2 {
            get { return (Owner as MainWindow).AudioStreamControl.MainMusic.Band2; }
            set { (Owner as MainWindow).AudioStreamControl.MainMusic.Band2 = value; }
        }

        public float Band3 {
            get { return (Owner as MainWindow).AudioStreamControl.MainMusic.Band3; }
            set { (Owner as MainWindow).AudioStreamControl.MainMusic.Band3 = value; }
        }

        public float Band4 {
            get { return (Owner as MainWindow).AudioStreamControl.MainMusic.Band4; }
            set { (Owner as MainWindow).AudioStreamControl.MainMusic.Band4 = value; }
        }

        public float Band5 {
            get { return (Owner as MainWindow).AudioStreamControl.MainMusic.Band5; }
            set { (Owner as MainWindow).AudioStreamControl.MainMusic.Band5 = value; }
        }

        public float Band6 {
            get { return (Owner as MainWindow).AudioStreamControl.MainMusic.Band6; }
            set { (Owner as MainWindow).AudioStreamControl.MainMusic.Band6 = value; }
        }

        public float Band7 {
            get { return (Owner as MainWindow).AudioStreamControl.MainMusic.Band7; }
            set { (Owner as MainWindow).AudioStreamControl.MainMusic.Band7 = value; }
        }

        public float Band8 {
            get { return (Owner as MainWindow).AudioStreamControl.MainMusic.Band8; }
            set { (Owner as MainWindow).AudioStreamControl.MainMusic.Band8 = value; }
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
            if ((Owner as MainWindow).AudioStreamControl.MainMusic.PathToMusic != null) {
                (Owner as MainWindow).AudioStreamControl.MainMusic.StopEqualizer();
                (Owner as MainWindow).AudioStreamControl.MainMusic.StopAndPlayFromPosition((Owner as MainWindow).AudioStreamControl.MainMusic.CurrentTrackPosition);
            }
        }
    }
}
