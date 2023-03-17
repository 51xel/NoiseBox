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

            public LogIntoFile(string path = "./Logs/logs.txt") {
                if (path == null || String.IsNullOrWhiteSpace(path)) {
                    throw new ArgumentNullException("Path can`t be null");
                }

                _pathToFile = path;

                if (!File.Exists(_pathToFile)) {
                    if (!Directory.Exists(Path.GetDirectoryName(path))) {
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                    }
                    File.Create(_pathToFile).Close();

                    WriteMessageIntoFile("===Created [" + DateTime.Now + "]\n");
                }
                else {
                    WriteMessageIntoFile("===New Starting [" + DateTime.Now + "]\n");
                }
            }

            public void Print(string message, LogInfoType logType, [CallerMemberName] string callerName = "") {
                WriteMessageIntoFile($"[{DateTime.Now}][{logType.ToString()}][{callerName}] " + message);
            }

            private void WriteMessageIntoFile(string message) {
                using (StreamWriter writer = new StreamWriter(_pathToFile, true)) {
                    writer.WriteLine(message);
                }
            }
        }
    }
}