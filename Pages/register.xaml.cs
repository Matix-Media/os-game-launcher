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

namespace OS_Game_Launcher.Pages
{
    /// <summary>
    /// Interaktionslogik für register.xaml
    /// </summary>
    public partial class register : Page
    {
        Windows.login _parent;
        RestClient client;

        public register(Windows.login __parent)
        {
            InitializeComponent();

            _parent = __parent;
            client = Utils.Client;
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _parent._mainFrame.Navigate(_parent.loginP);
        }

        private async Task<bool> RegisterAccount(string username, string email, string tag, string password)
        {
            Utils.DisplayLoading(_overlayFrame);
            var request = new RestRequest("/user/register");
            request.AddParameter("username", username);
            request.AddParameter("display_name", tag);
            request.AddParameter("email", email);
            request.AddParameter("password", password);
            var response = client.Post(request);
            var data = JObject.Parse(response.Content);

            Utils.DisplayLoading(_overlayFrame);

            if (data.ContainsKey("success"))
            return true;

            string error_fb = "";

            if (data.ContainsKey("username"))
            {
                foreach (var obj in data["username"])
                {
                    error_fb += obj + "\n";
                }
            }
            if (data.ContainsKey("password"))
            {
                foreach (var obj in data["password"])
                {
                    error_fb += obj + "\n";
                }
            }
            if (data.ContainsKey("display_name"))
            {
                foreach (var obj in data["display_name"])
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
            if (password.Password != password_repeat.Password)
            {
                new Windows.msgBox("The passwords do not match.").ShowDialog();
                return;
            }

            var success = RegisterAccount(username.Text, email.Text, tag.Text, password.Password);
            if (await success)
            {
                Console.WriteLine("Account successfully created");
                new Windows.msgBox("Your Account has been successfully created. You can now login.").ShowDialog();
                _parent._mainFrame.Navigate(_parent.loginP);
            } else
            {
                Console.WriteLine("Error occurred while creating account.");
                return;
            }
        }
    }
}
