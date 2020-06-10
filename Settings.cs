using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_Game_Launcher
{
    public static class Settings
    {
        public readonly static Dictionary<string, string> SettingsIndentifier = new Dictionary<string, string>() {
            { "DGIP", "DefaultGameInstallationPath" },
            { "CDS", "CreateDesktopShortcuts" },
            { "SDN", "SendDesktopNotifications" }
        };

        public static bool SendDesktopNotifications = true;
        public static bool CreateDesktopShortcuts = true;
        public static string DefaultGameInstallationPath = null;

        public static void Load()
        {
            var rootKey = Utils.RegistryOpenCreateKey(Registry.CurrentUser, Properties.Settings.Default.regestryPath);

            DefaultGameInstallationPath = (string)Utils.RegistryGetSet(rootKey, SettingsIndentifier["DGIP"], Utils.GetDefaultInstallationPath());
            CreateDesktopShortcuts = Convert.ToBoolean(Utils.RegistryGetSet(rootKey, SettingsIndentifier["CDS"], true));
            SendDesktopNotifications = Convert.ToBoolean(Utils.RegistryGetSet(rootKey, SettingsIndentifier["SDN"], true));
        }

        public static void Save()
        {
            var rootKey = Utils.RegistryOpenCreateKey(Registry.CurrentUser, Properties.Settings.Default.regestryPath);

            rootKey.SetValue(Settings.SettingsIndentifier["DGIP"], DefaultGameInstallationPath);
            rootKey.SetValue(Settings.SettingsIndentifier["CDS"], CreateDesktopShortcuts);
            rootKey.SetValue(Settings.SettingsIndentifier["SDN"], SendDesktopNotifications);
        }
    }
}
