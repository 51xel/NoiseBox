﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
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

        private void ButtonMouseEnter(object sender, MouseEventArgs e) {
            ((sender as Button).Content as Image).Opacity = 1;
        }

        private void ButtonMouseLeave(object sender, MouseEventArgs e) {
            ((sender as Button).Content as Image).Opacity = 0.6;
        }
    }
}