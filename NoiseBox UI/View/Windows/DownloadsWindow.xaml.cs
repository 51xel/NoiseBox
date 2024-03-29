﻿using NoiseBox;
using NoiseBox.Log;
using NoiseBox_UI.Utils;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WinForms = System.Windows.Forms;

namespace NoiseBox_UI.View.Windows {
    public partial class DownloadsWindow : Window, INotifyPropertyChanged {

        private Process process;

        private static ILog _log = LogSettings.SelectedLog;

        private string _selectedDirectory;
        public string SelectedDirectory {
            get {
                return _selectedDirectory;
            }
            set {
                _selectedDirectory = value;

                Properties.Settings.Default.DownloadsFolder = value;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public DownloadsWindow() {
            InitializeComponent();
            WinMax.DoSourceInitialized(this);
            DataContext = this;

            if (string.IsNullOrEmpty(Properties.Settings.Default.DownloadsFolder)) {
                SelectedDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            }
            else {
                SelectedDirectory = Properties.Settings.Default.DownloadsFolder;
            }
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

            if (WindowState == WindowState.Normal || WindowState == WindowState.Maximized) {
                (Owner as MainWindow).FunctionButtons.DownloadingProgress.Visibility = Visibility.Collapsed;
            }
            else if (WindowState == WindowState.Minimized) {
                if (DownloadingProgress.Visibility == Visibility.Visible) {
                    (Owner as MainWindow).FunctionButtons.DownloadingProgress.Visibility = Visibility.Visible;
                }
            }
        }

        private void Window_Closing(object sender, EventArgs e) {
            Cancel_Click(null, null);
        }

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

            var BinariesDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries");
            var ffmpegLocation = Path.Combine(BinariesDirPath, @"ffmpeg\bin");

            string downloadedFileDir = SelectedDirectory;
            string downloadedFileID = Guid.NewGuid().ToString();

            var psi = new ProcessStartInfo(Path.Combine(BinariesDirPath, "yt-dlp.exe")) {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = $" --ffmpeg-location \"{ffmpegLocation}\" -x --audio-format mp3 -o \"{downloadedFileDir}\\[{downloadedFileID}]%(title)s.%(ext)s\" \"{LinkTextBox.Text}\""
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

                                DownloadingProgress.IsIndeterminate = false;
                                DownloadingProgress.Visibility = Visibility.Visible;

                                (Owner as MainWindow).FunctionButtons.DownloadingProgress.IsIndeterminate = false;

                                if (WindowState == WindowState.Minimized) {
                                    (Owner as MainWindow).FunctionButtons.DownloadingProgress.Visibility = Visibility.Visible;
                                }
                            }

                            NumberFormatInfo nfi = new NumberFormatInfo();
                            nfi.NumberDecimalSeparator = ".";

                            DownloadingProgress.Value = double.Parse(match.Groups[1].ToString(), nfi);
                            (Owner as MainWindow).FunctionButtons.DownloadingProgress.Value = DownloadingProgress.Value;
                        }

                        if (e.Data.StartsWith("[ExtractAudio]")) {
                            yt_dlp_Output.Text = $"Extracting audio...";

                            CancelColumn.Width = new GridLength(0, GridUnitType.Star);

                            DownloadingProgress.IsIndeterminate = true;

                            (Owner as MainWindow).FunctionButtons.DownloadingProgress.Value = 0; // for indeterminate to work
                            (Owner as MainWindow).FunctionButtons.DownloadingProgress.IsIndeterminate = true;
                        }
                    });
                }
            };

            var hadErrors = false;
            string errorMessage = "\n";

            process.ErrorDataReceived += (_, e) => {
                if (!string.IsNullOrEmpty(e.Data)) {
                    if (e.Data.StartsWith("ERROR"))
                        hadErrors = true;

                    errorMessage += e.Data + "\n";
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

                AddSongToSelectedPlaylist(downloadedFileDir, downloadedFileID);
            }
            else {
                yt_dlp_Output.Inlines.Add(new Run("[Error while downloading]") { Foreground = Brushes.IndianRed });

                _log.Print(errorMessage, LogInfoType.ERROR);

                if (!(_log is LogIntoFile)) {
                    yt_dlp_Output.Inlines.Add(new Run("\nSee logs for more info"));
                }
                else {
                    yt_dlp_Output.Inlines.Add(new Run("\nSee "));
                    yt_dlp_Output.Inlines.Add(GetLogsHyperlink());
                    yt_dlp_Output.Inlines.Add(new Run(" for more info"));
                }
            }

            DownloadColumn.Width = new GridLength(100, GridUnitType.Star);
            CancelColumn.Width = new GridLength(0, GridUnitType.Star);

            (Owner as MainWindow).FunctionButtons.DownloadingProgress.Visibility = Visibility.Collapsed;
            DownloadingProgress.Visibility = Visibility.Collapsed;
        }

        private Hyperlink GetLogsHyperlink() {
            var hyperlink = new Hyperlink();
            hyperlink.Inlines.Add("logs");
            hyperlink.NavigateUri = new Uri(((LogIntoFile)_log).LogsPath, UriKind.Absolute);

            hyperlink.RequestNavigate += (_, _) => {
                Process.Start(new ProcessStartInfo {
                    FileName = "notepad.exe",
                    Arguments = hyperlink.NavigateUri.AbsolutePath,
                });
            };

            return hyperlink;
        }

        private void LinkTextBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                Download_Click(null, null);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) { // is also called when window is closed
            if (process != null) {
                process.Close();
                process.Dispose();
            }

            yt_dlp_Output.Text = "";
            yt_dlp_Output.Inlines.Add(new Run("[Canceled]") { Foreground = Brushes.IndianRed });

            DownloadingProgress.Visibility = Visibility.Collapsed;
            (Owner as MainWindow).FunctionButtons.DownloadingProgress.Visibility = Visibility.Collapsed;

            DownloadColumn.Width = new GridLength(100, GridUnitType.Star);
            CancelColumn.Width = new GridLength(0, GridUnitType.Star);
        }

        private void SaveTo_Click(object sender, RoutedEventArgs e) {
            var dialog = new WinForms.FolderBrowserDialog();
            dialog.InitialDirectory = SelectedDirectory;
            var res = dialog.ShowDialog();

            if (res == WinForms.DialogResult.OK) {
                SelectedDirectory = dialog.SelectedPath;
            }
        }

        private void AddSongToSelectedPlaylist(string downloadedFileDir, string downloadedFileID) {
            string downloadedFilePathOld = (new DirectoryInfo(downloadedFileDir)).GetFiles($"[{downloadedFileID}]*")[0].FullName;
            string downloadedFilePath = Regex.Replace(downloadedFilePathOld, @$"\[{downloadedFileID}\]", "");

            if (File.Exists(downloadedFilePath)) {
                File.Delete(downloadedFilePath);
            }

            File.Move(downloadedFilePathOld, downloadedFilePath);

            Song song = new Song { Path = downloadedFilePath };

            if (MusicLibrary.AddSong(song)) {
                Playlist selectedPlaylist = (Owner as MainWindow).SelectedPlaylist;

                if (selectedPlaylist == null) {
                    selectedPlaylist = MusicLibrary.GetPlaylists().FirstOrDefault();

                    if (selectedPlaylist != null) {
                        (Owner as MainWindow).SelectPlaylistByName(selectedPlaylist.Name);

                        MusicLibrary.AddSongToPlaylist(song.Id, selectedPlaylist.Name);
                        (Owner as MainWindow).SongList.List.Items.Add(song);
                    }
                }
                else {
                    MusicLibrary.AddSongToPlaylist(song.Id, selectedPlaylist.Name);
                    (Owner as MainWindow).SongList.List.Items.Add(song);
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            Helper.FindVisualChildren<Grid>(this).FirstOrDefault().Focus();
        }
    }
}
