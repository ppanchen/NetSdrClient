namespace EchoTspServer.Application.Interfaces
{
    public interface IUdpSender : IDisposable
    {
        void StartSending(int intervalMilliseconds);
        void StopSending();
    }
}
