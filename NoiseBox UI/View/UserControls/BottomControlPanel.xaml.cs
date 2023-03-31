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
            rotateAnimation.Duration = TimeSpan.FromMilliseconds(300);
            rotateAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };

            ExpanderImage.RenderTransformOrigin = new Point(0.5, 0.5);

            RotateTransform rotateTransform = new RotateTransform();
            ExpanderImage.RenderTransform = rotateTransform;

            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        private void MainVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
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


        private void VCVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            var icon = VCVolumeButton.Content as MaterialDesignThemes.Wpf.PackIcon;

            if (VCVolumeSlider.Value == 0) {
                icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.MicrophoneVariantOff;
            }
            else {
                icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.MicrophoneVariant;
            }
        }

        double vcVolumeSliderBeforeMuteValue = 0;
        private void VCVolumeButton_Click(object sender, RoutedEventArgs e) {
            if (VCVolumeSlider.Value != 0) {
                vcVolumeSliderBeforeMuteValue = VCVolumeSlider.Value;
                //VCVolumeSlider.Value = 0;

                AnimateVolumeSliderValue(VCVolumeSlider, 0);
            }
            else {
                //VCVolumeSlider.Value = vcVolumeSliderBeforeMuteValue;

                AnimateVolumeSliderValue(VCVolumeSlider, vcVolumeSliderBeforeMuteValue);
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
}
