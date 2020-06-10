using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using System.Windows;
using System.Windows.Controls;
using System.Net;
using System.Threading;
using System.Globalization;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Microsoft.Win32;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Deployment.Application;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Windows.Threading;
using Newtonsoft.Json;
using System.Security.RightsManagement;
using Notifications.Wpf;
using IWshRuntimeLibrary;

namespace OS_Game_Launcher
{
    public static class Utils
    {
        public static RestClient Client = new RestClient(Properties.Settings.Default.host);
        public static NotificationManager notificationManager;

        public static async Task PutTaskDelay(int Miliseconds)
        {
            await Task.Delay(Miliseconds);
        }

        public static void Init()
        {
            Client.UserAgent = "OSGameLauncherClient/0.0.1";

            Guid clientUuidGenerated = Guid.NewGuid();
            RegistryKey launcherRootReg = Registry.CurrentUser;
            RegistryKey rootReg = RegistryOpenCreateKey(launcherRootReg, Properties.Settings.Default.regestryPath);
            string clientUuid = (string) RegistryGetSet(rootReg, "clientUuid", clientUuidGenerated.ToString());

            Console.WriteLine("Client UUID: " + clientUuid);
            Client.AddDefaultHeader("client-uuid", clientUuid);
            Client.AddDefaultHeader("client-version", getRunningVersion().ToString());

            notificationManager = new NotificationManager();

            Settings.Load();
            
        }

        public static string getExecutablePath()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().Location;
        }

        public async static Task<bool> WaitForExitAsync(Process process, int timeout)
        {
            return await Task.Run(() => process.WaitForExit(timeout));
        }

        public async static void setURLProtocol()
        {
            if (!RegistryContainsSubKey(Registry.ClassesRoot, "osgamelauncher"))
            {
                Console.WriteLine("URL Protocol not registered!");
                Process p = new Process();
                p.StartInfo.FileName = "RegisterRegistry.exe";
                p.StartInfo.Verb = "runas";
                p.StartInfo.Arguments = "\"" + getExecutablePath() + "\"";
                //p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.UseShellExecute = true;
                


                try
                {
                    p.Start();
                    await WaitForExitAsync(p, 100000);
                } catch
                {
                    Utils.showMessage("The \"RegisterRegistry\" program is required to let you start games from your desktop!");
                }
                
            }
            

        }

        public static NamedPipeManager PipeManager;
        public static void startPipeServer()
        {
            PipeManager = new NamedPipeManager("OSGameLauncher");
            PipeManager.StartServer();

            PipeManager.ReceiveString += PipeManager_OpenRequest;
        }

        public static void stopPipServer()
        {
            PipeManager.StopServer();
        }

