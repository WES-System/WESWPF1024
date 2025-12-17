using WES.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WES.Models
{
    public class ConfigModel : NotifyBase
    {
        private string timeInterval = "10";
        /// <summary>
        /// 时间间隔
        /// </summary>
        public string TimeInterval
        {
            get => timeInterval;
            set
            {
                timeInterval = value;
                DoNotify();
            }
        }

        private string line = "DA3";
        /// <summary>
        /// 线别
        /// </summary>
        public string Line
        {
            get => line;
            set
            {
                line = value;
                DoNotify();
            }
        }

        private string cylinderIP = "192.168.1.1";
        /// <summary>
        /// 气缸PLC IP
        /// </summary>
        public string CylinderIP
        {
            get => cylinderIP;
            set
            {
                cylinderIP = value;
                DoNotify();
            }
        }

        private string cylinderPort = "502";
        /// <summary>
        /// 气缸PLC 端口
        /// </summary>
        public string CylinderPort
        {
            get => cylinderPort;
            set
            {
                cylinderPort = value;
                DoNotify();
            }
        }

        private string fsIP = "192.168.1.1";
        /// <summary>
        /// 森林系统IP
        /// </summary>
        public string FSIP
        {
            get => fsIP;
            set
            {
                fsIP = value;
                DoNotify();
            }
        }

        private string fsPort = "8000";
        /// <summary>
        /// 森林系统端口
        /// </summary>
        public string FSPort
        {
            get => fsPort;
            set
            {
                fsPort = value;
                DoNotify();
            }
        }

        private string wxCMIP = "192.168.1.1";
        /// <summary>
        /// 外箱检测机IP
        /// </summary>
        public string WXCMIP
        {
            get => wxCMIP;
            set
            {
                wxCMIP = value;
                DoNotify();
            }
        }

        private string wxCMPort = "8000";
        /// <summary>
        /// 外箱检测机端口
        /// </summary>
        public string WXCMPort
        {
            get => wxCMPort;
            set
            {
                wxCMPort = value;
                DoNotify();
            }
        }

        private string robotIP = "192.168.1.1";
        /// <summary>
        /// 手臂IP
        /// </summary>
        public string RobotIP
        {
            get => robotIP;
            set
            {
                robotIP = value;
                DoNotify();
            }
        }

        private string robotPort = "8002";
        /// <summary>
        /// 手臂端口
        /// </summary>
        public string RobotPort
        {
            get => robotPort;
            set
            {
                robotPort = value;
                DoNotify();
            }
        }

        private string barcodeIP = "192.168.1.1";
        /// <summary>
        /// 扫码枪IP
        /// </summary>
        public string BarcodeIP
        {
            get => barcodeIP;
            set
            {
                barcodeIP = value;
                DoNotify();
            }
        }

        private string barcodePort = "2001";
        /// <summary>
        /// 扫码枪端口
        /// </summary>
        public string BarcodePort
        {
            get => barcodePort;
            set
            {
                barcodePort = value;
                DoNotify();
            }
        }

        private string rotatingPlatformIP = "192.168.1.1";
        /// <summary>
        /// 旋转平台PLC IP
        /// </summary>
        public string RotatingPlatformIP
        {
            get => rotatingPlatformIP;
            set
            {
                rotatingPlatformIP = value;
                DoNotify();
            }
        }

        private string rotatingPlatformPort = "502";
        /// <summary>
        /// 旋转平台PLC 端口
        /// </summary>
        public string RotatingPlatformPort
        {
            get => rotatingPlatformPort;
            set
            {
                rotatingPlatformPort = value;
                DoNotify();
            }
        }

        private string scene = "0";
        /// <summary>
        /// 场景 0-->码垛入库 1-->理货 2-->自动出库 3-->手动出库
        /// </summary>
        public string Scene
        {
            get => scene;
            set
            {
                scene = value;
                DoNotify();
            }
        }

        private string packMachineNo = "";
        /// <summary>
        /// 码板机编号
        /// </summary>
        public string PackMachineNo
        {
            get => packMachineNo;
            set
            {
                packMachineNo = value;
                DoNotify();
            }
        }

        private string robotTriggerSrt = "start";
        /// <summary>
        /// 手臂流程触发字符
        /// </summary>
        public string RobotTriggerSrt
        {
            get => robotTriggerSrt;
            set
            {
                robotTriggerSrt = value;
                DoNotify();
            }
        }

        private string scannerTriggerStr = "photo";
        /// <summary>
        /// 扫码枪触发字符
        /// </summary>
        public string ScannerTriggerStr
        {
            get => scannerTriggerStr;
            set
            {
                scannerTriggerStr = value;
                DoNotify();
            }
        }

        private string getShelfPoint_URL = "http://10.60.209.99:22222/Wincc/AllocateBox";
        /// <summary>
        /// 获取架位信息地址
        /// </summary>
        public string GetShelfPoint_URL
        {
            get => getShelfPoint_URL;
            set
            {
                getShelfPoint_URL = value;
                DoNotify();
            }
        }

        private string wxUpload_URL = "http://10.60.209.99:22222/Wincc/AnnounceWX";
        /// <summary>
        /// 外箱数据上抛地址
        /// </summary>
        public string WXUpload_URL
        {
            get => wxUpload_URL;
            set
            {
                wxUpload_URL = value;
                DoNotify();
            }
        }

        private string completeUpload_URL = "http://10.60.209.99:22222/Wincc/Arm";
        /// <summary>
        /// 入库完成上抛地址
        /// </summary>
        public string CompleteUpload_URL
        {
            get => completeUpload_URL;
            set
            {
                completeUpload_URL = value;
                DoNotify();
            }
        }
    }
}
