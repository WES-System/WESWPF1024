using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WES.Models
{
    /// <summary>
    /// 获取机台架位信息响应类(森林——>上位机)
    /// </summary>
    public class GetShelfPositionResponse
    {
        /// <summary> 
        /// 机台序列号
        /// </summary>
        public string usn { get; set; }

        /// <summary>
        /// 坐标
        /// </summary>
        public string coordinate { get; set; }
    }
}
