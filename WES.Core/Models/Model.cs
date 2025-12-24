using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;

namespace Models
{
    /// <summary>
    /// 数据库表单：父表
    /// </summary>
    [DataContract]
    public class FreeSqlDateArgsBase
    {
        /// <summary>
        /// 属性：主键
        /// </summary>
        [DataMember]
        [Column(IsPrimary = true, IsIdentity = true)]
        public long Id { get; set; }
        /// <summary>
        /// 属性：乐观锁
        /// </summary>
        [Column(IsVersion = true)]
        public long Version { get; set; }
        /// <summary>
        /// 属性：创建时间
        /// </summary>
        [DataMember]
        [Column(ServerTime = DateTimeKind.Local, CanUpdate = false)]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 属性：更新时间
        /// </summary>
        //[Column(ServerTime = DateTimeKind.Utc, CanInsert = false)]
        [Column(ServerTime = DateTimeKind.Local)]
        public DateTime UpdateTime { get; set; }
    }


    public class TaskBaseInfo
    {
        public string reqCode { get; set; }
        public string taskCode { get; set; }
    }

    public class MapInfo
    {
        public string reqCode { get; set; }
        public string mapShortName { get; set; }
        //public PositionCodePath[] positionCodePath { get; set; }
        public string robotCount {
            get { return "-1"; }
        }
    }

    [Serializable]
    public class CreatTaskInfo
    {
        /// <summary>
        /// 任务编号
        /// </summary>
        public string robotTaskCode { get; set; }

        /// <summary>
        /// 任务类型
        /// </summary>
        public string taskType { get; set; }
        
        /// <summary>
        /// 任务地点
        /// </summary>
        public List<TaskLocation> targetRoute { get; set; }
    }

    [Serializable]
    public class TaskLocation
    {
        /// <summary>
        /// 目标路径序列0
        /// </summary>
        public int seq { get; set; }
        /// <summary>
        /// 目标类型
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// 与type对应的目标编号
        /// </summary>
        public string code { get; set; }
    }


    public class DataFromClient
    {
        public string message { get; set; }
        public TaskBaseInfo taskInfo { set; get; }
        public MapInfo mapInfo { get; set; }
    }


    public class DataFromSever
    {
        public string code { get; set; }
        public string message { get; set; }
        public string reqCode { get; set; }
        public string data { get; set; }
    }

    public class AGVStatus
    {
        public string robotCode { set; get; }
        public string robotIp { set; get; }
        public string battery { set; get; }       
        public string status { set; get; }
 
    }   
}
