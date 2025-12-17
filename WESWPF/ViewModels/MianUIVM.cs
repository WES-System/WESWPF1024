using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WES.Commons;
using WES.Models;
using WES.Views;

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
            string pageName = obj.ToString();
            if (string.IsNullOrEmpty(pageName))
            {
                // 点击一级菜单（无页面）时仅切换展开状态
                var parentMenu = Menus.FirstOrDefault(m => m.SubMenus.Any(s => s.PageName == previousPage));
                if (parentMenu != null)
                    parentMenu.IsExpanded = !parentMenu.IsExpanded;
                return;
            }

            switch (pageName)
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
                    if (Global.CurrentUser.UserName != "administrator")
                    {
                        if (new Login().ShowDialog() != true)
                        {
                            return;
                        }
                    }
                    if (previousPage != obj.ToString() && Global.CurrentUser.UserName != "administrator")
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
                    if (Global.CurrentUser.UserName != "administrator")
                    {
                        if (new Login().ShowDialog() != true)
                        {
                            return;
                        }
                    }
                    if (previousPage != obj.ToString() && Global.CurrentUser.UserName != "administrator")
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
                    if (Global.CurrentUser.UserName != "administrator")
                    {
                        if (new Login().ShowDialog() != true)
                        {
                            return;
                        }
                    }
                    if (previousPage != obj.ToString() && Global.CurrentUser.UserName != "administrator")
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
            Menus = new List<MenuModel>
            {
                // 首页（无二级菜单）
                new MenuModel
                {
                    ContentName = "系统首页",
                    PageName = "Home",
                    Icon = IconChar.Home,
                    IsChecked = true
                },
                // 设备管理（带二级菜单）
                new MenuModel
                {
                    ContentName = "设备管理",
                    PageName = "", // 一级菜单无需页面标识
                    Icon = IconChar.Microchip,
                    SubMenus = new List<MenuModel>
                    {
                        new MenuModel { ContentName = "手动操作", PageName = "Manual", Icon = IconChar.Barcode },
                        //new MenuModel { ContentName = "机械臂控制", PageName = "RobotControl", Icon = IconChar.Robot }
                    }
                },
                // 系统设置（带二级菜单）
                new MenuModel
                {
                    ContentName = "系统设置",
                    PageName = "",
                    Icon = IconChar.Cog,
                    SubMenus = new List<MenuModel>
                    {
                        new MenuModel { ContentName = "基础配置", PageName = "Settings", Icon = IconChar.Sliders },
                        new MenuModel { ContentName = "架位管理", PageName = "RackManagement", Icon = IconChar.Link }
                    }
                }
            };
            //Menus = new List<MenuModel>()
            //{
            //    new MenuModel() { ContentName = "主程序", PageName = "Home", Icon = FontAwesome.Sharp.IconChar.Medapps ,IsChecked = true },
            //    new MenuModel() { ContentName = "手动操作", PageName = "Manual", Icon = FontAwesome.Sharp.IconChar.Tools },
            //    new MenuModel() { ContentName = "架位管理", PageName = "RackManagement", Icon = FontAwesome.Sharp.IconChar.FileCircleCheck },
            //    new MenuModel() { ContentName = "参数设置", PageName = "Settings", Icon = FontAwesome.Sharp.IconChar.Gear },
            //};
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
                    if (elapsed.TotalMinutes > 24)
                    {
                        Global.CurrentUser.UserName = "Q22050534";
                    }
                }
            });
        }
    }
}
