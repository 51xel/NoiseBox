using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NoiseBox_UI.View.UserControls {
    public partial class SizeControlWindowButtons : UserControl {
        public SizeControlWindowButtons() {
            InitializeComponent();
        }

        private void MaximizeButtonOverMouseEnter(object sender, MouseEventArgs e) {
            MaximizeButtonImage.Opacity = 1;
        }
        private void MaximizeButtonOverMouseLeave(object sender, MouseEventArgs e) {
            MaximizeButtonImage.Opacity = 0.6;
        }

        private void MinimizeButtonOverMouseEnter(object sender, MouseEventArgs e) {
            MinimizeButtonImage.Opacity = 1;
        }
        private void MinimizeButtonOverMouseLeave(object sender, MouseEventArgs e) {
            MinimizeButtonImage.Opacity = 0.6;
        }

        private void CloseButtonOverMouseEnter(object sender, MouseEventArgs e) {
            CloseButtonImage.Opacity = 1;
        }
        private void CloseButtonOverMouseLeave(object sender, MouseEventArgs e) {
            CloseButtonImage.Opacity = 0.6;
        }
    }
}
