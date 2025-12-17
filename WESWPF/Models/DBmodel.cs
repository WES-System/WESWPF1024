using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBModels
{
    public class AGVTaskStatus
    {
        public string TaskCode { get; set; }
        public string robotCode { get; set; }
        public string TaskType { get; set; }
        public string podCode { get; set; }
        public string positionCodePath { get; set; }
        public string TaskStatus { get; set; }
        public string robotIp { set; get; }
        public string robotBattery { set; get; }
        public string robotStatus { set; get; }
        public DateTime CreatTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime LastTime { get; set; }
    }
    public class PositionCodePath
    {
        public string positionCode { get; set; }
        public string type { get; set; }
        public override string ToString()
        {
            return $"{positionCode}({type})";
        }
    }

}
