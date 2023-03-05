using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// <summary>
    /// Interaction logic for SongList.xaml
    /// </summary>
    public partial class SongList : UserControl {
        public RoutedEventHandler ClickRowElement;

        public SongList() {
            InitializeComponent();
        }
    }

    public class ToolTipConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value.ToString().Length > 20) {
                return value.;
            }
            else {
                return null;
            }
            //throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
