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
    /// 巷道地码表
    /// </summary>
    [Table(Name = "FloorCode")]
    [Index("idx_floor_code_code", nameof(Geocode), IsUnique = true)]
    public class FloorCode : FreeSqlDateArgsBase, ICloneable
    {
        /// <summary>
        /// 巷道编码
        /// </summary>
        [Column(StringLength = 50)]
        public string RoadWayCode { get; set; }

        /// <summary>
        /// 地码编码
        /// </summary>
        [Column(StringLength = 50)]
        public string Geocode { get; set; }

        /// <summary>
        /// 启用标识
        /// </summary>
        [Column]
        public bool IsRun { get; set; }

        /// <summary>
        /// X坐标
        /// </summary>
        [Column(Precision = 10, Scale = 2, IsNullable = true)]
        public decimal? PositionX { get; set; }

        /// <summary>
        /// Y坐标
        /// </summary>
        [Column(Precision = 10, Scale = 2, IsNullable = true)]
        public decimal? PositionY { get; set; }

        /// <summary>
        /// 状态：0-正常, 1-维护中, 2-停用
        /// </summary>
        [Column(MapType = typeof(string))]
        public FloorCodeStatus Status { get; set; } = FloorCodeStatus.Normal;

        /// <summary>
        /// 优先级(越大越高)
        /// </summary>
        [Column]
        public int InitPriority { get; set; } = 1;

        [Navigate(nameof(RuninShelf.ParentId))]
        [JsonIgnore]
        public List<RuninShelf> RuninShelfs { get; set; }

        public object Clone() => MemberwiseClone();
    }

    /// <summary>
    /// 地码状态枚举
    /// </summary>
    public enum FloorCodeStatus
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

}
