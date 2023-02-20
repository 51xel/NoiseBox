using NAudio.Wave;

namespace wg_pad_library {
    public class DeviceControll {
        public static int GetDeviceId(string nameDevice) {
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

        public static List<string> GetListDevices() {
            var list = new List<string>();

            for (int n = -1; n < WaveOut.DeviceCount; n++) {
                list.Add(WaveOut.GetCapabilities(n).ProductName);
            }

            return list;
        }
    }
}
