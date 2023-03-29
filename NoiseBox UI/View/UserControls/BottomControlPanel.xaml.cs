using NAudio.Wave;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NoiseBox_UI.View.UserControls {
    public partial class BottomControlPanel : UserControl, INotifyPropertyChanged {
        public BottomControlPanel() {
            DataContext = this;
            InitializeComponent();

            State = ButtonState.Paused;
            Mode = PlaybackMode.Loop;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public enum ButtonState {
            Playing,
            Paused
        }

        public enum PlaybackMode {
            Loop,
            Loop1,
            NoLoop
        }

        private string _buttonStateImagePath;
        private string _playbackModeImagePath;
        private ButtonState _buttonState;
        private PlaybackMode _playbackMode;

        public string ButtonStateImagePath {
            get { return _buttonStateImagePath; }
            set {
                _buttonStateImagePath = value;

                OnPropertyChanged();
            }
        }

        public string PlaybackModeImagePath {
            get { return _playbackModeImagePath; }
            set {
                _playbackModeImagePath = value;

                OnPropertyChanged();
            }
        }

        public ButtonState State {
            get {
                return _buttonState;
            }
            set {
                switch (value) {
                    case ButtonState.Paused:
                        ButtonStateImagePath = "/Images/Icons/play.png";
                        _buttonState = value;
                        break;
                    case ButtonState.Playing:
                        ButtonStateImagePath = "/Images/Icons/pause.png";
                        _buttonState = value;
                        break;
                }
            }
        }

        public PlaybackMode Mode {
            get {
                return _playbackMode;
            }
            set {
                switch (value) {
                    case PlaybackMode.Loop:
                        PlaybackModeImagePath = "/Images/Icons/Loop.png";
                        _playbackMode = value;
                        break;
                    case PlaybackMode.Loop1:
                        PlaybackModeImagePath = "/Images/Icons/Loop1.png";
                        _playbackMode = value;
                        break;
                    case PlaybackMode.NoLoop:
                        PlaybackModeImagePath = "/Images/Icons/NoLoop.png";
                        _playbackMode = value;
                        break;
                }
            }
        }

        private void PlaybackModeButton_Click(object sender, RoutedEventArgs e) {
            switch (Mode) {
                case PlaybackMode.Loop:
                    Mode = PlaybackMode.Loop1;
                    break;
                case PlaybackMode.Loop1:
                    Mode = PlaybackMode.NoLoop;
                    break;
                case PlaybackMode.NoLoop:
                    Mode = PlaybackMode.Loop;
                    break;
            }
        }

        DispatcherTimer VSHeightTimer;

        private void VSExpandTimer_Tick(object sender, EventArgs e) {
            var rowHeight = VolumeSlidersGrid.RowDefinitions[0].Height;

            if (rowHeight.Value < 100) {
                VolumeSlidersGrid.RowDefinitions[0].Height = new GridLength(rowHeight.Value + 5, GridUnitType.Star);
                VolumeSlidersGrid.RowDefinitions[1].Height = new GridLength(rowHeight.Value + 5, GridUnitType.Star);
            }
            else {
                VSHeightTimer.Stop();
            }
        }

        private void VSContractTimer_Tick(object sender, EventArgs e) {
            var rowHeight = VolumeSlidersGrid.RowDefinitions[0].Height;

            if (rowHeight.Value > 0) {
                VolumeSlidersGrid.RowDefinitions[0].Height = new GridLength(rowHeight.Value - 5, GridUnitType.Star);
                VolumeSlidersGrid.RowDefinitions[1].Height = new GridLength(rowHeight.Value - 5, GridUnitType.Star);
            }
            else {
                VSHeightTimer.Stop();
            }
        }

        private void ToggleVolumeSliders(object sender, RoutedEventArgs e) {
            VSHeightTimer = new DispatcherTimer();
            VSHeightTimer.Interval = TimeSpan.FromMilliseconds(5);
            
            if (VolumeSlidersGrid.RowDefinitions[0].Height.Value == 0) {
                VSHeightTimer.Tick += VSExpandTimer_Tick;

                RotateToggle(0, -180);
            }
            else {
                VSHeightTimer.Tick += VSContractTimer_Tick;

                RotateToggle(-180, 0);
            }

            VSHeightTimer.Start();
        }

        private void RotateToggle(double from, double to) {
            DoubleAnimation rotateAnimation = new DoubleAnimation();
            rotateAnimation.From = from;
            rotateAnimation.To = to;
            rotateAnimation.Duration = TimeSpan.FromSeconds(0.2);

            ExpanderImage.RenderTransformOrigin = new Point(0.5, 0.5);

            RotateTransform rotateTransform = new RotateTransform();
            ExpanderImage.RenderTransform = rotateTransform;

            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
        }

        public async void VisualizeAudio(string path) {

            // reset SeekBar scaling to 1
            SeekBar.RenderTransformOrigin = new Point(0.5, 0.5);
            SeekBar.RenderTransform = new ScaleTransform() { ScaleY = 1 };
            SeekBar.Opacity = 1;

            // animate UniGrid scaleY from 1 to 0
            UniGrid.RenderTransformOrigin = new Point(0.5, 0.5);
            UniGrid.RenderTransform = new ScaleTransform() { ScaleY = 1 };
            DoubleAnimation scaleYAnimation = new DoubleAnimation {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(1),
            };

            // animate SeekBar opacity from 0 to 1
            var seekBarOpacityAnimation = new DoubleAnimation {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(1),
            };

            var storyboardSeekBarOpacity = new Storyboard();
            Storyboard.SetTarget(seekBarOpacityAnimation, SeekBar);
            Storyboard.SetTargetProperty(seekBarOpacityAnimation, new PropertyPath(Control.OpacityProperty));
            storyboardSeekBarOpacity.Children.Add(seekBarOpacityAnimation);

            if (SeekBar.Opacity <= 0) {
                storyboardSeekBarOpacity.Begin();
            }

            Storyboard storyboard = new Storyboard();
            Storyboard.SetTarget(scaleYAnimation, UniGrid);
            Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
            storyboard.Children.Add(scaleYAnimation);
            storyboard.Begin();

            var peaks = new List<float>();

            await Task.Run(() => { 

                using (var mp3 = new Mp3FileReader(path)) {
                    // Convert the MP3 to a sample provider that provides audio samples
                    var sampleProvider = mp3.ToSampleProvider();

                    // Create a buffer to hold audio samples
                    var buffer = new float[sampleProvider.WaveFormat.SampleRate];

                    // Read audio samples into the buffer and calculate peak heights
                    while (sampleProvider.Read(buffer, 0, buffer.Length) > 0) {
                        // Calculate the peak height of the buffer
                        float sum = 0;

                        for (int i = 0; i < buffer.Length; i++) {
                            sum += Math.Abs(buffer[i]);
                        }
                        float average = sum / buffer.Length;

                        peaks.Add(average);
                    }

                    float peaksMax = peaks.Max();
                    for (int i = 0; i < peaks.Count; i++) {
                        peaks[i] = (peaks[i] / peaksMax) * (int)UniGrid.ActualHeight; // peak height
                    }
                }

            });

            UniGrid.Children.Clear();

            foreach (var peak in peaks) {
                UniGrid.Children.Add(new Border() {
                    CornerRadius = new CornerRadius(2),
                    Height = peak,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3b2a5a")),
                    Margin = new Thickness(1)
                });
            }

            UniGrid_SizeChanged(null, null);

            // animate SeekBar opacity from 1 to 0
            SeekBar.RenderTransformOrigin = new Point(0.5, 0.5);
            SeekBar.RenderTransform = new ScaleTransform() { ScaleY = 1 };
            seekBarOpacityAnimation = new DoubleAnimation {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(1),
            };

            storyboardSeekBarOpacity = new Storyboard();
            Storyboard.SetTarget(seekBarOpacityAnimation, SeekBar);
            Storyboard.SetTargetProperty(seekBarOpacityAnimation, new PropertyPath(Control.OpacityProperty));
            storyboardSeekBarOpacity.Children.Add(seekBarOpacityAnimation);
            storyboardSeekBarOpacity.Begin();

            // animate UniGrid scaleY from 0 to 1
            UniGrid.RenderTransformOrigin = new Point(0.5, 0.5);
            UniGrid.RenderTransform = new ScaleTransform() { ScaleY = 1 };
            scaleYAnimation = new DoubleAnimation {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(1),
            };

            scaleYAnimation.Completed += (_, _) => {
                // set SeekBar scaling to 2
                SeekBar.RenderTransformOrigin = new Point(0.5, 0.5);
                SeekBar.RenderTransform = new ScaleTransform() { ScaleY = 2 };
            };

            storyboard = new Storyboard();
            Storyboard.SetTarget(scaleYAnimation, UniGrid);
            Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
            storyboard.Children.Add(scaleYAnimation);
            storyboard.Begin();
        }

        private void UniGrid_SizeChanged(object sender, SizeChangedEventArgs e) {

            int n = UniGrid.Children.Count;

            if (n == 0) {
                return;
            }

            int k = (int)UniGrid.ActualWidth / 6;

            List<int> numbers = new List<int>();
            for (int i = 0; i < n; i++) {
                numbers.Add(i);
            }

            List<int> reducedList = numbers.EvenlySpacedSubset(k);

            for (int i = 0; i < UniGrid.Children.Count; i++) {
                var border = UniGrid.Children[i] as Border;

                if (reducedList.Contains(i)) {
                    border.Visibility = Visibility.Visible;
                }
                else {
                    border.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void SeekBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            var val = (sender as Slider).Value;
            var borders = UniGrid.Children;

            var before = (int)(borders.Count * val / 100);

            for (int i = 0; i < borders.Count; i++) {
                if (i < before)
                    (borders[i] as Border).Background = new SolidColorBrush(Colors.BlueViolet);
                else
                    (borders[i] as Border).Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3b2a5a"));
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class ListExtensions {
        public static List<T> EvenlySpacedSubset<T>(this List<T> list, int count) {
            int length = list.Count;
            int[] indices = Enumerable.Range(0, count)
                                       .Select(i => (int)Math.Round((double)(i * (length - 1)) / (count - 1)))
                                       .ToArray();
            return indices.Select(i => list[i]).ToList();
        }
    }
}
