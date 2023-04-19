using NAudio.Extras;
using NAudio.Wave;
using NoiseBox.Log;
using System;
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace NoiseBox {
    public class AudioStream {
        protected WaveOutEvent _outputDevice;
        protected ILog _log = LogSettings.SelectedLog;

        public event EventHandler<EventArgs> StoppedEvent;

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

        protected void SelectOutputDevice(string deviceName) {
            if (String.IsNullOrWhiteSpace(deviceName)) {
                _log.Print("Device name can`t be null", LogInfoType.ERROR);

                throw new ArgumentNullException("Device name can`t be null");
            }
            else {
                _outputDevice = new WaveOutEvent() {
                    DeviceNumber = DeviceControll.GetOutputDeviceId(deviceName)
                };

                _outputDevice.PlaybackStopped += PlaybackStopped;

                _log.Print("Device has been selected", LogInfoType.INFO);
            }
        }

        private void PlaybackStopped(object sender, EventArgs e) {
            if (StoppedEvent != null) {
                StoppedEvent(sender, e);
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

        private Equalizer _equalizer;
        private EqualizerBand[] _bands;

        private float _musicVolume;

        public float MusicVolume {
            get {
                if (_audioFile != null) {
                    return _audioFile.Volume;
                }
                else {
                    return _musicVolume;
                }
            }
            set {
                if (value < 0f || value > 1f) {
                    _log.Print("Volume < 0.0 or > 1.0", LogInfoType.ERROR);

                    if (value < 0) {
                        _musicVolume = 0f;
                    }
                    else {
                        _musicVolume = 1f;
                    }
                }
                else {
                    _musicVolume = value;
                }

                if (_audioFile != null) {
                    _audioFile.Volume = _musicVolume;
                }
            }
        }

        public MusicStream(string deviceName) : base(deviceName) { }

        public override void Play() {
            if (_pathToMusic != null) {
                if (!IsPlaying && !IsPaused) {
                    _audioFile = new AudioFileReader(_pathToMusic);
                    if (_equalizer != null) {
                        _equalizer = new Equalizer(_audioFile, _bands);
                        _outputDevice.Init(_equalizer);
                    }
                    else {
                        _outputDevice.Init(_audioFile);
                    }
                }

                base.Play();
            }
        }

        public void StopAndPlayFromPosition(double startingPosition) {
            float oldVol = MusicVolume;

            Stop();

            _audioFile = new AudioFileReader(_pathToMusic);
            _audioFile.CurrentTime = TimeSpan.FromSeconds(startingPosition);

            if (_equalizer != null) {
                _equalizer = new Equalizer(_audioFile, _bands);
                _outputDevice.Init(_equalizer);
            }
            else {
                _outputDevice.Init(_audioFile);
            }

            MusicVolume = oldVol;

            base.Play();
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

        public void CloseStream() {
            Stop();
            StopEqualizer();

            _outputDevice.Dispose();
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
                    return _audioFile.TotalTime.TotalSeconds;
                }
                else {
                    return 0;
                }
            }
        }

        public double CurrentTrackPosition {
            get {
                if (_audioFile != null) {
                    return _audioFile.CurrentTime.TotalSeconds;
                }
                else {
                    return 0;
                }
            }
            set {
                if (_audioFile != null) {
                    _audioFile.CurrentTime = TimeSpan.FromSeconds(value);
                }
            }
        }

        public void Seek(double offset) {
            CurrentTrackPosition += offset;
        }

        public void InitializeEqualizer() {
            if (_audioFile != null) {
                _bands = new EqualizerBand[]{
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 40, Gain = 0},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 80, Gain = 0},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 320, Gain = 0},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 640, Gain = 0},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 1280, Gain = 0},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 2560, Gain = 0},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 5120, Gain = 0},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 10240, Gain = 0},
                };

                _equalizer = new Equalizer(_audioFile, _bands);

                _log.Print("Initialize equalizer", LogInfoType.INFO);
            }
        }

        public void StopEqualizer() {
            _bands = null;

            _equalizer = null;

            _log.Print("Stop equalizer", LogInfoType.INFO);
        }

        public bool IsEqualizerWorking {
            get { return _equalizer != null; }
        }

        public float MinimumGain => -30;

        public float MaximumGain => 30;

        public float GetBandGain(int index) {
            if (_bands != null && index >= 0 && index <= 7) {
                return _bands[index].Gain;
            }
            else {
                return 0;
            }
        }

        public void SetBandGain(int index, float value) {
            if (_bands != null && index >= 0 && index <= 7) {
                if (_bands[index].Gain != value) {
                    _bands[index].Gain = value;
                    _equalizer.Update();
                }
            }
        }

        public List<EqualizerBand> GetBandsList() {
            var equalizerBands = new List<EqualizerBand>();

            foreach (var band in _bands) {
                equalizerBands.Add(new EqualizerBand {
                    Bandwidth = band.Bandwidth,
                    Frequency = band.Frequency,
                    Gain = band.Gain
                });
            }

            return equalizerBands;
        }

        public void SetBandsList(List<EqualizerBand> equalizerBandsToAdd) {
            if (equalizerBandsToAdd.Count == 8) {
                for (int i = 0; i < 8; i++) {
                    _bands[i] = new EqualizerBand {
                        Bandwidth = equalizerBandsToAdd[i].Bandwidth,
                        Frequency = equalizerBandsToAdd[i].Frequency,
                        Gain = equalizerBandsToAdd[i].Gain
                    };
                }
                _equalizer.Update();
            }
        }

        public void ReselectOutputDevice(string deviceName) {
            if (IsPlaying) {
                var tempPosition = CurrentTrackPosition;
                var tempDeviceVolume = OutputDeviceVolume;
                var tempMusicVolume = MusicVolume;

                Stop();

                _outputDevice.Dispose();

                SelectOutputDevice(deviceName);

                StopAndPlayFromPosition(tempPosition);

                OutputDeviceVolume = tempDeviceVolume;
                MusicVolume = tempMusicVolume;
            }
            else {
                _outputDevice.Dispose();

                SelectOutputDevice(deviceName);
            }
        }
    }

    public class BandsSettings {
        public List<EqualizerBand> EqualizerBands;
        public string Name;
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

        public MicrophoneStream(string deviceIn, string deviceOut) : base(deviceOut) {
            SelectInputDevice(deviceIn);
        }

        public void SelectInputDevice(string deviceName) {
            if (String.IsNullOrWhiteSpace(deviceName)) {
                _log.Print("Name of input device can`t be null", LogInfoType.ERROR);

                throw new ArgumentNullException("Device name can`t be null");
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