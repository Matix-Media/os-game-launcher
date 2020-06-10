using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.IO;
using System.Net;
using Ionic.Zip;
using System.Windows.Navigation;
using System.Diagnostics;
using CommandLine;
using OS_Game_Launcher.Pages;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Drawing;
using System.Reflection;

namespace OS_Game_Launcher
{
    class Account
    {

        public static bool InstallationRunning = false;
        public static bool extracting = false;
        public static int InstallingGameID = -1;
        public static string InstallingGamePath = "";
        public static MainWindow mainWindow;
        public static WebClient webClient = new WebClient();

        public async static Task Logout()
        {
            var request = new RestRequest("/user/logout");
            var cTokeS = new CancellationTokenSource();
            var response = await Utils.Client.ExecuteGetAsync(request, cTokeS.Token);
            var data = JObject.Parse(response.Content);

            if (data.ContainsKey("success"))
            {
                if ((bool)data["success"] == false)
                {
                    new Windows.msgBox(data["error_message"].ToString()).ShowDialog();
                }
            }

            Utils.RestartApp();
        }

        public async static Task<Object> BuyGame(int gameID)
        {
            var request = new RestRequest("/game/" + gameID + "/buy");
            var cTokeS = new CancellationTokenSource();
            var response = await Utils.Client.ExecuteGetAsync(request, cTokeS.Token);
            var data = JObject.Parse(response.Content);

            if (data.ContainsKey("success"))
            {
                if ((bool)data["success"] == false)
                {
                    new Windows.msgBox(data["error_message"].ToString()).ShowDialog();

                    return false;
                }

                return data;
            } else
            {
                Console.WriteLine("Unknown error occurred during checkout.");
                return false;
            }
        }

        public async static Task<JObject> GetAccountDetails()
        {
            var request = new RestRequest("/user/info/details");
            var cTokeS = new CancellationTokenSource();
            var response = await Utils.Client.ExecuteGetAsync(request, cTokeS.Token);
            var data = JObject.Parse(response.Content);

            return data;
        }

        public async static Task<JObject> UpdateAccountDetails(string username = null, string email = null, string tag = null, string image = null)
        {
            var request = new RestRequest("/user/info/details/update");
            var cTokeS = new CancellationTokenSource();
            if (username != null) request.AddParameter("username", username);
            if (email != null) request.AddParameter("email", email);
            if (tag != null) request.AddParameter("display_name", tag);
            if (image != null)
            {
                
                request.AddParameter("profile_picture", "true");
                request.AddFile("image", image);
                //request.AddHeader("Content-Type", "form-data");
            }

            var response = await Utils.Client.ExecutePostAsync(request, cTokeS.Token);
            var data = JObject.Parse(response.Content);
            return data;
        }

        public async static Task<JObject> ChangePassword(string old_password, string new_password)
        {
            var request = new RestRequest("/user/password/change");
            var cTokeS = new CancellationTokenSource();
            request.AddParameter("old_password", old_password);
            request.AddParameter("new_password", new_password);

            var response = await Utils.Client.ExecutePostAsync(request, cTokeS.Token);
            var data = JObject.Parse(response.Content);
           
            return data;
        }
        

        public async static Task<Object> CheckGame(int gameID)
        {
            var request = new RestRequest("/game/" + gameID + "/check");
            var cTokeS = new CancellationTokenSource();
            var response = await Utils.Client.ExecuteGetAsync(request, cTokeS.Token);
            var data = JObject.Parse(response.Content);

            if (data.ContainsKey("success"))
            {
                if ((bool)data["success"] == false)
                {
                    Console.WriteLine("Error Checking Game " + gameID + " (" + (string)data["error_code"] + ") " + (string)data["error_message"]);
                    return false;
                } else
                {
                    return data;
                }

                
            }
            else
            {
                Console.WriteLine("Unknown error occurred during game stats check.");
                return false;
            }
        }

