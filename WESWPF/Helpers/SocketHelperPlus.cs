using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WES.Helpers
{
    public class SocketHelperPlus : IDisposable
    {
        public Socket Socket { get; private set; }
        private readonly string ipAddress;
        private readonly int port;
        private readonly MySocketType socketType;
        private volatile bool isRunning;
        private readonly object syncLock = new object();

        private readonly List<Socket> clientSockets = new List<Socket>(); // 客户端套接字列表需线程安全

        // 接收和发送的编码
        public Encoding ReceiveEncoding { get; set; }
        public Encoding SendEncoding { get; set; }

        public event Action<string> LogEvent;
        public event Action<string, Socket> OnDataReceived;
        public event Action<Socket> OnConnectionLost;
        public event Action<Socket> OnClientConnected;

        public SocketHelperPlus(string ipAddress, int port, MySocketType socketType, Encoding receiveEncoding = null, Encoding sendEncoding = null)
        {
            this.ipAddress = ipAddress;
            this.port = port;
            this.socketType = socketType;
            ReceiveEncoding = receiveEncoding ?? Encoding.UTF8;
            SendEncoding = sendEncoding ?? Encoding.UTF8;
        }

        public void Start()
        {
            if (!isRunning)
            {
                isRunning = true;
                if (socketType == MySocketType.Client)
                {
                    StartClient();
                }
                else if (socketType == MySocketType.Server)
                {
                    StartServer();
                }
            }
        }

        public void Stop()
        {
            lock (syncLock)
            {
                isRunning = false;
                // Close main socket
                Socket?.Close();
                Socket = null;

                // Close all client sockets
                foreach (var clientSocket in clientSockets)
                {
                    try
                    {
                        clientSocket.Shutdown(SocketShutdown.Both);
                        clientSocket.Close();
                    }
                    catch
                    {
                        // Ignore any exceptions during shutdown
                    }
                }
                clientSockets.Clear();
            }
        }

        private void StartServer()
        {
            Task.Run(() =>
            {
                try
                {
                    Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                    Socket.Bind(endPoint);
                    Socket.Listen(10);

                    LogEvent?.Invoke($"Socket服务器已启动，监听地址：{endPoint}");

                    while (isRunning)
                    {
                        Socket clientSocket = Socket.Accept();
                        lock (syncLock)
                        {
                            clientSockets.Add(clientSocket);
                        }

                        LogEvent?.Invoke($"{clientSocket.RemoteEndPoint} 已连接");
                        OnClientConnected?.Invoke(clientSocket);
                        StartReceiving(clientSocket);
                    }
                }
                catch (Exception ex)
                {
                    if (isRunning)
                    {
                        LogEvent?.Invoke($"启动 Socket 服务器时发生错误：{ex.Message}");
                        RestartService();
                    }
                }
            });
        }

        private void StartClient()
        {
            Task.Run(() =>
            {
                try
                {
                    Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    Socket.Connect(new IPEndPoint(IPAddress.Parse(ipAddress), port));

                    LogEvent?.Invoke($"已连接到 Socket 服务器 {ipAddress}:{port}");
                    OnClientConnected?.Invoke(Socket);
                    StartReceiving(Socket);
                }
                catch (Exception ex)
                {
                    if (isRunning)
                    {
                        LogEvent?.Invoke($"连接到 Socket 服务器时发生异常：{ex.Message}");
                        RestartService();
                    }
                }
            });
        }

        private void StartReceiving(Socket clientSocket)
        {
            Task.Run(() =>
            {
                while (isRunning)
                {
                    try
                    {
                        byte[] buffer = new byte[1024];
                        int receivedBytes = clientSocket.Receive(buffer);
                        if (receivedBytes == 0)
                        {
                            HandleConnectionLoss(clientSocket);
                            break;
                        }

                        string data = ReceiveEncoding.GetString(buffer, 0, receivedBytes);
                        OnDataReceived?.Invoke(data, clientSocket);
                    }
                    catch (SocketException ex)
                    {
                        LogEvent?.Invoke($"接收数据时出错：{ex.Message}");
                        HandleConnectionLoss(clientSocket);
                        break;
                    }
                    catch (Exception ex)
                    {
                        LogEvent?.Invoke($"接收发生未知异常：{ex.Message}");
                        HandleConnectionLoss(clientSocket);
                        break;
                    }
                }
            });
        }

        public void SendData(string data, Socket targetSocket)
        {
            try
            {
                byte[] bytes = SendEncoding.GetBytes(data);
                targetSocket?.Send(bytes);
                LogEvent?.Invoke($"发送数据：{data} --> {targetSocket.RemoteEndPoint}");
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke($"发送数据时发生异常：{ex.Message}");
                HandleConnectionLoss(targetSocket);
            }
        }

        private void HandleConnectionLoss(Socket clientSocket)
        {
            lock (syncLock)
            {
                if (clientSockets.Contains(clientSocket))
                {
                    clientSockets.Remove(clientSocket);
                }

                try
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
                catch
                {
                    // Ignore exceptions during shutdown
                }
            }

            OnConnectionLost?.Invoke(clientSocket);
        }

        private void RestartService()
        {
            Task.Run(async () =>
            {
                if (!isRunning) return;
                Stop();
                await Task.Delay(3000); // 等待3秒重启服务
                Start();
            });
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }
    }
}
