using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EchoTspServer.Application.Interfaces
{
    public interface IClientHandler
    {
        Task HandleClientAsync(TcpClient client, CancellationToken token);
    }
}
