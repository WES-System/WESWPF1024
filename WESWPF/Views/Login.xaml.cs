using WES.ViewModels;
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

namespace WES.Views
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            DataContext = new LoginVM();
        }
        private void WinMove(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void WinClose(object sender, RoutedEventArgs e)
        {
            Close();
            //Application.Current.Shutdown();
        }
    }
}
