using BeRecorderWinUI3.AppWindows;
using BeRecorderWinUI3.Helpers;
using BeRecorderWinUI3.Managers;
using BeRecorderWinUI3.Pages.FirstSetup;
using BeRecorderWinUI3.Views;
using FirstSetupTools;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BeRecorderWinUI3
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>

        // Windows
        public static Settings Settings { get; set; } = new Settings();
        public static SetupWindow SetupWindow { get; set; } = new SetupWindow(new List<Type> { typeof(SetupIntroPage), typeof(SetupOutroPage) });

        public App()
        {
            this.InitializeComponent();
        }

        private async Task InitializeApplication()
        {
            Debug.WriteLine(ApplicationData.Current.LocalCacheFolder.Path);

            Settings = await Settings.GetSettingsAsync();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            if (!await FileHelper.CacheFileExists("settings.json"))
            {
                new MicaBackground().TrySetMicaBackdrop(SetupWindow);

                SetupWindow.SetupCompleted += () =>
                {
                    m_window = new MainWindow();
                    m_window.Activate();
                };

                SetupWindow.Activate();

                return;
            }

            await InitializeApplication();

            m_window = new MainWindow();
            m_window.Activate();
        }

        private Window m_window;
    }
}
