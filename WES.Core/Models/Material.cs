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
    /// 物料表
    /// </summary>
    [Table(Name = "Material")]
    [Index("idx_material_code", nameof(MachineCode), IsUnique = true)]
    public class Material : FreeSqlDateArgsBase, ICloneable
    {
        /// <summary>
        /// 机台编码
        /// </summary>
        [Column]
        [DataMember]
        public string MachineCode { get; set; }

        /// <summary>
        /// 载具编码
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        [DataMember]
        public string CarrierCode { get; set; }

        /// <summary>
        /// 架位编码
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        [DataMember]
        public string ShelfCode { get; set; }

        /// <summary>
        /// 销售订单号(Sales order)
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        public string So { get; set; }

        /// <summary>
        /// MO的类型(Motype)
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        public string Motype { get; set; }

        /// <summary>
        /// 库别(Store Location)
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        public string Storeloc { get; set; }

        /// <summary>
        /// BatchNO
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        public string Batchid { get; set; }

        /// <summary>
        /// 集货地
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        public string Mcid { get; set; }

        /// <summary>
        /// OOB通过Area来区分:比如:"CTN",且会包含error信息
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        public string Area { get; set; }

        /// <summary>
        /// "Key":"UPN | BATCHID"
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        public string Key { get; set; }

        /// <summary>
        /// Carton ID
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        public string cartonid { get; set; }

        /// <summary>
        /// carton里面的机台数量
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        public int usnqtybycarton { get; set; }

        /// <summary>
        /// 一个carton里面有多少机台
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        public int qtyper { get; set; }

        /// <summary>
        /// 是否是特殊料号
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        public string specialupnflag { get; set; }

        /// <summary>
        /// 线别
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        public string line { get; set; }

        /// <summary>
        /// MO工单
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        public string mo { get; set; }

        /// <summary>
        /// 机种
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        public string modelfamily { get; set; }

        /// <summary>
        /// 厂别
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        public string plant { get; set; }

        /// <summary>
        /// 是否是大箱子
        /// Y/N
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        public string oversizeflag { get; set; }

        /// <summary>
        /// 入库货架位置
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        [DataMember]
        public string InboundShelfLocation { get; set; }

        /// <summary>
        /// 入库货架位置
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        [DataMember]
        public string SecondInboundShelfLocation { get; set; }

        /// <summary>
        /// 出库货架位置
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        [DataMember]
        public string OutboundShelfLocation { get; set; }

        /// <summary>
        /// 当前货架码
        /// </summary>
        [Column(IsNullable = true)]
        [DataMember]
        public string FloorCode { get; set; }

        /// <summary>
        /// 入库线别
        /// </summary>
        [Column(IsNullable = true)]
        [DataMember]
        public string InboundLine { get; set; }

        /// <summary>
        /// 出库线别
        /// </summary>
        [Column(IsNullable = true)]
        [DataMember]
        public string OutboundLine { get; set; }

        /// <summary>
        /// 状态：0-正常, 1-入库, 2-出库
        /// </summary>
        [Column(MapType = typeof(string))]
        [DataMember]
        public MaterialStatus Status { get; set; }

        /// <summary>
        /// 机械臂编码
        /// </summary>
        [Column(MapType = typeof(string), IsNullable = true)]
        [DataMember]
        public string RobotCode { get; set; }

        [Column]
        [DataMember]
        public bool FristTimes { get; set; } = true;

        [Column]
        [DataMember]
        public bool SecondTimes { get; set; } = true;

        [Column]
        [DataMember]
        public bool ThirdTimes { get; set; } = true;

        /// <summary>
        /// 是否激活
        /// </summary>
        [Column]
        [DataMember]
        public bool IsActive { get; set; } = true;

        public object Clone() => MemberwiseClone();
    }

    /// <summary>
    /// 货架状态枚举
    /// </summary>
    public enum MaterialStatus
    {
        /// <summary>
        /// 正常
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 入库
        /// </summary>
        InboundRecord = 1,
        /// <summary>
        /// 出库
        /// </summary>
        OutboundRecord = 2,
        /// <summary>
        /// 完成
        /// </summary>
        Completed = 3,
        /// <summary>
        /// 入库完成
        /// </summary>
        InboundCompleted = 4,
        /// <summary>
        /// 出库完成
        /// </summary>
        OutboundCompleted = 5,
        /// <summary>
        /// 失败
        /// </summary>
        Failed = 6
    }
}
