using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WES.Models
{
    /// <summary>
    /// 获取机台架位信息请求类(上位机——>森林)
    /// </summary>
    public class GetShelfPositionRequest
    {
        /// <summary>
        /// 场景
        /// </summary>
        public string scene { get; set; }

        /// <summary>
        /// 机台序列号
        /// </summary>
        public string usn { get; set; }

        /// <summary>
        /// 是否启用外箱结果
        /// </summary>
        public string isuse { get; set; }

        /// <summary>
        /// 是否上位机ByPass
        /// </summary>
        public string isenable { get; set; }
    }
}