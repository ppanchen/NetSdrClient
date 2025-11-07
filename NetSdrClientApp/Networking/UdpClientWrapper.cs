using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetSdrClientApp.Networking
{
    public class UdpClientWrapper : NetworkClientBase, IUdpClient
    {
        private readonly IPEndPoint _localEndPoint;
        private UdpClient? _udpClient;

        public UdpClientWrapper(int port)
        {
            _localEndPoint = new IPEndPoint(IPAddress.Any, port);
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }

        public async Task StartListeningAsync()
        {
            _cts = new CancellationTokenSource();
            Log("Start listening for UDP messages...");

            try
            {
                _udpClient = new UdpClient(_localEndPoint);
                while (!_cts.Token.IsCancellationRequested)
                {
                    UdpReceiveResult result = await _udpClient.ReceiveAsync(_cts.Token);
                    OnMessageReceived(result.Buffer);
                    Log($"Received from {result.RemoteEndPoint}");
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Log($"Error receiving message: {ex.Message}");
            }
        }

        public override void Stop()
        {
            base.Stop();
            _udpClient?.Close();
            Log("Stopped listening for UDP messages.");
        }

        public void StopListening()
        {
            throw new NotImplementedException();
        }
    }
}
