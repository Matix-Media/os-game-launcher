using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace OS_Game_Launcher.Pages
{
    /// <summary>
    /// Interaktionslogik für aboutOSG.xaml
    /// </summary>
    public partial class aboutOSG : Page
    {
        public aboutOSG()
        {
            InitializeComponent();
        }

        public bool PageLoaded = false;
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (PageLoaded)
                return;

            Utils.DisplayLoading(_overlayFrame);

            versionTag.Text = Utils.getRunningVersion().ToString();


            await UpdateNews();


            Utils.HideLoading(_overlayFrame);

            PageLoaded = true;

        }

        public async Task UpdateNews()
        {
            var newsFeed = await Account.GetNews();

            if (Utils.ValidateRequest(newsFeed, false))
            {
                List<NewsArticle> articles = new List<NewsArticle>();

                articles.Add(new NewsArticle() { Title = (string)newsFeed["latest"]["title"], Date = (string)newsFeed["latest"]["date"], Contents = (string)newsFeed["latest"]["content"], Image = (string)newsFeed["latest"]["image"] });

                foreach (var article in newsFeed["news"])
                {
                    articles.Add(new NewsArticle() { Title = (string)article["title"], Date = (string)article["date"], Contents = (string)article["content"], Image = (string)article["image"] });
                }

                newsPanel.ItemsSource = articles;

            }
            else
            {
                if (newsFeed.ContainsKey("error_message"))
                {
                    Utils.showMessage("Error while getting news feed: " + (string)newsFeed["error_message"]);
                }
                else
                {
                    Utils.showMessage("Unknown error occurred while trying to get news feed.");
                }
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://os-game-launcher.matix-media.net");
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Utils.DisplayLoading(_overlayFrame);

            await UpdateNews();

            Utils.HideLoading(_overlayFrame);

        }
    }

    public class NewsArticle
    {
        public string Title { get; set; }
        public string Date { get; set; }
        public string Contents { get; set; }
        public string Image { get; set; }
    }
}
