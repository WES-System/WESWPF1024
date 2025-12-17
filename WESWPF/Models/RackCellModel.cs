using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WES.Models
{
    /// <summary>
    /// 用于存放料框的位置信息
    /// </summary>
    public class RackCellModel
    {
        /// <summary>
        /// ID标识 自增 主键
        /// </summary>
        [Column(IsPrimary = true, IsIdentity = true)]
        public int ID { get; set; }

        /// <summary>
        /// RackCell编号
        /// </summary>
        [Column]
        public string RackCellCode { get; set; }

        /// <summary>
        /// 行索引
        /// </summary>
        [Column]
        public int RackCellRowIndex { get; set; }

        /// <summary>
        /// 列索引
        /// </summary>
        [Column]
        public int RackCellColumnIndex { get; set; }

        /// <summary>
        /// RackCell类型三种 正 反 特殊
        /// </summary>
        [Column]
        public string RackCellType { get; set; }
    }
}
