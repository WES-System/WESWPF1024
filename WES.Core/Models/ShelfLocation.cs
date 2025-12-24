using FreeSql.DataAnnotations;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace WES.Models
{
    /// <summary>
    /// 货位表
    /// </summary>
    [Table(Name = "ShelfLocation")]
    [Index("idx_locations_shelf_pos", nameof(ShelfCode), IsUnique = true)]
    public class ShelfLocation : FreeSqlDateArgsBase, ICloneable
    {
        /// <summary>
        /// 货位编码
        /// </summary>
        [Column]
        public string ShelfCode { get; set; }

        /// <summary>
        /// 机台编码
        /// </summary>
        [Column(IsNullable = true)]
        [DataMember]
        public string MachineCode { get; set; }

        /// <summary>
        /// 载具编码
        /// </summary>
        [Column(IsNullable = true)]
        [DataMember]
        public string CarrierCode { get; set; }

        /// <summary>
        /// 列号（1-12）
        /// </summary>
        [Column]
        public int ColumnNum { get; set; }

        /// <summary>
        /// 行号（1-9）
        /// </summary>
        [Column]
        public int RowNum { get; set; }

        /// <summary>
        /// 启用标识
        /// </summary>
        [Column]
        public bool IsRun { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        [Column(IsNullable = true)]
        public int? Quantity { get; set; } = 0;

        /// <summary>
        /// 状态：0-空闲, 1-占用, 2-锁定
        /// </summary>
        [Column(MapType = typeof(string))]
        public ShelfLocationStatus Status { get; set; }

        /// <summary>
        /// 优先级(越大越高)
        /// </summary>
        [Column]
        public int InitPriority { get; set; } = 1;

        [Column(IsNullable = false)]
        public long ParentId { get; set; }

        [Navigate(nameof(ParentId))]
        [JsonIgnore]
        public ShelfLocation Parent { get; set; }

        public object Clone() => MemberwiseClone();
    }

    /// <summary>
    /// 货位状态枚举
    /// </summary>
    public enum ShelfLocationStatus
    {
        /// <summary>
        /// 空闲
        /// </summary>
        Idle = 0,
        /// <summary>
        /// 占用
        /// </summary>
        Occupied = 1,
        /// <summary>
        /// 锁定
        /// </summary>
        Locked = 2,
        /// <summary>
        /// 禁用
        /// </summary>
        Disabled = 3
    }
}
