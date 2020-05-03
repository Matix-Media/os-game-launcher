using System;
using System.Collections.Generic;
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
using RestSharp;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace OS_Game_Launcher.Pages
{
    /// <summary>
    /// Interaktionslogik für register.xaml
    /// </summary>
    /// 
    public partial class login : Page
    {
        Windows.login _parent;
        RestClient client;
        private bool PageChanged = false;

        public login(Windows.login __parent)
        {
            InitializeComponent();

            _parent = __parent;
            client = Utils.Client;

        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!PageChanged)
            {
                PageChanged = true;
                if (await CheckIfLoggedIn())
                {
                    Console.WriteLine("Found active login session");
                    _parent.DialogResult = true;
                    _parent.Close();
                }
            }
            
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _parent._mainFrame.Navigate(_parent.registerP);
        }

        private async Task<bool> CheckIfLoggedIn()
        {
            Utils.DisplayLoading(_overlayFrame);
            var request = new RestRequest("/user/session");
            var cTokeS = new CancellationTokenSource();
            var response = await client.ExecuteTaskAsync(request, cTokeS.Token);
            var data = JObject.Parse(response.Content);
            Utils.HideLoading(_overlayFrame);
            if ((bool)data["logged_in"] == true)
            {
                return true;
            } else
            {
                return false;
            }
        }

        private async Task<bool> LoginAccount(string email, string password)
        {
            Utils.DisplayLoading(_overlayFrame);
            var request = new RestRequest("/user/login");
            request.AddParameter("email", email);
            request.AddParameter("password", password);
            var cTokeS = new CancellationTokenSource();
            var response = await client.ExecutePostTaskAsync(request, cTokeS.Token);
            var data = JObject.Parse(response.Content);
            Utils.HideLoading(_overlayFrame);

            if (data.ContainsKey("success"))
            {
                if ((bool)data["success"] == false)
                {
                    if ((string)data["error_code"] == "OSG-U0")
                    {
                        Console.WriteLine("Already logged in. Passing login");
                        return true;
                    }
                    new Windows.msgBox(data["error_message"].ToString()).ShowDialog();
                    return false;
                } else if((bool)data["success"] == true)
                {
                    return true;
                }
                
            }

            string error_fb = "";

            if (data.ContainsKey("password"))
            {
                foreach (var obj in data["password"])
                {
                    error_fb += obj + "\n";
                }
            }
            if (data.ContainsKey("email"))
            {
                foreach (var obj in data["email"])
                {
                    error_fb += obj + "\n";
                }
            }
            new Windows.msgBox(error_fb).ShowDialog();

            return false;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var success = await LoginAccount(email.Text, password.Password);
            if (success)
            {
                Console.WriteLine("Successfully logged in Account");
                _parent.DialogResult = true;
                _parent.Close();
            } else
            {
                Console.WriteLine("Error occurred while logging in account.");
                return;
            }
        }

        
    }
}
