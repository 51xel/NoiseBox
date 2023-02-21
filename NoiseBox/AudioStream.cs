using NAudio.Wave;
using NoiseBox.Log;

namespace NoiseBox {
    public class AudioStream {
        private WaveOutEvent _outputDevice;
        private AudioFileReader _audioFile;
        private ILog _log;

        public double CurrentTrackLength {
            get {
                if (_audioFile != null) {
                    return _audioFile.Length / _audioFile.WaveFormat.AverageBytesPerSecond;
                }
                else {
                    return 0;
                }
            }
        }

        public double CurrentTrackPosition {
            get {
                if (_audioFile != null) {
                    return _audioFile.Position / _audioFile.WaveFormat.AverageBytesPerSecond;
                }
                else {
                    return 0;
                }
            }
            set {
                if (_audioFile != null) {
                    long newPos = (long)(_audioFile.WaveFormat.AverageBytesPerSecond * value); // value in seconds

                    if ((newPos % _audioFile.WaveFormat.BlockAlign) != 0) {
                        newPos -= newPos % _audioFile.WaveFormat.BlockAlign;
                    }

                    newPos = Math.Max(0, Math.Min(_audioFile.Length, newPos));
                    
                    _audioFile.Position = newPos;
                }
            }
        }

        public void Seek(double offset) {
            CurrentTrackPosition += offset;
        }

        public string PathToMusic { get; set; }

        public float Volume {
            get {
                return _audioFile.Volume;
            }
            set {
                if (value < 0f || value > 1f) {
                    _log.Print("[ERROR][AudioStream] Volume < 0.0 or > 1.0", LogInfoType.ERROR);

                    if (value < 0) {
                        _audioFile.Volume = 0;
                    }
                    else {
                        _audioFile.Volume = 1;
                    }
                }
                else {
                    _audioFile.Volume = value;
                }
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

            _log.Print("[INFO][AudioStream] Device has been selected", LogInfoType.INFO);
        }

        public void Play() {
            if (PathToMusic == null || String.IsNullOrWhiteSpace(PathToMusic)) {
                _log.Print("[ERROR][AudioStream] Path to music can`t be null", LogInfoType.ERROR);
            }
            else if (!File.Exists(PathToMusic)) {
                _log.Print("[WARNING][AudioStream] File does not exist", LogInfoType.WARNING);
            }
            else {
                _log.Print("[INFO][AudioStream] Playing to " + DeviceControll.GetNameById(_outputDevice.DeviceNumber), LogInfoType.INFO);

                if (IsPaused) {
                    _outputDevice.Play();
                }
                else {
                    _audioFile = new AudioFileReader(PathToMusic);
                    _outputDevice.Init(_audioFile);
                    _outputDevice.Play();

                    //TEMPORARY while for test
                    //while (IsPlaying) {
                    //    Thread.Sleep(1000);
                    //}
                    //========================
                }
            }
            
        }

        public void Stop() {
            _log.Print("[INFO][AudioStream] Stopped " + DeviceControll.GetNameById(_outputDevice.DeviceNumber), LogInfoType.INFO);

            _outputDevice.Stop();
        }

        public void Pause() {
            _log.Print("[INFO][AudioStream] Paused " + DeviceControll.GetNameById(_outputDevice.DeviceNumber), LogInfoType.INFO);

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

        public bool IsStopped {
            get {
                return _outputDevice.PlaybackState == PlaybackState.Stopped;
            }
        }
    }
}
