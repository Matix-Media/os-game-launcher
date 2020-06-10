using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OS_Game_Launcher.Pages
{
    /// <summary>
    /// Interaktionslogik für accountSettings.xaml
    /// </summary>
    public partial class settings : Page
    {
        private bool PageChanged = false;

        public settings()
        {
            InitializeComponent();
        }

        public async Task refresh()
        {
            await LoadSettings();
        }

        public string DownloadPath = null;
        public string LastDownloadPath = null;

        public bool CreateDesktopLink = true;
        public bool LastCreateDesktopLink = true;
        public bool SendDesktopNotific = true;
        public bool LastSendDesktopNotific = true;

        public async Task LoadSettings()
        {
            Utils.DisplayLoading(_overlayFrame);

            Settings.Load();

            DownloadPath = Settings.DefaultGameInstallationPath;
            LastDownloadPath = DownloadPath;
            downloadPathButton.Content = DownloadPath;

            CreateDesktopLink = Settings.CreateDesktopShortcuts;
            LastCreateDesktopLink = CreateDesktopLink;
            createDesktopLinkCheckbox.IsChecked = CreateDesktopLink;

            SendDesktopNotific = Settings.SendDesktopNotifications;
            LastSendDesktopNotific = SendDesktopNotific;
            sendDesktopNotificCheckbox.IsChecked = SendDesktopNotific;

            Utils.HideLoading(_overlayFrame);
        }

        

        public async Task  SaveSettings()
        {
            Utils.DisplayLoading(_overlayFrame);

            if (DownloadPath != LastDownloadPath)
            {
                Settings.DefaultGameInstallationPath = DownloadPath;
                LastDownloadPath = DownloadPath;
            }
                
            if (CreateDesktopLink != LastCreateDesktopLink)
            {
                Settings.CreateDesktopShortcuts = CreateDesktopLink;
                LastCreateDesktopLink = CreateDesktopLink;
            }
                
            if (SendDesktopNotific != LastSendDesktopNotific) {
                Settings.SendDesktopNotifications = SendDesktopNotific;
                LastSendDesktopNotific = SendDesktopNotific;
            }

            Settings.Save();

            Utils.HideLoading(_overlayFrame);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (PageChanged == false)
            {
                PageChanged = true;
                await LoadSettings();

            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = DownloadPath;
            dialog.Multiselect = false;
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var filePath = dialog.FileName;
                if (Directory.Exists(filePath))
                {
                    DownloadPath = filePath;
                    downloadPathButton.Content = filePath;
                    Console.WriteLine("Selected path: " + filePath);
                } else
                {
                    Utils.showMessage("The selected path is not a valid folder.");
                }
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            await SaveSettings();
        }

        private void createDesktopLinkCheckbox_Click(object sender, RoutedEventArgs e)
        {
            CreateDesktopLink = (bool)createDesktopLinkCheckbox.IsChecked;
        }

        private void sendDesktopNotificCheckbox_Click(object sender, RoutedEventArgs e)
        {
            SendDesktopNotific = (bool)sendDesktopNotificCheckbox.IsChecked;
        }
    }
}
