using System;
using System.Threading.Tasks;


namespace NetSdrClientApp.Networking
{
    public interface IUdpClient
    {
        event EventHandler<MessageReceivedEventArgs>? MessageReceived;

        Task StartListeningAsync();

        void StopListening();
        
        void Exit();
    }
}