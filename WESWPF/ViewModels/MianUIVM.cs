using WES.Commons;
using WES.Models;
using WES.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tiering;

namespace WES.ViewModels
{
    public class MianUIVM : NotifyBase
    {
        #region 字段属性
        string previousPage = "Home";

        
        private string _currentTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        public string CurrentTime
        {
            get => _currentTime;
            set
            {
                _currentTime = value;
                DoNotify();
            }
        }

        private FrameworkElement mainContent;
        public FrameworkElement MainContent
        {
            get => mainContent;
            set
            {
                mainContent = value;
                DoNotify();
            }
        }

        private List<MenuModel> menus;
        public List<MenuModel> Menus
        {
            get => menus;
            set
            {
                menus = value;
                DoNotify();
            }
        }
        #endregion

        #region 命令
        private CommandBase navChangeCommand;
        public CommandBase NavChangeCommand
        {
            get
            {
                if (navChangeCommand == null)
                {
                    navChangeCommand = new CommandBase()
                    {
                        DoExcute = DoNavChanged,
                        DoCanExecute = obj => { return true; }
                    };
                }
                return navChangeCommand;
            }
        }

        private void DoNavChanged(object obj)
        {
            switch (obj)
            {
                case "Home":
                    if (GlobalParams.Home == null)
                    {
                        Type type = Type.GetType("WES.Views." + obj.ToString());
                        ConstructorInfo cti = type.GetConstructor(Type.EmptyTypes);
                        GlobalParams.Home = (FrameworkElement)cti.Invoke(null);
                    }
                    MainContent = GlobalParams.Home;
                    previousPage = obj.ToString();
                    break;
                case "Manual":
                    if (Global.CurrentUser.UserName == "LayUI")
                    {
                        if (new Login().ShowDialog() != true)
                        {
                            return;
                        }
                    }
                    if (previousPage != obj.ToString() && Global.CurrentUser.UserName == "LayUI")
                    {
                        Menus.First(c => c.PageName == previousPage).IsChecked = true;
                        return;
                    }
                    if (GlobalParams.Manual == null)
                    {
                        Type type = Type.GetType("WES.Views." + obj.ToString());
                        ConstructorInfo cti = type.GetConstructor(Type.EmptyTypes);
                        GlobalParams.Manual = (FrameworkElement)cti.Invoke(null);
                    }
                    MainContent = GlobalParams.Manual;
                    previousPage = obj.ToString();
                    break;
                case "Settings":
                    if (Global.CurrentUser.UserName == "LayUI")
                    {
                        if (new Login().ShowDialog() != true)
                        {
                            return;
                        }
                    }
                    if (previousPage != obj.ToString() && Global.CurrentUser.UserName == "LayUI")
                    {
                        Menus.First(c => c.PageName == previousPage).IsChecked = true;
                        return;
                    }
                    if (GlobalParams.Settings == null)
                    {
                        Type type = Type.GetType("WES.Views." + obj.ToString());
                        ConstructorInfo cti = type.GetConstructor(Type.EmptyTypes);
                        GlobalParams.Settings = (FrameworkElement)cti.Invoke(null);
                    }
                    MainContent = GlobalParams.Settings;
                    previousPage = obj.ToString();
                    break;
                case "RackManagement":
                    if (Global.CurrentUser.UserName == "LayUI")
                    {
                        if (new Login().ShowDialog() != true)
                        {
                            return;
                        }
                    }
                    if (previousPage != obj.ToString() && Global.CurrentUser.UserName == "LayUI")
                    {
                        Menus.First(c => c.PageName == previousPage).IsChecked = true;
                        return;
                    }
                    if (GlobalParams.RackManagement == null)
                    {
                        Type type = Type.GetType("WES.Views." + obj.ToString());
                        ConstructorInfo cti = type.GetConstructor(Type.EmptyTypes);
                        GlobalParams.RackManagement = (FrameworkElement)cti.Invoke(null);
                    }
                    MainContent = GlobalParams.RackManagement;
                    previousPage = obj.ToString();
                    break;
                default:
                    break;
            }
        }
        #endregion

        public MianUIVM()
        {
            DoNavChanged("Home");
            InitMenu();
            UpdateData();
        }

        private void InitMenu()
        {
            Menus = new List<MenuModel>()
            {
                new MenuModel() { ContentName = "主程序", PageName = "Home", Icon = FontAwesome.Sharp.IconChar.Medapps ,IsChecked = true },
                new MenuModel() { ContentName = "手动操作", PageName = "Manual", Icon = FontAwesome.Sharp.IconChar.Tools },
                new MenuModel() { ContentName = "架位管理", PageName = "RackManagement", Icon = FontAwesome.Sharp.IconChar.FileCircleCheck },
                new MenuModel() { ContentName = "参数设置", PageName = "Settings", Icon = FontAwesome.Sharp.IconChar.Gear },
            };
        }

        //更新时间
        private void UpdateData()
        {
            Task timeTask = Task.Run(async() =>
            {
                while (true)
                {
                    await Task.Delay(900);
                    CurrentTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    TimeSpan elapsed = DateTime.Now - Global.LoginUserTime;
                    if (elapsed.TotalMinutes > 7)
                    {
                        Global.CurrentUser.UserName = "LayUI";
                    }
                }
            });
        }
    }
}
