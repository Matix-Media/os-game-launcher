using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
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
using System.Net;
using System.Windows.Threading;
using Microsoft.Win32;

namespace OS_Game_Launcher
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool LoggedIn = false;

        public Pages.library libraryP = new Pages.library();
        public Pages.discover discoverP = new Pages.discover();
        public Pages.accountSettings accountP = new Pages.accountSettings();
        public Pages.settings settingsP = new Pages.settings();
        public Pages.aboutOSG aboutP = new Pages.aboutOSG();

        public MainWindow()
        {
            Console.WriteLine("\nNew Session");

            Utils.Init();
            
            if (!Utils.CheckForServerConnection())
            {
                Utils.showMessage("We can't connect to our servers. Please check your Internet connection or contact our support.");
                App.Current.Shutdown(1);
            }

            Utils.CheckVersion();

            if (!LoggedIn)
            {
                Window loginW = new Windows.login();
                bool? dialogResult = loginW.ShowDialog();
                switch (dialogResult) {
                    case true:
                        Console.WriteLine("Successfully logged in.");
                        break;
                    case false:
                        this.Close();
                        break;
                    default:
                        this.Close();
                        break;
                }
            }

            InitializeComponent();

            launcherVersion.Text = Utils.getRunningVersion() + " (alpha)";
            _mainFrame.Navigate(new Pages.loading());
        }

        private bool AllowedSaveWinProps = false;
        private async void Window_Initialized(object sender, EventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            var loadingWin = new Windows.loading();
            loadingWin.Show();

            Utils.setURLProtocol();

            await Utils.fixInstallingGames();

            await UpdateUserData();

            Utils.startPipeServer();

            _mainFrame.Navigate(libraryP);
            await Account.HandleStartupArgs(App.startupArgs);

            loadingWin.Close();

            

            this.Visibility = Visibility.Visible;
        }
        /*
        private async void loaded(object sender, RoutedEventArgs e)
        {
            await UpdateUserData();
            _mainFrame.Navigate(libraryP);
        }
        */

        /// <summary>
        /// TitleBar_MouseDown - Drag if single-click, resize if double-click
        /// </summary>
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                if (e.ClickCount == 2)
                {
                    AdjustWindowSize();
                }
                else
                {
                    Application.Current.MainWindow.DragMove();
                }
        }

        /// <summary>
        /// CloseButton_Clicked
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// MaximizedButton_Clicked
        /// </summary>
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            AdjustWindowSize();
        }

        /// <summary>
        /// Minimized Button_Clicked
        /// </summary>
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Adjusts the WindowSize to correct parameters when Maximize button is clicked
        /// </summary>
        private void AdjustWindowSize(bool justState=false)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                if (justState == false)
                {
                    this.WindowState = WindowState.Normal;
                    
                    MaxButton.Content = "⬜";
                    Root.Margin = new Thickness(0);
                } else
                {
                    MaxButton.Content = "◱";
                    Root.Margin = new Thickness(8);
                   
                }
                
                
            }
            else
            {
                if (justState == false)
                {
                    this.WindowState = WindowState.Maximized;
                    MaxButton.Content = "◱";
                    Root.Margin = new Thickness(8);
                } else
                {
                    
                    MaxButton.Content = "⬜";
                    Root.Margin = new Thickness(0);
                }
                
                
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Navigate(libraryP);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _mainFrame.Navigate(discoverP);
        }

        

        public async Task UpdateUserData()
        {
            

            var request = new RestRequest("/user/info/home");
            var cTokeS = new CancellationTokenSource();
            var response = await Utils.Client.ExecuteGetAsync(request, cTokeS.Token);
            var data = JObject.Parse(response.Content);

            if (data.ContainsKey("success"))
            {
                if ((bool)data["success"] == false)
                {
                    if ((string)data["error_code"] == "OSG-U1")
                    {
                        Console.WriteLine("Not logged in.");
                        Utils.RestartApp();
                    }

                    new Windows.msgBox(data["error_message"].ToString()).ShowDialog();
                }
            }

            userTag.Text = (string)data["tag"];
            userBalance.Text = ((float)data["balance"]).ToString("C");

            if (await Utils.CheckUrl((string)data["profile_picture_url"]))
            {
                Console.WriteLine("User profile picture available");

                if (await Utils.UrlIsImage((string)data["profile_picture_url"]))
                {
                    Console.WriteLine("User profile picture is image");
                    var btmp = new BitmapImage(new Uri((string)data["profile_picture_url"]));
                    userProfilePicture.ImageSource = btmp;
                } else
                {
                    Console.WriteLine("User profile picture is not an image");
                }
            } else
            {
                Console.WriteLine("User profile picture is not available");
            }

            
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Button openBtn = sender as Button;
            ContextMenu contextMenu = openBtn.ContextMenu;
            contextMenu.PlacementTarget = openBtn;
            contextMenu.IsOpen = true;
            e.Handled = true;
        }

        private async void LogoutClick(object sender, RoutedEventArgs e)
        {
            var loadingWindow = new Windows.loading(true);
            loadingWindow.Show();
            this.IsEnabled = false;
            await Account.Logout();
            loadingWindow.Close();
        }

        private async void gameDownloadCancelButton_Click(object sender, RoutedEventArgs e)
        {
            await Account.CancelDownload();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                Console.WriteLine("Window got unintended Maximized");
                AdjustWindowSize(true);
            }
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Utils.stopPipServer();
            if (AllowedSaveWinProps)
            {
                Console.WriteLine("Saving window Properties");
                saveWindowProperties();

            }
                
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            _mainFrame.Navigate(accountP);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            _mainFrame.Navigate(settingsP);
        }

        private void aboutButton_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Navigate(aboutP);
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            _mainFrame.Navigate(aboutP);
        }

        public void saveWindowProperties()
        {
            var key = Utils.RegistryOpenCreateKey(Registry.CurrentUser, Properties.Settings.Default.regestryPath);
            
            if (this.WindowState == WindowState.Maximized)
            {
                key.SetValue("WinTop", RestoreBounds.Top);
                key.SetValue("WinLeft", RestoreBounds.Left);
                key.SetValue("WinHeight", RestoreBounds.Height);
                key.SetValue("WinWidth", RestoreBounds.Width);
                key.SetValue("WinMax", true);
            } else
            {
                key.SetValue("WinTop", this.Top);
                key.SetValue("WinLeft", this.Left);
                key.SetValue("WinHeight", this.Height);
                key.SetValue("WinWidth", this.Width);
                key.SetValue("WinMax", false);
            }
        }

        public void readWindowProperties()
        {
            var key = Utils.RegistryOpenCreateKey(Registry.CurrentUser, Properties.Settings.Default.regestryPath);

            Top = Convert.ToDouble(Utils.RegistryGetSet(key, "WinTop", 20));
            Console.WriteLine(Utils.RegistryGetSet(key, "WinTop", 20));
            Left = Convert.ToDouble(Utils.RegistryGetSet(key, "WinLeft", 20));
            Height = Convert.ToDouble(Utils.RegistryGetSet(key, "WinHeight", 715));
            Width = Convert.ToDouble(Utils.RegistryGetSet(key, "WinWidth", 1080));

            if (bool.Parse((string)Utils.RegistryGetSet(key, "WinMax", false))) {
                WindowState = WindowState.Maximized;
            }
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            readWindowProperties();
            AllowedSaveWinProps = true;
        }

        public void SetFocus()
        {
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;

            Topmost = true;
            Activate();
            Topmost = false;
        }
    }
}
