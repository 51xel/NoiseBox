using NAudio.Wave;
using wg_pad_library.Log;

namespace wg_pad_library {
    public class AudioStream {
        private WaveOutEvent _outputDevice;
        private ILog _log;

        public string PathToMusic { get; set; }

        public float Volume {
            get {
                return _outputDevice.Volume;
            }
            set {
                if (value < 0 || value > 100) {
                    _log.Print("[ERROR][AudioStream] Volume < 0 or > 100", LogInfoType.ERROR);
                }

                _outputDevice.Volume = value;
            }
        }

        public AudioStream(ILog log, string nameDevice = "CABLE Input (VB-Audio Virtual C") {
            if(log == null) {
                throw new ArgumentNullException("Log can`t be null");
            }

            _log = log;

            SelectDevice(nameDevice);
        }

        public void SelectDevice(string nameDevice) {
            if (nameDevice == null || String.IsNullOrWhiteSpace(nameDevice)) {
                _log.Print("[ERROR][AudioStream] Name device can`t be null", LogInfoType.ERROR);
            }

            _outputDevice = new WaveOutEvent() { 
                DeviceNumber = DeviceControll.GetDeviceId(nameDevice)
            };
        }

        public void Play() {
            if (PathToMusic == null || String.IsNullOrWhiteSpace(PathToMusic)) {
                _log.Print("[ERROR][AudioStream] Path to music can`t be null", LogInfoType.ERROR);
            }
            else if (!File.Exists(PathToMusic)) {
                _log.Print("[WARNING][AudioStream] File does not exist", LogInfoType.WARNING);
            }
            else {
                if (IsPaused) {
                    _outputDevice.Play();
                }
                else {
                    using (var audioFile = new AudioFileReader(PathToMusic)) {
                        _outputDevice.Init(audioFile);
                        _outputDevice.Play();

                        //TEMPORARY while for test
                        while (IsPlaying) {
                            Thread.Sleep(1000);
                        }
                        //========================
                    }
                }
            }
        }

        public void Stop() {
            _outputDevice.Stop();
        }

        public void Pause() {
            _outputDevice.Pause();
        }

        public bool IsPlaying {
            get {
                return _outputDevice.PlaybackState == PlaybackState.Playing;
            }
        }

        public bool IsPaused {
            get {
                return _outputDevice.PlaybackState == PlaybackState.Paused;
            }
        }
    }
}
