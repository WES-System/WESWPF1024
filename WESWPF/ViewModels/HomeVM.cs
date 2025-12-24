using WES.Commons;
using WES.Helpers;
using WES.Models;
using log4net.Config;
using log4net.Layout;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Wpf.Ui.Controls;

namespace WES.ViewModels
{
    public class HomeVM : NotifyBase
    {
        string curruntTime = DateTime.Now.ToString("yyyyMMdd");//当前时间

        private string basePath = "YieldRecords";//生产记录文件夹

        private int scanningCount = 1;//扫码次数

        private bool isFull = false;//是否爆仓

        private bool isCancel = false;//是否取消任务

        private bool isRuning = true;//运行状态

        private bool sbReady = false;//手臂到位

        private bool isWXEnable = true;
        /// <summary>
        /// 是否启用外箱
        /// </summary>
        public bool IsWXEnable
        {
            get => isWXEnable;
            set
            {
                isWXEnable = value;
                SaveWXConfig();
                DoNotify();
            }
        }

        private bool isByPass;
        /// <summary>
        /// 是否ByPass
        /// </summary>
        public bool IsByPass
        {
            get => isByPass;
            set
            {
                isByPass = value;
                SaveWXConfig();
                DoNotify();
            }
        }

        private string mType;
        /// <summary>
        /// 机台类型
        /// </summary>
        public string MType
        {
            get => mType;
            set
            {
                mType = value;
                DoNotify();
            }
        }

        private string curruntUSN = "";//"PW0KK0VYPWN0B5806010";
        /// <summary>
        /// 当前条码
        /// </summary>
        public string CurruntUSN
        {
            get => curruntUSN;
            set
            {
                curruntUSN = value;
                DoNotify();
            }
        }

        private string curruntRockCellMSG = "";//"PW0K4CLVPWN0B571300A,0,0,0,1,01";
        /// <summary>
        /// 当前机台架位信息
        /// </summary>
        public string CurruntRockCellMSG
        {
            get => curruntRockCellMSG;
            set
            {
                curruntRockCellMSG = value;
                DoNotify();
            }
        }

        private string previousRockCellMSG = "";
        /// <summary>
        /// 已被手臂抓取的机台架位信息
        /// </summary>
        public string PreviousRockCellMSG
        {
            get => previousRockCellMSG;
            set
            {
                previousRockCellMSG = value;
                DoNotify();
            }
        }

        private int count = 0;
        /// <summary>
        /// 生产总数
        /// </summary>
        public int Count
        {
            get => count;
            set
            {
                count = value;
                DoNotify();
            }
        }

        private int ngCount = 0;
        /// <summary>
        /// 生产NG总数
        /// </summary>
        public int NGCount
        {
            get => ngCount;
            set
            {
                ngCount = value;
                DoNotify();
            }
        }

        private int okCount = 0;
        /// <summary>
        /// 生产OK总数
        /// </summary>
        public int OKCount
        {
            get => okCount;
            set
            {
                okCount = value;
                DoNotify();
            }
        }

        private Task _monitoringTask;
        private CancellationTokenSource _monitoringCts;
        private readonly object _taskLock = new object();

        private Socket sbClient;//手臂客户端

        private SocketHelper fsServer;//森林Socket服务

        private SocketHelper robotServer;//手臂Socket服务

        private SocketHelper wxCheckServer;//外箱Socket服务

        private SocketHelper scannerClient;//扫码枪Socket客户端

        private ModbusTcpClient cylinderClient;//气缸Modbus客户端

        private ModbusTcpClient modbusClient;//旋转平台Modbus客户端

        private ConfigModel config = GlobalParams.config;//系统参数配置

        private DispatcherTimer timer;//计时器

        private readonly Dispatcher dispatcher = Dispatcher.CurrentDispatcher;//当前线程

        public ObservableCollection<string> Logs { get; } = new ObservableCollection<string>();//日志

        public ObservableCollection<MachineMSGModel> UploadFailMSG { get; } = new ObservableCollection<MachineMSGModel>();//上传失败

        //渐变色刷
        private static GradientStopCollection gradientStops1 = new GradientStopCollection()
        {
            new GradientStop(Colors.White, 0),
            new GradientStop(Colors.Red, 1)
        };
        private static GradientStopCollection gradientStops2 = new GradientStopCollection()
        {
            new GradientStop(Colors.White, 0),
            new GradientStop(Colors.Green, 1)
        };

        private static readonly RadialGradientBrush RedLight = new RadialGradientBrush { GradientOrigin = new Point(0.5, 0.5), GradientStops = gradientStops1 };//红灯
        private static readonly RadialGradientBrush GreenLight = new RadialGradientBrush { GradientOrigin = new Point(0.5, 0.5), GradientStops = gradientStops2 };//绿灯

        private Brush scannerStatus = RedLight;
        /// <summary>
        /// 扫码枪状态
        /// </summary>
        public Brush ScannerStatus
        {
            get => scannerStatus;
            set
            {
                scannerStatus = value;
                DoNotify();
            }
        }

        private Brush robotStatus = RedLight;
        /// <summary>
        /// 手臂状态
        /// </summary>
        public Brush RobotStatus
        {
            get => robotStatus;
            set
            {
                robotStatus = value;
                DoNotify();
            }
        }

        private Brush wxStatus = RedLight;
        /// <summary>
        /// 外箱状态
        /// </summary>
        public Brush WXStatus
        {
            get => wxStatus;
            set
            {
                wxStatus = value;
                DoNotify();
            }
        }

        private Brush rotatingPlatformStatus = RedLight;
        /// <summary>
        /// 旋转平台状态
        /// </summary>
        public Brush RotatingPlatformStatus
        {
            get => rotatingPlatformStatus;
            set
            {
                rotatingPlatformStatus = value;
                DoNotify();
            }
        }

        private MachineMSGModel selectModel;
        /// <summary>
        /// 当前选择的上抛失败机台信息
        /// </summary>
        public MachineMSGModel SelectModel
        {
            get => selectModel;
            set
            {
                selectModel = value;
                DoNotify();
            }
        }

