using FreeSql.DataAnnotations;
using WES.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WES.Models
{
    [Table(Name = "MachineRecordInfo")]
    public class MachineMSGModel : NotifyBase
    {
        private int id;
        /// <summary>
        /// ID
        /// </summary>
        [Column(IsPrimary = true, IsIdentity = true)]
        public int ID
        {
            get => id;
            set
            {
                id = value;
                DoNotify();
            }
        }

        private string rawData;
        /// <summary>
        /// 原始数据
        /// </summary>
        public string RawData
        {
            get => rawData;
            set
            {
                rawData = value;
                DoNotify();
            }
        }

        private string usn;
        /// <summary>
        /// 机台USN
        /// </summary>
        public string USN
        {
            get => usn;
            set
            {
                usn = value;
                DoNotify();
            }
        }

        private string mType;
        /// <summary>
        /// 机台类型 正常入库or机台回流
        /// </summary>
        public string MType
        {
            get => mType;
            set
            {
                mType = value;
                DoNotify();
            }
        }

        private string rackCellMsg;
        /// <summary>
        /// 架位信息
        /// </summary>
        public string RackCellMsg
        {
            get => rackCellMsg;
            set
            {
                rackCellMsg = value;
                DoNotify();
            }
        }

        private bool isUploaded;
        /// <summary>
        /// 是否已上传
        /// </summary>
        public bool IsUploaded
        {
            get => isUploaded;
            set
            {
                isUploaded = value;
                DoNotify();
            }
        }

        private DateTime createTime = DateTime.Now;
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime
        {
            get => createTime;
            set
            {
                createTime = value;
                DoNotify();
            }
        }
    }
}
