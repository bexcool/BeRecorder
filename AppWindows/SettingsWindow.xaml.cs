using BeRecorderWinUI3.Helpers;
using BeRecorderWinUI3.Pages.Settings;
using BeRecorderWinUI3.Views;
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
using Windows.UI.Popups;
using WinUIEx;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Window = Microsoft.UI.Xaml.Window;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BeRecorderWinUI3.AppWindows
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsWindow : Window
    {
        public static SettingsWindow Current { get; set; }
        internal static Settings TempSettings;

        public SettingsWindow()
        {
            this.InitializeComponent();

            Current = this;

            InitializeWindow();
        }

        private async void InitializeWindow()
        {
            // Get window manager
            var manager = WindowManager.Get(this);

            // Set Mica background
            new MicaBackground().TrySetMicaBackdrop(this);

            // Set custom title bar
            this.ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            // Set window size
            manager.MinWidth = 600;
            manager.MinHeight = 400;
            manager.Width = 800;
            manager.Height = 550;

            // Prepare temp settings
            TempSettings = await Settings.GetSettingsAsync();

            // Set active navigation item
            NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems[0];
            MainFrame.Navigate(typeof(VideoPage));
        }

        private void NavigationViewControl_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            Type type;
            var item = args.InvokedItemContainer;

            switch (Convert.ToInt64(item.Tag))
            {
                case 0:
                    type = typeof(VideoPage);
                    if (MainFrame.CurrentSourcePageType == type) break;
                    MainFrame.Navigate(type);
                    break;

                case 1:
                    type = typeof(OutputPage);
                    if (MainFrame.CurrentSourcePageType == type) break;
                    MainFrame.Navigate(type);
                    break;

                case 2:
                    type = typeof(SoundPage);
                    if (MainFrame.CurrentSourcePageType == type) break;
                    MainFrame.Navigate(type);
                    break;
            }
        }

        private async void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            var applyContentDialog = new ContentDialog
            {
                Title = "Applying setings...",
                Content = new ProgressRing
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    IsActive = true
                },
                XamlRoot = this.Content.XamlRoot
            };
            _ = applyContentDialog.ShowAsync();

            await TempSettings.Save();
            App.Settings = await Settings.GetSettingsAsync();
            MainWindow.Current.InitializeRecorder();

            applyContentDialog.Hide();
        }

        private void SettingsWindow_Activated(object sender, WindowActivatedEventArgs args)
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

        private void SettingsWindow_Closed(object sender, WindowEventArgs args)
        {
            MainWindow.Current.SettingsWindow = null;
        }
    }
}