        public async static Task<Object> CheckGames(List<int> gameIDs)
        {
            string jsonIDs = JsonConvert.SerializeObject(gameIDs);

            var request = new RestRequest("/games/check");
            request.AddParameter("ids", jsonIDs);
            var cTokeS = new CancellationTokenSource();
            var response = await Utils.Client.ExecuteGetAsync(request, cTokeS.Token);
            var data = JObject.Parse(response.Content);

            if (data.ContainsKey("success"))
            {
                if ((bool)data["success"] == false)
                {
                    Console.WriteLine("Error Checking Games (" + (string)data["error_code"] + ") " + (string)data["error_message"]);
                    return false;
                }
                else
                {
                    return data["games"];
                }


            }
            else
            {
                Console.WriteLine("Unknown error occurred during checking games stats.");
                return false;
            }
        }

        public static Object CheckGameInstalled(int gameID)
        {
            RegistryKey launcherRootReg = Registry.CurrentUser;
            RegistryKey gamesRootReg = Utils.RegistryOpenCreateKey(launcherRootReg, Properties.Settings.Default.regestryPath + "\\Games");
            if (Utils.RegistryContainsSubKey(gamesRootReg, gameID.ToString()))
            {
                RegistryKey gameKey = gamesRootReg.OpenSubKey(gameID.ToString());
                Console.WriteLine("Game found in registry");
                string installationPath = (string)gameKey.GetValue("InstallationPath");
                if (Directory.Exists(installationPath))
                {
                    return installationPath;
                } else
                {
                    gamesRootReg.DeleteSubKeyTree(gameID.ToString());
                    return false;
                }
                
            }
            return false;
        }

        public static bool CheckGameRunning(Process gameProc)
        {
            return Utils.IsRunning(gameProc);
        }

        public static bool CheckGameInstalling(int gameID)
        {
            if (CheckGameInstalled(gameID) is string)
            {
                RegistryKey gameKey = Registry.CurrentUser.OpenSubKey(Properties.Settings.Default.regestryPath + "\\Games\\" + gameID, true);
                bool installing = Convert.ToBoolean(gameKey.GetValue("Installing"));
                Console.WriteLine("Installing: " + installing);
                return installing;
            }
            else
            {
                return false;
            }
        }

        private static int extractProgress;
        private static string extractCurrent;

