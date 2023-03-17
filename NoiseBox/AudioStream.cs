using NAudio.Wave;
using NoiseBox.Log;

namespace NoiseBox {
    public class AudioStream {
        protected WaveOutEvent _outputDevice;
        protected ILog _log = LogSettings.SelectedLog;

        public float DeviceOutVolume {
            get {
                return _outputDevice.Volume;
            }
            set {
                if (value < 0f || value > 1f) {
                    _log.Print("Volume < 0.0 or > 1.0", LogInfoType.ERROR);

                    if (value < 0) {
                        _outputDevice.Volume = 0f;
                    }
                    else {
                        _outputDevice.Volume = 1f;
                    }
                }
                else {
                    _outputDevice.Volume = value;
                }
            }
        }

        public AudioStream(string deviceName) {
            SelectDeviceOut(deviceName);
        }

        public void SelectDeviceOut(string deviceName) {
            if (deviceName == null || String.IsNullOrWhiteSpace(deviceName)) {
                _log.Print("Name device can`t be null", LogInfoType.ERROR);
            }

            _outputDevice = new WaveOutEvent() { 
                DeviceNumber = DeviceControll.GetDeviceOutId(deviceName)
            };

            _log.Print("Device has been selected", LogInfoType.INFO);
        }

        public virtual void Play() { 
            _log.Print("Playing to " + DeviceControll.GetDeviceOutNameById(_outputDevice.DeviceNumber), LogInfoType.INFO);
        }

        public virtual void Stop() {
            _log.Print("Stopped " + DeviceControll.GetDeviceOutNameById(_outputDevice.DeviceNumber), LogInfoType.INFO);

            _outputDevice.Stop();
        }

        public virtual void Pause() {
            _log.Print("Paused " + DeviceControll.GetDeviceOutNameById(_outputDevice.DeviceNumber), LogInfoType.INFO);

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

        public int GetDeviceId() {
            return _outputDevice.DeviceNumber;
        }
    }

    public class MusicStream : AudioStream {
        private AudioFileReader _audioFile;
        private string _pathToMusic;

        public float MusicVolume {
            get {
                return _audioFile.Volume;
            }
            set {
                if (value < 0f || value > 1f) {
                    _log.Print("Volume < 0.0 or > 1.0", LogInfoType.ERROR);

                    if (value < 0) {
                        _audioFile.Volume = 0f;
                    }
                    else {
                        _audioFile.Volume = 1f;
                    }
                }
                else {
                    _audioFile.Volume = value;
                }
            }
        }

        public MusicStream(string pathToMusic, string deviceName = "CABLE Input (VB-Audio Virtual C") : base(deviceName) {
            PathToMusic = pathToMusic;

            _audioFile = new AudioFileReader(_pathToMusic);
        }

        public override void Play() {
            base.Play();

            if(!IsPlaying && !IsPaused){
                _audioFile = new AudioFileReader(_pathToMusic);
                _outputDevice.Init(_audioFile);
            }

            _outputDevice.Play();
        }

        public string PathToMusic {
            get {
                return _pathToMusic;
            }
            set {
                if (value == null || String.IsNullOrWhiteSpace(value)) {
                    _log.Print("Path to music can`t be null", LogInfoType.ERROR);
                }
                else if (!File.Exists(value)) {
                    _log.Print("File does not exist", LogInfoType.WARNING);
                }
                else {
                    _pathToMusic = value;
                }
            }
        }
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
    }

    public class MicrophoneStream : AudioStream {
        private WaveInEvent _waveSource;
        private WaveInProvider _waveIn;
        private VolumeWaveProvider16 _waveInVolume;

        public float DeviceInVolume {
            get {
                return _waveInVolume.Volume;
            }
            set {
                if (value < 0f || value > 1f) {
                    _log.Print("Volume < 0.0 or > 1.0", LogInfoType.ERROR);

                    if (value < 0) {
                        _waveInVolume.Volume = 0f;
                    }
                    else {
                        _waveInVolume.Volume = 1f;
                    }
                }
                else {
                    _waveInVolume.Volume = value;
                }
            }
        }

        public MicrophoneStream(string deviceIn, string deviceOut = "CABLE Input (VB-Audio Virtual C") : base(deviceOut) {
            SelectDeviceIn(deviceIn);
        }

        public void SelectDeviceIn(string deviceName) {
            if (deviceName == null || String.IsNullOrWhiteSpace(deviceName)) {
                _log.Print("Name device can`t be null", LogInfoType.ERROR);
            }

            _waveSource = new WaveInEvent() {
                DeviceNumber = DeviceControll.GetDeviceInId(deviceName)
            };

            _waveSource.WaveFormat = new WaveFormat(44100, WaveIn.GetCapabilities(DeviceControll.GetDeviceInId(deviceName)).Channels);

            _log.Print("Device has been selected", LogInfoType.INFO);
        }

        public override void Play() {
            base.Play();

            if (!IsPlaying && !IsPaused) {
                _waveIn = new WaveInProvider(_waveSource);
                _waveInVolume = new VolumeWaveProvider16(_waveIn);

                _outputDevice.Init(_waveInVolume);
            }

            _waveSource.StartRecording();
            _outputDevice.Play();
        }

        public override void Stop() {
            base.Stop();

            _waveSource.StopRecording();
        }

        public override void Pause() {
            base.Pause();

            _waveSource.StopRecording();
        }
    }
}
