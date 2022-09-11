using BeRecorderWinUI3.Helpers;
using Newtonsoft.Json;
using ScreenRecorderLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BeRecorderWinUI3.Views
{
    public class Settings
    {
        public VideoSettings Video { get; set; } = new VideoSettings();
        public OutputSettings Output { get; set; } = new OutputSettings();
        public SoundSettings Sound { get; set; } = new SoundSettings();

        public static async Task<Settings> GetSettingsAsync()
        {
            if (await FileHelper.CacheFileExists("settings.json"))
            {
                var settings = JsonConvert.DeserializeObject<Settings>(await FileHelper.CacheReadText("settings.json"));
                if (settings != null) return settings;
            }
            return new Settings();
        }

        public async Task Save()
        {
            await FileHelper.CacheWriteText("settings.json", JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public class VideoSettings
        {
            public VideoSettings() { }

            private int _frameRate = 60;
            public int FrameRate
            {
                get { return _frameRate; }
                set { _frameRate = value; OnPropertyChanged(nameof(FrameRate)); }
            }

            private Size _baseResolution = new Size(1920, 1080);
            public Size BaseResolution
            {
                get { return _baseResolution; }
                set { _baseResolution = value; OnPropertyChanged(nameof(BaseResolution)); }
            }

            private Size _outputResolution = new Size(1920, 1080);
            public Size OutputResolution
            {
                get { return _outputResolution; }
                set { _outputResolution = value; OnPropertyChanged(nameof(OutputResolution)); }
            }

            private bool _fixedFrameRate = true;
            public bool FixedFrameRate
            {
                get { return _fixedFrameRate; }
                set { _fixedFrameRate = value; OnPropertyChanged(nameof(FixedFrameRate)); }
            }

            public event PropertyChangedEventHandler PropertyChanged = delegate { };
            public void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class OutputSettings
        {
            public OutputSettings() { }

            private int _encoderIndex = 0;
            public int EncoderIndex
            {
                get { return _encoderIndex; }
                set { _encoderIndex = value; OnPropertyChanged(nameof(EncoderIndex)); }
            }

            private int _bitrate = 8000;
            public int Bitrate
            {
                get { return _bitrate; }
                set { _bitrate = value; OnPropertyChanged(nameof(Bitrate)); }
            }

            private int _bitrateControlModeIndex = 0;
            public int BitrateControlModeIndex
            {
                get { return _bitrateControlModeIndex; }
                set { _bitrateControlModeIndex = value; OnPropertyChanged(nameof(BitrateControlModeIndex)); }
            }

            private string _outputPath = Path.GetTempPath();
            public string OutputPath
            {
                get { return _outputPath; }
                set { _outputPath = value; OnPropertyChanged(nameof(OutputPath)); }
            }

            public event PropertyChangedEventHandler PropertyChanged = delegate { };
            public void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class SoundSettings
        {
            public SoundSettings() { }

            private int _inputDevice = 0;
            public int InputDevice
            {
                get { return _inputDevice; }
                set { _inputDevice = value; OnPropertyChanged(nameof(InputDevice)); }
            }

            private float _inputDeviceVolume = 50;
            public float InputDeviceVolume
            {
                get { return _inputDeviceVolume; }
                set { if (_inputDeviceVolume != value) { _inputDeviceVolume = value; OnPropertyChanged(nameof(InputDeviceVolume)); } }
            }

            private int _outputDevice = 0;
            public int OutputDevice
            {
                get { return _outputDevice; }
                set { _outputDevice = value; OnPropertyChanged(nameof(OutputDevice)); }
            }

            private float _outputDeviceVolume = 50;
            public float OutputDeviceVolume
            {
                get { return _outputDeviceVolume; }
                set { if (_outputDeviceVolume != value) { _outputDeviceVolume = value; OnPropertyChanged(nameof(OutputDeviceVolume)); } }
            }

            public event PropertyChangedEventHandler PropertyChanged = delegate { };
            public void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
