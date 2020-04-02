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

namespace OS_Game_Launcher
{
    public static class Utils
    {
        public static RestClient Client = new RestClient(Properties.Settings.Default.host);

        public static async Task PutTaskDelay(int Miliseconds)
        {
            await Task.Delay(Miliseconds);
        }

        public static void Init()
        {
            Client.UserAgent = "OSGameLauncherClient/0.0.1";
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
            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.MultipleChoices)
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
            File.SetAttributes(targetDir, FileAttributes.Normal);

            string[] files = Directory.GetFiles(targetDir);
            string[] dirs = Directory.GetDirectories(targetDir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
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
                using (var stream = File.OpenRead(filename))
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
                return String.Format("{0:d} d {1:d} h", span.Days, Math.Abs(span.Hours));
            }
            if (span.Hours != 0)
            {
                return String.Format("{0:d} h {1:d} m", span.Hours, Math.Abs(span.Minutes));
            }
            if (span.Minutes != 0)
            {
                return String.Format("{0:d} m {1:d} s", span.Minutes, Math.Abs(span.Seconds));
            }
            return String.Format("{0:d}s", span.Seconds);
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
                using (var client = new WebClient())
                using (client.OpenRead(Properties.Settings.Default.host))
                    return true;
            }
            catch
            {
                return false;
            }
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
        public string Title { get; internal set; }
        public int ID { get; internal set; }
        public float Price { get; internal set; }
        public int PublisherID { get; internal set; }
        public bool Installed { get; internal set; }
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


