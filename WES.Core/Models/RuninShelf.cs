using FreeSql.DataAnnotations;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WES.Models
{
    /// <summary>
    /// 货架表
    /// </summary>
    [Table(Name = "RuninShelf")]
    [Index("idx_shelf_code", nameof(ShelfCode), IsUnique = true)] // 使用索引实现唯一性
    public class RuninShelf : FreeSqlDateArgsBase, ICloneable
    {
        /// <summary>
        /// 货架编码
        /// </summary>
        [Column(StringLength = 50)]
        public string ShelfCode { get; set; }

        /// <summary>
        /// 货架名称
        /// </summary>
        [Column(StringLength = 100)]
        public string ShelfName { get; set; }

        /// <summary>
        /// 启用标识
        /// </summary>
        [Column]
        public bool IsRun { get; set; }

        /// <summary>
        /// 总列数
        /// </summary>
        [Column(IsNullable = true)]
        public int? TotalColumns { get; set; }

        /// <summary>
        /// 总行数
        /// </summary>
        [Column(IsNullable = true)]
        public int? TotalRows { get; set; }


        /// <summary>
        /// 状态：0-正常, 1-维护中, 2-停用
        /// </summary>
        [Column(MapType = typeof(string))]
        public ShelfStatus Status { get; set; } = ShelfStatus.Normal;

        /// <summary>
        /// 列
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// 行
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// 方向
        /// </summary>
        [Column(MapType = typeof(string))]
        public Direction Direction { get; set; } = Direction.East;

        public StateType StateType { get; set; } = StateType.LCSPallet;

        /// <summary>
        /// 优先级(越大越高)
        /// </summary>
        [Column]
        public int InitPriority { get; set; } = 1;

        [Column(IsNullable = false)]
        public long ParentId { get; set; }

        [Navigate(nameof(ParentId))]
        [JsonIgnore]
        public FloorCode Parent { get; set; }

        /// <summary>
        /// 货位列表
        /// </summary>
        [Navigate(nameof(ShelfLocation.ParentId))]
        [JsonIgnore]
        public List<ShelfLocation> ShelfLocations { get; set; }

        public object Clone() => MemberwiseClone();
    }


    /// <summary>
    /// 方向枚举
    /// </summary>
    public enum Direction
    {
        /// <summary>
        /// 东
        /// </summary>
        East = 0,
        /// <summary>
        /// 南
        /// </summary>
        South = 1,
        /// <summary>
        /// 西
        /// </summary>
        West = 2,
        /// <summary>
        /// 北
        /// </summary>
        North = 3,
        /// <summary>
        /// 右
        /// </summary>
        Right = 4,
        /// <summary>
        /// 左
        /// </summary>
        Left = 5,
    }

    /// <summary>
    /// 货架状态枚举
    /// </summary>
    public enum ShelfStatus
    {
        /// <summary>
        /// 正常
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 占用
        /// </summary>
        Occupied = 2,
        /// <summary>
        /// 维护中
        /// </summary>
        Maintenance = 3,
        /// <summary>
        /// 禁用
        /// </summary>
        Disabled = 4
    }

    /// <summary>
    /// 状态类型枚举
    /// </summary>
    public enum StateType
    {
        /// <summary>
        /// 类型1
        /// </summary>
        LCSCaton = 0,
        /// <summary>
        /// 类型2
        /// </summary>
        LCSPallet = 1,
        /// <summary>
        /// 类型3
        /// </summary>
        LCSSpecial = 2
    }
}
