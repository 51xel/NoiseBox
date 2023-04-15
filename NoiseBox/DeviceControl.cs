using NAudio.Wave;

namespace NoiseBox {
    public class DeviceControll {
        public static int GetOutputDeviceId(string nameDevice) {
            if (String.IsNullOrWhiteSpace(nameDevice)) {
                throw new ArgumentNullException("Device name can`t be null");
            }

            for (int n = -1; n < WaveOut.DeviceCount; n++) {
                if (nameDevice == WaveOut.GetCapabilities(n).ProductName) {
                    return n;
                }
            }

            return 0;
        }

        public static int GetInputDeviceId(string nameDevice) {
            if (String.IsNullOrWhiteSpace(nameDevice)) {
                throw new ArgumentNullException("Device name can`t be null");
            }

            for (int n = -1; n < WaveIn.DeviceCount; n++) {
                if (nameDevice == WaveIn.GetCapabilities(n).ProductName) {
                    return n;
                }
            }

            return 0;
        }

        public static List<string> GetOutputDevicesList() {
            var list = new List<string>();

            for (int n = -1; n < WaveOut.DeviceCount; n++) {
                list.Add(WaveOut.GetCapabilities(n).ProductName);
            }

            return list;
        }

        public static List<string> GetInputDevicesList() {
            var list = new List<string>();

            for (int n = -1; n < WaveIn.DeviceCount; n++) {
                list.Add(WaveIn.GetCapabilities(n).ProductName);
            }

            return list;
        }

        public static string GetOutputDeviceNameById(int id) {
            return WaveOut.GetCapabilities(id).ProductName;
        }

        public static string GetInputDeviceNameById(int id) {
            return WaveIn.GetCapabilities(id).ProductName;
        }
    }
}
