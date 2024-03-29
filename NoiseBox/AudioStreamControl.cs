﻿using NAudio.Extras;
using NoiseBox.Log;

namespace NoiseBox {
    public class AudioStreamControl {
        protected ILog _log = LogSettings.SelectedLog;

        public MusicStream MainMusic;
        public MusicStream AdditionalMusic;

        public MicrophoneStream Microphone;

        private bool _delayedEqualizerInitialization = false;
        private string _selectedBandName;

        public AudioStreamControl(string mainOutputDevice) {
            if (String.IsNullOrWhiteSpace(mainOutputDevice)) {
                _log.Print("Device name can`t be null", LogInfoType.ERROR);
            }
            else {
                MainMusic = new MusicStream(mainOutputDevice);
            }
        }

        public void ActivateAdditionalMusic(string additionalOutputDevice) {
            if (String.IsNullOrWhiteSpace(additionalOutputDevice)) {
                _log.Print("Device name can`t be null", LogInfoType.ERROR);
            }
            else {
                if (MainMusic != null) {
                    if (AdditionalMusic == null) {
                        AdditionalMusic = new MusicStream(additionalOutputDevice);

                        if (PathToMusic != null) {
                            AdditionalMusic.PathToMusic = PathToMusic;

                            if (MainMusic.IsPlaying) {
                                StopAndPlayFromPosition(CurrentTrackPosition);
                            }
                        }
                    }
                    else {
                        AdditionalMusic.ReselectOutputDevice(additionalOutputDevice);
                    }
                }
                else {
                    _log.Print("MainMusic should be initialised", LogInfoType.ERROR);
                }
            }
        }

        public void ActivateMic(string inputDevice, string outputDevice) {
            if (String.IsNullOrWhiteSpace(inputDevice) || String.IsNullOrWhiteSpace(outputDevice)) {
                _log.Print("Device name can`t be null", LogInfoType.ERROR);
            }
            else {
                Microphone = new MicrophoneStream(inputDevice, outputDevice);

                Microphone.Play();
            }
        }

        public void Stop() {
            MainMusic.Stop();

            if (AdditionalMusic != null) {
                AdditionalMusic.Stop();
            }
        }

        public void Play() {
            MainMusic.Play();

            if (AdditionalMusic != null) {
                AdditionalMusic.Play();
            }
        }

        public void Pause() {
            MainMusic.Pause();

            if (AdditionalMusic != null) {
                AdditionalMusic.Pause();
            }
        }

        public void StopAndPlayFromPosition(double startingPosition) {
            MainMusic.StopAndPlayFromPosition(startingPosition);

            if (AdditionalMusic != null) {
                AdditionalMusic.StopAndPlayFromPosition(startingPosition);
            }

            if (_delayedEqualizerInitialization && !String.IsNullOrEmpty(_selectedBandName)) {
                InitializeEqualizer();

                if (MainMusic.IsEqualizerWorking) {
                    EqualizerLibrary.LoadFromJson();

                    var band = EqualizerLibrary.BandsSettings.FirstOrDefault(n => n.Name == _selectedBandName);

                    if (band != null) {
                        SetBandsList(band.EqualizerBands);

                        _log.Print("Profile has been selected", LogInfoType.INFO);
                    }
                }
            }

            _delayedEqualizerInitialization = false;
        }

        public string PathToMusic {
            get {
                return MainMusic.PathToMusic;
            }
            set {
                MainMusic.PathToMusic = value;

                if (AdditionalMusic != null) {
                    AdditionalMusic.PathToMusic = value;
                }
            }
        }

        public double CurrentTrackLength {
            get {
                return MainMusic.CurrentTrackLength;
            }
        }

        public double CurrentTrackPosition {
            get {
                return MainMusic.CurrentTrackPosition;
            }
            set {
                MainMusic.CurrentTrackPosition = value;

                if (AdditionalMusic != null) {
                    AdditionalMusic.CurrentTrackPosition = value;
                }
            }
        }

        public void Seek(double offset) {
            MainMusic.Seek(offset);

            if (AdditionalMusic != null) {
                AdditionalMusic.Seek(offset);
            }
        }

        public void InitializeEqualizer(string selectedBandName = null) {
            MainMusic.InitializeEqualizer();

            if (AdditionalMusic != null) {
                AdditionalMusic.InitializeEqualizer();
            }

            if (!MainMusic.IsEqualizerWorking) {
                _delayedEqualizerInitialization = true;
                _selectedBandName = selectedBandName;
            }
        }

        public void StopEqualizer() {
            MainMusic.StopEqualizer();

            if (AdditionalMusic != null) {
                AdditionalMusic.StopEqualizer();
            }
        }

        public void SetBandGain(int index, float value) {
            MainMusic.SetBandGain(index, value);

            if (AdditionalMusic != null) {
                AdditionalMusic.SetBandGain(index, value);
            }
        }

        public void SetBandsList(List<EqualizerBand> equalizerBandsToAdd) {
            MainMusic.SetBandsList(equalizerBandsToAdd);

            if (AdditionalMusic != null) {
                AdditionalMusic.SetBandsList(equalizerBandsToAdd);
            }
        }
    }
}
