using WES.Commons;
using WES.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WES.ViewModels
{
    public class ManualVM : NotifyBase
    {
        private string oldCode;
        public string OldCode
        {
            get => oldCode;
            set
            {
                oldCode = value;
                DoNotify();
            }
        }

        private string newCode;
        public string NewCode
        {
            get => newCode;
            set
            {
                newCode = value;
                DoNotify();
            }
        }

        private string rpStatus;
        public string RPStatus
        {
            get => rpStatus;
            set
            {
                rpStatus = value;
                DoNotify();
            }
        }

        #region 命令
        private CommandBase userCommand;
        public CommandBase UserCommand
        {
            get
            {
                if (userCommand == null)
                {
                    userCommand = new CommandBase()
                    {
                        DoExcute = ManualOperation,
                        DoCanExecute = obj => { return true; }
                    };
                }
                return userCommand;
            }
        }

        private async void ManualOperation(object obj)
        {
            switch (obj.ToString())
            {
                case "trigger":
                    if (GlobalParams.scannerClient == null)
                    {
                        MessageBox.Show("扫码枪未连接");
                        return;
                    }
                    TriggerScanner();
                    break;
                case "writeTrigger":
                    if (GlobalParams.rotatingPlatformClient == null)
                    {
                        RPStatus = "旋转平台未连接";
                        return;
                    }
                    GlobalParams.rotatingPlatformClient?.WriteRegisters(0, new ushort[] { 10 });
                    MessageBox.Show("OK");
                    break;
                case "small":
                    if (GlobalParams.rotatingPlatformClient == null)
                    {
                        RPStatus = "旋转平台未连接";
                        return;
                    }
                    GlobalParams.rotatingPlatformClient?.WriteRegisters(1, new ushort[] { 40 });
                    MessageBox.Show("OK");
                    break;
                case "big":
                    if (GlobalParams.rotatingPlatformClient == null)
                    {
                        RPStatus = "旋转平台未连接";
                        return;
                    }
                    GlobalParams.rotatingPlatformClient?.WriteRegisters(1, new ushort[] { 30 });
                    MessageBox.Show("OK");
                    break;
                case "query":
                    if (GlobalParams.rotatingPlatformClient == null)
                    {
                        RPStatus = "旋转平台未连接";
                        return;
                    }
                    ushort[] res = GlobalParams.rotatingPlatformClient?.ReadHoldingRegisters(1, 1);
                    if (res != null)
                    {
                        switch (res.FirstOrDefault())
                        {
                            case 20:
                                RPStatus = $"扫码完成:{res.FirstOrDefault()}";
                                break;
                            case 30:
                                RPStatus = $"大机台流出:{res.FirstOrDefault()}";
                                break;
                            case 40:
                                RPStatus = $"小机台流出:{res.FirstOrDefault()}";
                                break;
                            default:
                                RPStatus = $"未知状态:{res.FirstOrDefault()}";
                                break;
                        }
                    }
                    else
                    {
                        RPStatus = "未获取到旋转平台状态值,请检查连接";
                    }
                    break;
                case "scannerOK":
                    if (GlobalParams.rotatingPlatformClient == null)
                    {
                        RPStatus = "旋转平台未连接";
                        return;
                    }
                    GlobalParams.rotatingPlatformClient?.WriteRegisters(1, new ushort[] { 20 });
                    MessageBox.Show("OK");
                    break;
                case "convert":
                    string NewCodeTemp = await GlobalParams.BarcodeConvertAsync(OldCode);
                    if (NewCodeTemp.StartsWith("1S") || NewCodeTemp.StartsWith("PW"))
                    {
                        NewCode = NewCodeTemp;
                        MessageBox.Show("OK");
                    }
                    else
                    {
                        NewCode = "转换失败";
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion

        public ManualVM()
        {
            InitManualCommunicate();
        }

        private void InitManualCommunicate()
        {
            if (GlobalParams.scannerClient == null)
            {
                return;
            }
            GlobalParams.scannerClient.OnDataReceived += ReciveBarCode;
        }

        private void ReciveBarCode(string message, Socket socket)
        {
            message = message.Replace("\r", "").Replace("\n", "");
            LogHelper.Info($"手动触发扫码,接收到{socket.RemoteEndPoint}信息:{message}");
        }

        private void TriggerScanner()
        {
            GlobalParams.manualTrigger = true;
            GlobalParams.scannerClient?.SendData(GlobalParams.config.ScannerTriggerStr, GlobalParams.scannerClient?.Socket);
        }
    }
}
