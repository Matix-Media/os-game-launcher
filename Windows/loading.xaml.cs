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
using System.Windows.Shapes;

namespace OS_Game_Launcher.Windows
{
    /// <summary>
    /// Interaktionslogik für loading.xaml
    /// </summary>
    public partial class loading : Window
    {
        public loading(bool topMost = false)
        {
            InitializeComponent();

            _overlayFrame.Navigate(new Pages.loading());

            if (topMost)
            {
                this.Topmost = true;
            }
        }
    }
}
