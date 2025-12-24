using NModbus;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace WES.Helpers
{
    public class ModbusHelper : IDisposable
    {
        private bool _running = true;
        private readonly object _lockObj = new object();
        private TcpClient _tcpClient;
        private CancellationTokenSource _connectCts;
        private CancellationTokenSource _monitorCts;

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
        /// 监测地址位
        /// </summary>
        public ushort MonitorAddress { get; set; } = 0;

        public bool ConnectStatus { get; private set; }

        public ModbusHelper(IPEndPoint iPEndPoint)
        {
            ModbusIPEndPoint = iPEndPoint;
            Connect();
        }

        public ModbusHelper(string ip, string port)
        {
            ModbusIPEndPoint = new IPEndPoint(IPAddress.Parse(ip), int.Parse(port));
            Connect();
        }

        public bool[] ReadCoils(ushort startAddress, ushort length)
        {
            lock (_lockObj)
            {
                return ModbusMaster?.ReadCoils(SlaveAddress, startAddress, length);
            }
        }

        public ushort[] ReadHoldingRegisters(ushort startAddress, ushort length)
        {
            lock (_lockObj)
            {
                return ModbusMaster?.ReadHoldingRegisters(SlaveAddress, startAddress, length);
            }
        }

        public void WriteCoils(ushort startAddress, bool[] data)
        {
            lock (_lockObj)
            {
                ModbusMaster?.WriteMultipleCoils(SlaveAddress, startAddress, data);
            }
        }

        public void WriteRegisters(ushort startAddress, ushort[] data)
        {
            lock (_lockObj)
            {
                ModbusMaster?.WriteMultipleRegisters(SlaveAddress, startAddress, data);
            }
        }

        private void Connect()
        {
            lock (_lockObj)
            {
                // 如果已连接或正在连接，直接返回
                if (ConnectStatus || _connectCts != null && !_connectCts.IsCancellationRequested)
                {
                    return;
                }

                // 取消之前的连接任务（如果存在）
                _connectCts?.Cancel();
                _connectCts = new CancellationTokenSource();
                var token = _connectCts.Token;

                _running = true;
                Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested && _running)
                    {
                        try
                        {
                            await Task.Delay(1000, token).ConfigureAwait(false);

                            // 使用异步连接方法
                            _tcpClient = new TcpClient();
                            await _tcpClient.ConnectAsync(ModbusIPEndPoint.Address, ModbusIPEndPoint.Port)
                                .ConfigureAwait(false);

                            lock (_lockObj)
                            {
                                ModbusMaster = new ModbusFactory().CreateMaster(_tcpClient);
                                ConnectStatus = true;
                            }

                            ConnectEvent?.Invoke();
                            StartConnectMonitor();
                            break;
                        }
                        catch (OperationCanceledException)
                        {
                            // 任务被取消时正常退出
                            break;
                        }
                        catch (Exception e)
                        {
                            LogEvent?.Invoke("连接失败", e.Message);
                            await CleanupResources().ConfigureAwait(false);
                        }
                    }
                }, token);
            }
        }

        private void StartConnectMonitor()
        {
            lock (_lockObj)
            {
                // 取消之前的监测任务
                _monitorCts?.Cancel();
                _monitorCts = new CancellationTokenSource();
                var token = _monitorCts.Token;

                Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested && _running)
                    {
                        try
                        {
                            await Task.Delay(1000, token).ConfigureAwait(false);

                            lock (_lockObj)
                            {
                                ModbusMaster?.ReadHoldingRegisters(SlaveAddress, MonitorAddress, 1);
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            // 任务被取消时正常退出
                            break;
                        }
                        catch (Exception e)
                        {
                            LogEvent?.Invoke("连接异常", e.Message);
                            await CleanupResources().ConfigureAwait(false);
                            Connect(); // 重新连接
                            break;
                        }
                    }
                }, token);
            }
        }

        private async Task CleanupResources()
        {
            lock (_lockObj)
            {
                if (ConnectStatus)
                {
                    ConnectStatus = false;
                    DisConnectEvent?.Invoke();
                }
            }

            // 取消监测任务
            if (_monitorCts != null)
            {
                _monitorCts.Cancel();
                _monitorCts.Dispose();
                _monitorCts = null;
            }

            // 释放ModbusMaster
            if (ModbusMaster != null)
            {
                try
                {
                    ModbusMaster.Dispose();
                }
                catch (Exception e)
                {
                    LogEvent?.Invoke("释放ModbusMaster异常", e.Message);
                }
                finally
                {
                    lock (_lockObj)
                    {
                        ModbusMaster = null;
                    }
                }
            }

            // 关闭TCP连接
            if (_tcpClient != null)
            {
                try
                {
                    if (_tcpClient.Connected)
                    {
                        _tcpClient.Client.Shutdown(SocketShutdown.Both);
                        await Task.Run(() => _tcpClient.Close()).ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    LogEvent?.Invoke("关闭TCP连接异常", e.Message);
                }
                finally
                {
                    _tcpClient = null;
                }
            }
        }

        public void Dispose()
        {
            _running = false;

            // 取消所有任务
            _connectCts?.Cancel();
            _monitorCts?.Cancel();

            // 清理资源
            Task.Run(async () => await CleanupResources().ConfigureAwait(false)).Wait();

            // 释放取消令牌
            _connectCts?.Dispose();
            _monitorCts?.Dispose();
        }
    }
}