        public static void PipeManager_OpenRequest(string text)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (!string.IsNullOrEmpty(text))
                {
                    String[] seperators = { "::" };
                    var msgParts = text.Split(seperators, 2, StringSplitOptions.RemoveEmptyEntries);
                    Console.WriteLine("Received Pipe Message: " + text);
                    Console.WriteLine("Message Action: " + msgParts[0]);
                    switch (msgParts[0])
                    {
                        case "STARTUP_ARGS":
                            List<string> args = JsonConvert.DeserializeObject<List<string>>(msgParts[1]);
                            _ = Account.HandleStartupArgs(args);
                            break;
                        case "CUSTOM_ACTION":
                            switch (msgParts[1])
                            {
                                case "SET_MAIN_WND_FOCUS":
                                    MainWindow mainWindow = (MainWindow)App.Current.MainWindow;
                                    mainWindow.SetFocus();
                                    break;
                                default:
                                    Console.WriteLine("Unknown custom action");
                                    break;
                            }
                            break;
                        default:
                            Console.WriteLine("Unknown operator");
                            break;
                    }
                }
            });
            
            
        }

        public static byte[] file_get_byte_contents(string fileName)
        {
            byte[] sContents;
            if (fileName.ToLower().IndexOf("http:") > -1)
            {
                // URL 
                System.Net.WebClient wc = new System.Net.WebClient();
                sContents = wc.DownloadData(fileName);
            }
            else
            {
                // Get file size
                FileInfo fi = new FileInfo(fileName);

                // Disk
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);
                sContents = br.ReadBytes((int)fi.Length);
                br.Close();
                fs.Close();
            }

            return sContents;
        }

        public static async void CheckVersion()
        {
            Console.WriteLine("Checking for updates");
            Version currentAppVersion = getRunningVersion();

            var request = new RestRequest("/update");
            var cTokeS = new CancellationTokenSource();
            var response = await Client.ExecuteGetAsync(request, cTokeS.Token);
            var data = JObject.Parse(response.Content);

            string newestAppVersion = (string)data["version"];
            Version newestAppVersion_ = Version.Parse(newestAppVersion);
            string patchNotes = (string)data["patch-notes"];
            string installer = (string)data["url"];
            bool mandatory = (bool)data["mandatory"];
            string md5 = (string)data["md5"];

            if (currentAppVersion.CompareTo(newestAppVersion_) < 0)
            {
                Console.WriteLine("Version is outdated!");
                notificationManager.Show(new NotificationContent
                {
                    Title = "OS Game-Launcher Update",
                    Message = "A new version of the OS Game-Launcher is available!",
                    Type = NotificationType.Information
                }, expirationTime: TimeSpan.FromSeconds(15));
                showMessage("A newer version of OS Game-Launcher is available. Your currently installed version is " +
                    currentAppVersion.ToString() + " and the newest version is " + newestAppVersion + "!\n\nUpdate " +
                    currentAppVersion.ToString() + " => " + newestAppVersion + "\n\nChange log: " + patchNotes + "\n\nNew version is getting Downloaded...");

                Console.WriteLine("Installed version outdated. Downloading new version");

                string tempFolderPath = Path.GetTempPath();
                string installationPath = Path.Combine(tempFolderPath, "osg-updater-" + newestAppVersion.Replace(".", "-") + ".exe");
                DriveInfo installDrive = GetDriverFromPrefix(Path.GetPathRoot(new FileInfo(installationPath).FullName));
                var freeDiscSpace = installDrive.AvailableFreeSpace;

                if (await GetHttpStatusCode(installer) &&
                                await GetFileSize(new Uri(installer)) < freeDiscSpace)
                {
                    WebClient webClient = new WebClient();
                    await webClient.DownloadFileTaskAsync(installer, installationPath);
                    webClient.Dispose();
                    Console.WriteLine("Updater successfully downloaded! Comparing MD5");
                    string downloaded_comp_hash = CalculateMD5(installationPath);
                    if (downloaded_comp_hash == (md5).ToLowerInvariant())
                    {
                        Process p = new Process();
                        p.StartInfo.FileName = installationPath;
                        p.StartInfo.Verb = "runas";
                        p.Start();

                    } else
                    {
                        Console.WriteLine("MD5 not same! DANGER!! Deleting installer");
                        await Utils.DeleteAsync(new FileInfo(installationPath));
                        Console.WriteLine("Update cleaned up!");
                        showMessage("The update installer is not valid! For more infos visit: " + patchNotes);
                    }

                } else
                {
                    Console.WriteLine("Installer not available or not enough Disc space");
                    showMessage("The update installer is not available or you don't have enough disc space! For more infos visit: " + patchNotes);
                }

                Application.Current.Shutdown(0);
            } else if (currentAppVersion.CompareTo(newestAppVersion_) == 0)
            {
                Console.WriteLine("Version is up to date!");
            } else
            {
                Console.WriteLine("Version is not published  yet!");
            }

            
        }

        public static Version getRunningVersion()
        {
            try
            {
                return ApplicationDeployment.CurrentDeployment.CurrentVersion;
            }
            catch (Exception)
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

        public static void DisplayLoading(Frame frame)
        {
            frame.Visibility = Visibility.Visible;
            frame.Navigate(new Pages.loading());
        }

        public static void HideLoading(Frame frame)
        {
            frame.Navigate(null);
            frame.Visibility = Visibility.Hidden;
            
        }

        public static string UrlShortcutToDesktop(string linkName, string linkUrl, string IconFile, int IconIndex)
        {
            string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            using (StreamWriter writer = new StreamWriter(Path.Combine(deskDir, linkName + ".url")))
            {
                writer.WriteLine("[InternetShortcut]");
                writer.WriteLine("URL=" + linkUrl);
                writer.WriteLine("IconFile=" + IconFile);
                writer.WriteLine("IconIndex=" + IconIndex);
                writer.WriteLine("HotKey=0");
                writer.WriteLine("IDList=");
                writer.Flush();
            }

            return Path.Combine(deskDir, linkName + ".url");
        }

        public static string CreateShortcut(string Name, string Target, string Description=null, string Hotkey=null, string WorkingDirectory=null, string IconPath=null, string Arguments=null)
        {
            if (!Settings.CreateDesktopShortcuts)
                return null;

            object shDesktop = (object)"Desktop";
            WshShell shell = new WshShell();
            string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\" + Name + ".lnk";
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);

            shortcut.TargetPath = Target;
            if (WorkingDirectory != null) shortcut.WorkingDirectory = WorkingDirectory;
            if (Description != null) shortcut.Description = Description;
            if (Hotkey != null) shortcut.Hotkey = Hotkey;
            if (IconPath != null) shortcut.IconLocation = IconPath;
            if (Arguments != null) shortcut.Arguments = Arguments;

            shortcut.Save();

            return shortcutAddress;
        }

        public static bool? showMessage(string message, bool showCancelButton = false)
        {
            return new Windows.msgBox(message, showCancelButton).ShowDialog();
        }

        public static void RestartApp()
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown(0);
        }

        public async static Task<bool> CheckUrl(string url)
        {
            try
            {
            using (var client = new PrivateWebClientHead())
                {
                    client.HeadOnly = true;
                    var cTokeS = new CancellationTokenSource();
                    await client.DownloadStringTaskAsync(new Uri(url));
                    return true;
                }
            } catch
            {
                return false;
            }   
        }

        public async static Task<bool> UrlIsImage(string url)
        {
            var req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.Method = "HEAD";
            using (var resp = await req.GetResponseAsync())
            {
                return resp.ContentType.ToLower(CultureInfo.InvariantCulture)
                           .StartsWith("image/");
            }
        }

        public static ImageBrush UniformImageBrush(BitmapImage image, int width, int height)
        {
            ImageBrush uniformToFillBrush = new ImageBrush();
            uniformToFillBrush.ImageSource = image;
            uniformToFillBrush.Stretch = Stretch.UniformToFill;

            // Freeze the brush (make it unmodifiable) for performance benefits.
            //uniformToFillBrush.Freeze();

            return uniformToFillBrush;
        }

        public static bool RegistryContainsSubKey(RegistryKey regKey, string value)
        {
            return (regKey.GetSubKeyNames().Contains(value));
        }

        public static RegistryKey RegistryOpenCreateKey(RegistryKey regKey, string regPath)
        {
            var reg = regKey.OpenSubKey(regPath, true);
            if (reg == null)
            {
                return regKey.CreateSubKey(regPath, true);
            } else
            {
                return reg;
            }
        }

        public static object RegistryGetSet(RegistryKey reg, string keyName, object setter)
        {
            if (reg.GetValue(keyName) == null)
            {
                reg.SetValue(keyName, setter);
                return setter;
            } else
            {
                return reg.GetValue(keyName);
            }
        }

        public static string ProgramFilesx86()
        {
            if (8 == IntPtr.Size
                || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
            {
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }

            return Environment.GetEnvironmentVariable("ProgramFiles");
        }

        public static string SizeSuffix(Int64 value, int decimalPlaces = 1)
        {
            string[] SizeSuffixes =
                   { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }

        public static string GetDefaultInstallationPath()
        {
            string DefaultPathGenerated = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OS Game-Launcher Games");
            string DefaultPathReg = (string)Utils.RegistryGetSet(RegistryOpenCreateKey(Registry.CurrentUser, Properties.Settings.Default.regestryPath), "DefaultGameInstallationPath", DefaultPathGenerated);
            return DefaultPathReg;
        }

        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static string CreateDirectoryIfNotExists(string path)
        {
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            return path;
        }

        public async static Task<bool> GetHttpStatusCode(string url)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest
                                           .Create(url);
            webRequest.AllowAutoRedirect = false;
            HttpWebResponse response = await Task.Run(() => (HttpWebResponse)webRequest.GetResponse());
            Console.Write(response.StatusCode.ToString());
            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted || 
                response.StatusCode == HttpStatusCode.MultipleChoices || response.StatusCode == HttpStatusCode.Redirect)
            {
                return true;
            } else
            {
                return false;
            }

        }

        public static Task DeleteAsync(this FileInfo fi)
        {
            return Task.Factory.StartNew(() => fi.Delete());
        }

        public static void DeleteDirectory(string targetDir)
        {
            System.IO.File.SetAttributes(targetDir, FileAttributes.Normal);

            string[] files = Directory.GetFiles(targetDir);
            string[] dirs = Directory.GetDirectories(targetDir);

            foreach (string file in files)
            {
                System.IO.File.SetAttributes(file, FileAttributes.Normal);
                System.IO.File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(targetDir, false);
        }

        public static Task DeleteFolderAsync(this FileInfo fi)
        {
            return Task.Factory.StartNew(() => DeleteDirectory(fi.FullName));
        }

        public static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = System.IO.File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public async static Task<int> GetFileSize(Uri uriPath)
        {
            var webRequest = HttpWebRequest.Create(uriPath);
            webRequest.Method = "HEAD";

            using (var webResponse = await webRequest.GetResponseAsync())
            {
                var fileSize = Convert.ToInt32(webResponse.Headers.Get("Content-Length"));
                return fileSize;
            }
        }

        public static bool IsRunning(this Process process)
        {
            if (process == null)
                return false;

            try
            {
                Process.GetProcessById(process.Id);
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        }

        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }

        public static string FormatRushTime(TimeSpan span)
        {
            if (span.Days != 0)
            {
                return String.Format("{0:d} days {1:d} hours", span.Days, Math.Abs(span.Hours));
            }
            if (span.Hours != 0)
            {
                return String.Format("{0:d} hours {1:d} minutes", span.Hours, Math.Abs(span.Minutes));
            }
            if (span.Minutes != 0)
            {
                return String.Format("{0:d} minutes", span.Minutes, Math.Abs(span.Seconds));
            }
            return String.Format("{0:d} seconds", span.Seconds);
        }

        public static string FormatNumber(int num)
        {
            if (num >= 100000)
                return FormatNumber(num / 1000) + " K";
            if (num >= 10000)
            {
                return (num / 1000D).ToString("0.#") + " K";
            }
            return num.ToString("#,0");
        }

        public static DriveInfo GetDriverFromPrefix(string drivePrefix)
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (var drive in allDrives)
            {
                Console.WriteLine(drive.Name);
                if (drive.Name == drivePrefix)
                {
                    return drive;
                }
            }
            return allDrives[0];
        }

        public static bool CheckForServerConnection()
        {
            try
            {
                WebClient wc = new WebClientWithTimeout();
                _ = wc.DownloadString(Properties.Settings.Default.host);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async static Task<List<int>> getInstalledGames()
        {
            List<int> games = new List<int>();

            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(Properties.Settings.Default.regestryPath + "\\Games");
            var foundGames = regKey.GetSubKeyNames();
            foreach (object game in foundGames)
            {
                games.Add(Convert.ToInt32(game));
            }
            Console.WriteLine();


            return games;
        }

        public async static Task fixInstallingGames()
        {
            RegistryKey launcherRootReg = Registry.CurrentUser;
            RegistryKey gamesRootReg = RegistryOpenCreateKey(launcherRootReg, Properties.Settings.Default.regestryPath + "\\Games");
            var installedGames = await getInstalledGames();
            foreach (int game in installedGames)
            {
                var installed = Account.CheckGameInstalled(game);
                if (installed is string)
                {
                    bool gameInstalling = Account.CheckGameInstalling(game);
                    if (gameInstalling)
                    {
                        Console.WriteLine("Game " + game + " in installing process! Fixing...");
                        await Account.UninstallGame(game, true);
                    }
                    

                }

            }
        }


        public static bool ValidateRequest(JObject requestResult, bool hasSuccess = true)
        {
            if (requestResult.ContainsKey("success"))
            {
                if ((bool)requestResult["success"] == true)
                {
                    return true;
                } else
                {
                    return false;
                }
            } else
            {
                if (hasSuccess)
                {
                    return false;
                } else
                {
                    return true;
                }
                
            }
        }
    }

    class WebClientWithTimeout : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest wr = base.GetWebRequest(address);
            wr.Timeout = 10000; // timeout in milliseconds (ms)
            return wr;
        }
    }

    class PrivateWebClientHead : WebClient
    {
        public bool HeadOnly { get; set; }
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest req = base.GetWebRequest(address);
            if (HeadOnly && req.Method == "GET")
            {
                req.Method = "HEAD";
            }
            return req;
        }
    }


    public class Game
    {
        public BitmapImage Cover { get; internal set; }
        public string CoverPath { get; internal set; }
        public string Title { get; internal set; }
        public int ID { get; internal set; }
        public float Price { get; internal set; }
        public int PublisherID { get; internal set; }
        public bool Installed { get; internal set; }
        public string GroupingHeader { get; internal set; }
        public Visibility InstalledVisibility { get; internal set; }
        public Visibility OwnedVisibility { get; internal set; }
    }

    public enum Dimensions
    {
        Width,
        Height
    }

    public enum AnchorPosition
    {
        Top,
        Center,
        Bottom,
        Left,
        Right
    }

}


