using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Globalization;

namespace OS_Game_Launcher.Pages
{
    /// <summary>
    /// Interaktionslogik für game_details.xaml
    /// </summary>
    public partial class game_details : Page
    {
        public int gameID;
        private string sourceCode;
        private string donationLink;
        private int publisherID = -1;
        private string gameNameCode;
        private string gamePriceCode;
        private bool FirstLoadDone;

        public game_details(int gameID)
        {
            InitializeComponent();

            this.gameID = gameID;
        }

        private async Task UpdateGameInfo()
        {
            var request = new RestRequest("/game/" + gameID);
            var cTokeS = new CancellationTokenSource();
            var response = await Utils.Client.ExecuteGetAsync(request, cTokeS.Token);
            var data = JObject.Parse(response.Content);

            if (data.ContainsKey("success"))
            {
                if ((bool)data["success"] == false)
                {
                    if ((string)data["error_code"] == "OSG-U1")
                    {
                        var loadingWindow = new Windows.loading();
                        loadingWindow.Show();
                        Application.Current.MainWindow.IsEnabled = false;

                        Console.WriteLine("Not logged in.");
                        await Account.Logout();
                        Utils.RestartApp();
                    }
                    new Windows.msgBox(data["error_message"].ToString()).ShowDialog();
                    return;
                }
                else if ((bool)data["success"] == true)
                {
                    gameName.Text = (string)data["game"]["name"];
                    gameNameCode = (string)data["game"]["name"];
                    gamePrice.Text = ((float)data["game"]["price"]).ToString("C");
                    gamePriceCode = ((float)data["game"]["price"]).ToString("C");
                    gameVersion.Text = ((string)data["game"]["version"]);
                    gameDescription.Text = (string)data["game"]["description"];
                    gameTags.Text = (string)data["game"]["tags"];
                    gameDownloadCount.Text = Utils.FormatNumber((int)data["game"]["downloads"]);
                    gameDownloadSize.Text = Utils.SizeSuffix(await Utils.GetFileSize(new Uri((string)data["game"]["download_comp"])));
                    if ((string)data["game"]["license"] == null)
                    {
                        gameLicense.Text = "Not know";
                        gameLicenseBuy.Text = "Not known";
                    } else
                    {
                        gameLicense.Text = (string)data["game"]["license"];
                        gameLicenseBuy.Text = (string)data["game"]["license"];
                    }

                    sourceCode = (string)data["game"]["source_code"];
                    if (data["publisher"] != null)
                    {
                        donationLink = (string)data["publisher"]["donation_link"];
                        publisherID = (int)data["publisher"]["ID"];
                        publisherText.Text = (string)data["publisher"]["name"];

                        Console.WriteLine("Publisher Account found");
                    } else
                    {
                        Console.WriteLine("No Publisher account found for this game");
                    }

                    if (donationLink == "")
                        publisherDonate.Visibility = Visibility.Hidden;

                    if (sourceCode == "")
                        SourceCodeButton.Visibility = Visibility.Hidden;

                    if (publisherID == -1)
                    {
                        publisherButton.Visibility = Visibility.Hidden;
                    }
                        


                    if ((bool)data["game_owned"])
                    {
                        gameStatusBarShop.Visibility = Visibility.Hidden;
                        gameStatusBarOwned.Visibility = Visibility.Visible;

                        var GameInstalledCheck = Account.CheckGameInstalled(gameID);
                        if (GameInstalledCheck is string)
                        {
                            Console.WriteLine("Game Installed into " + (string)GameInstalledCheck);

                            if (Account.CheckGameInstalling(gameID) == true)
                            {
                                btnPlay.Visibility = Visibility.Hidden;
                                btnInstall.Visibility = Visibility.Visible;

                                Console.WriteLine("Game installing");
                                btnInstall.Content = "Installing...";
                                btnInstall.IsEnabled = false;
                            } else
                            {
                                btnPlay.Visibility = Visibility.Visible;
                                btnInstall.Visibility = Visibility.Hidden;
                            }
                        }
                         else
                        {
                            btnPlay.Visibility = Visibility.Hidden;
                            btnInstall.Visibility = Visibility.Visible;
                            btnInstall.Content = "Install";
                            btnInstall.IsEnabled = true;
                        }
                        

                        if (Account.CheckGameRunning(runningGameProcess))
                        {
                            btnPlay.Content = "Stop";
                            btnPlay.IsEnabled = true;
                        }

                        if ((int)data["game_user_stats"]["times_played"] > 0)
                        {
                            gamePlaytime.Text = Utils.FormatRushTime(TimeSpan.FromMinutes((int)data["game_user_stats"]["mins_played"]));
                            gameLastPlayed.Text = ((DateTime)data["game_user_stats"]["last_played"]).ToString("d", CultureInfo.CreateSpecificCulture("de-DE"));
                        } else
                        {
                            gamePlaytime.Text = "You have never played the game";
                            gameLastPlayed.Text = "You have never played the game";
                        }
                        
                        
                    } else
                    {
                        gameStatusBarShop.Visibility = Visibility.Visible;
                        gameStatusBarOwned.Visibility = Visibility.Hidden;
                    }

                    if (Account.CheckGameInstalling(gameID))
                    {
                        btnInstall.Content = "Installing...";
                    }
                        

                    if (data["game"]["banner"] != null)
                    {
                        if (await Utils.CheckUrl((string)data["game"]["banner"]))
                        {
                            if (await Utils.UrlIsImage((string)data["game"]["banner"]))
                            {
                                var imageBrush = Utils.UniformImageBrush(new BitmapImage(new Uri((string)data["game"]["banner"])), (int)gameBannerRectangel.Width, (int)gameBannerRectangel.Height);
                                gameBannerRectangel.Fill = imageBrush;
                                gameBlurBackground.Fill = imageBrush;
                            }
                        }
                        
                    }
                    

                    return;
                }
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            BannerHeightDef.Height = new GridLength(this.ActualHeight / 2);
            if (!FirstLoadDone)
            {
                Utils.DisplayLoading(_overlayFrame);
                await UpdateGameInfo();
                Utils.HideLoading(_overlayFrame);
                FirstLoadDone = true;
                GameRefreshLoop();
            }
            
        }

        private void publisherButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Navigating to publisher with ID " + publisherID);
        }

