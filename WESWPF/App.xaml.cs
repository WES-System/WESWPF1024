using WES.Helpers;
using WES.Models;
using WES.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WES
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            LogHelper.Info("<----------------------------程序运行---------------------------->");
            //Thread thread = new Thread(new ThreadStart(() =>
            //{
            //    Views.MainUI.loadPage = new Views.LoadPage();
            //    Views.MainUI.loadPage.Show();
            //    Views.MainUI.loadPage.Activate();
            //    System.Windows.Threading.Dispatcher.Run();
            //}));
            //thread.SetApartmentState(ApartmentState.STA);
            //thread.IsBackground = true;
            //thread.Start();
            PGSQLHelper.GetInstance();
            GlobalParams.config = ConfigHelper.LoadConfigByXML<ConfigModel>("WES.exe.config");
            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            LogHelper.Info("<----------------------------程序退出---------------------------->\r\n");
            await Task.Delay(20);
            base.OnExit(e);
        }
    }
}
