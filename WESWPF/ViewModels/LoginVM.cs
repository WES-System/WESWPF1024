using WES.Commons;
using WES.Helpers;
using WES.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WES.ViewModels
{
    public class LoginVM : NotifyBase
    {
        private XMLHelper helper;

        private string _account = "administrator";
        public string Account
        {
            get => _account;
            set
            {
                _account = value;
                DoNotify();
            }
        }

        private string _password = "";
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                DoNotify();
            }
        }

        private string _tipMsg = "";
        public string TipMsg
        {
            get => _tipMsg;
            set
            {
                _tipMsg = value;
                DoNotify();
            }
        }

        private CommandBase _loginCommand;
        public CommandBase LoginCommand
        {
            get
            {
                if (_loginCommand == null)
                {
                    _loginCommand = new CommandBase()
                    {
                        DoExcute = new Action<object>(Login),
                        DoCanExecute = obj => { return true; }
                    };
                }
                return _loginCommand;
            }
        }

        public LoginVM()
        {
            helper = new XMLHelper("XML\\Config.xml");
        }

        private void Login(object obj)
        {
            List<User> users = helper.QueryXMLModels<User>();
            if (users.Count > 0)
            {
                foreach (User user in users)
                {
                    if (Account == user.UserName && Password == user.Password)
                    {
                        Window o = obj as Window;
                        o.DialogResult = true;
                        o.Close();
                        Global.CurrentUser = user;
                        Global.LoginUserTime = DateTime.Now;
                        return;
                    }
                }
                TipMsg = "账号密码不匹配";
            }
            else
            {
                TipMsg = "未找到用户信息，请检查配置";
            }
        }
    }
}
