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

namespace OS_Game_Launcher.Pages
{
    /// <summary>
    /// Interaktionslogik für publisher.xaml
    /// </summary>
    public partial class publisher : Page
    {
        public int publisherID;
        public string publisherWebLink;

        public publisher(int publisherID)
        {
            InitializeComponent();

            this.publisherID = publisherID;

            Utils.DisplayLoading(_overlayFrame);

        }

        public async Task getGames()
        {
            


            var request = new RestRequest("/games/publisher/" + publisherID);
            var cTokeS = new CancellationTokenSource();
            var response = await Utils.Client.ExecuteGetAsync(request, cTokeS.Token);
            var data = JObject.Parse(response.Content);

            if (data.ContainsKey("success"))
            {
                if ((bool)data["success"] == false)
                {
                    new Windows.msgBox(data["error_message"].ToString()).ShowDialog();
                    Utils.HideLoading(_overlayFrame);
                    return;
                }
                else if ((bool)data["success"] == true)
                {
                    var rawResults = data["results"];
                    if (((JArray)rawResults).Count > 0)
                    {
                        List<Game> results = new List<Game>();

                        foreach (var result in data["results"])
                        {
                            var game = new Game();

                            game.CoverPath = (string)result["cover"];


                            game.Title = (string)result["name"];
                            game.ID = (int)result["ID"];
                            game.OwnedVisibility = Visibility.Hidden;
                            game.InstalledVisibility = Visibility.Hidden;


                            results.Add(game);
                        }

                        publisherGames.ItemsSource = results;

                        Utils.HideLoading(_overlayFrame);

                        List<int> foundIDs = new List<int>();
                        foreach (var game in results)
                        {
                            if (await Utils.CheckUrl(game.CoverPath))
                            {
                                if (await Utils.UrlIsImage(game.CoverPath))
                                {
                                    game.Cover = new BitmapImage(new Uri(game.CoverPath));
                                }
                            }
                            publisherGames.ItemsSource = null;
                            publisherGames.ItemsSource = results;

                            foundIDs.Add(game.ID);
                        }

                        var checkedGames = await Account.CheckGames(foundIDs);
                        if (checkedGames is bool)
                        {
                            Console.WriteLine("Error getting game stats");
                        }
                        else
                        {
                            foreach (var game in results)
                            {
                                if (((JObject)checkedGames).ContainsKey(game.ID.ToString()))
                                {
                                    if ((bool)((JObject)checkedGames)[game.ID.ToString()]["game_owned"])
                                    {
                                        var installed = Account.CheckGameInstalled(game.ID);
                                        if (installed is string)
                                        {
                                            game.InstalledVisibility = Visibility.Visible;
                                        }
                                        else
                                        {
                                            game.OwnedVisibility = Visibility.Visible;
                                        }
                                    }
                                }
                                else
                                    Console.WriteLine("Can not find game in game stats");
                            }
                            publisherGames.ItemsSource = null;
                            publisherGames.ItemsSource = results;
                        }
                    }
                    else
                    {
                        Utils.HideLoading(_overlayFrame);
                    }
                }
            }
            Utils.HideLoading(_overlayFrame);
        }

        public async Task UpdatePublisherInfo()
        {
            var request = new RestRequest("/publisher/" + publisherID);
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
                    var publisherInfo = data["publisher"];
                    publisherName.Text = (string)publisherInfo["name"];

                    publisherWebLink = (string)publisherInfo["website"];
                    Uri publisherUri = new Uri((string)publisherInfo["website"]);
                    publisherWebsite.Text = publisherUri.Host;


                    publisherMail.Text = (string)publisherInfo["email"];
                    publisherJoin.Text = Convert.ToString(DateTime.Parse((string)publisherInfo["joined"]).Year);

                    publisherAbout.Text = (string)publisherInfo["about"];

                    if (await Utils.CheckUrl((string)publisherInfo["background"]) && await Utils.UrlIsImage((string)publisherInfo["background"]))
                    {
                        var imageBrush = Utils.UniformImageBrush(new BitmapImage(new Uri((string)publisherInfo["background"])), (int)blurBackground.Width, (int)blurBackground.Height);
                        blurBackground.Fill = imageBrush;
                    } else
                    {
                        // pass
                    }

                    if (await Utils.CheckUrl((string)publisherInfo["profile_picture"]) && await Utils.UrlIsImage((string)publisherInfo["profile_picture"]))
                    {
                        publisherLogo.Source = new BitmapImage(new Uri((string)publisherInfo["profile_picture"]));
                    }
                        


                    return;
                }
            }
        }

        private void publisherGames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Game SelectedItem = (Game)publisherGames.SelectedItem;
            if (SelectedItem == null)
                return;

            Console.WriteLine("Selection changed to game: " + SelectedItem.ID);

            publisherGames.SelectedItem = null;

            Account.NavigateToGame(SelectedItem.ID, this.NavigationService);
        }

        private bool PageLoaded = false;
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!PageLoaded)
            {
                await UpdatePublisherInfo();
                await getGames();

                PageLoaded = true;
            }
            

            
        }
    }
}
