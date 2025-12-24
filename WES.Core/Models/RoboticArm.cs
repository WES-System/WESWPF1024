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
    /// 手臂表
    /// </summary>
    [Table(Name = "RoboticArm")]
    [Index("idx_robotic_arm_code", nameof(RobotCode), IsUnique = true)]
    public class RoboticArm : FreeSqlDateArgsBase, ICloneable
    {
        /// <summary>
        /// 机械臂编码
        /// </summary>
        [Column(StringLength = 50)]
        public string RobotCode { get; set; }

        /// <summary>
        /// 当前巷道
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        public string CurrentRoadWayCode { get; set; }

        /// <summary>
        /// 当前地码
        /// </summary>
        [Column(StringLength = 30, IsNullable = true)]
        public string CurrentFloorCode { get; set; }

        /// <summary>
        /// 状态：0-空闲, 1-工作中, 2-故障, 3-维护中
        /// </summary>
        [Column(MapType = typeof(string))]
        public RobotStatus Status { get; set; } = RobotStatus.Idle;

        /// <summary>
        /// 优先级(越大越高)
        /// </summary>
        [Column]
        public int InitPriority { get; set; } = 1;


        public object Clone() => MemberwiseClone();
    }

    /// <summary>
    /// 机械臂状态枚举
    /// </summary>
    public enum RobotStatus
    {
        /// <summary>
        /// 空闲
        /// </summary>
        Idle = 0,
        /// <summary>
        /// 工作中
        /// </summary>
        Working = 1,
        /// <summary>
        /// 故障
        /// </summary>
        Fault = 2,
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
