namespace NoiseBox {
    namespace Log {
        public enum LogInfoType {
            INFO,
            WARNING,
            ERROR
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
            public void Print(string message, LogInfoType logType) {
                //Console.WriteLine(message);
            }
        }
    }
}