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
    /// Interaktionslogik für msgBox.xaml
    /// </summary>
    public partial class msgBox : Window
    {
        public msgBox(string message, bool showCancel = false)
        {
            InitializeComponent();

            _message.Text = message;
            if (showCancel)
            {
                cancelBtn.Visibility = Visibility.Visible;
            }
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
