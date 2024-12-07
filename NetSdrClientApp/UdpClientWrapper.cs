using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class UdpClientWrapper : IUdpClientWrapper
{
    private readonly IPEndPoint _localEndPoint;
    private UdpClient _udpClient;

    public event EventHandler<byte[]>? MessageReceived;

    public UdpClientWrapper(string host, int port)
    {
        _localEndPoint = new IPEndPoint(IPAddress.Parse(host), port);
        _udpClient = new UdpClient(_localEndPoint);
    }

    public async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                UdpReceiveResult result = await _udpClient.ReceiveAsync();
                MessageReceived?.Invoke(this, result.Buffer);

                string receivedMessage = Encoding.UTF8.GetString(result.Buffer);
                Console.WriteLine($"Received: {receivedMessage} from {result.RemoteEndPoint}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error receiving message: {ex.Message}");
        }
    }

    public void Close()
    {
        _udpClient.Close();
    }
}