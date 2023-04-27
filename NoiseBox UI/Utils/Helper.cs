using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows;
using System.IO;

namespace NoiseBox_UI.Utils {
    internal class Helper {
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject {
            if (depObj != null) {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++) {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                    if (child != null && child is T)
                        yield return (T)child;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }

        public static List<string> GetAllMp3Files(string[] files) { // all mp3 files including in directories and subdirectories
            List<string> mp3Files = new List<string>();

            foreach (var file in files) {
                if (File.Exists(file)) {
                    if (Path.GetExtension(file).Equals(".mp3", StringComparison.OrdinalIgnoreCase)) {
                        mp3Files.Add(file);
                    }
                }
                else if (Directory.Exists(file)) {
                    string dir = file;

                    foreach (string mp3File in Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories)
                        .Where(f => Path.GetExtension(f).Equals(".mp3", StringComparison.OrdinalIgnoreCase))) {

                        mp3Files.Add(mp3File);
                    }
                }
            }

            return mp3Files;
        }
    }
}