        public async static Task<bool> InstallGame(int gameID, string installationPath, MainWindow mainWindow)
        {
            if (InstallationRunning == false)
            {
                InstallingGameID = gameID;
                InstallingGamePath = installationPath;
                InstallationRunning = true;
                Account.mainWindow = mainWindow;
                mainWindow.gameDownloadType.Text = "";
                mainWindow.gameDownloadStatus.Text = "";
                mainWindow.gameDownloadProgress.Value = 0;
                mainWindow.gameDownloadProgress.IsIndeterminate = true;
                mainWindow.gameDownloadProgress.Visibility = System.Windows.Visibility.Visible;
                mainWindow.gameDownloadStatus.Visibility = System.Windows.Visibility.Visible;
                mainWindow.gameDownloadName.Visibility = System.Windows.Visibility.Visible;
                mainWindow.gameDownloadType.Visibility = System.Windows.Visibility.Visible;
                

                DriveInfo installDrive = Utils.GetDriverFromPrefix(Path.GetPathRoot(new FileInfo(installationPath).FullName));
                var freeDiscSpace = installDrive.AvailableFreeSpace;

                var request = new RestRequest("/game/" + gameID);
                var cTokeS = new CancellationTokenSource();
                var response = await Utils.Client.ExecuteGetAsync(request, cTokeS.Token);
                var data = JObject.Parse(response.Content);

                if (data.ContainsKey("success"))
                {
                    if ((bool)data["success"] == false)
                    {
                        Utils.showMessage("Error installing game:\n" + (string)data["error_message"]);
                    }
                    else
                    {
                        if (await Utils.GetHttpStatusCode((string)data["game"]["download_comp"]) && 
                            await Utils.GetFileSize(new Uri((string)data["game"]["download_comp"])) < freeDiscSpace)
                        {
                            mainWindow.gameDownloadName.Text = (string)data["game"]["name"];

                            RegistryKey gameKey = Utils.RegistryOpenCreateKey(Registry.CurrentUser, Properties.Settings.Default.regestryPath + "\\Games\\" + gameID);
                            gameKey.SetValue("InstallationPath", installationPath);
                            gameKey.SetValue("Installing", true);
                            gameKey.SetValue("Installed", true);
                            gameKey.SetValue("InstalledVersion", data["game"]["version"]);
                            gameKey.SetValue("ExecutionPath", data["game"]["exec_path"]);
                            gameKey.SetValue("Name", data["game"]["name"]);

                            Console.WriteLine("Registry Path: " + gameKey.ToString());
                            Console.WriteLine("Creating Game folder");
                            Utils.CreateDirectoryIfNotExists(installationPath);

                            
                            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);

                            string zipFilePath = Path.Combine(installationPath, Utils.Truncate(Guid.NewGuid().ToString(), 16) + ".zip");

                            Console.WriteLine("Download starting (Installing to: " + installationPath + ")");
                            mainWindow.gameDownloadType.Text = "Downloading...";
                            mainWindow.gameDownloadProgress.IsIndeterminate = false;
                            mainWindow.gameDownloadCancelButton.Visibility = System.Windows.Visibility.Visible;
                            mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                            try
                            {
                                await webClient.DownloadFileTaskAsync((string)data["game"]["download_comp"], zipFilePath);
                            } catch (WebException e) {
                                Console.WriteLine("Error downloading: " + e.Message + " (" + e.Status + ")");

                                return false;
                            }
                            
                            mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                            mainWindow.gameDownloadCancelButton.Visibility = System.Windows.Visibility.Hidden;
                            Console.WriteLine("Download done. Checking MD5");
                            webClient.Dispose();
                            mainWindow.gameDownloadType.Text = "Installing...";
                            mainWindow.gameDownloadProgress.IsIndeterminate = true;
                            string downloaded_comp_hash = Utils.CalculateMD5(zipFilePath);
                            Console.WriteLine("Comparing mdd5 Hashes: " + downloaded_comp_hash + " = " + (string)data["game"]["comp_md5"]);
                            if (downloaded_comp_hash == ((string)data["game"]["comp_md5"]).ToLowerInvariant())
                            {
                                Console.WriteLine("Checking done. Extracting");
                                ZipFile zip = ZipFile.Read(zipFilePath);
                                long ZipUncompressedSize = 0;
                                foreach (ZipEntry e in zip) { ZipUncompressedSize += e.UncompressedSize; }
                                Console.WriteLine("Required space: " + ZipUncompressedSize + " bytes");
                                
                                if (freeDiscSpace > ZipUncompressedSize)
                                {
                                    zip.ExtractProgress += new EventHandler<ExtractProgressEventArgs>(zipExtractProgressCallback);
                                    
                                    extracting = true;
                                    await Task.Run(() => zip.ExtractAll(installationPath, ExtractExistingFileAction.OverwriteSilently));
                                    Console.WriteLine("Disposing zipper");
                                    await Utils.PutTaskDelay(1000);
                                    zip.Dispose();

                                    Console.WriteLine("Extracting done. Deleting archive");
                                    await Utils.PutTaskDelay(3000);
                                    await Utils.DeleteAsync(new FileInfo(zipFilePath));
                                    Console.WriteLine("Done deleting");

                                    await AddGameDownload(gameID);

                                    if (Settings.CreateDesktopShortcuts)
                                        try
                                        {
                                            string IconPath = Path.Combine(Settings.DefaultGameInstallationPath, gameID.ToString(), (string)data["game"]["exec_path"]);

                                            Console.WriteLine("Icon located at: " + IconPath);

                                            Console.WriteLine("Shortcut directing to: " + System.Reflection.Assembly.GetEntryAssembly().Location);

                                            string ShortcutPath = Utils.UrlShortcutToDesktop((string)data["game"]["name"], "osgamelauncher://start-game/" + gameID, IconPath, (int)data["game"]["exe_icon_id"]);

                                            //string ShortcutPath = Utils.CreateShortcut((string)data["game"]["name"], 
                                            //    System.Reflection.Assembly.GetEntryAssembly().Location, Arguments: "--game-startup=" + gameID, 
                                            //    Description: (string)data["game"]["description"], IconPath: IconPath);

                                            if (ShortcutPath != null) gameKey.SetValue("ShortcutPath", ShortcutPath); 
                                            else Console.WriteLine("Could not create Desktop Shortcut, unknown error.");

                                        } catch (Exception e)
                                        {
                                            Console.WriteLine("Could not create Desktop Shortcut with error: " + e.Message + " - " + e.Source + " - " + e.StackTrace);
                                        }

                                    gameKey.SetValue("Installing", false);
                                    InstallationRunning = false;
                                } else
                                {
                                    Utils.showMessage("Installation failed!\nYou don't have enough free disk space.");
                                    goto fail;
                                }
                            } else
                            {
                                Utils.showMessage("The download is not valid!\nPlease try again or contact our support.");
                                Console.WriteLine("Deleting registry keys");
                                RegistryKey parentKey = Utils.RegistryOpenCreateKey(Registry.CurrentUser, Properties.Settings.Default.regestryPath + "\\Games");
                                parentKey.DeleteSubKeyTree(gameID.ToString());
                                Console.WriteLine("Done resetting registry. Deleting download");
                                await Utils.DeleteFolderAsync(new FileInfo(installationPath));
                                Console.WriteLine("Done deleting");
                            }


                            mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                            mainWindow.gameDownloadProgress.Value = 0;
                            mainWindow.gameDownloadStatus.Text = null;
                            mainWindow.gameDownloadName.Text = null;
                            mainWindow.gameDownloadType.Text = null;
                            mainWindow.gameDownloadProgress.IsIndeterminate = false;
                            mainWindow.gameDownloadProgress.Visibility = System.Windows.Visibility.Hidden;
                            mainWindow.gameDownloadStatus.Visibility = System.Windows.Visibility.Hidden;
                            mainWindow.gameDownloadName.Visibility = System.Windows.Visibility.Hidden;
                            mainWindow.gameDownloadType.Visibility = System.Windows.Visibility.Hidden;
                            mainWindow.gameDownloadCancelButton.Visibility = System.Windows.Visibility.Hidden;
                            InstallationRunning = false;
                            return true;
                        } else
                        {
                            Utils.showMessage("Download failed!\nAccess on download resource not given or not enough disc space available.");
                        }
                        
                    }


                }
                else
                {
                    Console.WriteLine("Unknown error occurred during game installation (" + gameID + ")");
                }
            } else
            {
                Utils.showMessage("Another game is getting already installed.");
                mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                mainWindow.gameDownloadProgress.Visibility = System.Windows.Visibility.Hidden;
                mainWindow.gameDownloadStatus.Visibility = System.Windows.Visibility.Hidden;
                mainWindow.gameDownloadName.Visibility = System.Windows.Visibility.Hidden;
                mainWindow.gameDownloadType.Visibility = System.Windows.Visibility.Hidden;
                mainWindow.gameDownloadCancelButton.Visibility = System.Windows.Visibility.Hidden;
                InstallationRunning = false;
                return true;
            }
            fail:
            InstallationRunning = false;
            mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
            mainWindow.gameDownloadProgress.Visibility = System.Windows.Visibility.Hidden;
            mainWindow.gameDownloadStatus.Visibility = System.Windows.Visibility.Hidden;
            mainWindow.gameDownloadName.Visibility = System.Windows.Visibility.Hidden;
            mainWindow.gameDownloadType.Visibility = System.Windows.Visibility.Hidden;
            mainWindow.gameDownloadCancelButton.Visibility = System.Windows.Visibility.Hidden;
            InstallationRunning = false;
            return false;
        }

