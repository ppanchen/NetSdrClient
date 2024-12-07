using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetSdrClientApp
{
    public class TcpClientWrapper : ITcpClient
    {
        private string _host;
        private int _port;
        private TcpClient _tcpClient;
        private NetworkStream? _stream;

        public event EventHandler<byte[]>? MessageReceived;

        public TcpClientWrapper(string host, int port)
        {
            _host = host;
            _port = port;
            _tcpClient = new TcpClient();
        }

        public void Connect()
        {
            if (_tcpClient.Connected)
            {
                Console.WriteLine("Already connected.");
                return;
            }

            try
            {
                _tcpClient.Connect(_host, _port);
                _stream = _tcpClient.GetStream();
                Console.WriteLine($"Connected to {_host}:{_port}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            if (_tcpClient.Connected)
            {
                _stream?.Close();
                _tcpClient.Close();
                Console.WriteLine("Disconnected.");
            }
            else
            {
                Console.WriteLine("No active connection to disconnect.");
            }
        }

        public async Task SendMessageAsync(byte[] data, CancellationToken cancellationToken)
        {
            if (!_tcpClient.Connected || _stream == null)
                throw new InvalidOperationException("Not connected to a server.");

            await _stream.WriteAsync(data, 0, data.Length, cancellationToken);
            Console.WriteLine($"Message sent.");
        }

        public async Task StartListeningAsync(CancellationToken cancellationToken)
        {
            if (!_tcpClient.Connected || _stream == null)
                throw new InvalidOperationException("Not connected to a server.");

            try
            {
                //using (NetworkStream stream = _tcpClient.GetStream())
                {
                    byte[] buffer = new byte[8192];

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                        if (bytesRead > 0)
                        {
                            MessageReceived?.Invoke(this, buffer.AsSpan(0, bytesRead).ToArray());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in listening loop: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Listener stopped.");
            }
        }
    }

}
