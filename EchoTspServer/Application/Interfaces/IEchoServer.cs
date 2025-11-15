using System.Threading.Tasks;

namespace EchoTspServer.Application.Interfaces
{
    public interface IEchoServer
    {
        Task StartAsync();
        void Stop();
    }
}
