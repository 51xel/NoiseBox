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

        private void ToggleVolumeSliders(object sender, RoutedEventArgs e) {
            if (VolumeSlidersGrid.RowDefinitions[0].Height.Value == new GridLength(0, GridUnitType.Star).Value) {
                VolumeSlidersGrid.RowDefinitions[0].Height = new GridLength(33.3, GridUnitType.Star);
                VolumeSlidersGrid.RowDefinitions[1].Height = new GridLength(33.3, GridUnitType.Star);

                RotateToggle(0, -180);
            }
            else {
                VolumeSlidersGrid.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Star);
                VolumeSlidersGrid.RowDefinitions[1].Height = new GridLength(0, GridUnitType.Star);

                RotateToggle(-180, 0);
            }
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

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
