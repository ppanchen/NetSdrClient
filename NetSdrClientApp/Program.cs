using System;
using System.Threading.Tasks;
using NetSdrClientApp;
using NetSdrClientApp.Networking;

class Program
{
    static async Task Main()
    {
        Console.WriteLine(@"Usage:
C - connect
D - disconnect
F - set frequency
S - Start/Stop IQ listener
Q - quit");

        var tcpClient = new TcpClientWrapper("127.0.0.1", 5000);
        var udpClient = new UdpClientWrapper(60000);

        var netSdr = new NetSdrClient(tcpClient, udpClient);

        while (true)
        {
            var key = Console.ReadKey(intercept: true).Key;

            switch (key)
            {
                case ConsoleKey.C:
                    await netSdr.ConnectAsync();
                    Console.WriteLine("Connected to SDR.");
                    break;

                case ConsoleKey.D:
                    netSdr.Disconnect();  // ✅ виправлено Disconect → Disconnect
                    Console.WriteLine("Disconnected.");
                    break;

                case ConsoleKey.F:
                    await netSdr.ChangeFrequencyAsync(20000000, 1);
                    Console.WriteLine("Frequency set to 20 MHz.");
                    break;

                case ConsoleKey.S:
                    if (netSdr.IQStarted)
                    {
                        await netSdr.StopIQAsync();
                        Console.WriteLine("IQ stream stopped.");
                    }
                    else
                    {
                        await netSdr.StartIQAsync();
                        Console.WriteLine("IQ stream started.");
                    }
                    break;

                case ConsoleKey.Q:
                    Console.WriteLine("Exiting...");
                    return; // ✅ вихід з методу Main

                default:
                    Console.WriteLine("Unknown command.");
                    break;
            }
        }
    }
}
