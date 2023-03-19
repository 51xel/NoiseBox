using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoiseBox.Log;

namespace NoiseBox {
    public class AudioStreamControl {
        protected ILog _log = LogSettings.SelectedLog;

        private string _outputDevice;
        private string _secondOutputDevice;

        private string _inputDevice;

        public MusicStream MainMusic;
        public MusicStream AdditionalMusic;

        public readonly MicrophoneStream Microphone;

        public AudioStreamControl(string outputDevice) {
            if (String.IsNullOrWhiteSpace(outputDevice)) {
                _log.Print("Name device can`t be null", LogInfoType.ERROR);
            }
            else {
                _outputDevice = outputDevice;

                MainMusic = new MusicStream(_outputDevice);
            }
        }
    }
}
