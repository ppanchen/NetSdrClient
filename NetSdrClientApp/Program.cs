using NetSdrClientApp;
using NetSdrClientApp.Networking;

var tcpClient = new TcpClientWrapper("127.0.0.1", 8080);
var udpClient = new UdpClientWrapper(60000);

var netSdr = new NetSdrClient(tcpClient, udpClient);

while (true)
{
    var key = Console.ReadKey(intercept: true).Key;
    if (key == ConsoleKey.C)
    {
        await netSdr.ConnectAsync();
    }
    else if (key == ConsoleKey.D)
    {
        netSdr.Disconect();
    }
    else if (key == ConsoleKey.F)
    {
        await netSdr.ChangeFrequencyAsync(200000, 1);
    }
    else if (key == ConsoleKey.S)
    {
        if (netSdr.IQStarted)
        {
            await netSdr.StopIQAsync();
        }
        else
        {
            await netSdr.StartIQAsync();
        }
    }
    else if (key == ConsoleKey.Q)
    {
        break;
    }
}
