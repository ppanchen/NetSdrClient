using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq; // потрібно для .Concat

/// <summary>
/// This program was designed for test purposes only
/// Not for a review
/// </summary>
namespace TestServerApp
{
    public class EchoServer
    {
        private readonly int _port;
        private TcpListener _listener;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public EchoServer(int port)
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
                    var client = await _listener.AcceptTcpClientAsync();
                    Console.WriteLine("Client connected.");
                    _ = Task.Run(() => HandleClientAsync(client, _cancellationTokenSource.Token));
                }
                catch (ObjectDisposedException)
                {
                    // Listener has been closed
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Accept error: {ex.Message}");
                }
            }

            Console.WriteLine("Server shutdown.");
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            using (client)
            using (var stream = client.GetStream())
            {
                try
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead;

                    while (!token.IsCancellationRequested &&
                           (bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                    {
                        // Echo back the received message
                        await stream.WriteAsync(buffer, 0, bytesRead, token);
                        Console.WriteLine($"Echoed {bytesRead} bytes to the client.");
                    }
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    Console.WriteLine($"Client error: {ex.Message}");
                }
                finally
                {
                    Console.WriteLine("Client disconnected.");
                }
            }
        }

        public void Stop()
        {
            try
            {
                _cancellationTokenSource.Cancel();
                _listener?.Stop();
                Console.WriteLine("Server stopped.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping server: {ex.Message}");
            }
        }

        public static async Task Main(string[] args)
        {
            var server = new EchoServer(5000);

            // Start the server
            _ = Task.Run(() => server.StartAsync());

            string host = "127.0.0.1"; // Target IP
            int port = 60000;          // Target Port
            int intervalMilliseconds = 5000; // Send every 5 seconds

            using (var sender = new UdpTimedSender(host, port))
            {
                Console.WriteLine("Press any key to start sending...");
                Console.ReadKey(intercept: true);

                sender.StartSending(intervalMilliseconds);

                Console.WriteLine("Press 'q' to quit...");
                while (Console.ReadKey(intercept: true).Key != ConsoleKey.Q)
                {
                    // wait for quit
                }

                sender.StopSending();
                server.Stop();
                Console.WriteLine("Sender stopped.");
            }

            await Task.Delay(500); // дати трохи часу на завершення
        }
    }

    public class UdpTimedSender : IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private readonly UdpClient _udpClient;
        private Timer _timer;
        private ushort _counter = 0;
        private readonly Random _random = new Random();

        public UdpTimedSender(string host, int port)
        {
            _host = host;
            _port = port;
            _udpClient = new UdpClient();
        }

        public void StartSending(int intervalMilliseconds)
        {
            if (_timer != null)
                throw new InvalidOperationException("Sender is already running.");

            _timer = new Timer(SendMessageCallback, null, 0, intervalMilliseconds);
        }

        private void SendMessageCallback(object state)
        {
            try
            {
                byte[] samples = new byte[1024];
                _random.NextBytes(samples);
                _counter++;

                byte[] header = new byte[] { 0x04, 0x84 };
                byte[] counterBytes = BitConverter.GetBytes(_counter);
                byte[] msg = header.Concat(counterBytes).Concat(samples).ToArray();

                var endpoint = new IPEndPoint(IPAddress.Parse(_host), _port);
                _udpClient.Send(msg, msg.Length, endpoint);
                Console.WriteLine($"Message sent to {_host}:{_port}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }

        public void StopSending()
        {
            _timer?.Dispose();
            _timer = null;
        }

        public void Dispose()
        {
            StopSending();
            _udpClient.Dispose();
        }
    }
}
