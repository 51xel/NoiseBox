using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NoiseBox_UI {
    public partial class DownloadsWindow : Window {
        public DownloadsWindow() {
            InitializeComponent();
            WinMax.DoSourceInitialized(this);
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e) {
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

        private async void Button_Click(object sender, RoutedEventArgs e) {
            yt_dlp_Output.Text = "";

            LinkTextBox.Text = LinkTextBox.Text.Trim();

            if (!Uri.IsWellFormedUriString(LinkTextBox.Text, UriKind.Absolute) &&
                !Uri.IsWellFormedUriString("http://" + LinkTextBox.Text, UriKind.Absolute) &&
                !Uri.IsWellFormedUriString("https://" + LinkTextBox.Text, UriKind.Absolute)) {
          
                yt_dlp_Output.Inlines.Add(new Run("[Invalid url]") { Foreground = Brushes.IndianRed });
                return;
            }

            var directory = System.AppContext.BaseDirectory.Split(Path.DirectorySeparatorChar);
            var slice = new ArraySegment<string>(directory, 0, directory.Length - 4);
            var BinariesDirPath = Path.Combine(Path.Combine(slice.ToArray()), "Binaries");

            var psi = new ProcessStartInfo(Path.Combine(BinariesDirPath, "yt-dlp.exe")) {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = $" --ffmpeg-location \"{Path.Combine(BinariesDirPath, @"ffmpeg\bin")}\"  -x --audio-format mp3 -o %(title)s.%(ext)s " + LinkTextBox.Text
            };

            using var process = new Process { StartInfo = psi };

            var isDownloading = false;

            process.OutputDataReceived += (_, e) => {
                if (!string.IsNullOrEmpty(e.Data)) {
                    yt_dlp_Output.Dispatcher.Invoke(() => {
                        if (!isDownloading) {
                            isDownloading = true;
                            yt_dlp_Output.Inlines.Add(new Run("Downloading...") { Foreground = Brushes.CornflowerBlue });
                        }
                    });
                }
            };

            var hadErrors = false;

            process.ErrorDataReceived += (_, e) => {
                if (!string.IsNullOrEmpty(e.Data)) {
                    hadErrors = true;
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            yt_dlp_Output.Text = "";

            if (!hadErrors) {
                yt_dlp_Output.Inlines.Add(new Run("[Download finished]") { Foreground = Brushes.LawnGreen });
            }
            else {
                yt_dlp_Output.Inlines.Add(new Run("[Error while downloading]") { Foreground = Brushes.IndianRed });
            }
        }

        private void LinkTextBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                Button_Click(null, null);
            }
        }
    }
}
