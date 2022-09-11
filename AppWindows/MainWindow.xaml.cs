using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using ScreenRecorderLib;
using Windows.UI.ViewManagement;
using System.Timers;
using Windows.UI.Core;
using BeRecorderWinUI3.Helpers;
using WindowActivatedEventArgs = Microsoft.UI.Xaml.WindowActivatedEventArgs;
using System.Diagnostics;
using NAudio.CoreAudioApi;
using BeRecorderWinUI3.Views;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.System;
using Windows.Storage;
using NAudio.Wave;
using WinUIEx;
using BeRecorderWinUI3.Managers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BeRecorderWinUI3.AppWindows
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public static MainWindow Current;
        public SettingsWindow SettingsWindow = new SettingsWindow();

        // Recording
        private Recorder Rec;
        private Timer RecorderTimer;
        private int RecorderTimeElapsedSeconds;
        private int LastAmountOfFrames;
        private string RecordedFileName;

        // Audio
        WaveIn AudioWaveRecorder;
        List<AudioDevice> InputDevices;
        List<AudioDevice> OutputDevices;

        // Timers
        Timer UpdateValuesTimer;

        public MainWindow()
        {
            this.InitializeComponent();

            Current = this;

            InitializeWindow();
        }

        private void InitializeWindow()
        {
            // Set Mica background
            new MicaBackground().TrySetMicaBackdrop(this);

            // Set custom title bar
            this.ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            // Set window size
            var manager = WindowManager.Get(this);
            manager.MinWidth = 500;
            manager.MinHeight = 400;
            manager.Width = 500;
            manager.Height = 400;

            // Initialize recorder
            InitializeRecorder();
        }

        public void InitializeRecorder()
        {
            // Clear variables
            if (Rec != null) Rec.Dispose();

            // Initialize recorder
            Rec = Recorder.CreateRecorder();
            Rec.OnRecordingComplete += Rec_OnRecordingComplete;
            Rec.OnRecordingFailed += Rec_OnRecordingFailed;
            Rec.OnStatusChanged += Rec_OnStatusChanged;
            Rec.OnFrameRecorded += Rec_OnFrameRecorded;

            RecorderTimer = new Timer();
            RecorderTimer.Interval = 1000;
            RecorderTimer.Elapsed += (sender, e) =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    RecorderTimeElapsedSeconds++;
                    ElapsedTimeTextBlock.Text = $"{new TimeSpan(0, 0, RecorderTimeElapsedSeconds).ToString("mm':'ss")}";

                    // Show framerate
                    FramerateTextBlock.Text = $"{Rec.CurrentFrameNumber - LastAmountOfFrames} FPS ({Rec.CurrentFrameNumber} Frames)";
                    LastAmountOfFrames = Rec.CurrentFrameNumber;
                });
            };

            // Load Settings
            ResetRecordingInfo();

            // Initialize audio devices
            // Check if user has input devices
            if (Recorder.GetSystemAudioDevices(AudioDeviceSource.InputDevices).Count != 0)
            {
                if (AudioWaveRecorder == null)
                {
                    AudioWaveRecorder = new WaveIn();
                    AudioWaveRecorder.WaveFormat = new WaveFormat(44100, 1);
                    AudioWaveRecorder.StartRecording();
                }
                if (AudioWaveRecorder.DeviceNumber != App.Settings.Sound.InputDevice) AudioWaveRecorder.DeviceNumber = App.Settings.Sound.InputDevice;
            }
            InputSlider.Value = App.Settings.Sound.InputDeviceVolume;
            OutputSlider.Value = App.Settings.Sound.OutputDeviceVolume;

            // Set Recorder options
            SetRecorderOptions();

            // Update values timer
            UpdateValuesTimer = new Timer();
            UpdateValuesTimer.Interval = 100;
            UpdateValuesTimer.Elapsed += (sender, e) =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    // Show audio levels
                    AudioInputProgressBar.Value = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)[App.Settings.Sound.InputDevice].AudioMeterInformation.MasterPeakValue * 400 * (InputSlider.Value / 100);
                    AudioOutputProgressBar.Value = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)[App.Settings.Sound.OutputDevice].AudioMeterInformation.MasterPeakValue * 800 * (OutputSlider.Value / 100);
                });
            };

            UpdateValuesTimer.Start();
        }

        private void SetRecorderOptions()
        {
            var settings = App.Settings;

            Rec.SetOptions(new RecorderOptions
            {
                AudioOptions = new AudioOptions
                {
                    IsAudioEnabled = true,
                    IsOutputDeviceEnabled = true,
                    IsInputDeviceEnabled = true,
                    AudioOutputDevice = Recorder.GetSystemAudioDevices(AudioDeviceSource.OutputDevices)[App.Settings.Sound.OutputDevice].DeviceName,
                    AudioInputDevice = Recorder.GetSystemAudioDevices(AudioDeviceSource.InputDevices)[App.Settings.Sound.InputDevice].DeviceName,
                    InputVolume = (float?)InputSlider.Value / 50,
                    OutputVolume = (float?)OutputSlider.Value / 50,
                },
                VideoEncoderOptions = new VideoEncoderOptions
                {
                    Bitrate = settings.Output.Bitrate * 1000,
                    Framerate = settings.Video.FrameRate,
                    IsFixedFramerate = settings.Video.FixedFrameRate,
                    //Currently supported are H264VideoEncoder and H265VideoEncoder
                    Encoder = new H264VideoEncoder
                    {
                        BitrateMode = H264BitrateControlMode.CBR,
                        EncoderProfile = H264Profile.Baseline,
                    },
                    //Fragmented Mp4 allows playback to start at arbitrary positions inside a video stream,
                    //instead of requiring to read the headers at the start of the stream.
                    IsFragmentedMp4Enabled = true,
                    //If throttling is disabled, out of memory exceptions may eventually crash the program,
                    //depending on encoder settings and system specifications.
                    IsThrottlingDisabled = false,
                    //Hardware encoding is enabled by default.
                    IsHardwareEncodingEnabled = true,
                    //Low latency mode provides faster encoding, but can reduce quality.
                    IsLowLatencyEnabled = false,
                    //Fast start writes the mp4 header at the beginning of the file, to facilitate streaming.
                    IsMp4FastStartEnabled = false
                },
                OutputOptions = new OutputOptions
                {
                    IsVideoCaptureEnabled = true,
                    RecorderMode = RecorderMode.Video,
                    SourceRect = new ScreenRect(0, 0, App.Settings.Video.BaseResolution.Width, App.Settings.Video.BaseResolution.Height),
                    OutputFrameSize = new ScreenSize(App.Settings.Video.OutputResolution.Width, App.Settings.Video.OutputResolution.Height)
                }
            });
        }

        public void ShowVideoSavedNotification()
        {
            ToastNotifier ToastNotifier = ToastNotificationManager.CreateToastNotifier();
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);

            XmlNodeList toastNodeList = toastXml.GetElementsByTagName("text");
            toastNodeList.Item(0).AppendChild(toastXml.CreateTextNode("Video saved"));
            toastNodeList.Item(1).AppendChild(toastXml.CreateTextNode("Show recorded video in a folder."));

            XmlElement audio = toastXml.CreateElement("audio");
            audio.SetAttribute("src", "ms-winsoundevent:Notification.SMS");

            ToastNotification toast = new ToastNotification(toastXml);
            toast.ExpirationTime = DateTime.Now.AddSeconds(4);
            ToastNotifier.Show(toast);

            toast.Activated += async (sender, e) =>
            {
                FolderLauncherOptions folderLauncherOptions = new FolderLauncherOptions();
                folderLauncherOptions.ItemsToSelect.Add(await (await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(RecordedFileName))).GetFileAsync(Path.GetFileName(RecordedFileName)));

                await Launcher.LaunchFolderAsync(await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(RecordedFileName)), folderLauncherOptions);
            };
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState == WindowActivationState.Deactivated)
            {
                AppTitleTextBlock.Foreground = (SolidColorBrush)App.Current.Resources["WindowCaptionForegroundDisabled"];
                AppTitleFontIcon.Foreground = (SolidColorBrush)App.Current.Resources["WindowCaptionForegroundDisabled"];
            }
            else
            {
                AppTitleTextBlock.Foreground = (SolidColorBrush)App.Current.Resources["WindowCaptionForeground"];
                AppTitleFontIcon.Foreground = (SolidColorBrush)App.Current.Resources["WindowCaptionForeground"];
            }
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            UpdateValuesTimer.Stop();
            UpdateValuesTimer.Dispose();
            RecorderTimer.Stop();
            RecorderTimer.Dispose();
            App.Current.Exit();
        }

        private void StartRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            StartRecordingButton.IsEnabled = false;
            (StartRecordingButton.Content as FontIcon).Opacity = 0.5;

            StopRecordingButton.IsEnabled = true;

            SetRecorderOptions();
            CreateRecording();
            ElapsedTimeTextBlock.Text = "00:00";
            RecorderTimeElapsedSeconds = 0;
            LastAmountOfFrames = 0;
            RecorderTimer.Start();
        }

        private void StopRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            RecorderTimer.Stop();
            Rec.Stop();
            ShowVideoSavedNotification();

            StartRecordingButton.IsEnabled = true;
            (StartRecordingButton.Content as FontIcon).Opacity = 1;

            ResetRecordingInfo();

            StopRecordingButton.IsEnabled = false;
        }

        private void ResetRecordingInfo()
        {
            var settings = App.Settings;
            BitrateTextBlock.Text = $"{settings.Output.Bitrate} kb/s";
            FramerateTextBlock.Text = $"{settings.Video.FrameRate} FPS ({Rec.CurrentFrameNumber} Frames)";
        }

        private void RecordingSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow ??= new SettingsWindow();
            SettingsWindow.Activate();
        }

        private void MicrophoneButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void InputSlider_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            await App.Settings.Save();
        }

        private async void OutputSlider_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            await App.Settings.Save();
        }

        #region Recorder

        void CreateRecording()
        {
            //Record to a file
            RecordedFileName = Path.Combine(App.Settings.Output.OutputPath, $"{DateTime.Now.ToString("yyyyMMdd-HHmm-ss")}.mp4");
            Rec.Record(RecordedFileName);
        }

        private async void Rec_OnFrameRecorded(object sender, FrameRecordedEventArgs e)
        {
            //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ElapsedTimeTextBlock.Text = $"Time elapsed");
        }

        private void Rec_OnRecordingComplete(object sender, RecordingCompleteEventArgs e)
        {
            //Get the file path if recorded to a file
            string path = e.FilePath;
        }
        private void Rec_OnRecordingFailed(object sender, RecordingFailedEventArgs e)
        {
            string error = e.Error;
        }
        private void Rec_OnStatusChanged(object sender, RecordingStatusEventArgs e)
        {
            RecorderStatus status = e.Status;
        }

        #endregion

        private void InputSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Rec.GetDynamicOptionsBuilder().SetDynamicAudioOptions(new DynamicAudioOptions { InputVolume = (float?)e.NewValue / 50 }).Apply();
        }

        private void OutputSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Rec.GetDynamicOptionsBuilder().SetDynamicAudioOptions(new DynamicAudioOptions { OutputVolume = (float?)e.NewValue / 50 }).Apply();
        }
    }
}
