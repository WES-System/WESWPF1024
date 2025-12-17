using WES.Commons;
using WES.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// MainUI.xaml 的交互逻辑
    /// </summary>
    public partial class MainUI : Window
    {
        public MainUI()
        {
            InitializeComponent();
            DataContext = new MianUIVM();
        }

        private void WinMove(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseBtnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确认关闭程序？", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                Close();
            }
        }

        private void MaxBtnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void MinBtnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }


    }
}
