using BeRecorderWinUI3.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ScreenRecorderLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BeRecorderWinUI3.Pages.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SoundPage : Page
    {
        private List<string> InputDeviceNames = new();
        private List<string> OutputDeviceNames = new();

        public SoundPage()
        {
            this.InitializeComponent();

            DataContext = this;

            InitializePage();
        }

        private void InitializePage()
        {
            // Initialize devices
            var inputDevices = Recorder.GetSystemAudioDevices(AudioDeviceSource.InputDevices);
            foreach (var oDevice in inputDevices) InputDeviceNames.Add(oDevice.FriendlyName);

            var outputDevices = Recorder.GetSystemAudioDevices(AudioDeviceSource.OutputDevices);
            foreach (var oDevice in outputDevices) OutputDeviceNames.Add(oDevice.FriendlyName);
        }
    }
}
