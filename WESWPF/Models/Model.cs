using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
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
