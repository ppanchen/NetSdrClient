using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetSdrClientApp.Networking
{
    public class TcpClientWrapper : NetworkClientBase, ITcpClient
    {
        private string _host;
        private int _port;
        private TcpClient? _tcpClient;
        private NetworkStream? _stream;

        public bool Connected => _tcpClient?.Connected == true && _stream != null;

        public TcpClientWrapper(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public void Connect()
        {
            if (Connected)
            {
                Log($"Already connected to {_host}:{_port}");
                return;
            }

            try
            {
                _tcpClient = new TcpClient();
                _tcpClient.Connect(_host, _port);
                _stream = _tcpClient.GetStream();
                _cts = new CancellationTokenSource();

                Log($"Connected to {_host}:{_port}");
                _ = ListenAsync();
            }
            catch (Exception ex)
            {
                Log($"Failed to connect: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            Stop();
            _stream?.Close();
            _tcpClient?.Close();
            _stream = null;
            _tcpClient = null;
            Log("Disconnected.");
        }

        public async Task SendMessageAsync(byte[] data)
        {
            if (!Connected || _stream == null)
                throw new InvalidOperationException("Not connected to a server.");

            LogBytes("Message sent", data);
            await _stream.WriteAsync(data, 0, data.Length);
        }

        private async Task ListenAsync()
        {
            if (_stream == null) return;
            try
            {
                Log("Start listening for incoming messages...");
                while (!_cts!.Token.IsCancellationRequested)
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, _cts.Token);
                    if (bytesRead > 0)
                        OnMessageReceived(buffer.AsSpan(0, bytesRead).ToArray());
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Log($"Error in listening loop: {ex.Message}");
            }
            finally
            {
                Log("Listener stopped.");
            }
        }
    }
}
