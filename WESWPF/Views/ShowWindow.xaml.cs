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
    /// ShowWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ShowWindow : Window
    {
        public string Line { get; set; }

        public ShowWindow()
        {
            InitializeComponent();
        }

        private void CloseBtnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void WinMove(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ChangeClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(LineName.Text.Trim()))
            {
                Line = LineName.Text;
                DialogResult = true;
                CloseBtnClick(null, null);
            }
        }
    }
}
