using EchoTspServer.Application.Services;
using EchoTspServer.Infrastructure;

class Program
{
    static async Task Main()
    {
        var logger = new ConsoleLogger();
        var handler = new ClientHandler(logger);
        var server = new EchoServer(5000, logger, handler);

        _ = Task.Run(() => server.StartAsync());

        var sender = new UdpTimedSender("127.0.0.1", 60000, logger);
        sender.StartSending(5000);

        Console.WriteLine("Press 'q' to quit...");
        while (Console.ReadKey(intercept: true).Key != ConsoleKey.Q) { }

        sender.StopSending();
        server.Stop();
    }
}
