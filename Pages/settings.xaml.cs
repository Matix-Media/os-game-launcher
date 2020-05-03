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
            await loadSettings();
        }

        public string downloadPath = null;
        public string lastDownloadPath = null;

        public bool sendDesktopNotific = true;
        public bool lastSendDesktopNotific = true;

        public async Task loadSettings()
        {
            Utils.DisplayLoading(_overlayFrame);

            var rootKey = Utils.RegistryOpenCreateKey(Registry.CurrentUser, Properties.Settings.Default.regestryPath);
            downloadPath = (string)Utils.RegistryGetSet(rootKey, "DefaultGameInstallationPath", Utils.GetDefaultInstallationPath());
            lastDownloadPath = downloadPath;
            downloadPathButton.Content = downloadPath;

            sendDesktopNotific = Convert.ToBoolean(Utils.RegistryGetSet(rootKey, "SendDesktopNotifications", true));
            lastSendDesktopNotific = sendDesktopNotific;

            Utils.HideLoading(_overlayFrame);
        }

        

        public async Task  saveSettings()
        {
            Utils.DisplayLoading(_overlayFrame);
            var rootKey = Utils.RegistryOpenCreateKey(Registry.CurrentUser, Properties.Settings.Default.regestryPath);

            if (downloadPath != lastDownloadPath) rootKey.SetValue("DefaultGameInstallationPath", downloadPath);

            if (sendDesktopNotific != lastSendDesktopNotific) rootKey.SetValue("SendDesktopNotifications", sendDesktopNotific);


            Utils.HideLoading(_overlayFrame);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (PageChanged == false)
            {
                PageChanged = true;
                await loadSettings();

            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = downloadPath;
            dialog.Multiselect = false;
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var filePath = dialog.FileName;
                if (Directory.Exists(filePath))
                {
                    downloadPath = filePath;
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
            await saveSettings();
        }
    }
}
