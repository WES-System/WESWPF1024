using NModbus;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace WES.Helpers
{
    public class ModbusTcpClient : IDisposable
    {
        private readonly TimeSpan _connectionRetryInterval = TimeSpan.FromSeconds(5); // 连接失败后的重试间隔
        private readonly TimeSpan _monitoringInterval = TimeSpan.FromSeconds(1);      // 连接监控的心跳间隔
        private readonly int _connectTimeoutMs = 5000;                               // TCP连接超时 (毫秒)
        private bool _running = true;
        private readonly object _lockObj = new object();
        private TcpClient _tcpClient;
        private CancellationTokenSource _cts; // 统一的CancellationTokenSource，用于取消所有后台任务

        /// <summary>
        /// 日志事件
        /// </summary>
        public event Action<string, string> LogEvent;
        public event Action ConnectEvent;
        public event Action DisConnectEvent;

        public IPEndPoint ModbusIPEndPoint { get; private set; }
        public IModbusMaster ModbusMaster { get; private set; }

        /// <summary>
        /// 从站地址
        /// </summary>
        public byte SlaveAddress { get; set; } = 1;

        /// <summary>
        /// 监测地址位(用于心跳检测)
        /// </summary>
        public ushort MonitorAddress { get; set; } = 0;

        public bool ConnectStatus { get; private set; }

        public ModbusTcpClient(IPEndPoint iPEndPoint)
        {
            ModbusIPEndPoint = iPEndPoint ?? throw new ArgumentNullException(nameof(iPEndPoint));
            Initialize();
        }

        public ModbusTcpClient(string ip, string port)
        {
            if (string.IsNullOrWhiteSpace(ip)) throw new ArgumentException("IP cannot be null or empty.", nameof(ip));
            if (!int.TryParse(port, out int p) || p <= 0) throw new ArgumentException("Port must be a valid positive integer.", nameof(port));

            ModbusIPEndPoint = new IPEndPoint(IPAddress.Parse(ip), p);
            Initialize();
        }

        private void Initialize()
        {
            _cts = new CancellationTokenSource();
            // 启动主连接/重连任务
            _ = Task.Run(MainConnectionLoop, _cts.Token);
        }

        public bool[] ReadCoils(ushort startAddress, ushort length)
        {
            lock (_lockObj)
            {
                EnsureConnected();
                return ModbusMaster?.ReadCoils(SlaveAddress, startAddress, length);
            }
        }

        public ushort[] ReadHoldingRegisters(ushort startAddress, ushort length)
        {
            lock (_lockObj)
            {
                EnsureConnected();
                return ModbusMaster?.ReadHoldingRegisters(SlaveAddress, startAddress, length);
            }
        }

        public void WriteCoils(ushort startAddress, bool[] data)
        {
            lock (_lockObj)
            {
                EnsureConnected();
                ModbusMaster?.WriteMultipleCoils(SlaveAddress, startAddress, data);
            }
        }

        public void WriteRegisters(ushort startAddress, ushort[] data)
        {
            lock (_lockObj)
            {
                EnsureConnected();
                ModbusMaster?.WriteMultipleRegisters(SlaveAddress, startAddress, data);
            }
        }

        private void EnsureConnected()
        {
            if (!ConnectStatus)
            {
                throw new InvalidOperationException("Modbus client is not connected.");
            }
        }

        /// <summary>
        /// 主连接循环：负责建立初始连接和所有后续的重连。
        /// </summary>
        private async Task MainConnectionLoop()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    await AttemptConnectionAsync(_cts.Token).ConfigureAwait(false);
                    if (ConnectStatus)
                    {
                        // 连接成功，启动监控任务
                        await ConnectionMonitorLoop(_cts.Token).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    // 正常退出
                    break;
                }
                catch (Exception ex)
                {
                    LogEvent?.Invoke("主连接循环发生未预期异常", ex.ToString());
                    // 发生严重错误，短暂等待后继续尝试
                    await Task.Delay(_connectionRetryInterval, _cts.Token).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// 尝试建立一次TCP连接。
        /// </summary>
        private async Task AttemptConnectionAsync(CancellationToken ct)
        {
            LogEvent?.Invoke("正在尝试连接", $"到 {ModbusIPEndPoint}");

            var tcpClient = new TcpClient();
            try
            {
                using (var connectCts = CancellationTokenSource.CreateLinkedTokenSource(ct))
                {
                    connectCts.CancelAfter(_connectTimeoutMs);

                    var connectTask = tcpClient.ConnectAsync(ModbusIPEndPoint.Address, ModbusIPEndPoint.Port);
                    await Task.WhenAny(connectTask, Task.Delay(Timeout.Infinite, connectCts.Token)).ConfigureAwait(false);

                    if (!connectTask.IsCompleted)
                    {
                        throw new TimeoutException($"连接到 {ModbusIPEndPoint} 超时 ({_connectTimeoutMs}ms)");
                    }

                    await connectTask.ConfigureAwait(false); // 确保抛出连接异常

                    // 连接成功，初始化ModbusMaster
                    var modbusFactory = new ModbusFactory();
                    var master = modbusFactory.CreateMaster(tcpClient);

                    // 原子性地更新状态
                    lock (_lockObj)
                    {
                        _tcpClient = tcpClient;
                        ModbusMaster = master;
                        ConnectStatus = true;
                    }

                    ConnectEvent?.Invoke();
                    LogEvent?.Invoke("连接成功", $"已连接到 {ModbusIPEndPoint}");
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                // 清理本次尝试创建的资源
                tcpClient?.Dispose();
                LogEvent?.Invoke("连接失败", ex.Message);
                throw; // 让上层决定重试策略
            }
        }

        /// <summary>
        /// 连接监控循环：在连接成功后运行，通过心跳包维持连接状态。
        /// </summary>
        private async Task ConnectionMonitorLoop(CancellationToken ct)
        {
            LogEvent?.Invoke("启动连接监控", "开始发送心跳包");

            while (!ct.IsCancellationRequested && ConnectStatus)
            {
                try
                {
                    await Task.Delay(_monitoringInterval, ct).ConfigureAwait(false);

                    // 执行心跳读取
                    lock (_lockObj)
                    {
                        if (!ConnectStatus) break; // 双重检查
                        ModbusMaster?.ReadHoldingRegisters(SlaveAddress, MonitorAddress, 1);
                    }
                }
                catch (OperationCanceledException)
                {
                    break; // 正常退出
                }
                catch (Exception ex)
                {
                    LogEvent?.Invoke("心跳检测失败", ex.Message);
                    // 心跳失败，跳出监控循环，让主循环处理重连
                    break;
                }
            }

            // 如果是因为异常退出，则触发断开事件
            if (ConnectStatus)
            {
                await HandleDisconnectionAsync("心跳检测失败，连接已断开").ConfigureAwait(false);
            }
        }

        private async Task HandleDisconnectionAsync(string reason)
        {
            bool wasConnected;
            lock (_lockObj)
            {
                wasConnected = ConnectStatus;
                ConnectStatus = false;
                // 不在此处清理资源，统一在 Cleanup 中处理
            }

            if (wasConnected)
            {
                LogEvent?.Invoke("连接断开", reason);
                DisConnectEvent?.Invoke();
                await CleanupResourcesAsync().ConfigureAwait(false);
            }
        }

        private async Task CleanupResourcesAsync()
        {
            TcpClient clientToDispose = null;
            IModbusMaster masterToDispose = null;

            lock (_lockObj)
            {
                clientToDispose = _tcpClient;
                masterToDispose = ModbusMaster;
                _tcpClient = null;
                ModbusMaster = null;
            }

            // 在锁外执行耗时的Dispose操作，避免阻塞其他线程
            if (masterToDispose != null)
            {
                try
                {
                    masterToDispose.Dispose();
                }
                catch (Exception ex)
                {
                    LogEvent?.Invoke("释放ModbusMaster异常", ex.Message);
                }
            }

            if (clientToDispose != null)
            {
                try
                {
                    clientToDispose.Client?.Shutdown(SocketShutdown.Both);
                    clientToDispose.Close();
                    await Task.Delay(10).ConfigureAwait(false); // 给Close一点时间
                }
                catch (Exception ex)
                {
                    LogEvent?.Invoke("关闭TCP连接异常", ex.Message);
                }
                finally
                {
                    clientToDispose.Dispose();
                }
            }
        }

        public void Dispose()
        {
            if (_cts.IsCancellationRequested) return;

            _cts.Cancel();
            _ = Task.Run(async () =>
            {
                await HandleDisconnectionAsync("客户端被主动释放").ConfigureAwait(false);
                _cts.Dispose();
            });
        }
    }
}