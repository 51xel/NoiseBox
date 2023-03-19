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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NoiseBox_UI.View.UserControls {
    public partial class BottomControlPanel : UserControl, INotifyPropertyChanged {
        public BottomControlPanel() {
            DataContext = this;
            InitializeComponent();

            State = ButtonState.Paused;
        }

        public enum ButtonState {
            Playing,
            Paused
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void SeekBar_DragStarted(object sender, DragStartedEventArgs e) {
            ((MainWindow)Window.GetWindow(this)).UserIsDraggingSlider = true;
        }

        private void SeekBar_DragCompleted(object sender, DragCompletedEventArgs e) {
            ((MainWindow)Window.GetWindow(this)).UserIsDraggingSlider = false;
        }

        private string _pathToImage;
        private ButtonState _buttonState;

        public string PathToImage {
            get { return _pathToImage; }
            set {
                _pathToImage = value;

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
                        PathToImage = "/Images/Icons/play.png";
                        _buttonState = value;
                        break;
                    case ButtonState.Playing:
                         PathToImage = "/Images/Icons/pause.png";
                        _buttonState = value;
                        break;
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
