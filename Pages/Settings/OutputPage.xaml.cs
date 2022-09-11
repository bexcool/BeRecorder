using BeRecorderWinUI3.AppWindows;
using BeRecorderWinUI3.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.WindowsAPICodePack.Dialogs;
using ScreenRecorderLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Forms;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BeRecorderWinUI3.Pages.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OutputPage : Page
    {
        public string[] Encoders = { "H264", "H265/HEVC" };
        public string[] EncoderBitrateModes = { "CBR", "Quality", "Uncontrained VBR" };
        public string[] EncoderProfiles = { "CBR", "Quality", "Uncontrained VBR" };

        public OutputPage()
        {
            this.InitializeComponent();

            DataContext = this;

            InitializePage();
        }

        private void InitializePage()
        {

        }

        private void SelectOutputPathButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();

            dialog.InitialDirectory = SettingsWindow.TempSettings.Output.OutputPath;
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                SettingsWindow.TempSettings.Output.OutputPath = dialog.FileName;
            }
        }
    }
}
