
public interface IUdpClient
{
    event EventHandler<byte[]>? MessageReceived;
    void Close();
    Task ReceiveMessagesAsync(CancellationToken cancellationToken);
}