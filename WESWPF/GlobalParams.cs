using WES.Commons;
using WES.Helpers;
using WES.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WES
{
    public class GlobalParams
    {
        public static bool manualTrigger;
        public static FrameworkElement Home = null;//主页
        public static FrameworkElement Manual = null;//手动
        public static FrameworkElement Settings = null;//设置
        public static FrameworkElement RackManagement = null;//架位信息
        public static ConfigModel config = null;//配置
        public static SocketHelper scannerClient = null;
        public static ModbusHelper rotatingPlatformClient = null;
        public static SFCS_Server.WebServiceSoapClient client = new SFCS_Server.WebServiceSoapClient();
        public static UploadData.WebServiceSoapClient clientUpload = new UploadData.WebServiceSoapClient();


        /// <summary>
        /// 条码转换 1S-->PW
        /// </summary>
        /// <param name="usn"></param>
        /// <returns></returns>
        public static async Task<string> BarcodeConvertAsync(string usn)
        {
            string resultUSN = "";
            try
            {
                // 构造参数
                SFCS_Server.clsDynamicParameter clsDynamicParameter = new SFCS_Server.clsDynamicParameter()
                {
                    strParam = "pstr_sn",
                    strValue = usn
                };
                SFCS_Server.clsDynamicParameter[] clsDynamicParameters = new SFCS_Server.clsDynamicParameter[] { clsDynamicParameter };
                // 异步调用 `DynamicDBFunctionAsync` 方法
                SFCS_Server.DynamicDBFunctionResponse response = await client.DynamicDBFunctionAsync("F_getusn_seal", "AO", clsDynamicParameters);
                resultUSN = response.DynamicDBFunctionResult; // 读取异步返回结果
            }
            catch (Exception e)
            {
                LogHelper.Debug($"条码{usn}转换异常:{e.Message}");
            }

            return resultUSN;
        }

        /// <summary>
        /// 上传数量
        /// </summary>
        /// <param name="number"></param>
        /// <param name="line"></param>
        public static async void UpLoadDataToSFCS(string number, string line)
        {
            try
            {
                // 构造参数
                UploadData.clsDynamicParameter clsDynamicParameter1 = new UploadData.clsDynamicParameter()
                {
                    strParam = "F_RESULT",//数量
                    strValue = number,
                };
                UploadData.clsDynamicParameter clsDynamicParameter2 = new UploadData.clsDynamicParameter()
                {
                    strParam = "F_TYPE",//线别
                    strValue = line,
                };
                UploadData.clsDynamicParameter[] clsDynamicParameters = new UploadData.clsDynamicParameter[] { clsDynamicParameter1, clsDynamicParameter2 };
                // 异步调用DynamicDBFunctionAsync方法
                UploadData.DynamicDBFunctionResponse response = await clientUpload.DynamicDBFunctionAsync("func_updatefalcskbdetail", "AO", clsDynamicParameters);
                string result = response.DynamicDBFunctionResult; // 读取异步返回结果
            }
            catch (Exception e)
            {
                LogHelper.Debug($"上传生产数据失败:{e.Message}");
            }
        }
    }

}
