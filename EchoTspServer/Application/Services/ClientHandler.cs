using EchoTspServer.Application.Interfaces;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EchoTspServer.Application.Services
{
    public class ClientHandler : IClientHandler
    {
        private readonly ILogger _logger;

        public ClientHandler(ILogger logger)
        {
            _logger = logger;
        }

        public async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            using NetworkStream stream = client.GetStream();
            try
            {
                byte[] buffer = new byte[8192];
                int bytesRead;
                while (!token.IsCancellationRequested &&
                       (bytesRead = await stream.ReadAsync(buffer, token)) > 0)
                {
                    await stream.WriteAsync(buffer.AsMemory(0, bytesRead), token);
                    _logger.Info($"Echoed {bytesRead} bytes to client.");
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                _logger.Error($"Client error: {ex.Message}");
            }
            finally
            {
                client.Close();
                _logger.Info("Client disconnected.");
            }
        }
    }
}
