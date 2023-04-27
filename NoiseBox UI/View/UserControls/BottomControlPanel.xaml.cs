using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace NoiseBox_UI.View.UserControls {
    public partial class BottomControlPanel : UserControl, INotifyPropertyChanged {
        public bool Rendering = false;

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
        private bool _isToggling = false;

        private void VSExpandTimer_Tick(object sender, EventArgs e) {
            var rowHeight = VolumeSlidersGrid.RowDefinitions[0].Height;

            if (rowHeight.Value < 100) {
                VolumeSlidersGrid.RowDefinitions[0].Height = new GridLength(rowHeight.Value + 5, GridUnitType.Star);
                VolumeSlidersGrid.RowDefinitions[1].Height = new GridLength(rowHeight.Value + 5, GridUnitType.Star);
            }
            else {
                _isToggling = false;
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
                _isToggling = false;
                VSHeightTimer.Stop();
            }
        }

        private void ToggleVolumeSliders(object sender, RoutedEventArgs e) {
            if (!_isToggling) {
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

                _isToggling = true;
                VSHeightTimer.Start();
            }
        }

        private void RotateToggle(double from, double to) {
            DoubleAnimation rotateAnimation = new DoubleAnimation();
            rotateAnimation.From = from;
            rotateAnimation.To = to;
            rotateAnimation.Duration = TimeSpan.FromMilliseconds(300);
            rotateAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };

            ExpanderImage.RenderTransformOrigin = new Point(0.5, 0.5);

            RotateTransform rotateTransform = new RotateTransform();
            ExpanderImage.RenderTransform = rotateTransform;

            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
        }

        public void ShowSeekBarHideBorders() {
            DoubleAnimation seekBarOpacityAnimation;
            Storyboard storyboardSeekBarOpacity;

            DoubleAnimation uniGridScaleAnimation;
            Storyboard storyboardUniGridScale;

            // reset SeekBar scaling to 1
            SeekBar.RenderTransformOrigin = new Point(0.5, 0.5);
            SeekBar.RenderTransform = new ScaleTransform() { ScaleY = 1 };

            // animate SeekBar opacity from 0 to SeekBar.Opacity
            seekBarOpacityAnimation = new DoubleAnimation {
                From = SeekBar.Opacity,
                To = 1,
                Duration = TimeSpan.FromSeconds(1),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };

            storyboardSeekBarOpacity = new Storyboard();

            Storyboard.SetTarget(seekBarOpacityAnimation, SeekBar);
            Storyboard.SetTargetProperty(seekBarOpacityAnimation, new PropertyPath(Control.OpacityProperty));
            storyboardSeekBarOpacity.Children.Add(seekBarOpacityAnimation);

            storyboardSeekBarOpacity.Begin();

            // animate UniGrid scaleY from 1 to 0
            UniGrid.RenderTransformOrigin = new Point(0.5, 0.5);
            UniGrid.RenderTransform = new ScaleTransform() { ScaleY = 1 };

            uniGridScaleAnimation = new DoubleAnimation {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };

            storyboardUniGridScale = new Storyboard();

            Storyboard.SetTarget(uniGridScaleAnimation, UniGrid);
            Storyboard.SetTargetProperty(uniGridScaleAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
            storyboardUniGridScale.Children.Add(uniGridScaleAnimation);
            storyboardUniGridScale.Begin();
        }

        public void HideSeekBarShowBorders() {
            DoubleAnimation seekBarOpacityAnimation;
            Storyboard storyboardSeekBarOpacity;

            DoubleAnimation uniGridScaleAnimation;
            Storyboard storyboardUniGridScale;

            SeekBar.RenderTransformOrigin = new Point(0.5, 0.5);
            SeekBar.RenderTransform = new ScaleTransform() { ScaleY = 1 };

            // animate SeekBar opacity from 1 to 0
            seekBarOpacityAnimation = new DoubleAnimation {
                From = SeekBar.Opacity,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };

            storyboardSeekBarOpacity = new Storyboard();

            Storyboard.SetTarget(seekBarOpacityAnimation, SeekBar);
            Storyboard.SetTargetProperty(seekBarOpacityAnimation, new PropertyPath(Control.OpacityProperty));
            storyboardSeekBarOpacity.Children.Add(seekBarOpacityAnimation);
            storyboardSeekBarOpacity.Begin();

            // animate UniGrid scaleY from 0 to 1
            UniGrid.RenderTransformOrigin = new Point(0.5, 0.5);
            UniGrid.RenderTransform = new ScaleTransform() { ScaleY = 1 };
            uniGridScaleAnimation = new DoubleAnimation {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };

            uniGridScaleAnimation.Completed += (_, _) => {
                // set SeekBar scaling to 2
                SeekBar.RenderTransformOrigin = new Point(0.5, 0.5);
                SeekBar.RenderTransform = new ScaleTransform() { ScaleY = 2 };
            };

            storyboardUniGridScale = new Storyboard();

            Storyboard.SetTarget(uniGridScaleAnimation, UniGrid);
            Storyboard.SetTargetProperty(uniGridScaleAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
            storyboardUniGridScale.Children.Add(uniGridScaleAnimation);
            storyboardUniGridScale.Begin();
        }

        public async void VisualizeAudio(string path) {
            if (Rendering) {
                Rendering = false;
                await Task.Delay(10);
            }
            else {
                ShowSeekBarHideBorders();
            }

            var peaks = new List<float>();

            await Render(path, peaks);

            if (peaks.Count == 0) {
                return;
            }

            UniGrid.Children.Clear();

            foreach (var peak in peaks) {
                UniGrid.Children.Add(new Border() {
                    CornerRadius = new CornerRadius(2),
                    Height = peak,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#673ab7")),
                    Opacity = 0.4,
                    Margin = new Thickness(1)
                });
            }

            UniGrid_SizeChanged(null, null);
            SeekBar_ValueChanged(null, null);

            HideSeekBarShowBorders();
        }

        private async Task Render(string path, List<float> peaks) {
            await Task.Run(() => {
                Rendering = true;

                using (var mp3 = new Mp3FileReader(path)) {
                    int peakCount = 300;

                    int bytesPerSample = (mp3.WaveFormat.BitsPerSample / 8) * mp3.WaveFormat.Channels;
                    int samplesPerPeak = (int)(mp3.Length / (double)(peakCount * bytesPerSample));
                    int bytesPerPeak = bytesPerSample * samplesPerPeak;

                    var buffer = new byte[bytesPerPeak];

                    for (int x = 0; x < peakCount; x++) {
                        if (!Rendering) {
                            peaks.Clear();

                            return;
                        }

                        int bytesRead = mp3.Read(buffer, 0, bytesPerPeak);
                        if (bytesRead == 0)
                            break;

                        float sum = 0;

                        for (int n = 0; n < bytesRead; n += 2) {
                            if (!Rendering) {
                                peaks.Clear();

                                return;
                            }

                            sum += Math.Abs(BitConverter.ToInt16(buffer, n));
                        }

                        float average = sum / (bytesRead / 2);

                        peaks.Add(average);
                    }

                    if (peaks.Count != 0) {
                        float peaksMax = peaks.Max();
                        for (int i = 0; i < peaks.Count; i++) {
                            if (!Rendering) {
                                peaks.Clear();

                                return;
                            }

                            peaks[i] = (peaks[i] / peaksMax) * (int)(UniGrid.ActualHeight * 0.95); // peak height

                            if (peaks[i] < 2) {
                                peaks[i] = 2;
                            }
                        }
                    }

                    Rendering = false;
                }
            });
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
            var val = SeekBar.Value;
            var borders = UniGrid.Children;

            var before = (int)(borders.Count * val / 100);

            for (int i = 0; i < borders.Count; i++) {
                if (i < before)
                    (borders[i] as Border).Opacity = 1;
                else
                    (borders[i] as Border).Opacity = 0.4;
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MainVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e){
            var icon = MainVolumeButton.Content as MaterialDesignThemes.Wpf.PackIcon;
            var val = MainVolumeSlider.Value;

            if (val == 0) {
                icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeMute;
            }
            else if (val < 50) {
                icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeMedium;
            }
            else if (val >= 50) {
                icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeHigh;
            }
        }

        double mainVolumeSliderBeforeMuteValue = 0;
        private void MainVolumeButton_Click(object sender, RoutedEventArgs e) {
            if (MainVolumeSlider.Value != 0) {
                mainVolumeSliderBeforeMuteValue = MainVolumeSlider.Value;
                //MainVolumeSlider.Value = 0;

                AnimateVolumeSliderValue(MainVolumeSlider, 0);
            }
            else {
                //MainVolumeSlider.Value = mainVolumeSliderBeforeMuteValue;

                AnimateVolumeSliderValue(MainVolumeSlider, mainVolumeSliderBeforeMuteValue);
            }
        }

        private void MicVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            var icon = MicVolumeButton.Content as MaterialDesignThemes.Wpf.PackIcon;

            if (MicVolumeSlider.Value == 0) {
                icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.MicrophoneOff;
            }
            else {
                icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Microphone;
            }
        }

        double micVolumeSliderBeforeMuteValue = 0;
        private void MicVolumeButton_Click(object sender, RoutedEventArgs e) {
            if (MicVolumeSlider.Value != 0) {
                micVolumeSliderBeforeMuteValue = MicVolumeSlider.Value;
                //MicVolumeSlider.Value = 0;

                AnimateVolumeSliderValue(MicVolumeSlider, 0);
            }
            else {
                //MicVolumeSlider.Value = micVolumeSliderBeforeMuteValue;

                AnimateVolumeSliderValue(MicVolumeSlider, micVolumeSliderBeforeMuteValue);
            }
        }

        private void AdditionalVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            var icon = AdditionalVolumeButton.Content as MaterialDesignThemes.Wpf.PackIcon;

            if (AdditionalVolumeSlider.Value == 0) {
                icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.MicrophoneVariantOff;
            }
            else {
                icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.MicrophoneVariant;
            }
        }

        double AdditionalVolumeSliderBeforeMuteValue = 0;
        private void AdditionalVolumeButton_Click(object sender, RoutedEventArgs e) {
            if (AdditionalVolumeSlider.Value != 0) {
                AdditionalVolumeSliderBeforeMuteValue = AdditionalVolumeSlider.Value;

                AnimateVolumeSliderValue(AdditionalVolumeSlider, 0);
            }
            else {
                AnimateVolumeSliderValue(AdditionalVolumeSlider, AdditionalVolumeSliderBeforeMuteValue);
            }
        }

        private void AnimateVolumeSliderValue(Slider slider, double newVal) {
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = slider.Value;
            doubleAnimation.To = newVal;
            doubleAnimation.Duration = TimeSpan.FromMilliseconds(300);
            doubleAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };

            slider.BeginAnimation(Slider.ValueProperty, doubleAnimation);
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
