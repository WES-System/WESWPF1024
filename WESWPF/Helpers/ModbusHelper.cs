using NModbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WES.Helpers
{
    public class ModbusHelper : IDisposable
    {
        private bool _running = true;

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

        public bool ConnectStatus { get; set; }

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
            return ModbusMaster?.ReadCoils(SlaveAddress, startAddress, length);
        }

        public ushort[] ReadHoldingRegisters(ushort startAddress, ushort length)
        {
            return ModbusMaster?.ReadHoldingRegisters(SlaveAddress, startAddress, length);
        }

        public void WriteCoils(ushort startAddress, bool[] data)
        {
            ModbusMaster?.WriteMultipleCoils(SlaveAddress, startAddress, data);
        }

        public void WriteRegisters(ushort startAddress, ushort[] data)
        {
            ModbusMaster?.WriteMultipleRegisters(SlaveAddress, startAddress, data);
        }


        private void Connect()
        {
            _running = true;
            Task.Run(() =>
            {
                while (_running)
                {
                    Thread.Sleep(1000);
                    try
                    {
                        TcpClient tcpClient = new TcpClient();
                        tcpClient.Connect(ModbusIPEndPoint);
                        ModbusMaster = new ModbusFactory().CreateMaster(tcpClient);
                        ConnectStatus = true;
                        ConnectEvent?.BeginInvoke(null, null);
                        ConnectMonitor();
                        break;
                    }
                    catch (Exception e)
                    {
                        LogEvent?.Invoke("连接失败", e.Message);
                    }
                }
            });
        }

        private void ConnectMonitor()
        {
            Task.Run(() =>
            {
                while (_running)
                {
                    Thread.Sleep(1000);
                    try
                    {
                        ModbusMaster?.ReadHoldingRegisters(SlaveAddress, MonitorAddress, 1);
                    }
                    catch (Exception e)
                    {
                        LogEvent?.Invoke("连接异常", e.Message);
                        ConnectStatus = false;
                        //DisConnectEvent?.BeginInvoke(null, null);
                        Dispose();
                        Connect();
                        break;
                    }
                }
            });
        }

        public void Dispose()
        {
            try
            {
                _running = false;
                ModbusMaster?.Dispose();
                ModbusMaster = null;
                GC.Collect();
                DisConnectEvent?.BeginInvoke(null, null);
            }
            catch (Exception e)
            {
                LogEvent?.Invoke("释放资源异常", e.Message);
            }
        }
    }
}
