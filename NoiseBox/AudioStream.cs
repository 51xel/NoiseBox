using NAudio.Wave;
using NoiseBox.Log;

namespace NoiseBox {
    public class AudioStream {
        protected WaveOutEvent _outputDevice;
        protected ILog _log = LogSettings.SelectedLog;

        public float OutputDeviceVolume {
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
            SelectOutputDevice(deviceName);
        }

        public void SelectOutputDevice(string deviceName) {
            if (String.IsNullOrWhiteSpace(deviceName)) {
                _log.Print("Name device can`t be null", LogInfoType.ERROR);

                throw new ArgumentNullException("Name device can`t be null");
            }
            else {
                _outputDevice = new WaveOutEvent() {
                    DeviceNumber = DeviceControll.GetOutputDeviceId(deviceName)
                };

                _log.Print("Device has been selected", LogInfoType.INFO);
            }
        }

        public virtual void Play() { 
            _log.Print("Playing to " + DeviceControll.GetOutputDeviceNameById(_outputDevice.DeviceNumber), LogInfoType.INFO);

            _outputDevice.Play();
        }

        public virtual void Stop() {
            _log.Print("Stopped " + DeviceControll.GetOutputDeviceNameById(_outputDevice.DeviceNumber), LogInfoType.INFO);

            _outputDevice.Stop();
        }

        public virtual void Pause() {
            _log.Print("Paused " + DeviceControll.GetOutputDeviceNameById(_outputDevice.DeviceNumber), LogInfoType.INFO);

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

        public int GetOutputDeviceId() {
            return _outputDevice.DeviceNumber;
        }
    }

    public class MusicStream : AudioStream {
        private AudioFileReader _audioFile;
        private string _pathToMusic;

        public float MusicVolume {
            get {
                if (_audioFile != null) {
                    return _audioFile.Volume;
                }
                else {
                    return 0;
                }
            }
            set {
                if (_audioFile != null) {
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
        }

        public MusicStream(string deviceName = "CABLE Input (VB-Audio Virtual C") : base(deviceName) {}

        public override void Play() {
            if (_pathToMusic != null) {
                if (!IsPlaying && !IsPaused) {
                    _audioFile = new AudioFileReader(_pathToMusic);
                    _outputDevice.Init(_audioFile);
                }

                base.Play();
            }
        }

        public override void Stop() {
            base.Stop();

            Dispose();
        }

        private void Dispose() {
            if (_audioFile != null) {
                _audioFile.Dispose();
                _audioFile = null;
            }
        }

        public string PathToMusic {
            get {
                return _pathToMusic;
            }
            set {
                if (!File.Exists(value)) {
                    _log.Print("File does not exist", LogInfoType.WARNING);

                    throw new FileNotFoundException("File does not exist");
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

        public float InputDeviceVolume {
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
            SelectInputDevice(deviceIn);
        }

        public void SelectInputDevice(string deviceName) {
            if (String.IsNullOrWhiteSpace(deviceName)) {
                _log.Print("Name of input device can`t be null", LogInfoType.ERROR);

                throw new ArgumentNullException("Name device can`t be null");
            }
            else {
                _waveSource = new WaveInEvent() {
                    DeviceNumber = DeviceControll.GetInputDeviceId(deviceName)
                };

                _waveSource.WaveFormat = new WaveFormat(44100, WaveIn.GetCapabilities(DeviceControll.GetInputDeviceId(deviceName)).Channels);

                _log.Print("Input device has been selected", LogInfoType.INFO);
            }
        }

        public override void Play() {
            if (!IsPlaying && !IsPaused) {
                _waveIn = new WaveInProvider(_waveSource);
                _waveInVolume = new VolumeWaveProvider16(_waveIn);

                _outputDevice.Init(_waveInVolume);
            }

            _waveSource.StartRecording();
            base.Play();
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
