using Microsoft.Win32;
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
    public partial class accountSettings : Page
    {
        private bool PageChanged = false;

        public accountSettings()
        {
            InitializeComponent();
        }

        public async Task refresh()
        {
            await getDetails();
        }

        public string lastUsername;
        public string lastEmail;
        public string lastTag;
        public string profilePicturePath = null;
        private long maxImageSize = 20971520;

        public async Task getDetails()
        {
            Utils.DisplayLoading(_overlayFrame);
            profilePicturePath = null;
            profilePicture.Content = "Select from file...";
            var details = await Account.GetAccountDetails();

            if ((bool)details["success"])
            {
                username.Text = (string)details["user_details"]["username"];
                email.Text = (string)details["user_details"]["email"];
                tag.Text = (string)details["user_details"]["tag"];

                lastUsername = (string)details["user_details"]["username"];
                lastEmail = (string)details["user_details"]["email"];
                lastTag = (string)details["user_details"]["tag"];

            }
            else
            {
                Utils.showMessage((string)details["error_message"]);
            }
            Utils.HideLoading(_overlayFrame);
        }

        public async Task changePassword()
        {
            Utils.DisplayLoading(_overlayFrame);

            string nw_pw = new_password.Password;
            string conf_pw = confirm_password.Password;
            string od_pw = old_password.Password;

            old_password.Clear();
            new_password.Clear();
            confirm_password.Clear();

            Console.WriteLine("Changing password");

            if (nw_pw != conf_pw)
            {
                Utils.HideLoading(_overlayFrame);
                Utils.showMessage("The new password does not match.");
            } else
            {
                var data = await Account.ChangePassword(od_pw, nw_pw);

                if (data.ContainsKey("success"))
                {
                    if ((bool)data["success"] == false)
                    {
                        Utils.showMessage((string)data["error_message"]);
                    }
                    else
                    {
                        Console.WriteLine("Successfully updated user password");
                    }
                }

                string error_fb = "";

                if (data.ContainsKey("old_password"))
                {
                    foreach (var obj in data["old_password"])
                    {
                        error_fb += obj + "\n";
                    }
                }
                if (data.ContainsKey("new_password"))
                {
                    foreach (var obj in data["new_password"])
                    {
                        error_fb += obj + "\n";
                    }
                    tag.Text = lastTag;
                }

                Utils.HideLoading(_overlayFrame);

                if (error_fb != "")
                {
                    Utils.showMessage(error_fb);
                }
            }

            
            Utils.HideLoading(_overlayFrame);
        }

        public async Task updateUserData()
        {
            Utils.DisplayLoading(_overlayFrame);

            string changeUsername = null;
            string changeEmail = null;
            string changeTag = null;
            string changeProfilePicture = profilePicturePath;
            if (lastUsername != username.Text) changeUsername = username.Text;
            if (lastEmail != email.Text) changeEmail = email.Text;
            if (lastTag != tag.Text) changeTag = tag.Text;

            var data = await Account.UpdateAccountDetails(changeUsername, changeEmail, changeTag, changeProfilePicture);

            if (data.ContainsKey("success"))
            {
                if ((bool)data["success"] == false)
                {
                    Utils.showMessage((string)data["error_message"]);
                } else
                {
                    Console.WriteLine("Successfully updated user data");
                }
                MainWindow mainWindow = (MainWindow) App.Current.MainWindow;
                await mainWindow.UpdateUserData();
            }

            string error_fb = "";

            if (data.ContainsKey("username"))
            {
                foreach (var obj in data["username"])
                {
                    error_fb += obj + "\n";
                }
                username.Text = lastUsername;
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
                tag.Text = lastTag;
            }
            if (data.ContainsKey("email"))
            {
                foreach (var obj in data["email"])
                {
                    error_fb += obj + "\n";
                }
                email.Text = lastEmail;
            }

            Utils.HideLoading(_overlayFrame);

            if (error_fb != "")
            {
                Utils.showMessage(error_fb);
            } else
            {
                await getDetails();
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (PageChanged == false)
            {
                PageChanged = true;
                await getDetails();

            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Slecting new profile picture");
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.webm";

            if (openFileDialog.ShowDialog() == true)
            {
                string file = openFileDialog.FileName;
                if (File.Exists(file))
                {
                     FileInfo fileInfo = new FileInfo(file);
                     
                    if (fileInfo.Length > maxImageSize)
                    {
                        Utils.showMessage("The selected file is to big!\nYour file must be smaller than " + Utils.SizeSuffix(maxImageSize));
                    } else
                    {
                        profilePicture.Content = fileInfo.Name;
                        profilePicturePath = file;
                    }

                } else
                {
                    Utils.showMessage("The selected file was not found!");
                }
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            await updateUserData();
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            await changePassword();
        }
    }
}
