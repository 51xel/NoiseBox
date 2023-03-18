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
using static System.Net.WebRequestMethods;

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

        private void Window_StateChanged(object sender, EventArgs e) {
            if (WindowState == WindowState.Normal || WindowState == WindowState.Maximized) {
                (Owner as MainWindow).FunctionButtons.DownloadingProgress.Visibility = Visibility.Collapsed;
            }
            else if (WindowState == WindowState.Minimized) {
                if (DownloadingProgress.Visibility == Visibility.Visible) {
                    (Owner as MainWindow).FunctionButtons.DownloadingProgress.Visibility = Visibility.Visible;
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e) {
            Cancel_Click(null, null);
        }

        Process process;

        private async void Download_Click(object sender, RoutedEventArgs e) {
            yt_dlp_Output.Text = "Starting to download...";
            
            LinkTextBox.Text = LinkTextBox.Text.Trim();

            if (!Uri.IsWellFormedUriString(LinkTextBox.Text, UriKind.Absolute) &&
                !Uri.IsWellFormedUriString("http://" + LinkTextBox.Text, UriKind.Absolute) &&
                !Uri.IsWellFormedUriString("https://" + LinkTextBox.Text, UriKind.Absolute)) {

                yt_dlp_Output.Text = "";
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
                Arguments = $" --ffmpeg-location \"{Path.Combine(BinariesDirPath, @"ffmpeg\bin")}\" -x --audio-format mp3 -o %(title)s.%(ext)s \"{LinkTextBox.Text}\""
            };

            process = new Process { StartInfo = psi };

            var isDownloading = false;

            process.OutputDataReceived += (_, e) => {
                if (!string.IsNullOrEmpty(e.Data)) {
                    yt_dlp_Output.Dispatcher.Invoke(() => {
                        var match = Regex.Match(e.Data, @"\[download\]\s*(\d+\.?\d*)%\s*of[\s~]*(\d+\.?\d*\wiB)");

                        if (match.Groups[1].Success) {
                            if (!isDownloading) {
                                isDownloading = true;
                                yt_dlp_Output.Text = $"Downloading... ({match.Groups[2].ToString()})";

                                DownloadingProgress.Visibility = Visibility.Visible;

                                if (WindowState == WindowState.Minimized) {
                                    (Owner as MainWindow).FunctionButtons.DownloadingProgress.Visibility = Visibility.Visible;
                                }
                            }

                            DownloadingProgress.Value = Convert.ToDouble(match.Groups[1].ToString());
                            (Owner as MainWindow).FunctionButtons.DownloadingProgress.Value = DownloadingProgress.Value;
                        }

                        if (e.Data.StartsWith("[ExtractAudio]")) {
                            yt_dlp_Output.Text = $"Extracting audio...";

                            DownloadingProgress.Visibility = Visibility.Collapsed;
                            (Owner as MainWindow).FunctionButtons.DownloadingProgress.Visibility = Visibility.Collapsed;

                            CancelColumn.Width = new GridLength(0, GridUnitType.Star);
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

            DownloadColumn.Width = new GridLength(0, GridUnitType.Star);
            CancelColumn.Width = new GridLength(100, GridUnitType.Star);

            await process.WaitForExitAsync();

            process.Dispose();

            yt_dlp_Output.Text = "";

            if (!hadErrors) {
                yt_dlp_Output.Inlines.Add(new Run("[Downloading finished]") { Foreground = Brushes.LawnGreen });
            }
            else {
                yt_dlp_Output.Inlines.Add(new Run("[Error while downloading]") { Foreground = Brushes.IndianRed });
            }

            DownloadColumn.Width = new GridLength(100, GridUnitType.Star);
            CancelColumn.Width = new GridLength(0, GridUnitType.Star);
        }

        private void LinkTextBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                Download_Click(null, null);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            process.Close();
            process.Dispose();

            yt_dlp_Output.Text = "";
            yt_dlp_Output.Inlines.Add(new Run("[Canceled]") { Foreground = Brushes.IndianRed });

            DownloadingProgress.Visibility = Visibility.Collapsed;
            (Owner as MainWindow).FunctionButtons.DownloadingProgress.Visibility = Visibility.Collapsed;

            DownloadColumn.Width = new GridLength(100, GridUnitType.Star);
            CancelColumn.Width = new GridLength(0, GridUnitType.Star);
        }
    }
}
