using System.Text;

namespace NoiseBox {
    namespace Log {
        public enum LogInfoType {
            INFO,
            WARNING,
            ERROR
        }
      
        public class ILogSettings {
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

            static ILogSettings() {
                SelectedLog = new LogIntoFile();
            }
        }

        public interface ILog {
            void Print(string message, LogInfoType logType);
        }

        public class LogIntoConsole : ILog {
            public void Print(string message, LogInfoType logType) {
                Console.WriteLine(message);
            }
        }

        public class LogIntoFile : ILog {
            private string _pathToFile;

            public LogIntoFile(string path = "./logs.txt") {
                if (path == null || String.IsNullOrWhiteSpace(path)) {
                    throw new ArgumentNullException("Path can`t be null");
                }

                _pathToFile = path;

                if (!File.Exists(_pathToFile)) {
                    byte[] buffer = Encoding.Default.GetBytes("===Created " + DateTime.Now + "\n");

                    var tempFile = File.Create(_pathToFile);
                    tempFile.WriteAsync(buffer, 0, buffer.Length);

                    tempFile.Close();
                }
                else {
                    using (StreamWriter writer = new StreamWriter(_pathToFile, true)) {
                        writer.WriteLine("===New Starting [" + DateTime.Now + "]\n");
                    }
                }
            }

            public void Print(string message, LogInfoType logType) {
                using (StreamWriter writer = new StreamWriter(_pathToFile, true)) {
                    writer.WriteLine($"[{DateTime.Now}]" + message);
                }
            }
        }
    }
}