using FreeSql.DataAnnotations;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WES.Models
{
    /// <summary>
    /// 任务进度表
    /// </summary>
    [Table(Name = "OrderTable")]
    public class OrderTable : FreeSqlDateArgsBase, ICloneable
    {
        /// <summary>
        /// 手臂编号
        /// </summary>
        [Column(IsNullable = false)]
        public string RobotCode { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [Column(MapType = typeof(string))]
        public StateType StateType { get; set; }

        /// <summary>
        /// 任务进度
        /// </summary>
        [Column(MapType = typeof(string))]
        public CompletionEnum RunCompletion { get; set; }



        public object Clone() => MemberwiseClone();
    }

    /// <summary>
    /// 任务进度
    /// </summary>
    public enum CompletionEnum
    {
        /// <summary>
        /// 未开始
        /// </summary>
        Unhandled = 0,
        /// <summary>
        /// 进行中
        /// </summary>
        Processing = 1,
        /// <summary>
        /// 右相机
        /// </summary>
        URRight = 2,
        /// <summary>
        /// 左相机
        /// </summary>
        URLeft = 3,
        /// <summary>
        /// 插线完成
        /// </summary>
        URProduct = 4,
        /// <summary>
        /// 流程完成
        /// </summary>
        Completed = 5,
        /// <summary>
        /// 处理完成
        /// </summary>
        Cancel = 6,
        /// <summary>
        /// 错误
        /// </summary>
        Error = 7,
    }
}
