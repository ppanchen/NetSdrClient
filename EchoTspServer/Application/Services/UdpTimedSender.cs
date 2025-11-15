using EchoTspServer.Application.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EchoTspServer.Application.Services
{
    public class UdpTimedSender : IUdpSender
    {
        private readonly string _host;
        private readonly int _port;
        private readonly ILogger _logger;
        private readonly UdpClient _udpClient = new();
        private Timer? _timer;
        private ushort _counter = 0;

        public UdpTimedSender(string host, int port, ILogger logger)
        {
            _host = host;
            _port = port;
            _logger = logger;
        }

        public void StartSending(int intervalMilliseconds)
        {
            if (_timer != null)
                throw new InvalidOperationException("Sender already running.");

            _timer = new Timer(SendMessage, null, 0, intervalMilliseconds);
        }

        private void SendMessage(object? _)
        {
            try
            {
                var rnd = new Random();
                var samples = new byte[1024];
                rnd.NextBytes(samples);
                _counter++;

                var msg = new byte[] { 0x04, 0x84 }
                    .Concat(BitConverter.GetBytes(_counter))
                    .Concat(samples)
                    .ToArray();

                var endpoint = new IPEndPoint(IPAddress.Parse(_host), _port);
                _udpClient.Send(msg, msg.Length, endpoint);
                _logger.Info($"Sent UDP packet to {_host}:{_port}");
            }
            catch (Exception ex)
            {
                _logger.Error($"UDP send error: {ex.Message}");
            }
        }

        public void StopSending()
        {
            _timer?.Dispose();
            _timer = null;
        }

        public void Dispose()
        {
            StopSending();
            _udpClient.Dispose();
        }
    }
}
