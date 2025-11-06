using EchoTspServer.Application.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EchoTspServer.Application.Services
{
    public class EchoServer : IEchoServer
    {
        private readonly int _port;
        private readonly ILogger _logger;
        private readonly IClientHandler _clientHandler;
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;
        private bool _isStopped = false;

        public EchoServer(int port, ILogger logger, IClientHandler clientHandler)
        {
            _port = port;
            _logger = logger;
            _clientHandler = clientHandler;
        }

        public async Task StartAsync()
        {
            _cts = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            _logger.Info($"Server started on port {_port}.");

            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    _logger.Info("Client connected.");
                    _ = Task.Run(() => _clientHandler.HandleClientAsync(client, _cts.Token));
                }
            }
            catch (ObjectDisposedException)
            {
                // Listener has been closed normally
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted)
            {
                // Listener closed — expected when stopping
            }
            finally
            {
                _logger.Info("Server shutdown.");
            }
        }

        public void Stop()
        {
            if (_isStopped) return; // already stopped — ignore
            _isStopped = true;

            try
            {
                _cts?.Cancel();
                _listener?.Stop();
            }
            catch (ObjectDisposedException)
            {
                // Already disposed — safe to ignore
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
                _logger.Info("Server stopped.");
            }
        }
    }
}