        #region 命令
        private CommandBase processFailCommand;
        /// <summary>
        /// 上抛失败
        /// </summary>
        public CommandBase ProcessFailCommand
        {
            get
            {
                if (processFailCommand == null)
                {
                    processFailCommand = new CommandBase()
                    {
                        DoExcute = ProcessFail,
                        DoCanExecute = obj => { return true; }
                    };
                }
                return processFailCommand;
            }
        }
        private async void ProcessFail(object obj)
        {
            switch (obj.ToString())
            {
                case "delete":
                    if (SelectModel != null)
                    {
                        LogHelper.Info($"手动删除机台到位信息:{SelectModel.RawData}");
                        await SqliteSQLHelper.Instance.DeleteWithResultAsync(SelectModel);
                        dispatcher.Invoke(() =>
                        {
                            UploadFailMSG.Remove(SelectModel);
                        });
                    }
                    break;
                case "upload":
                    if (SelectModel != null)
                    {
                        CompleteUploadRequest b = new CompleteUploadRequest
                        {
                            usn = SelectModel.USN,
                            PackMachineNo = config.PackMachineNo//码垛不需要配置 发空
                        };
                        b.Point = "";
                        string jsonStr = JsonSerializer.Serialize(b);
                        string responseStr = HttpHelper.RequestData(config.CompleteUpload_URL, "Post", jsonStr);//上抛给森林系统
                        CompleteUploadResponse res = JsonSerializer.Deserialize<CompleteUploadResponse>(responseStr);
                        if (res.Result == "SUCCESS")
                        {
                            LogHelper.Info($"手动向森林系统上抛数据{jsonStr}成功,森林系统响应{responseStr}");
                            dispatcher.Invoke(() =>
                            {
                                UploadFailMSG.Remove(SelectModel);
                            });
                            await SqliteSQLHelper.Instance.DeleteWithResultAsync(SelectModel);
                        }
                        else
                        {
                            LogHelper.Info($"手动向森林系统发送数据{jsonStr}失败,森林系统响应{responseStr}");
                        }
                    }
                    break;
                case "deleteAll":
                    if (UploadFailMSG.Count > 0)
                    {
                        if (System.Windows.MessageBox.Show("确认删除所有机台信息?", "提示", System.Windows.MessageBoxButton.YesNo, MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                        {
                            LogHelper.Info("正在删除所有机台信息,请稍等...");
                            List<MachineMSGModel> tempList = UploadFailMSG.ToList();
                            foreach (MachineMSGModel item in tempList)
                            {
                                LogHelper.Info($"手动删除机台到位信息:{item.RawData}");
                                await SqliteSQLHelper.Instance.DeleteWithResultAsync(item);
                                dispatcher.Invoke(() =>
                                {
                                    UploadFailMSG.Remove(item);
                                });
                            }
                        }
                    }
                    break;
                case "uploadAll":
                    if (UploadFailMSG.Count > 0)
                    {
                        if (System.Windows.MessageBox.Show("确认上抛所有机台信息?", "提示", System.Windows.MessageBoxButton.YesNo, MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                        {
                            LogHelper.Info("正在上抛所有机台信息,请稍等...");
                            List<MachineMSGModel> tempList = UploadFailMSG.ToList();
                            foreach (MachineMSGModel item in tempList)
                            {
                                CompleteUploadRequest b = new CompleteUploadRequest
                                {
                                    usn = SelectModel.USN,
                                    PackMachineNo = config.PackMachineNo//码垛不需要配置 发空
                                };
                                b.Point = "";
                                string jsonStr = JsonSerializer.Serialize(b);
                                string responseStr = HttpHelper.RequestData(config.CompleteUpload_URL, "Post", jsonStr);//上抛给森林系统
                                CompleteUploadResponse res = JsonSerializer.Deserialize<CompleteUploadResponse>(responseStr);
                                if (res.Result == "SUCCESS")
                                {
                                    LogHelper.Info($"手动向森林系统上抛数据{jsonStr}成功,森林系统响应{responseStr}");
                                    dispatcher.Invoke(() =>
                                    {
                                        UploadFailMSG.Remove(SelectModel);
                                    });
                                    await SqliteSQLHelper.Instance.DeleteWithResultAsync(SelectModel);
                                }
                                else
                                {
                                    LogHelper.Info($"手动向森林系统发送数据{jsonStr}失败,森林系统响应{responseStr}");
                                }
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private CommandBase functionCommand;
        /// <summary>
        /// 功能
        /// </summary>
        public CommandBase FunctionCommand
        {
            get
            {
                if (functionCommand == null)
                {
                    functionCommand = new CommandBase()
                    {
                        DoExcute = FunctionMethod,
                        DoCanExecute = obj => { return true; }
                    };
                }
                return functionCommand;
            }
        }

        private void FunctionMethod(object obj)
        {
            if (obj == null)
            {
                return;
            }
            switch (obj.ToString())
            {
                case "reset":
                    AppReset();
                    break;
                case "photoEx"://重置扫码参数
                    PhotoEX();
                    break;
                case "scannerRelink"://扫码枪重连
                    ScannerRelink();
                    break;
                case "wxrestart"://重启外箱服务
                    WXServerRestart();
                    break;
                case "robotrestart"://重启手臂服务
                    RobotServerRestart();
                    break;
                case "rprelink"://旋转平台重连
                    RPRestart();
                    break;
                default:
                    break;
            }
        }

        #endregion


        public HomeVM()
        {
            InitLog();
            InitCommunication();
            LoadDB();
            ReadYieldFromFile();
            LoadWXConfig();
            //InitTimer();
        }

        private async void test()
        {
            await SqliteSQLHelper.Instance.InsertAsync(new MachineMSGModel() { RawData = "1111", USN = "USN11111111", MType = "入库", RackCellMsg = "架位信息" });
        }

        #region 初始化日志
        /// <summary>
        /// 日志初始化
        /// </summary>
        private void InitLog()
        {
            //XmlConfigurator.Configure();
            //加载日志配置文件，只需要在程序启动时加载一次
            //string filePath = AppDomain.CurrentDomain.BaseDirectory + "Log4net.config";
            //LogHelper.SetConfig(filePath);
            string logPattern = "%-5p %d{HH:mm:ss} %m";
            LogAppender logAppender = new LogAppender() { Layout = new PatternLayout(logPattern) };
            logAppender.Name = "LogAppenderUI";
            logAppender.LogAppendEvent += ShowLog;
            //IAppender[] appenders = new IAppender[]
            //{
            //    logAppender
            //};
            BasicConfigurator.Configure(logAppender);
        }

        /// <summary>
        /// 输出日志
        /// </summary>
        /// <param name="o"></param>
        private void ShowLog(object log)
        {
            try
            {
                log = log.ToString().Replace("\r", "").Replace("\n", "");
                dispatcher.Invoke(new Action(() =>
                {
                    if (Logs.Count >= 100)
                    {
                        Logs.RemoveAt(Logs.Count - 1);
                    }
                    Logs.Insert(0, log.ToString());
                }));
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("输出日志异常:" + e.Message);
            }
        }
        #endregion

        #region 初始化通信
        /// <summary>
        /// 初始化通信
        /// </summary>
        private void InitCommunication()
        {
            //InitRobotCommunication();//手臂连接
            //InitScannerCommunication();//扫码枪连接
            //InitCylinderCommunication();//气缸连接
            if (config.Scene == "0")
            {
                //InitWXCMCommunication();//外箱连接

            }
            InitRobotModbusTcpCommunication();//手臂ModbusTcp连接
        }

        /// <summary>
        /// 初始化手臂Socket通信 server
        /// </summary>
        private void InitRobotCommunication(ConfigModel newConfig = null)
        {
            try
            {
                if (newConfig != null)
                {
                    config = newConfig;
                }
                robotServer = new SocketHelper(config.RobotIP, int.Parse(config.RobotPort), MySocketType.Server);
                robotServer.OnDataReceived += RobotRecive;
                robotServer.OnClientConnected += RobotConnect;
                robotServer.OnConnectionLost += RobotDisconnect;
                robotServer.Start();
            }
            catch (Exception e)
            {
                LogHelper.Debug($"初始化手臂通信异常:{e.Message}");
            }
        }

        /// <summary>
        /// 接收手臂数据
        /// </summary>
        /// <param name="message"></param>
        /// <param name="socket"></param>
        private async void RobotRecive(string message, Socket socket)
        {
            try
            {
                string sbMessage = message.Replace("\r", "").Replace("\n", "").ToUpper();
                LogHelper.Info($"上位机接收到手臂{socket.RemoteEndPoint}消息:{sbMessage}");
                //码垛手臂到位信号START
                if (sbMessage.Contains(config.RobotTriggerSrt.ToUpper()))
                {
                    if (!isRuning)
                    {
                        LogHelper.Info("上一个机台入库异常,请处理后复位程序");
                        return;
                    }
                    sbReady = true;
                    if (CurruntRockCellMSG != "" && robotServer.ClientSockets.Count != 0)
                    {
                        sbReady = false;
                        robotServer?.SendData(CurruntRockCellMSG + "\r\n", sbClient);
                        LogHelper.Info($"收到手臂取料信号,向手臂发送机台架位信息{CurruntRockCellMSG}");
                    }
                    else
                    {
                        LogHelper.Info("手臂已到位,等待上位机获取架位信息");
                    }
                }
                //码垛手臂夹取机台完成
                else if (sbMessage.Contains("GET"))
                {
                    LogHelper.Info("手臂抓取机台OK");
                    PreviousRockCellMSG = CurruntRockCellMSG;
                    LogHelper.Info($"当前机台架位信息留底:{PreviousRockCellMSG}");
                    LogHelper.Info($"清除当前机台架位信息:{CurruntRockCellMSG}");
                    CurruntRockCellMSG = "";
                }
                //码垛手臂复位
                else if (sbMessage.Contains("RESET"))
                {
                    if (CurruntRockCellMSG != "")//代表旋转平台到位
                    {
                        LogHelper.Info($"当前存在机台架位信息{CurruntRockCellMSG},手臂可以继续抓取");
                    }
                    LogHelper.Info($"手臂复位,清除上一个机台信息:{PreviousRockCellMSG}");
                    PreviousRockCellMSG = "";
                }
                //手臂作业完成 返回usn和结果 usn,ok 手动特殊 多一个点位信息usn,ok,A (A or B)
                else if (sbMessage.Contains("OK"))
                {
                    bool compareResults = false;//条码比对
                    if (config.Scene == "0")
                    {
                        if (PreviousRockCellMSG.Contains(sbMessage.Split(',')[0].ToUpper()))
                        {
                            LogHelper.Info($"手臂返回机台到位信息{sbMessage}与{PreviousRockCellMSG}对比OK");
                            LogHelper.Info($"清除上一个机台架位信息{PreviousRockCellMSG}");
                            PreviousRockCellMSG = "";
                            robotServer?.SendData("Finish" + "\r\n", sbClient);
                            LogHelper.Info("收到手臂完成信号,回复手臂Finish");
                            compareResults = true;
                        }
                        else
                        {
                            LogHelper.Info($"手臂返回机台到位信息{sbMessage}与{PreviousRockCellMSG}对比失败,请检查机台是否错位");
                            isRuning = false;
                            System.Windows.MessageBox.Show($"第{PreviousRockCellMSG.Split(',')[4]}列{PreviousRockCellMSG.Split(',')[5]}号位机台错位!", "警告", System.Windows.MessageBoxButton.OK, MessageBoxImage.Warning);
                            PreviousRockCellMSG = "";
                            //Warning(0,0,"机台错位");
                            return;
                        }
                    }
                    if (compareResults)
                    {
                        CompleteUploadRequest b = new CompleteUploadRequest
                        {
                            usn = sbMessage.Split(',')[0],
                            PackMachineNo = config.PackMachineNo//码垛不需要配置 发空
                        };
                        b.Point = "";
                        if (config.Scene == "3")//手动出库场景
                        {
                            b.Point = sbMessage.Split(',')[2];//手动出库多一个点位 A or B
                        }
                        string jsonStr = JsonSerializer.Serialize(b);
                        string responseStr = HttpHelper.RequestData(config.CompleteUpload_URL, "Post", jsonStr);//上抛给森林系统
                        CompleteUploadResponse res = JsonSerializer.Deserialize<CompleteUploadResponse>(responseStr);
                        if (res.Result == "SUCCESS")
                        {
                            LogHelper.Info($"手臂作业完成,向森林系统发送数据{jsonStr}成功,森林系统响应{responseStr}");
                            await SqliteSQLHelper.Instance.DeleteWithResultAsync<MachineMSGModel>(c => c.RackCellMsg.Contains(sbMessage.Split(',')[0]));
                        }
                        else
                        {
                            LogHelper.Info($"手臂作业完成,向森林系统发送数据{jsonStr}失败,森林系统响应{responseStr}");
                            string responseStr1 = HttpHelper.RequestData(config.CompleteUpload_URL, "Post", jsonStr);//再次上抛给森林系统
                            CompleteUploadResponse res1 = JsonSerializer.Deserialize<CompleteUploadResponse>(responseStr1);
                            if (res1.Result == "SUCCESS")
                            {
                                LogHelper.Info($"再次向森林系统发送数据{jsonStr}成功,森林系统响应{responseStr1}");
                                await SqliteSQLHelper.Instance.DeleteWithResultAsync<MachineMSGModel>(c => c.RackCellMsg.Contains(sbMessage.Split(',')[0]));
                            }
                            else
                            {
                                MachineMSGModel machineMSG = (await SqliteSQLHelper.Instance.SelectWithResultAsync<MachineMSGModel>(c => c.USN == sbMessage.Split(',')[0])).Any1.FirstOrDefault();
                                LogHelper.Info($"手臂作业完成,再次向森林系统发送数据{jsonStr}失败,森林系统响应{responseStr1}");
                                dispatcher.Invoke(() =>
                                {
                                    UploadFailMSG.Add(machineMSG);
                                });
                                LogHelper.Info($"将上传失败记录{machineMSG.USN}添加到界面");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Debug($"接收手臂消息异常:{e.Message}");
            }
        }

        /// <summary>
        /// 手臂连接
        /// </summary>
        /// <param name="socket"></param>
        private void RobotConnect(Socket socket)
        {
            RobotStatus = GreenLight;
            sbClient = socket;
            LogHelper.Info($"手臂客户端{socket.RemoteEndPoint}连接成功");
        }

        /// <summary>
        /// 手臂断开
        /// </summary>
        /// <param name="socket"></param>
        private void RobotDisconnect(Socket socket)
        {
            RobotStatus = RedLight;
            LogHelper.Info($"手臂客户端{socket.RemoteEndPoint}断开连接");
        }

        /// <summary>
        /// 初始化森林系统Socket通信 server
        /// </summary>
        private void InitFSCommunication()
        {
            try
            {
                fsServer = new SocketHelper(config.FSIP, int.Parse(config.FSPort), MySocketType.Server);
                fsServer.OnDataReceived += FSRecive;
                fsServer.OnClientConnected += FSConnect;
                fsServer.OnConnectionLost += FSDisconnect;
                fsServer.Start();
            }
            catch (Exception e)
            {
                LogHelper.Debug($"初始化森林系统通信异常:{e.Message}");
            }
        }

        /// <summary>
        /// 接收森林系统数据
        /// </summary>
        /// <param name="message"></param>
        /// <param name="socket"></param>
        private void FSRecive(string message, Socket socket)
        {
            LogHelper.Info($"上位机接收到森林系统{socket.RemoteEndPoint}消息:{message}");
        }

        /// <summary>
        /// 森林系统连接
        /// </summary>
        /// <param name="socket"></param>
        private void FSConnect(Socket socket)
        {
            LogHelper.Info($"森林系统客户端{socket.RemoteEndPoint}连接成功");
        }

        /// <summary>
        /// 森林系统断开
        /// </summary>
        /// <param name="socket"></param>
        private void FSDisconnect(Socket socket)
        {
            LogHelper.Info($"森林系统{socket.RemoteEndPoint}断开连接");
        }

        /// <summary>
        /// 初始化外箱检测机Socket通信 server
        /// </summary>
        private void InitWXCMCommunication(ConfigModel newConfig = null)
        {
            try
            {
                if (newConfig != null)
                {
                    config = newConfig;
                }
                wxCheckServer = new SocketHelper(config.WXCMIP, int.Parse(config.WXCMPort), MySocketType.Server);
                wxCheckServer.OnDataReceived += WXRecive;
                wxCheckServer.OnClientConnected += WXConnect;
                wxCheckServer.OnConnectionLost += WXDisconnect;
                wxCheckServer.Start();
            }
            catch (Exception e)
            {
                LogHelper.Debug($"初始化外箱检测通信异常:{e.Message}");
            }
        }

        /// <summary>
        /// 接收外箱检测结果
        /// </summary>
        /// <param name="wxResult"></param>
        /// <param name="socket"></param>
        private void WXRecive(string wxResult, Socket socket)
        {
            string msg = wxResult.Replace("\r", "").Replace("\n", "");
            LogHelper.Info($"上位机接收到外箱检机{socket?.RemoteEndPoint}消息:{msg}");
            try
            {
                if (msg.ToUpper().Contains("WX"))
                {
                    string[] wxMsg = msg.Split('-');
                    string wxResultJson = "{\"usn\": \"" + wxMsg[1] + "\",\"wxres\": \"" + msg + "\"}";
                    HttpHelper.RequestData(config.WXUpload_URL, "Post", wxResultJson);
                    LogHelper.Info($"外箱检测机结果{msg}上抛森林系统完成");
                }
            }
            catch (Exception e)
            {
                LogHelper.Debug($"外箱结果上抛异常:{e.Message}");
            }
        }

        /// <summary>
        /// 外箱连接
        /// </summary>
        /// <param name="socket"></param>
        private void WXConnect(Socket socket)
        {
            WXStatus = GreenLight;
            LogHelper.Info($"外箱检测机{socket.RemoteEndPoint}连接成功");
        }

        /// <summary>
        /// 外箱断开
        /// </summary>
        /// <param name="socket"></param>
        private void WXDisconnect(Socket socket)
        {
            WXStatus = RedLight;
            LogHelper.Info($"外箱检测机{socket.RemoteEndPoint}断开连接");
        }

        /// <summary>
        /// 初始化扫码枪Socket通信 client
        /// </summary>
        private void InitScannerCommunication(ConfigModel newConfig = null)
        {
            try
            {
                if (newConfig != null)
                {
                    config = newConfig;
                }
                scannerClient = new SocketHelper(config.BarcodeIP, int.Parse(config.BarcodePort), MySocketType.Client);
                scannerClient.OnDataReceived += ScannerRecive;
                scannerClient.OnClientConnected += ScannerConnect;
                scannerClient.OnConnectionLost += ScannerDisconnect;
                scannerClient.Start();
            }
            catch (Exception e)
            {
                LogHelper.Debug($"初始化扫码枪通信异常:{e.Message}");
            }
        }

        /// <summary>
        /// 接收扫码枪数据
        /// </summary>
        /// <param name="barCode"></param>
        /// <param name="socket"></param>
        private async void ScannerRecive(string barCode, Socket socket)
        {
            try
            {
                barCode = barCode.Replace("\r", "").Replace("\n", "");
                LogHelper.Info($"接收到扫码枪{socket.RemoteEndPoint}消息{barCode}");
                //arm|1S21SDS1HR00PW0LV0HQ|0|0|M00003P610310|03
                if (barCode.Contains("arm"))
                {
                    GetShelfPositionResponse response = JsonSerializer.Deserialize<GetShelfPositionResponse>(barCode);
                    string usn = response.usn.ToUpper();
                    if (response.coordinate == null)//访问森林获取架位失败
                    {
                        LogHelper.Info("森林系统响应解析错误");
                        ReciceReturn($"armng|{usn}|0|8");
                        isFull = false;
                        isCancel = false;
                        return;
                    }
                    if (response.coordinate.Contains("ignore")) //爆仓 架位已满,稍后处理
                    {
                        LogHelper.Info($"当前机台{usn}无架位可分配,开始轮询森林系统直到分配架位");
                        await Task.Delay(1000);
                        //continue;
                    }
                    else if (response.coordinate.Contains("armng")) // 回流机台 数据格式: armng|PW0K4CM3PWN0B571300A|0|0
                    {
                        LogHelper.Info("森林系统反馈NG,机台回流");
                        ReciceReturn(response.coordinate);
                        isFull = false;
                        isCancel = false;
                        return;
                    }
                    else if (response.coordinate.Contains("arm")) // 入库机台 数据格式: arm|PW0K4CLVPWN0B571300A|0|0|M00001P610201|01
                    {
                        string rackCellCode = SplitKey(response.coordinate)[4];
                        var res = await SqliteSQLHelper.Instance.SelectWithResultAsync<RackCellModel>(c => c.RackCellCode == rackCellCode);
                        if (res.Any1.Count == 0)
                        {
                            LogHelper.Info($"架位{rackCellCode}本地不存在,机台回流");
                            ReciceReturn($"armng|{CurruntUSN}|0|0");
                        }
                        else
                        {
                            LogHelper.Info("机台入库");
                            ReciceStore(response.coordinate);
                        }
                        isFull = false;
                        isCancel = false;
                        return;
                    }
                }
                else
                {
                    return;
                }


                CurruntRockCellMSG = "";
                MType = "";
                barCode = barCode.ToUpper();
                if (barCode.Contains("NOREAD") || barCode.Trim() == string.Empty)//未读到条码
                {
                    CurruntUSN = barCode;
                    if (scanningCount < 3)
                    {
                        LogHelper.Info($"未识别到条码,向扫码枪发送{config.ScannerTriggerStr}再次触发扫码");
                        scannerClient.SendData("photo", scannerClient?.Socket);//触发扫码
                        scanningCount++;
                    }
                    else
                    {
                        LogHelper.Info("向旋转平台PLC寄存器地址1写20告知扫码完成");
                        modbusClient.WriteRegisters(1, new ushort[] { 20 });//控制旋转平台旋转90度
                        scanningCount = 1;
                        LogHelper.Info("三次扫码失败,默认机台条码:NOREAD,机台流出");
                        CurruntUSN = "NOREAD";
                        ReciceReturn($"armng|{CurruntUSN}|0|4");
                        return;
                    }
                }
                else
                {
                    LogHelper.Info("向旋转平台PLC寄存器地址1写20告知扫码完成");
                    modbusClient.WriteRegisters(1, new ushort[] { 20 });//控制旋转平台旋转90度
                    string tempusn = "";
                    List<string> usns = barCode.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    string pwusn = usns.FirstOrDefault(c => c.StartsWith("PW"));
                    if (pwusn != null)//pw开头的usn不为空
                    {
                        tempusn = pwusn;
                    }
                    else
                    {
                        string usn1s = usns.FirstOrDefault(c => c.StartsWith("1S"));
                        if (usn1s != null)//1S开头的usn不为空
                        {
                            string newCode = await GlobalParams.BarcodeConvertAsync(usn1s);
                            if (newCode.StartsWith("1S") || newCode.StartsWith("PW"))//转换成功
                            {
                                tempusn = newCode;
                            }
                            else
                            {
                                CurruntUSN = usn1s;
                                ReciceReturn($"armng|{CurruntUSN}|0|5");
                                LogHelper.Info($"条码{CurruntUSN}从SFCS查询SN失败,机台流出");
                                return;
                            }
                        }
                        else
                        {
                            CurruntUSN = "UnknownFormat";
                            ReciceReturn($"armng|{CurruntUSN}|0|6");
                            LogHelper.Info($"条码{CurruntUSN}格式未知,必须以PW、1S开头,机台流出");
                            return;
                        }
                    }
                    if ((CurruntUSN == tempusn && GlobalParams.manualTrigger) || CurruntUSN != tempusn)//手动触发允许重复扫码 其他情况不允许条码重复
                    {
                        GlobalParams.manualTrigger = false;
                        CurruntUSN = tempusn;
                        while (isFull)//爆仓后 下一机台进入修改状态 取消当前机台作业
                        {
                            isCancel = true;
                            await Task.Delay(3000);
                        }
                        if (curruntTime != DateTime.Now.ToString("yyyyMMdd"))
                        {
                            curruntTime = DateTime.Now.ToString("yyyyMMdd");
                            Count = 0;
                        }
                        Count++;
                        isFull = true;
                        RecordYield();
                        // 启动新任务
                        await Task.Run(() => ShelfPointAsync(CurruntUSN));
                    }
                    else
                    {
                        CurruntUSN = barCode;
                        LogHelper.Info($"条码{tempusn}重复扫码,机台流出");
                        ReciceReturn($"armng|{CurruntUSN}|0|7");
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Debug($"接收扫码枪数据异常:{e.Message}");
            }


            #region 爆仓测试
            //barCode = barCode.Replace("\r", "").Replace("\n", "");
            //LogHelper.Info($"接收到扫码枪{socket.RemoteEndPoint}消息{barCode}");
            //while (isFull)//爆仓后 下一机台进入修改状态 取消当前机台作业
            //{
            //    isCancel = true;
            //    await Task.Delay(20);
            //}
            //isFull = true;
            //// 启动新任务
            //await Task.Run(() => ShelfAsyncTest(barCode));
            #endregion
        }

        /// <summary>
        /// 扫码枪连接
        /// </summary>
        /// <param name="socket"></param>
        private void ScannerConnect(Socket socket)
        {
            ScannerStatus = GreenLight;
            GlobalParams.scannerClient = scannerClient;
            LogHelper.Info($"扫码枪{socket?.RemoteEndPoint}连接成功");
        }

        /// <summary>
        /// 扫码枪断开
        /// </summary>
        /// <param name="socket"></param>
        private void ScannerDisconnect(Socket socket)
        {
            ScannerStatus = RedLight;
            GlobalParams.scannerClient = null;
            LogHelper.Info($"扫码枪{socket?.RemoteEndPoint}连接断开,自动重连中,无需操作");
        }

        /// <summary>
        /// 初始化气缸ModbusTCP通信 client
        /// </summary>
        private void InitCylinderCommunication()
        {
            try
            {
                cylinderClient = new ModbusTcpClient(config.CylinderIP, config.CylinderPort)
                {
                    MonitorAddress = ushort.Parse("0"),
                };
                cylinderClient.ConnectEvent += CylinderConnect;
                cylinderClient.DisConnectEvent += CylinderDisConnect;
                cylinderClient.LogEvent += ShowCylinderLog;
            }
            catch (Exception e)
            {
                LogHelper.Debug($"初始化气缸通信异常:{e.Message}");
            }
        }

        /// <summary>
        /// 气缸PLC连接
        /// </summary>
        private void CylinderConnect()
        {
            LogHelper.Info("气缸PLC连接成功");
        }

        /// <summary>
        /// 气缸PLC断开
        /// </summary>
        private void CylinderDisConnect()
        {
            LogHelper.Info("气缸PLC断开连接,正在重连");
        }

        /// <summary>
        /// 气缸PLC日志
        /// </summary>
        /// <param name="msg1"></param>
        /// <param name="msg2"></param>
        private void ShowCylinderLog(string msg1, string msg2)
        {
            LogHelper.Info($"气缸PLC输出日志:{msg1 + msg2}");
        }

        /// <summary>
        /// 初始化旋转平台ModbusTCP通信
        /// </summary>
        private void InitRobotModbusTcpCommunication()
        {
            if (modbusClient != null)
            {
                modbusClient.Dispose();
            }
            try
            {
                modbusClient = new ModbusTcpClient(config.RotatingPlatformIP, config.RotatingPlatformPort)
                {
                    SlaveAddress = 1,
                    MonitorAddress = 0,
                };
                modbusClient.ConnectEvent += RotatingPlatformConnect;
                modbusClient.DisConnectEvent += RotatingPlatformDisConnect;
                modbusClient.LogEvent += ShowRotatingPlatformLog;
                if (modbusClient.ConnectStatus)
                {
                    RotatingPlatformConnect();
                }
                //TriggerProcess();
                // 4. 启动后台监控任务
                StartMonitoringTask();
            }
            catch (Exception e)
            {
                RotatingPlatformStatus = RedLight;
                LogHelper.Debug($"初始化旋转平台通信异常:{e.Message}");
            }
        }


        /// <summary>
        /// 旋转平台PLC连接
        /// </summary>
        private void RotatingPlatformConnect()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                RotatingPlatformStatus = GreenLight;
                GlobalParams.modbusClient = modbusClient;
                LogHelper.Info("旋转平台PLC连接成功");
            });
        }

        /// <summary>
        /// 旋转平台PLC断开
        /// </summary>
        private void RotatingPlatformDisConnect()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                RotatingPlatformStatus = RedLight;
                GlobalParams.modbusClient = null;
                LogHelper.Info("旋转平台PLC断开连接,正在重连");
            });
        }

        /// <summary>
        /// 旋转平台PLC日志
        /// </summary>
        /// <param name="msg1"></param>
        /// <param name="msg2"></param>
        private void ShowRotatingPlatformLog(string msg1, string msg2)
        {
            Application.Current.Dispatcher.Invoke(() =>
            LogHelper.Info("旋转平台PLC输出日志:" + msg1 + msg2));
        }

        private void StartMonitoringTask()
        {
            lock (_taskLock)
            {
                if (_monitoringTask != null && !_monitoringTask.IsCompleted)
                    return; // 已在运行

                _monitoringCts = new CancellationTokenSource();
                _monitoringTask = Task.Run(async () =>
                {
                    while (!_monitoringCts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            // 轮询 PLC 寄存器 0，检查是否到位（值为10）
                            var registers = modbusClient?.ReadHoldingRegisters(0, 1);
                            if (registers != null && registers.Length > 0 && registers[0] == 10)
                            {
                                // 发送确认：写 0 到寄存器 0
                                modbusClient.WriteRegisters(0, new ushort[] { 0 });

                                LogHelper.Info("手臂已到位,向扫码枪发送photo触发扫码");
                            }
                        }
                        catch (InvalidOperationException ex)
                        {
                            // Modbus 未连接，跳过本次轮询
                            LogHelper.Debug($"未连接，跳过本次轮询:{ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug($"触发扫码流程异常:{ex.Message}");
                        }
                        await Task.Delay(1000, _monitoringCts.Token); // 1000ms 轮询间隔
                    }
                }, _monitoringCts.Token);
            }
        }
        #endregion



        #region 私有方法
        /// <summary>
        /// 保存外箱配置
        /// </summary>
        private void SaveWXConfig()
        {
            try
            {
                Dictionary<string, bool> configDic = new Dictionary<string, bool>
                {
                    { "IsWXEnable", IsWXEnable },
                    { "IsByPass", IsByPass }
                };
                string json = JsonSerializer.Serialize(configDic); // 序列化为 JSON
                File.WriteAllText("config.json", json); // 写入文件
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"保存到文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 加载外箱配置
        /// </summary>
        /// <param name="filePath"></param>
        private void LoadWXConfig(string filePath = "config.json")
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    Dictionary<string, bool> configDic = JsonSerializer.Deserialize<Dictionary<string, bool>>(json)
                                  ?? new Dictionary<string, bool>(); // 反序列化 JSON
                    IsWXEnable = configDic["IsWXEnable"];
                    IsByPass = configDic["IsByPass"];
                }
                else
                {
                    IsWXEnable = true;
                    IsByPass = false;
                    LogHelper.Info("配置文件不存在，使用默认值。");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"加载文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理森林系统的正常入库信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<string> SplitKey(string key)//arm|PW0K4D0KPW90B571300A|0|0|M00001P610101|02
        {
            List<string> result = key.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();
            return result;
        }

        /// <summary>
        /// 获取拼接好的列索引,boxcell
        /// </summary>
        /// <param name="rackCellCode">料架单元</param>
        /// <param name="boxCell">料框在该单元中的位置</param>
        /// <returns></returns>
        private async Task<string> GetMessageToRobot(string rackCellCode, int boxCell)//M00001P610201|01
        {
            var rackCells = await SqliteSQLHelper.Instance.SelectWithResultAsync<RackCellModel>(c => c.RackCellCode == rackCellCode);
            if (rackCells.Any1?.Count == 0)
            {
                LogHelper.Info($"未找到RackCell{rackCellCode},请确认数据库数据是否完整");
                return "";
            }
            RackCellModel rackCell = rackCells.Any1.FirstOrDefault();
            int rowIndex = rackCell.RackCellRowIndex; //2
            int columnIndex = rackCell.RackCellColumnIndex; //1
            int newBoxCell = 0;
            switch (rackCell.RackCellType)
            {
                case "正":
                    newBoxCell = GetBoxCell(rowIndex, boxCell, 4);
                    break;
                case "反":
                    boxCell = 1 + (4 - boxCell);
                    newBoxCell = GetBoxCell(rowIndex, boxCell, 4);
                    break;
                case "特":
                    if (boxCell > 2)//boxcell为3 4时 需要将列加一 且重新生成boxcell
                    {
                        boxCell -= 2;
                        columnIndex++;
                    }
                    else//boxcell为1 2时 数据库列索引原样输出
                    {

                    }
                    newBoxCell = GetBoxCell(rowIndex, boxCell, 2);//将同一RackCell分为两列 料框最大容量为2
                    break;
                default:
                    break;
            }
            string columnStr = columnIndex.ToString();
            string boxcellStr = newBoxCell.ToString();
            if (columnIndex < 10)
            {
                columnStr = "0" + columnIndex;
            }
            if (newBoxCell < 10)
            {
                boxcellStr = "0" + newBoxCell;
            }
            string message = columnStr + "," + boxcellStr;//架位列 boxcell
            return message;
        }

        /// <summary>
        /// 获取手臂能够使用的boxcell编号
        /// </summary>
        /// <param name="rowIndex">第几行</param>
        /// <param name="boxCell">料框索引</param>
        /// <param name="maxCellCount">料框最大容量</param>
        /// <returns></returns>
        private int GetBoxCell(int rowIndex, int boxCell, int maxCellCount)
        {
            return boxCell + ((rowIndex - 1) * maxCellCount);
        }

        
        /// <summary>
        /// 触发扫码
        /// </summary>
        private void TriggerProcess()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        if (!isRuning)
                        {
                            LogHelper.Info("当前有异常未处理,请处理后再复位程序");
                            await Task.Delay(1000);
                            continue;
                        }
                        if (!modbusClient?.ConnectStatus == true)
                        {
                            await Task.Delay(1000);
                            continue;
                        }
                        ushort[] plcdata = modbusClient?.ReadHoldingRegisters(0, 1);
                        if (plcdata[0] == 10)
                        {
                            modbusClient.WriteRegisters(0, new ushort[] { 0 });
                            scannerClient?.SendData("photo", scannerClient?.Socket);
                            LogHelper.Info("旋转平台已到位,向扫码枪发送photo触发扫码");
                        }
                    }
                    catch (Exception e)
                    {
                        LogHelper.Debug($"触发扫码流程异常:{e.Message}");
                    }
                    await Task.Delay(200);
                }
            });
        }

        /// <summary>
        /// NG回流方法
        /// </summary>
        /// <param name="data"></param>
        private void ReciceReturn(string data)//回流机台 数据格式 普通NG："armng|usn|0|0" --- OOB："armng|usn|0|1" --- EC未过："armng|usn|0|2" --- 大机台："armng|usn|0|3";
        {
            try
            {
                List<string> strList = SplitKey(data);
                string message = strList[1] + ",0,0,0,0,0";//默认返回值（普通NG）
                switch (strList[3])
                {
                    case "0"://普通NG
                        MType = "普通NG机台";
                        CurruntRockCellMSG = message;
                        LogHelper.Info("向旋转平台PLC寄存器地址1写40,等待手臂抓取");
                        modbusClient?.WriteRegisters(1, new ushort[] { 40 });
                        break;
                    case "1"://OOB
                        MType = "OOB机台";
                        CurruntRockCellMSG = message;
                        LogHelper.Info("向旋转平台PLC寄存器地址1写40,等待手臂抓取");
                        modbusClient?.WriteRegisters(1, new ushort[] { 40 });
                        break;
                    case "2"://EC未过
                        MType = "EC未过机台";
                        CurruntRockCellMSG = message;
                        LogHelper.Info("向旋转平台PLC寄存器地址1写40,等待手臂抓取");
                        modbusClient?.WriteRegisters(1, new ushort[] { 40 });
                        break;
                    case "3"://大机台
                        MType = "大机台";
                        LogHelper.Info("向旋转平台PLC寄存器地址1写30,机台流出");
                        modbusClient?.WriteRegisters(1, new ushort[] { 30 });
                        LogHelper.Info($"{MType}，通知旋转平台流出");
                        break;
                    case "4"://扫码失败
                        MType = "扫码失败";
                        LogHelper.Info("向旋转平台PLC寄存器地址1写30,机台流出");
                        modbusClient?.WriteRegisters(1, new ushort[] { 30 });
                        LogHelper.Info($"{MType}，通知旋转平台流出");
                        break;
                    case "5"://条码转换失败
                        MType = "条码转换失败";
                        LogHelper.Info("向旋转平台PLC寄存器地址1写30,机台流出");
                        modbusClient?.WriteRegisters(1, new ushort[] { 30 });
                        LogHelper.Info($"{MType}，通知旋转平台流出");
                        break;
                    case "6"://条码格式未知
                        MType = "条码格式未知";
                        LogHelper.Info("向旋转平台PLC寄存器地址1写30,机台流出");
                        modbusClient?.WriteRegisters(1, new ushort[] { 30 });
                        LogHelper.Info($"{MType}，通知旋转平台流出");
                        break;
                    case "7"://重复扫码
                        MType = "重复扫码";
                        LogHelper.Info("向旋转平台PLC寄存器地址1写30,机台流出");
                        modbusClient?.WriteRegisters(1, new ushort[] { 30 });
                        LogHelper.Info($"{MType}，通知旋转平台流出");
                        break;
                    case "8":
                        MType = "无法解析架位";
                        LogHelper.Info("向旋转平台PLC寄存器地址1写30,机台流出");
                        modbusClient?.WriteRegisters(1, new ushort[] { 30 });
                        LogHelper.Info($"{MType}，通知旋转平台流出");
                        break;
                    default:
                        MType = "未知类型";
                        LogHelper.Info("向旋转平台PLC寄存器地址1写30,机台流出");
                        modbusClient?.WriteRegisters(1, new ushort[] { 30 });
                        LogHelper.Info($"NG类型：{MType}，通知旋转平台流出");
                        break;
                }
                if (sbReady && robotServer.ClientSockets.Count != 0 && CurruntRockCellMSG != "")
                {
                    sbReady = false;
                    robotServer?.SendData(CurruntRockCellMSG + "\r\n", sbClient);
                    LogHelper.Info($"已有手臂到位信号,NG类型：{MType}，向手臂发送信息:{CurruntRockCellMSG}");
                }
            }
            catch (Exception e)
            {
                LogHelper.Debug($"机台回流异常:{e.Message}");
            }
        }

        /// <summary>
        /// 入库方法
        /// </summary>
        /// <param name="data"></param>
        private async void ReciceStore(string data)//入库机台 arm|PW0K4CLVPWN0B571300A|0|0|M00001P610201|01
        {
            try
            {
                MType = "入库机台";
                List<string> strList = SplitKey(data);
                string message = strList[1] + ",0,0,0," + GetMessageToRobot(strList[4], int.Parse(strList[5]));
                CurruntRockCellMSG = message;
                LogHelper.Info("向旋转平台PLC寄存器地址1写40,等待手臂抓取");
                modbusClient?.WriteRegisters(1, new ushort[] { 40 });//继续流出
                await SqliteSQLHelper.Instance.InsertWithResultAsync(new MachineMSGModel() { RawData = data, USN = CurruntUSN, MType = MType, RackCellMsg = CurruntRockCellMSG });
                if (sbReady && robotServer.ClientSockets.Count != 0)
                {
                    sbReady = false;
                    robotServer?.SendData(CurruntRockCellMSG + "\r\n", sbClient);
                    LogHelper.Info($"已有手臂到位信号,向手臂发送架位信息{CurruntRockCellMSG}");
                }
            }
            catch (Exception e)
            {
                LogHelper.Debug($"机台入库异常:{e.Message}");
            }
        }

        /// <summary>
        /// 向森林系统获取架位信息
        /// </summary>
        /// <returns></returns>
        private async void ShelfPointAsync(string usn)
        {
            try
            {
                string isuse = isWXEnable ? "Y" : "N";
                string isenable = IsByPass ? "N" : "Y";
                string SN = "{\"scene\": \"" + "码垛入库" + "\",\"usn\": \"" + usn + "\",\"isuse\": \"" + isuse + "\",\"isenable\": \"" + isenable + "\"}";
                LogHelper.Info($"开始向森林系统上传场景及机台SN信息:{SN}");
                while (!isCancel)
                {
                    string responseStr = HttpHelper.RequestData(config.GetShelfPoint_URL, "Post", SN);
                    LogHelper.Info($"收到森林系统响应:{responseStr}");
                    GetShelfPositionResponse response = JsonSerializer.Deserialize<GetShelfPositionResponse>(responseStr);

                    // 测试数据
                    // response.coordinate = "armng|PW0K4CM3PWN0B571300A|0|0";
                    // response.coordinate = "arm|PW0K4CLVPWN0B571300A|0|0|M00001P610201|01";

                    if (response.coordinate == null)//访问森林获取架位失败
                    {
                        LogHelper.Info("森林系统响应解析错误");
                        ReciceReturn($"armng|{usn}|0|8");
                        isFull = false;
                        isCancel = false;
                        return;
                    }
                    if (response.coordinate.Contains("ignore")) //爆仓 架位已满,稍后处理
                    {
                        LogHelper.Info($"当前机台{usn}无架位可分配,开始轮询森林系统直到分配架位");
                        await Task.Delay(1000);
                        continue;
                    }
                    else if (response.coordinate.Contains("armng")) // 回流机台 数据格式: armng|PW0K4CM3PWN0B571300A|0|0
                    {
                        LogHelper.Info("森林系统反馈NG,机台回流");
                        ReciceReturn(response.coordinate);
                        isFull = false;
                        isCancel = false;
                        return;
                    }
                    else if (response.coordinate.Contains("arm")) // 入库机台 数据格式: arm|PW0K4CLVPWN0B571300A|0|0|M00001P610201|01
                    {
                        string rackCellCode = SplitKey(response.coordinate)[4];
                        var res = await SqliteSQLHelper.Instance.SelectWithResultAsync<RackCellModel>(c => c.RackCellCode == rackCellCode);
                        if (res.Any1.Count == 0)
                        {
                            LogHelper.Info($"架位{rackCellCode}本地不存在,机台回流");
                            ReciceReturn($"armng|{CurruntUSN}|0|0");
                        }
                        else
                        {
                            LogHelper.Info("机台入库");
                            ReciceStore(response.coordinate);
                        }
                        isFull = false;
                        isCancel = false;
                        return;
                    }
                }
                isFull = false;
                isCancel = false;
                LogHelper.Info("下一个机台进入扫码,上一任务取消");
            }
            catch (Exception e)
            {
                isFull = false;
                isCancel = false;
                LogHelper.Debug($"获取架位信息发生异常:{e.Message}");
            }
        }

        /// <summary>
        /// 爆仓测试方法(模拟获取架位信息)
        /// </summary>
        /// <param name="usn"></param>
        /// <param name="fsData"></param>
        /// <returns></returns>
        private async Task ShelfAsyncTest(string usn = "TestUSN", string fsData = "ignore")
        {
            LogHelper.Info($"开始任务机台SN:{usn}");
            int i = 1;
            while (!isCancel)
            {
                await Task.Delay(2000);//模拟耗时操作
                if (i >= 5)
                {
                    LogHelper.Info($"任务{i}完成,机台{usn}已获取架位信息");
                    isFull = false;
                    return;
                }
                else if (fsData.Contains("ignore"))
                {
                    LogHelper.Info($"任务{i}机台{usn}无架位可分配,开始轮询森林系统直到分配架位");
                    i++;
                    await Task.Delay(3000);
                    continue;
                }
            }
            isFull = false;
            isCancel = false;
            LogHelper.Info($"任务{i}被取消,机台{usn}获取架位信息流程退出");
        }

        /// <summary>
        /// 加载未上抛成功数据
        /// </summary>
        private async void LoadDB()
        {
            UploadFailMSG.Clear();
            var overdueMessages = await SqliteSQLHelper.Instance.SelectWithResultAsync<MachineMSGModel>(c => !c.IsUploaded);
            foreach (MachineMSGModel item in overdueMessages.Any1)
            {
                UploadFailMSG.Add(item);
            }
        }

        /// <summary>
        /// 初始化计时器
        /// </summary>
        private void InitTimer()
        {
            Task.Run(() =>
            {
                LogHelper.Info("正在初始化计时器");
                // 创建 DispatcherTimer 实例
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(int.Parse(config.TimeInterval)); // 设置定时间隔
                timer.Tick += TimerMethod; // 绑定 Tick 事件
                timer.Start(); // 启动计时器
                LogHelper.Info("计时器初始化完成");
            });
        }

        /// <summary>
        /// 计时器方法
        /// </summary>
        private void TimerMethod(object sender, EventArgs e)
        {
            LogHelper.Info($"{config.TimeInterval}秒之期已到,触发上抛,当前生产总数{Count}");
            GlobalParams.UpLoadDataToSFCS(Count.ToString(), config.Line);
        }

        /// <summary>
        /// 记录良率
        /// </summary>
        public void RecordYield()
        {
            // 如果目录不存在，则创建
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
                LogHelper.Info($"不存在文件夹{basePath},正在创建");
            }
            string fileName = Path.Combine(basePath, $"{DateTime.Now:yyyyMMdd}.txt");
            try
            {
                // 将良率写入文件中
                File.WriteAllText(fileName, $"总数: {Count}\r\nOK数: {Count}\r\nNG数: {0}\r\n良率: 100%\r\n更新时间: {DateTime.Now}");
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"写入良率异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取生产计数
        /// </summary>
        private void ReadYieldFromFile()
        {
            Task.Run(() =>
            {
                try
                {
                    string fileName = Path.Combine(basePath, $"{DateTime.Now:yyyyMMdd}.txt");
                    if (File.Exists(fileName))
                    {
                        LogHelper.Info($"正在加载生产数据文件{fileName}");
                        string content = File.ReadAllText(fileName);
                        string[] datas = content.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        Count = int.Parse(datas.Where(s => s.Contains("总数")).First().Substring(4));
                        OKCount = int.Parse(datas.Where(s => s.Contains("OK")).First().Substring(5));
                        NGCount = int.Parse(datas.Where(s => s.Contains("NG")).First().Substring(5));
                        string Yield = datas.Where(s => s.Contains("良率")).First().Substring(4);
                        LogHelper.Info($"文件{fileName}数据加载完成");
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Debug($"读取良率信息异常:{ex.Message}");
                }
            });
        }

        /// <summary>
        /// 程序复位
        /// </summary>
        private void AppReset()
        {
            LogHelper.Info("程序复位中,请稍等");
            sbReady = false;
            isRuning = true;
            CurruntRockCellMSG = "";
            PreviousRockCellMSG = "";
            LogHelper.Info("程序复位完成,请确保平台机台已拿走并重启手臂");
        }

        /// <summary>
        /// 扫码异常处理
        /// </summary>
        private void PhotoEX()
        {
            LogHelper.Info("拍照异常处理中,请稍等");
            //isphoto = true;
            LogHelper.Info("拍照异常处理完成,允许下一个流程开始");
        }

        /// <summary>
        /// 扫码枪重连
        /// </summary>
        private void ScannerRelink()
        {
            LogHelper.Info("扫码枪socket客户端重连中,请稍等");
            scannerClient?.Dispose();
            InitScannerCommunication(GlobalParams.config);
        }

        /// <summary>
        /// 重启外箱服务
        /// </summary>
        private void WXServerRestart()
        {
            LogHelper.Info("外箱检测socket服务重启中,请稍等");
            wxCheckServer?.Dispose();
            InitWXCMCommunication(GlobalParams.config);
        }

        /// <summary>
        /// 重启手臂服务
        /// </summary>
        private void RobotServerRestart()
        {
            LogHelper.Info("手臂socket服务重启中,请稍等");
            robotServer?.Dispose();
            InitRobotCommunication(GlobalParams.config);
        }

        /// <summary>
        /// 重启旋转平台连接
        /// </summary>
        private void RPRestart()
        {
            LogHelper.Info("旋转平台modbus客户端重连中,请稍等");
            modbusClient?.Dispose();
            InitRobotModbusTcpCommunication();
        }

        /// <summary>
        /// 报警
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">值</param>
        /// <param name="warningMsg">报警信息</param>
        private void Warning(ushort address, ushort value, string warningMsg)
        {
            try
            {
                if (modbusClient?.ConnectStatus == true)
                {
                    modbusClient.WriteRegisters(address, new ushort[] { value });
                    LogHelper.Info("报警信息:" + warningMsg);
                }
            }
            catch (Exception e)
            {
                LogHelper.Debug($"输出报警信号异常:{e.Message}");
            }
        }
        #endregion
    }
}
