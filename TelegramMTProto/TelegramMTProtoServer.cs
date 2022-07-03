using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramMTProto
{
    public class TelegramMTProtoServer : IDisposable
    {

        private SocketListener _socketListener;
        private readonly List<TelegramMTProtoSocket> _protoSockets = new List<TelegramMTProtoSocket>();
        public TelegramMTProtoServer(in string secret, in int port, in string ip = "default")
        {
            _secret = secret;
            _port = port;
            _ip = ip;
        }
        private readonly string _secret;
        private readonly int _port;
        private readonly string _ip;
        private int _backLog;
        private readonly object _lockListener = new object();
        private readonly object _lockConnection = new object();
        private volatile bool _isDisposed;

        
        public string Secret { get => _secret; private set { } }
        public bool IsClosed { get => _isDisposed; private set { } }
        public int Port { get => _port; private set { } }
       
        public void Start(in int backLog = 100)
        {
            ThrowIfDisposed();
            _backLog = backLog;
            _socketListener = new SocketListener();
            IPAddress ipAddress = null;
            if (_ip == "default")
            {
                ipAddress = IPAddress.Any;
            }
            else
            {
                if (!IPAddress.TryParse(_ip, out ipAddress))
                {
                    throw new Exception("ipAddress is not valid");
                }
            }
            var ipEndPoint = new IPEndPoint(ipAddress, _port);
            _socketListener.StartListen(ipEndPoint, _backLog);
            StartListener();
            TelegramMTProtoSockets.StartAsync();
        }
        private Task StartListener()
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        var socket = _socketListener.Accept();
                        SocketAccepted(socket);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        lock (_lockListener)
                        {
                            if (!_isDisposed)
                            {
                                try
                                {
                                    _socketListener.Stop();
                                    _socketListener = null;
                                    Start(_backLog);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                }
                            }
                        }
                    }
                }
            });
        }
        private void SocketAccepted(Socket socket)
        {
            try
            {
                Console.WriteLine("A new connection was created");
                var buffer = new byte[64];
                var result = socket.Receive(buffer);
                if (result == 64)
                {
                    var mtpSocket = new TelegramMTProtoSocket(socket);
                    mtpSocket.MTProtoSocketDisconnected += MTProtoSocketDisconnected;
                    mtpSocket.StartAsync(buffer, _secret);
                    lock (_lockConnection)
                    {
                        _protoSockets.Add(mtpSocket);
                    }
                }
                else
                {
                    socket.Dispose();
                    socket = null;
                }
                lock (_lockConnection)
                {
                    var endPointsCount = _protoSockets.Select(x => x.IPEndPoint.Address).Distinct().Count();
                    Console.WriteLine("Number of users(Ips):{0}", endPointsCount);
                    Console.WriteLine("Number of connections:{0}", _protoSockets.Count());
                }
                Array.Clear(buffer, 0, buffer.Length);
                buffer = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        private void MTProtoSocketDisconnected(object sender, EventArgs e)
        {
            var mtp = (TelegramMTProtoSocket)sender;
            mtp.Dispose();
            lock (_lockConnection)
            {
                _protoSockets.Remove(mtp);
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(in bool isDisposing)
        {
            lock (_lockListener)
            {
                try
                {
                    if (_isDisposed)
                    {
                        return;
                    }
                    _isDisposed = true;

                    if (!isDisposing)
                    {
                        return;
                    }
                    _socketListener.Stop();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                _socketListener = null;
                lock (_lockConnection)
                {
                    foreach (var mtp in _protoSockets)
                    {
                        try
                        {
                            mtp.Dispose();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }
                TelegramMTProtoSockets.Close();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("Connection was disposed.");
            }
        }
    }
}