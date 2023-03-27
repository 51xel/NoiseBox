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
using NAudio.Extras;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

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

        private float GetBand(int index) {
            return (Owner as MainWindow).AudioStreamControl.MainMusic.GetBand(index);
        }

        private void SetBand(int index, float value) {
            (Owner as MainWindow).AudioStreamControl.MainMusic.SetBand(index, value);
        }

        public float Band0 {
            get { return GetBand(0); }
            set { SetBand(0, value); OnPropertyChanged(); }
        }

        public float Band1 {
            get { return GetBand(1); }
            set { SetBand(1, value); OnPropertyChanged(); }
        }

        public float Band2 {
            get { return GetBand(2); }
            set { SetBand(2, value); OnPropertyChanged(); }
        }

        public float Band3 {
            get { return GetBand(3); }
            set { SetBand(3, value); OnPropertyChanged(); }
        }

        public float Band4 {
            get { return GetBand(4); }
            set { SetBand(4, value); OnPropertyChanged(); }
        }

        public float Band5 {
            get { return GetBand(5); }
            set { SetBand(5, value); OnPropertyChanged(); }
        }

        public float Band6 {
            get { return GetBand(6); }
            set { SetBand(6, value); OnPropertyChanged(); }
        }

        public float Band7 {
            get { return GetBand(7); }
            set { SetBand(7, value); OnPropertyChanged(); }
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

                ReloadButton_Click(null, null);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e) {
            Band0 = 0f;
            Band1 = 0f;
            Band2 = 0f;
            Band3 = 0f;
            Band4 = 0f;
            Band5 = 0f;
            Band6 = 0f;
            Band7 = 0f;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            for (int i = 0; i <= 7; i++) {
                Slider slider = new Slider();

                slider.Name = $"Slider{i}";
                slider.Maximum = Maximum;
                slider.Minimum = Minimum;
                slider.Orientation = Orientation.Vertical;
                slider.Style = (Style)FindResource("MaterialDesignDiscreteSlider");
                slider.TickFrequency = 1;
                slider.TickPlacement = TickPlacement.BottomRight;

                Binding binding = new Binding();
                binding.Path = new PropertyPath($"Band{i}");
                binding.Mode = BindingMode.TwoWay;
                slider.SetBinding(Slider.ValueProperty, binding);

                slider.HorizontalAlignment = HorizontalAlignment.Center;

                ColumnDefinition colDef = new ColumnDefinition();
                EqGrid.ColumnDefinitions.Add(colDef);

                EqGrid.Children.Add(slider);
                Grid.SetColumn(slider, i);
            }
        }
    }
}
