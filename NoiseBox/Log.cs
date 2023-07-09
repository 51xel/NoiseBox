using System.Runtime.CompilerServices;

namespace NoiseBox {
    namespace Log {
        public enum LogInfoType {
            INFO,
            WARNING,
            ERROR
        }

        public class LogSettings {
            private static ILog _selectedLog;
            public static ILog SelectedLog {
                get {
                    return _selectedLog;
                }
                set {
                    if (value == null) {
                        throw new ArgumentNullException("Log can`t be null");
                    }

                    _selectedLog = value;
                }
            }

            static LogSettings() {
                SelectedLog = new LogIntoFile();
            }
        }

        public interface ILog {
            void Print(string message, LogInfoType logType, [CallerMemberName] string callerName = "");
        }

        public class LogIntoConsole : ILog {
            public void Print(string message, LogInfoType logType, [CallerMemberName] string callerName = "") {
                Console.WriteLine(message);
            }
        }

        public class LogIntoFile : ILog {
            private string _pathToFile;
            private long _fileSizeLimit = 100000; // 100 KB

            public LogIntoFile(string path = "Logs/logs.txt") {
                if (path == null || String.IsNullOrWhiteSpace(path)) {
                    throw new ArgumentNullException("Path can`t be null");
                }

                _pathToFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NoiseBox", path);

                if (!File.Exists(_pathToFile)) {
                    Directory.CreateDirectory(Path.GetDirectoryName(_pathToFile));

                    File.Create(_pathToFile).Close();
                }

                WriteMessageIntoFile("\n===App Starting [" + DateTime.Now + "]===\n");
            }

            public void Print(string message, LogInfoType logType, [CallerMemberName] string callerName = "") {
                WriteMessageIntoFile($"[{DateTime.Now}][{logType.ToString()}][{callerName}] " + message.Replace("\n", ""));
            }

            private void WriteMessageIntoFile(string message) {
                if (File.Exists(_pathToFile) && new FileInfo(_pathToFile).Length > _fileSizeLimit) {

                    string pathToOldFile = Path.Combine(Path.GetDirectoryName(_pathToFile), Path.GetFileNameWithoutExtension(_pathToFile)) + "_old.txt";

                    if (File.Exists(pathToOldFile)) {
                        File.Delete(pathToOldFile);
                    }

                    File.Move(_pathToFile, pathToOldFile);

                    File.Create(_pathToFile).Close();
                }

                using (StreamWriter writer = new StreamWriter(_pathToFile, true)) {
                    writer.WriteLine(message);
                }
            }

            public string LogsPath => _pathToFile;
        }
    }
}