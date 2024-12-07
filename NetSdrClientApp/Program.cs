using NetSdrClientApp;

var cts = new CancellationTokenSource();

Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

var client = new TcpClientWrapper("127.0.0.1", 8080);

Task listeningTask = Task.CompletedTask;

try
{
    client.Connect();

    client.MessageReceived += (s, m) =>
    {
        foreach (var item in m.Select(b => Convert.ToString(b, toBase: 8)))
        {
            Console.Write(item + " ");
        }
        Console.WriteLine();
    };

    listeningTask = client.StartListeningAsync(cts.Token);
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

var loop  = Task.Run(async () =>
{
    while (true)
    {
        var key = Console.ReadKey(intercept: true).Key;
        if (key == ConsoleKey.M)
        {
            await client.SendMessageAsync(new byte[] { 0, 1 }, cts.Token);
        }
        else if (key == ConsoleKey.Q)
        {
            break;
        }
    }
});

await listeningTask;
client.Disconnect();
