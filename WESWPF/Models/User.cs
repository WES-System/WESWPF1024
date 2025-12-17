using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WES.Models
{
    public class User : XMLModelBase
    {
        /// <summary>
        /// 用户UID
        /// </summary>
        public string UID { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 注册时间
        /// </summary>
        public string RegistrationTime { get; set; }
    }
}
