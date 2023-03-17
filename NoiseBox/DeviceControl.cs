using NAudio.Wave;

namespace NoiseBox {
    public class DeviceControll {
        public static int GetDeviceOutId(string nameDevice) {
            if (nameDevice == null || String.IsNullOrWhiteSpace(nameDevice)) {
                throw new ArgumentNullException("Name device can`t be null");
            }

            for (int n = -1; n < WaveOut.DeviceCount; n++) {
                if (nameDevice == WaveOut.GetCapabilities(n).ProductName) {
                    return n;
                }
            }

            return 0;
        }

        public static int GetDeviceInId(string nameDevice) {
            if (nameDevice == null || String.IsNullOrWhiteSpace(nameDevice)) {
                throw new ArgumentNullException("Name device can`t be null");
            }

            for (int n = -1; n < WaveIn.DeviceCount; n++) {
                if (nameDevice == WaveIn.GetCapabilities(n).ProductName) {
                    return n;
                }
            }

            return 0;
        }

        public static List<string> GetListDevicesOut() {
            var list = new List<string>();

            for (int n = -1; n < WaveOut.DeviceCount; n++) {
                list.Add(WaveOut.GetCapabilities(n).ProductName);
            }

            return list;
        }

        public static List<string> GetListDevicesIn() {
            var list = new List<string>();

            for (int n = -1; n < WaveIn.DeviceCount; n++) {
                list.Add(WaveIn.GetCapabilities(n).ProductName);
            }

            return list;
        }

        public static string GetDeviceOutNameById(int id) {
            return WaveOut.GetCapabilities(id).ProductName;
        }

        public static string GetDeviceInNameById(int id) {
            return WaveIn.GetCapabilities(id).ProductName;
        }
    }
}
