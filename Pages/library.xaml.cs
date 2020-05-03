using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class library : Page
    {
        public library()
        {
            InitializeComponent();
        }

        public string lastSearchType = "all";
        public string lastSearchQuery = "";

        public async Task refresh()
        {
            await search(lastSearchQuery, lastSearchType);
        }

        public async Task search(string query = "", string type = "all")
        {
            Utils.DisplayLoading(_overlayFrame);

            noResults.Visibility = Visibility.Hidden;

            var request = new RestRequest("/games/owned/search");
            request.AddParameter("q", query);
            request.AddParameter("type", type);
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


                        List<Game> results = new List<Game>();

                        foreach (var result in data["results"])
                        {
                            var game = new Game();
                            game.CoverPath = (string)result["cover"];
                            /*
                            if (await Utils.CheckUrl((string)result["cover"]))
                            {
                                if (await Utils.UrlIsImage((string)result["cover"]))
                                {
                                    game.Cover = new BitmapImage(new Uri((string)result["cover"]));
                                }
                            }*/

                            if (Account.CheckGameInstalled((int)result["ID"]) is bool)
                            {
                                game.GroupingHeader = "Owned";
                            } else
                            {
                                game.GroupingHeader = "Installed";
                            }

                            game.Title = (string)result["name"];
                            game.ID = (int)result["ID"];
                            game.OwnedVisibility = Visibility.Hidden;
                            game.InstalledVisibility = Visibility.Hidden;


                            results.Add(game);
                        }

                        ICollectionView cvs = CollectionViewSource.GetDefaultView(results);
                        cvs.GroupDescriptions.Add(new PropertyGroupDescription("GroupingHeader"));
                        cvs.SortDescriptions.Add(new SortDescription("GroupingHeader", ListSortDirection.Ascending));
                        cvs.SortDescriptions.Add(new SortDescription("Title", ListSortDirection.Ascending));

                        gamesList.ItemsSource = cvs;

                        Utils.HideLoading(_overlayFrame);

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
                            cvs = CollectionViewSource.GetDefaultView(results);
                            gamesList.ItemsSource = cvs;

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

        private void gamesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Game SelectedItem = (Game)gamesList.SelectedItem;
            if (SelectedItem == null)
                return;

            Console.WriteLine("Selection changed to game: " + SelectedItem.ID);

            gamesList.SelectedItem = null;

            Account.NavigateToGame(SelectedItem.ID, this.NavigationService);
        }


        private bool PageChanged = false;
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (PageChanged == false)
            {
                PageChanged = true;
                await search();

            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await search(searchQuery.Text, "search");
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            await search();
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow mainWindow = (MainWindow) Application.Current.MainWindow;
            this.NavigationService.Navigate(mainWindow.discoverP);
        }
    }
}
