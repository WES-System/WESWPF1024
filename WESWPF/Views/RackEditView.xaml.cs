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
using WES.ViewModels;

namespace WES.Views
{
    /// <summary>
    /// RackEditView.xaml 的交互逻辑
    /// </summary>
    public partial class RackEditView : Window
    {
        public RackEditView()
        {
            InitializeComponent();
            DataContext = new RackEditVM();
        }

        private void WinMove(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseBtnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
