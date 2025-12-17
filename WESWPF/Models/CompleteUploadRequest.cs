using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WES.Models
{
    /// <summary>
    /// 上传机台到位信息请求类(上位机——>森林)
    /// </summary>
    public class CompleteUploadRequest
    {
        /// <summary>
        /// 手臂USN
        /// </summary>
        public string usn { get; set; }

        /// <summary>
        /// 扫码枪扫到的USN
        /// </summary>
        public string barcode { get; set; }

        /// <summary>
        /// 码板机编号 仅适用于自动出库场景
        /// </summary>
        public string PackMachineNo { get; set; }

        /// <summary>
        /// 抓取点位
        /// </summary>
        public string Point { get; set; }
    }
}
