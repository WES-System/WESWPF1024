using WES.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tiering
{
    public class Global
    {
        public static string MainDir = "";

        public static string NewDir = "";
        public static string Imagesuf = "";

        public static int SecondsTime = 1000;

        public static int ThreadTime = 1000;

        public static string TYPE = "SITE";

        public static string gApplicationDirectory = System.AppDomain.CurrentDomain.BaseDirectory + "\\DataBase\\Settings.ini";
        
        public static string Section = "Modbus";
        public static string AGV = "AGV";
        /// <summary>
        /// 服务器IP地址
        /// </summary>
        public static string gServerIP = string.Empty;
        /// <summary>
        /// 服务器端口号
        /// </summary>
        public static int gServerPort = int.MinValue;


        public static User CurrentUser = new User
        {
            UserName = "LayUI",
            Password = "Q22050534",
        };
        public static DateTime CurrentUserTime = DateTime.Now;
        public static DateTime LoginUserTime = DateTime.Now;
    }

    public class gVar
    {
        public static string evt_pubBy = "nifi"; //    nifi
        public static string Site = "WKS"; //  廠別 WKS
        public static string Plant = "F232"; // 廠別代碼 F232
        public static string Dept = "PAE"; // 設備部門 （用於部門設備對比分析） PAE       FA
        public static string Line = "HA15"; // 线别
        public static string MachineName = "AIS_INP_Whole"; // 設備名稱                 AIS_INP
        public static string MachineModel = "LN_MachineType"; // 設備類型               LN-L2-600SD   
        public static string MachineSN = "AIS-P5-0001"; // 設備編碼                     AIS-0001  設備財編
        public static string MachineNO = "NO.P5_0001"; // 設備編號                      NO.0001  綫上編號
        public static string MachineIP = "123.45.67.89"; // 設備IP
        public static string OperatorID = "K15070462"; // 維護員
        public static string flagformErr = "";


        //private static UserPrivilegeEnum _userPrivilege;//使用者状态全局变量
        //public static UserPrivilegeEnum UserPrivilege
        //{
        //    get
        //    {
        //        return _userPrivilege;
        //    }
        //    set
        //    {
        //        if (_userPrivilege == value)
        //            return;
        //        _userPrivilege = value;
        //        frmMain.AllComponent.ChangeUserPrivilegeMsg();
        //        frmSetting.AllComponent.ChangeUserPrivilegeMsg();
        //    }
        //}
        public static string MachineParameterFile = Application.StartupPath + "\\Machine.txt";
        //public static string MachineParameterFile = "D:\\ZebraAIS\\Machine.txt";
        public static string DataRoot = Directory.GetDirectoryRoot(Application.StartupPath);
        //public static string DataRoot = Directory.GetDirectoryRoot(MachineParameterFile);

        private static string _machineLocation = "HA28";
        public static string MachineLocation
        {
            get
            {
                return _machineLocation;
            }
            set
            {
                _machineLocation = value;
            }
        }

        private static string _parameterDir = DataRoot + "ZebraAIS_Data_" + MachineLocation + "\\";
        
        //1_HA28*******************************************
        public static string TemplateRoot = _parameterDir + "Template\\";
        private static string _templateName = "Zebra";
        //2 E:\ZebraAIS_Data_HA28\Template\\Zebra\\*******************************
        public static string CurrentTemplateDir = TemplateRoot + _templateName + "\\";
        public static string TemplateName
        {
            get
            {
                return _templateName;
            }
            set
            {
                //if (frmMain.AllComponent.txtxbModelName.Text != value)
                //{
                //    frmMain.AllComponent.txtxbModelName.Text = value;
                //}
                if (_templateName == value)
                {
                    return;
                }
                _templateName = value;

                CurrentTemplateDir = TemplateRoot + _templateName + "\\";
                //*拿掉模板更新记录 *//  INI_Io.WritePrivateProfileString("Global", "Product", gVar.TemplateName, gVar.GlobalParameterFile);
            }
        }


        public static UInt32 TotalCnt = 0;
        public static UInt32 OkCnt = 0;
        public static UInt32 NgCnt = 0;

        public static bool isInitial = true;
        public static bool isUpdateTemplate = false;

        /// <summary>
        /// Setting Page dgvSnapParameter
        /// </summary>
        //public const short DGV_CAM_IDX = 0;
        public const short DGV_CAM_COMMENT = 0;
        public const short DGV_CAM_EXPOSURE = 1;
        public const short DGV_CIRCLE_LIGHT = 2;
        public const short DGV_UP_BAR_LIGHT = 3;
        public const short DGV_DOWN_BAR_LIGHT0 = 4;
        public const short DGV_DOWN_BAR_LIGHT1 = 5;
        public const short DGV_DOWN_CIRCLE_LIGHT = 6;
        public const short DGV_PLC_SEND_ADDR = 7;
        public const short DGV_PLC_FEEDBACK = 8;
        /// <summary>
        /// Setting Page dgvMatch
        /// </summary>
        public const short DGV_MATCH_IDX = 0;
        public const short DGV_SET_SEARCH_ROI = 1;
        public const short DGV_MATCH_TRAIN = 2;
        public const short DGV_MATCH_ANGLE_START = 3;
        public const short DGV_MATCH_ANGLE_END = 4;
        public const short DGV_MATCH_MINI_SCORE = 5;
        public const short DGV_MATCH_CNT = 6;
        public const short DGV_DISP_SEARCH_ROI = 7;
        public const short DGV_MATCH_EXE = 8;
        public const short DGV_MATCH_RESULT_ROW = 9;
        public const short DGV_MATCH_RESULT_COLUMN = 10;
        public const short DGV_MATCH_RESULT_ANGLE = 11;
        public const short DGV_MATCH_RESULT_SCORE = 12;

        /// <summary>
        /// Setting Page dgvInspectModel
        /// </summary>
        public const short DGV_INSPECT_MATCH_ENABLE = 0;
        public const short DGV_INSPECT_MATCH_CAM_IDX = 1;
        public const short DGV_INSPECT_MATCH_MAP_IDX = 2;
        public const short DGV_INSPECT_POINT_NAME = 3;
        public const short DGV_INSPECT_COMMENT = 4;
        public const short DGV_INSPECT_SET_ROI = 5;
        public const short DGV_INSPECT_METHOD_NAME = 6;
        public const short DGV_INSPECT_EXE = 7;

        /// <summary>
        /// Setting Page dgvInspectParameter
        /// </summary>
        public const short DGV_METHOD_COMMENT = 0;
        public const short DGV_METHOD_PARA_NAME = 1;
        public const short DGV_METHOD_SET_PARA = 2;
        public const short DGV_METHOD_PARAMETER = 3;

        /// <summary>
        /// PLC Register define
        /// </summary>
        public const short PLC_SCANNER = 0;
        public const short PLC_STATION0_SNAP0 = 1;
        public const short PLC_STATION0_SNAP1 = 2;
        public const short PLC_STATION1_SNAP0 = 3;
        public const short PLC_RESULT_OK = 4; //127, 271
        public const short PLC_RESULT_NG = 5; //104, 204
        public const short PLC_BARLIGHT = 6;
        public const short PLC_CIRCLE_BARLIGHT = 7; //107 use
        public const short PLC_RESET = 7; //7000 use
        public const short PLC_RESULT_1 = 8;
        public const short PLC_RESULT_2 = 9;

        public const int PLC_STATE_OK = 50;
        public const int PLC_TRIGGER = 100;
        public const int PLC_TRIGGER_FINISH = 101;
        /// <summary>
        /// PC->PLC
        /// </summary>
        public const short SCANNER_TRIGGER_REG = 100;
        public const short STATION0_SNAP0_REG = 101;
        public const short STATION0_SNAP1_REG = 102;
        public const short STATION1_SNAP0_REG = 103;
        public const short INSPECT_RESULT_OK_REG = 127;
        public const short INSPECT_RESULT_NG_REG = 104;
        public const short UP_BARLIGHT_REG = 120;
        public const short DOWN_CIRCLE_LIGHT_REG = 107;
        public const short RESULT_1_REG = 108;
        public const short RESULT_2_REG = 109;

        /// <summary>
        /// PLC->PC
        /// </summary>
        public const short SCANNER_TRIGGER_FB = 200;
        public const short STATION0_SNAP0_FB = 201;
        public const short STATION0_SNAP1_FB = 202;
        public const short STATION1_SNAP0_FB = 203;
        public const short INSPECT_RESULT_OK_FB = 271;
        public const short INSPECT_RESULT_NG_FB = 204;
        public const short UP_BARLIGHT_FB = 206;
        public const short PLC_RESET_REG = 7000;
        public const short RESULT_1_FB = 208;
        public const short RESULT_2_FB = 209;

        public const short DGV_SYS_LOG_TIME = 0;
        public const short DGV_SYS_LOG_MSG = 1;

        public static uint PrivilegeTimeOut = 300000;

        public static uint PlcTimeOutCnt = 3;

        public static string ScannerSN = string.Empty;
        public static bool isLogSaveOrgImg = false;
        public static bool isLogSaveParameter = false;
        public enum myTimeType
        {
            yyyyMMdd,              //年月日       20180703
            HHmmss,                //时分秒       134509
            HHmmss_ms,             //时分秒毫秒   134503.889
            yyyyMMdd_HHmmss,       //年月日_时分秒  20180703_153204
            yyyyMMdd_HHmmss_ms,   //年月日_时分秒.毫秒  20180703_153204。007
            HH_mm_ss,
        }
        /// <summary>
        /// 获取时间日期yyyyMMdd
        /// </summary>
        /// <param name="strFormat">格式："yyyyMMdd"、"HHmmss"、 "HHmmss.ms"、"yyyyMMdd_HHmmss.ms"、“”</param>
        /// <returns></returns>
        public string getDateTime(myTimeType mTimeT)
        {
            string mydate = "";
            //if (mTimeT == myTimeType.yyyyMMdd)
            //{
            //}
            switch (mTimeT)
            {
                case myTimeType.yyyyMMdd:// "yyyyMMdd":
                    mydate = DateTime.Now.ToString("yyyyMMdd");
                    break;
                case myTimeType.HHmmss:// "HHmmss":
                    mydate = DateTime.Now.ToString("HHmmss");
                    break;
                case myTimeType.HHmmss_ms:// "HHmmss.ms":
                    mydate = DateTime.Now.ToString("HHmmss") + "." + DateTime.Now.Millisecond.ToString("000");
                    break;
                case myTimeType.yyyyMMdd_HHmmss_ms: // "yyyyMMdd_HHmmss.ms":
                    mydate = DateTime.Now.ToString("yyyyMMdd") + "_" + DateTime.Now.ToString("HHmmss");
                    break;
                default:
                    mydate = DateTime.Now.ToString("HH:mm:ss");
                    break;
            }
            return mydate;

        }

        //public static WebService.WebService webService = new WebService.WebService();
        public static string GetModelName(string SN)
        {
            string strModel = string.Empty;
            DataSet data;

            try
            {
                //data = webService.GetDynamicData("GETMODELFA", "USN", SN);
                //if (data.Tables.Count > 0)
                //{
                //    if (data.Tables[0].Rows.Count > 0)
                //    {
                //        strModel = data.Tables[0].Rows[0][0].ToString();
                //    }
                //}
            }
            catch (Exception ex)
            {

            }
            return strModel;
        }

        /// <summary>
        /// 上抛
        /// </summary>
        /// <param name="UnitSerialNumber">USN</param>
        /// <param name="Line">线别Line</param>
        /// <param name="StageCode">站别StageCode:AM</param>
        /// <param name="StationName">StationName：AIS</param>
        /// <param name="EmployeeID">EmployeeID</param>
        /// <param name="Pass">Pass</param>
        /// <param name="TrnDatas">TrnDatas</param>
        /// <returns></returns>
        public static string Complete(string UnitSerialNumber, string Line, string StageCode, string StationName, string EmployeeID, bool Pass, string[] TrnDatas)
        {
            string data = string.Empty;
            try
            {
                //string UnitSerialNumber, string Line, string StageCode, string StationName, string EmployeeID, bool Pass, [System.Xml.Serialization.XmlArrayItemAttribute("TrnData")] string[] TrnDatas
                //data = webService.Complete(UnitSerialNumber, Line, StageCode, StationName, EmployeeID, Pass, TrnDatas);
            }
            catch (Exception ex)
            {

            }
            return data;
        }

        /// <summary>
        /// 检查站别
        /// </summary>
        /// <param name="UnitSerialNumber"></param>
        /// <param name="StageCode"></param>
        /// <returns></returns>
        public static string CheckRoute(string UnitSerialNumber, string StageCode)
        {
            string data = string.Empty;
            try
            {
                //string UnitSerialNumber, string StageCode
                //data = webService.CheckRoute(UnitSerialNumber, StageCode);
            }
            catch (Exception ex)
            {

            }
            return data;
        }
    }
}
