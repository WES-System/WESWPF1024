using FreeSql.DataAnnotations;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WES.Models
{
    /// <summary>
    /// 出库记录表
    /// </summary>
    [Table(Name = "OutboundRecord")]
    [Index("idx_outbound_record_no", nameof(RecordNo), IsUnique = true)]
    public class OutboundRecord : FreeSqlDateArgsBase, ICloneable
    {
        /// <summary>
        /// 出库单号
        /// </summary>
        [Column(StringLength = 50)]
        public string RecordNo { get; set; }

        /// <summary>
        /// 机台编码
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        public string MachineCode { get; set; }

        /// <summary>
        /// 载具编码
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        public string CarrierCode { get; set; }

        /// <summary>
        /// 架位编码
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        public string ShelfCode { get; set; }

        /// <summary>
        /// 出库货架位置
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        public string OutboundShelfLocation { get; set; }

        /// <summary>
        /// 出库线别
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        public string OutboundLine { get; set; }

        [Column(StringLength = 30, IsNullable = true)]
        [DataMember]
        public string OutboundRobotCode { get; set; }

        [Column(StringLength = 30, IsNullable = true)]
        [DataMember]
        public string OutboundRobotName { get; set; }

        /// <summary>
        /// 状态：0-待处理, 1-处理中, 2-完成, 3-失败
        /// </summary>
        [Column(MapType = typeof(string))]
        public RecordStatus Status { get; set; } = RecordStatus.Pending;

        /// <summary>
        /// 备注
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        public string Remark { get; set; }
        public object Clone() => MemberwiseClone();
    }

    /// <summary>
    /// 记录状态枚举（入库/出库）
    /// </summary>
    public enum RecordStatus
    {
        /// <summary>
        /// 待处理
        /// </summary>
        Pending = 0,
        /// <summary>
        /// 处理中
        /// </summary>
        Processing = 1,
        /// <summary>
        /// 完成
        /// </summary>
        Completed = 2,
        /// <summary>
        /// 失败
        /// </summary>
        Failed = 3
    }

}
