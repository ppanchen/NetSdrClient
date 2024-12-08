using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class EchoTcpServer
{
    private readonly int _port;
    private TcpListener _listener;
    private CancellationTokenSource _cancellationTokenSource;

    public EchoTcpServer(int port)
    {
        _port = port;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public async Task StartAsync()
    {
        _listener = new TcpListener(IPAddress.Any, _port);
        _listener.Start();
        Console.WriteLine($"Server started on port {_port}.");

        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                Console.WriteLine("Client connected.");

                _ = Task.Run(() => HandleClientAsync(client, _cancellationTokenSource.Token));
            }
            catch (ObjectDisposedException)
            {
                // Listener has been closed
                break;
            }
        }

        Console.WriteLine("Server shutdown.");
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken token)
    {
        using (NetworkStream stream = client.GetStream())
        {
            try
            {
                byte[] buffer = new byte[8192];
                int bytesRead;

                while (!token.IsCancellationRequested && (bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                {
                    // Echo back the received message
                    await stream.WriteAsync(buffer, 0, bytesRead, token);
                    Console.WriteLine($"Echoed {bytesRead} bytes to the client.");
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                client.Close();
                Console.WriteLine("Client disconnected.");
            }
        }
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        _listener.Stop();
        _cancellationTokenSource.Dispose();
        Console.WriteLine("Server stopped.");
    }

    public static async Task Main(string[] args)
    {
        EchoTcpServer server = new EchoTcpServer(8080);

        // Start the server in a separate task
        _ = Task.Run(() => server.StartAsync());

        // Wait for user input to stop the server
        Console.WriteLine("Press 'q' to quit...");
        while (Console.ReadKey(intercept: true).Key != ConsoleKey.Q)
        {
            // Just wait until 'q' is pressed
        }

        // Stop the server
        server.Stop();
    }
}