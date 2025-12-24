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
    /// 入库记录表
    /// </summary>
    [Table(Name = "InboundRecord")]
    [Index("idx_inbound_record_no", nameof(RecordNo), IsUnique = true)]
    public class InboundRecord : FreeSqlDateArgsBase, ICloneable
    {
        /// <summary>
        /// 入库单号
        /// </summary>
        [Column(StringLength = 50, IsNullable = true)]
        public string RecordNo { get; set; } = Guid.NewGuid().ToString();

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
        /// 入库货架位置
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        public string InboundShelfLocation { get; set; }

        /// <summary>
        /// 入库线别
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        public string InboundLine { get; set; }

        [Column(StringLength = 30, IsNullable = true)]
        [DataMember]
        public string InboundRobotCode { get; set; }

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

}
