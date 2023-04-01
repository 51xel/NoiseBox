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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media.Animation;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Windows.Markup;
using Newtonsoft.Json.Linq;
using System.Reflection;
using NoiseBox.Log;
using System.Windows.Threading;

namespace NoiseBox_UI {
    public partial class CustomEqualizer : Window, INotifyPropertyChanged {
        private List<BandsSettings> _bandsSettings = new List<BandsSettings>();
        private string _jsonFilePath = "_bandsSettings.json";

        public CustomEqualizer() {
            InitializeComponent();
            WinMax.DoSourceInitialized(this);
            DataContext = this;

            LoadFromJson();
            UpdateProfiles();
        }

        public float Maximum {
            get { return (Owner as MainWindow).AudioStreamControl.MainMusic.MaximumGain; }
        }

        public float Minimum {
            get { return (Owner as MainWindow).AudioStreamControl.MainMusic.MinimumGain; }
        }

        private float GetBand(int index) {
            return (Owner as MainWindow).AudioStreamControl.MainMusic.GetBandGain(index);
        }

        private void SetBand(int index, float value) {
            (Owner as MainWindow).AudioStreamControl.MainMusic.SetBandGain(index, value);
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

                    Profiles_SelectionChanged(null, null);
                }
            }
            else if (StartStopText.Text == "Stop") {
                var win = Owner as MainWindow;

                win.AudioStreamControl.MainMusic.StopEqualizer();
                win.AudioStreamControl.MainMusic.StopAndPlayFromPosition(win.AudioStreamControl.MainMusic.CurrentTrackPosition);

                StartStopText.Text = "Start";

                ReloadButton_Click(null, null);
            }
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e) {
            for (int i = 0; i < 7; i++) {
                AnimationChangingSliderValue(i, 0);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) {
            if (StartStopText.Text == "Stop") {
                if (!String.IsNullOrEmpty(Profiles.SelectedItem as String)) {
                    var band = _bandsSettings.FirstOrDefault(n => n.Name == Profiles.SelectedItem as String);

                    band.EqualizerBands = (Owner as MainWindow).AudioStreamControl.MainMusic.GetBandsList();

                    SaveToJson();
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) {
            if (StartStopText.Text == "Stop") {
                if (!String.IsNullOrEmpty(Profiles.SelectedItem as String)) {
                    _bandsSettings.Remove(_bandsSettings.FirstOrDefault(n => n.Name == Profiles.SelectedItem as String));

                    Profiles.SelectedItem = -1;

                    ReloadButton_Click(null, null);

                    UpdateProfiles();

                    (Owner as MainWindow).SelectedBandsSettings = null;

                    SaveToJson();
                }
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e) {
            if (StartStopText.Text == "Stop") {
                NamePopup.IsOpen = true;
                NamePopupTextBox.Focus();
            }
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e) {
            if (StartStopText.Text == "Stop") {
                ReNamePopup.IsOpen = true;
                ReNamePopupTextBox.Focus();
            }
        }

        private void Profiles_SelectionChanged(object sender, RoutedEventArgs e) {
            if (StartStopText.Text == "Stop") {
                var band = _bandsSettings.FirstOrDefault(n => n.Name == Profiles.SelectedItem as String);

                if (band != null) {
                    (Owner as MainWindow).AudioStreamControl.MainMusic.SetBandsList(band.EqualizerBands);

                    (Owner as MainWindow).SelectedBandsSettings = band;

                    for (int i = 0; i < 7; i++) {
                        AnimationChangingSliderValue(i, band.EqualizerBands[i].Gain);
                    }
                }
            }
        }

        private void UpdateProfiles(string bandNameToSelect = null) {
            Profiles.Items.Clear();

            foreach (var profile in _bandsSettings) {
                Profiles.Items.Add(profile.Name);
            }

            if (bandNameToSelect != null) {
                Profiles.SelectedItem = bandNameToSelect;
            }
        }

        public void LoadSelectedBand(BandsSettings bandsSettingsToLoad) {
            if(bandsSettingsToLoad != null) {
                Profiles.SelectedItem = bandsSettingsToLoad.Name;
            }
        }

        private void SaveToJson() { 
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            string json = JsonConvert.SerializeObject(_bandsSettings, Formatting.Indented, settings);
            File.WriteAllText(_jsonFilePath, json);
        }

        private void LoadFromJson() {
            if (File.Exists(_jsonFilePath)) {
                string jsonString = File.ReadAllText(_jsonFilePath);
                _bandsSettings = JsonConvert.DeserializeObject<List<BandsSettings>>(jsonString);
            }
            else {
                File.Create(_jsonFilePath).Close();
            }
        }

        private void NamePopupTextBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                string popupTextBoxText = NamePopupTextBox.Text.Trim();

                if (!string.IsNullOrEmpty(popupTextBoxText)) {
                    if (_bandsSettings.FirstOrDefault(n => n.Name == popupTextBoxText) == null) {
                        var band = new BandsSettings();

                        band.Name = popupTextBoxText;
                        band.EqualizerBands = (Owner as MainWindow).AudioStreamControl.MainMusic.GetBandsList();

                        _bandsSettings.Add(band);

                        SaveToJson();

                        (Owner as MainWindow).SelectedBandsSettings = band;

                        UpdateProfiles(band.Name);
                    }

                    NamePopupTextBox.Text = "";
                    NamePopup.IsOpen = false;
                }
            }
        }

        private void ReNamePopupTextBox_KeyDown(object sender, KeyEventArgs e){
            if (e.Key == Key.Enter) {
                string popupTextBoxText = ReNamePopupTextBox.Text.Trim();

                if (!string.IsNullOrEmpty(popupTextBoxText)) {
                    var band = _bandsSettings.FirstOrDefault(n => n.Name == Profiles.SelectedItem as String);

                    if (band != null && _bandsSettings.FirstOrDefault(n => n.Name == popupTextBoxText) == null) {
                        band.Name = popupTextBoxText;

                        SaveToJson();

                        (Owner as MainWindow).SelectedBandsSettings = band;

                        UpdateProfiles(band.Name);
                    }
                    

                    ReNamePopupTextBox.Text = "";
                    ReNamePopup.IsOpen = false;
                }
            }
        }

        private void AnimationChangingSliderValue(int index, float to) {
            SetBand(index, to);

            var slider = ((Slider)EqGrid.FindName($"Slider{index}"));

            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = slider.Value;
            doubleAnimation.To = to;
            doubleAnimation.Duration = TimeSpan.FromMilliseconds(500);
            doubleAnimation.Completed += (_, _) => {
                slider.BeginAnimation(Slider.ValueProperty, null);
                slider.Value = GetBand(index);
            };

            slider.BeginAnimation(Slider.ValueProperty, doubleAnimation);
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

                EqGrid.RegisterName(slider.Name, slider);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
