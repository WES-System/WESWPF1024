using WES.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WES.Models
{
    public class XMLModelBase : NotifyBase
    {
        private int id;
        /// <summary>
        /// XML模型基类 ID作为xml节点属性便于查询，保证数据唯一
        /// </summary>
        public int ID
        {
            get => id;
            set
            {
                id = value;
                DoNotify();
            }
        }
    }
}
