using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Common;
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
        /// <summary>
        /// 枚举：状态
        /// </summary>
        [Column(MapType = typeof(int))]
        public StateEnum State { get; set; }
        /// <summary>
        /// 忽略：错误信息
        /// </summary>
        [Column(IsIgnore = true)]
        [JsonIgnore]
        public Exception Error { get; set; }
        /// <summary>
        /// 忽略：执行结果
        /// </summary>
        [Column(IsIgnore = true)]
        [JsonIgnore]
        public FreeSqlDateResultEnum Result { get; set; }
    }

    /// <summary>
    /// 基础状态枚举
    /// </summary>
    public enum StateEnum
    {
        /// <summary>
        /// 正常
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 占用
        /// </summary>
        Occupied = 1,
        /// <summary>
        /// 禁用
        /// </summary>
        Disabled = 2
    }

    /// <summary>
    /// 数据库操作结果枚举
    /// </summary>
    public enum FreeSqlDateResultEnum
    {
        /// <summary>
        /// 成功
        /// </summary>
        Success = 0,
        /// <summary>
        /// 失败
        /// </summary>
        Failure = 1,
        /// <summary>
        /// 处理中
        /// </summary>
        Processing = 2
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
    public class SceneData
    {
        /// <summary>
        /// 用于存放任务的字典，索引为任务码
        /// </summary>
        public Dictionary<string, DBModels. AGVTaskStatus> AGVTasks = new Dictionary<string, DBModels.AGVTaskStatus>();      
        public string SceneCode { get; set; }
        public string ClientIp { get; set; }
        public int TaskTotalCount
        { get {
                var keys = AGVTasks.Keys;
                return keys.Count;
            } }
        public int TaskCompletedCount
        { get {
                var keys = AGVTasks.Keys;
                int count = 0;
                foreach (var item in keys)
                {
                    if (AGVTasks[item].TaskStatus=="9")
                    {
                        count++;
                    }  
                }
                return count;
            } }
        public int TaskRunningCount
        { get {
                var keys = AGVTasks.Keys;
                int count = 0;
                foreach (var item in keys)
                {
                    if (AGVTasks[item].TaskStatus == "2")
                    {
                        count++;
                    }
                }
                return count;
            } }
        public int TaskReadyCount
        { get {
                var keys = AGVTasks.Keys;
                int count = 0;
                foreach (var item in keys)
                {
                    if (AGVTasks[item].TaskStatus == "1")
                    {
                        count++;
                    }
                }
                return count;
            } }
        public int TaskCancelCount
        {
            get
            {
                var keys = AGVTasks.Keys;
                int count = 0;
                foreach (var item in keys)
                {
                    if (AGVTasks[item].TaskStatus == "5")
                    {
                        count++;
                    }
                }
                return count;
            }
        }
    }
    public class AGVStatus
    {
        public string robotCode { set; get; }
        public string robotIp { set; get; }
        public string battery { set; get; }       
        public string status { set; get; }
 
    }   
}
