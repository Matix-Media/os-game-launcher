using Newtonsoft.Json;
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
    /// Interaktionslogik für library.xaml
    /// </summary>
    public partial class discover : Page
    {
        private bool PageChanged = false;

        public discover()
        {
            InitializeComponent();
        }

        public string lastSearchType = "all";
        public string lastSearchQuery = "";
        public int lastPage = 1;

        public async Task refresh()
        {
            await search(lastSearchQuery, lastSearchType, lastPage);
        }

        public async Task search(string query="", string type="all", int page=1)
        {
            Utils.DisplayLoading(_overlayFrame);

            noResults.Visibility = Visibility.Hidden;

            var request = new RestRequest("/games/search");
            request.AddParameter("q", query);
            request.AddParameter("type", type);
            request.AddParameter("page", page);
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
                        if (type != "all")
                            lastSearchQuery = query;
                        lastSearchType = type;
                        lastPage = page;
                        currentPage.Text = page.ToString();


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

                        gamesList.ItemsSource = results;

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
                            gamesList.ItemsSource = null;
                            gamesList.ItemsSource = results;

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
                            gamesList.ItemsSource = null;
                            gamesList.ItemsSource = results;
                        }
                    }
                    else
                    {
                        noResults.Visibility = Visibility.Visible;
                    }
                }
            }

            Utils.HideLoading(_overlayFrame);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {

            await search(searchQuery.Text, "search");

        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (PageChanged == false)
            {
                PageChanged = true;
                await search();
                
            }
            
        }

        private void gamesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Game SelectedItem = (Game)gamesList.SelectedItem;
            if (SelectedItem == null)
                return;

            Console.WriteLine("Selection changed to game: " + SelectedItem.ID);

            gamesList.SelectedItem = null;

            Account.NavigateToGame(SelectedItem.ID, this.NavigationService);
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            
            await search(lastSearchQuery, lastSearchType, lastPage + 1);
            
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if ((lastPage - 1) > 0)
            {

                await search(lastSearchQuery, lastSearchType, lastPage - 1);

            }
            
        }

        private async void Button_Click_3(object sender, RoutedEventArgs e)
        {
            await search();
        }
    }

    
}