        private static void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            mainWindow.gameDownloadProgress.Value = e.ProgressPercentage;
            mainWindow.gameDownloadStatus.Text = Utils.SizeSuffix(e.BytesReceived) + " / " + Utils.SizeSuffix(e.TotalBytesToReceive);
            mainWindow.TaskbarItemInfo.ProgressValue = (float)((float)e.ProgressPercentage / (float)100);
            //mainWindow.TaskbarItemInfo.ProgressValue = 0.3;
        }

        private static void zipExtractProgressCallback(object sender, ExtractProgressEventArgs e)
        {
            if (e.TotalBytesToTransfer > 0 && e.BytesTransferred > 0)
            {
                extractProgress = 80 + (Convert.ToInt32(100 * e.BytesTransferred / e.TotalBytesToTransfer) / 5);
                extractCurrent = Utils.SizeSuffix(e.BytesTransferred) + " / " + Utils.SizeSuffix(e.TotalBytesToTransfer);

                if (e.TotalBytesToTransfer == e.BytesTransferred)
                {
                    extracting = false;
                }
            }
        }

        public async static Task CancelDownload()
        {
            if (InstallationRunning)
            {
                mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                mainWindow.gameDownloadCancelButton.Visibility = System.Windows.Visibility.Hidden;
                mainWindow.gameDownloadProgress.IsIndeterminate = true;
                mainWindow.gameDownloadStatus.Text = String.Empty;
                mainWindow.gameDownloadStatus.Visibility = System.Windows.Visibility.Hidden;
                mainWindow.gameDownloadType.Text = "Canceling...";
                webClient.CancelAsync();
                webClient.Dispose();
                await Utils.PutTaskDelay(3000);
                Console.WriteLine("Download Canceled. Deleting registry keys");
                RegistryKey parentKey = Utils.RegistryOpenCreateKey(Registry.CurrentUser, Properties.Settings.Default.regestryPath + "\\Games");
                parentKey.DeleteSubKeyTree(InstallingGameID.ToString());
                Console.WriteLine("Done resetting registry. Deleting download");
                await Utils.DeleteFolderAsync(new FileInfo(InstallingGamePath));
                Console.WriteLine("Done deleting. Cancellation complete");
                mainWindow.gameDownloadProgress.IsIndeterminate = false;
                mainWindow.gameDownloadType.Text = "";
                mainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                mainWindow.gameDownloadProgress.Visibility = System.Windows.Visibility.Hidden;
                mainWindow.gameDownloadType.Visibility = System.Windows.Visibility.Hidden;
                mainWindow.gameDownloadName.Visibility = System.Windows.Visibility.Hidden;
                InstallationRunning = false;
            }
        }

        private static Dictionary<int, Process> runningGames = new Dictionary<int, Process>();

        public static bool CheckGameRunningByID(int gameID)
        {
            if (runningGames.ContainsKey(gameID))
            {
                if (CheckGameRunning(runningGames[gameID]))
                {
                    return true;
                } else
                {
                    runningGames.Remove(gameID);
                    return false;
                }
            } else
            {
                return false;
            }
        }

        public static object StartGame(int gameID)
        {
            var installPath = CheckGameInstalled(gameID);
            if (installPath is string)
            {
                if (!CheckGameInstalling(gameID))
                {
                    var regGame = Registry.CurrentUser.OpenSubKey(Properties.Settings.Default.regestryPath + "\\Games\\" + gameID, true);
                    var executionPath = regGame.GetValue("ExecutionPath", false);
                    if (executionPath is bool == false)
                    {
                        var gameExecutablePath = Path.Combine((string)installPath, (string)executionPath);
                        if (File.Exists(gameExecutablePath))
                        {
                            var gameProcess = new Process();
                            gameProcess.StartInfo.WorkingDirectory = (string)installPath;
                            gameProcess.StartInfo.FileName = gameExecutablePath;
                            Console.WriteLine("Starting now: " + gameExecutablePath);
                            var started = gameProcess.Start();
                            Console.WriteLine("Game started: " + started);
                            if (started)
                            {
                                try
                                {
                                    var gameProcID = gameProcess.Id;
                                } catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                    Utils.showMessage("Error starting game.\n" + ex.Message);
                                    return false;
                                }
                                if (runningGames.ContainsKey(gameID))
                                {
                                    runningGames[gameID] = gameProcess;
                                    Console.WriteLine("Seems like game " + gameID + " is already running. Updating process");
                                    Log.Warning("Game " + gameID + " was already running. Updated process");
                                } else
                                {
                                    runningGames.Add(gameID, gameProcess);
                                }
                                
                                return gameProcess;
                            } else
                            {
                                Console.WriteLine("Game is not running");
                            }
                        } else
                        {
                            Console.WriteLine("Game executable not found");
                            Utils.showMessage("Your game Installation is corrupted.\nPlease try to reinstall the game or contact our support.");
                        }
                        
                        
                    } else
                    {
                        Console.WriteLine("Game installation corrupted");
                        Utils.showMessage("Your game Installation is corrupted.\nPlease try to reinstall the game or contact our support.");
                    }
                } else
                {
                    Console.WriteLine("Game is already running or update in progress");
                }
            } else
            {
                Console.WriteLine("Game not installed on this system");
            }
            return false;
            
            
        }

        public static bool StopGame(Process gameProcess)
        {
            gameProcess.Kill();
            return true;
            /*

            var gameProcID = CheckGameRunning(gameID);
            if (gameProcID is int)
            {
                Process gameProc = Process.GetProcessById((int)gameProcID);
                Console.WriteLine(gameProc.ProcessName);
                return true;
            } else
            {
                Console.WriteLine("Process not found.");
                return false;
            }
            */
        }

        public async static Task<bool> UninstallGame(int gameID, bool hard = false)
        {
            var installPath = CheckGameInstalled(gameID);
            Console.WriteLine("Hard mode enabled on uninstalling");
            if (hard || (!CheckGameRunningByID(gameID) && !CheckGameInstalling(gameID) && installPath is string))
            {
                
                var regGames = Registry.CurrentUser.OpenSubKey(Properties.Settings.Default.regestryPath + "\\Games", true);
                Console.WriteLine("Uninstalling game " + gameID + " from " + installPath);

                if (regGames.OpenSubKey(gameID.ToString()).GetValueNames().Contains("ShortcutPath"))
                    if (File.Exists((string)regGames.OpenSubKey(gameID.ToString()).GetValue("ShortcutPath"))) {
                        try
                        {
                            File.Delete((string)regGames.OpenSubKey(gameID.ToString()).GetValue("ShortcutPath"));
                        } catch (Exception e)
                        {
                            Console.WriteLine("Error deleting shortcut from Desktop: " + e.Message + " - " + e.StackTrace);
                        }
                    }

                        regGames.DeleteSubKeyTree(gameID.ToString());
                await Utils.PutTaskDelay(1000);
                var instDir = new FileInfo((string)installPath);
                await Utils.DeleteFolderAsync(instDir);

                
                

                Console.WriteLine("Successfully uninstalled game " + gameID);
                return true;
            } else
            {
                Console.WriteLine("Error uninstalling game");
                return false;
            }
        }

        public async static Task<bool> AddGameSession(int gameID, int lenght)
        {
            var request = new RestRequest("/game/" + gameID + "/new_session");
            request.AddParameter("time", lenght);
            var cTokeS = new CancellationTokenSource();
            var response = await Utils.Client.ExecuteGetAsync(request, cTokeS.Token);
            var data = JObject.Parse(response.Content);

            return (bool)data["success"];


        }

        public async static Task<bool> AddGameDownload(int gameID)
        {
            var request = new RestRequest("/game/" + gameID + "/new_download");
            var cTokeS = new CancellationTokenSource();
            var response = await Utils.Client.ExecuteGetAsync(request, cTokeS.Token);
            var data = JObject.Parse(response.Content);

            return (bool)data["success"];


        }

        public async static Task<JObject> GetNews()
        {
            var request = new RestRequest("/get_news");
            var cTokeS = new CancellationTokenSource();
            var response = await Utils.Client.ExecuteGetAsync(request, cTokeS.Token);
            var data = JObject.Parse(response.Content);

            return data;
        }

        public static List<Pages.game_details> OpenGameDetails = new List<Pages.game_details>();
        public static bool NavigateToGame(int gameID, NavigationService navService, bool Refresh=false, bool StartGame=false)
        {
            foreach (var gameDeatil in OpenGameDetails)
            {
                if (gameDeatil.gameID == gameID)
                {
                    navService.Navigate(gameDeatil);
                    Console.WriteLine("Game already has active session. Using it");
                    if (StartGame) _ = gameDeatil.StartGame();
                    else
                        if (Refresh) gameDeatil.Refresh();

                    return true;
                }
            }
            Pages.game_details newPage = new Pages.game_details(gameID, StartGame);
            navService.Navigate(newPage);
            OpenGameDetails.Add(newPage);
            Console.WriteLine("Created new session for game");
            return false;

        }

        public async static Task<bool> RefreshGamePage(int GameID)
        {
            foreach (var gameDetail in OpenGameDetails)
            {
                if (gameDetail.gameID == GameID)
                {
                    Utils.DisplayLoading(gameDetail._overlayFrame);
                    gameDetail.Refresh();
                    Utils.HideLoading(gameDetail._overlayFrame);

                    return true;
                }
            }

            return false;
        }

        public static List<Pages.publisher> OpenPublisherDetails = new List<Pages.publisher>();
        public static bool NavigateToPublisher(int publisherID, NavigationService navService)
        {
            foreach (var publisherDeatil in OpenPublisherDetails)
            {
                if (publisherDeatil.publisherID == publisherID)
                {
                    navService.Navigate(publisherDeatil);
                    Console.WriteLine("Publisher already has active session. Using it");
                    return true;
                }
            }
            Pages.publisher newPage = new Pages.publisher(publisherID);
            navService.Navigate(newPage);
            OpenPublisherDetails.Add(newPage);
            Console.WriteLine("Created new session for publisher");
            return false;
        }

        public async static Task HandleStartupArgs(List<string> args)
        {
            Console.Write("Startup Args: ");
            foreach (string arg in args)
            {
                Console.Write(arg + " -- ");
            }
            Console.WriteLine();

            var parsedArgs = Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsed<CommandLineOptions>(o =>
            {
                if (o.gameStartup > -1)
                {
                    Console.WriteLine("Stated up with command to launch game " + o.gameStartup);
                    mainWindow = (MainWindow)App.Current.MainWindow;
                    NavigateToGame(o.gameStartup, mainWindow._mainFrame.NavigationService, StartGame: true);
                }
                else
                {
                    string UrlS = args[0];
                    if (args[0] == Assembly.GetEntryAssembly().Location && args.Count > 1)
                    {
                        UrlS = args[1];
                    }

                    Uri addr;
                    if (Uri.TryCreate(UrlS, UriKind.Absolute, out addr))
                    {
                        if (addr.Host == "start-game")
                        {
                            int GameID = Convert.ToInt32(addr.Segments[1]);
                            Console.WriteLine("Stated up with command to launch game " + GameID);
                            mainWindow = (MainWindow)App.Current.MainWindow;
                            NavigateToGame(GameID, mainWindow._mainFrame.NavigationService, StartGame: true);
                        }
                    } else
                    {
                        Console.WriteLine("No game to startup");
                    }
                    
                    
                }
            });
        }

    }

    public class CommandLineOptions
    {
        [Option("game-startup", Required=false, Default=-1)]
        public int gameStartup { get; set; }
    }
}