        private void SourceCodeButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(sourceCode);
        }

        private void publisherDonate_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(donationLink);
        }

        private async void btnBuy_Click(object sender, RoutedEventArgs e)
        {
            

            var dialogResult = Utils.showMessage("Do you want to buy " + gameNameCode + " for " + gamePriceCode + "?", true);

            switch (dialogResult)
            {
                case true:

                    Utils.DisplayLoading(_overlayFrame);
                    var result = await Account.BuyGame(gameID);

                    if (result is bool)
                        if ((bool)result == true)
                        {
                            Utils.showMessage("Unknown error occurred during checkout.");
                        }
                        else
                        {
                            Console.WriteLine("Checkout canceled for game with ID " + gameID);
                        }
                    else
                    {
                        Console.WriteLine("Successfully bought game with ID " + (int)((JObject)result)["game_bought"]);
                        var currentW = (MainWindow)Window.GetWindow(this);
                        currentW.userBalance.Text = ((float)((JObject)result)["new_balance"]).ToString("C");
                        await UpdateGameInfo();
                    }

                    Utils.HideLoading(_overlayFrame);
                    break;
                case false:
                default:
                    Console.WriteLine("Checkout canceled");
                    break;
            }

            

            
        }

        private async void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            if (Account.InstallationRunning == false)
            {
                if (Account.CheckGameInstalled(gameID) is bool)
                {
                    if (Account.CheckGameInstalling(gameID) == false)
                    {
                        btnInstall.Content = "Installing...";
                        btnInstall.IsEnabled = false;
                        var installed = await Account.InstallGame(gameID, System.IO.Path.Combine(Utils.GetDefaultInstallationPath(), gameID.ToString()), (MainWindow)Application.Current.MainWindow);
                        if (installed)
                        {
                            Utils.DisplayLoading(_overlayFrame);
                            await UpdateGameInfo();
                            Utils.HideLoading(_overlayFrame);
                        } else
                        {
                            btnInstall.Content = "Install";
                            btnInstall.IsEnabled = true;
                        }
                        
                    }
                }
            }
            
        }

        private void AdjustSize(object sender, SizeChangedEventArgs e)
        {
            BannerHeightDef.Height = new GridLength(this.ActualHeight / 2);
        }

        public bool RefreshLoopactive = true;
        private async void GameRefreshLoop()
        {
            while (true)
            {
                while(RefreshLoopactive)
                {
                    await UpdateGameInfo();
                    await Utils.PutTaskDelay(20000);
                }
                await Utils.PutTaskDelay(20000);
            }
            
        }

        private async void GameProcessRefresh()
        {
            var started = DateTime.Now;
            while (Account.CheckGameRunning(runningGameProcess))
            {
                await Utils.PutTaskDelay(1000);
                btnPlay.Content = "Stop";
            }

            Utils.DisplayLoading(_overlayFrame);
            var endTime = DateTime.Now;
            var IngameTime = endTime.Subtract(started).TotalMinutes;
            Console.WriteLine("Ingame Time: " + Convert.ToInt32(IngameTime) + " GameID: " + gameID);
            await Account.AddGameSession(gameID, Convert.ToInt32(IngameTime));
            btnPlay.Content = "Play";
            await UpdateGameInfo();
            Utils.HideLoading(_overlayFrame);
        }

        private Process runningGameProcess;

        private async void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (Account.CheckGameRunning(runningGameProcess))
            {
                var gameStop = Account.StopGame(runningGameProcess);
                if (gameStop)
                {
                    btnPlay.Content = "Play";
                    Utils.DisplayLoading(_overlayFrame);
                    await UpdateGameInfo();
                    Utils.HideLoading(_overlayFrame);
                }
            } else
            {
                MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
                mainWindow.gameStarting.Visibility = Visibility.Visible;
                mainWindow.gameStartingName.Visibility = Visibility.Visible;
                mainWindow.gameStartingName.Text = gameName.Text;
            
                var started = Account.StartGame(gameID);
                btnPlay.Content = "Stop";
                if (started is bool)
                {
                    btnPlay.Content = "Play";

                    Utils.DisplayLoading(_overlayFrame);
                    await UpdateGameInfo();
                    Utils.HideLoading(_overlayFrame);
                } else
                {
                    runningGameProcess = (Process)started;
                    GameProcessRefresh();
                    Console.WriteLine("Passed on");
                }

                mainWindow.gameStarting.Visibility = Visibility.Hidden;
                mainWindow.gameStartingName.Visibility = Visibility.Hidden;
                mainWindow.gameStartingName.Text = String.Empty;
            }
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button openBtn = sender as Button;
            ContextMenu contextMenu = openBtn.ContextMenu;
            contextMenu.PlacementTarget = openBtn;
            contextMenu.IsOpen = true;
            e.Handled = true;
        }

        private async void btnUninstall_Click(object sender, RoutedEventArgs e)
        {
            Utils.DisplayLoading(_overlayFrame);
            var result = await Account.UninstallGame(gameID);
            if (result)
            {
                await UpdateGameInfo();
            }
            Utils.HideLoading(_overlayFrame);
        }
    }
}
