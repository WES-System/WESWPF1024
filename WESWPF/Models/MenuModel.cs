using FontAwesome.Sharp;
using WES.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WES.Models
{
    public class MenuModel : NotifyBase
    {
        private string contentName = "Home";
        public string ContentName
        {
            get => contentName;
            set
            {
                contentName = value;
                DoNotify();
            }
        }

        private string pageName = "Home";
        public string PageName
        {
            get => pageName;
            set
            {
                pageName = value;
                DoNotify();
            }
        }

        private IconChar icon = IconChar.Home;
        public IconChar Icon
        {
            get => icon;
            set
            {
                icon = value;
                DoNotify();
            }
        }

        private bool isChecked = false;
        public bool IsChecked
        {
            get => isChecked;
            set
            {
                isChecked = value;
                DoNotify();
            }
        }
    }
}
