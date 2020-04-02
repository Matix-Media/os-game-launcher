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

        public MainWindow()
        {
            InitializeComponent();

            Utils.Init();
            
            if (!Utils.CheckForServerConnection())
            {
                Utils.showMessage("We can't connect to our servers. Please check your Internet connection or contact our support.");
                App.Current.Shutdown(1);
            }
               
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

            _mainFrame.Navigate(new Pages.loading());
        }

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
        private void AdjustWindowSize()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                MaxButton.Content = "⬜";
                Root.Margin = new Thickness(0);
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                MaxButton.Content = "◱";
                Root.Margin = new Thickness(8);
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

        private async void loaded(object sender, RoutedEventArgs e)
        {
            await UpdateUserData();
            _mainFrame.Navigate(libraryP);
        }

        public async Task UpdateUserData()
        {
            this.Visibility = Visibility.Hidden;
            var loadingWin = new Windows.loading();
            loadingWin.Show();

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
                    userProfilePicture.Source = new BitmapImage(new Uri((string)data["profile_picture_url"]));
                } else
                {
                    Console.WriteLine("User profile picture is not an image");
                }
            } else
            {
                Console.WriteLine("User profile picture is not available");
            }

            loadingWin.Close();
            this.Visibility = Visibility.Visible;
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
    }
}
