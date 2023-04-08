using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace NoiseBox_UI.Utils.CustomControls {
    public class CustomSlider : Slider {
        private Thumb _thumb = null;

        public CustomSlider() {
            Style = FindResource("MaterialDesignSlider") as Style;
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            if (_thumb != null) {
                _thumb.MouseEnter -= thumb_MouseEnter;
            }

            _thumb = (GetTemplateChild("PART_Track") as Track).Thumb;

            if (_thumb != null) {
                _thumb.MouseEnter += thumb_MouseEnter;
            }
        }

        private void thumb_MouseEnter(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                // the left button is pressed on mouse enter so the thumb must have been moved under the mouse
                // in response to a click on the track. Generate a MouseLeftButtonDown event.

                MouseButtonEventArgs args = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left);

                args.RoutedEvent = MouseLeftButtonDownEvent;

                (sender as Thumb).RaiseEvent(args);
            }
        }
    }
}
