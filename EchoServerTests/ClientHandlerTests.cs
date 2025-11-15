using System.Net.Sockets;
using System.Text;
using EchoTspServer.Application.Interfaces;
using EchoTspServer.Application.Services;
using Moq;
using NUnit.Framework;

namespace EchoTspServer.Tests
{
    [TestFixture]
    public class ClientHandlerTests
    {
        private Mock<ILogger> _loggerMock;
        private ClientHandler _handler;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger>();
            _handler = new ClientHandler(_loggerMock.Object);
        }

        [Test]
        public async Task HandleClientAsync_EchoesDataBack()
        {
            // Arrange: створимо два з'єднані TCP сокети
            using var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
            listener.Start();
            int port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;

            var clientTask = new TcpClient();
            var connectTask = clientTask.ConnectAsync("127.0.0.1", port);

            var serverClient = await listener.AcceptTcpClientAsync();
            await connectTask;
            listener.Stop();

            var token = new CancellationTokenSource(TimeSpan.FromSeconds(2)).Token;

            // Act
            var handleTask = _handler.HandleClientAsync(serverClient, token);

            var stream = clientTask.GetStream();
            var message = Encoding.UTF8.GetBytes("ping");
            await stream.WriteAsync(message, 0, message.Length);

            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

            // Assert
            Assert.That(Encoding.UTF8.GetString(buffer, 0, bytesRead), Is.EqualTo("ping"));
            _loggerMock.Verify(l => l.Info(It.Is<string>(s => s.Contains("Echoed"))), Times.AtLeastOnce);

            serverClient.Close();
            clientTask.Close();
        }

        //[Test]
        //public async Task HandleClientAsync_HandlesException_LogsError()
        //{
        //    // Arrange
        //    var fakeClient = new Mock<TcpClient>();
        //    fakeClient.Setup(c => c.GetStream()).Throws(new Exception("fake fail"));

        //    // Act
        //    await _handler.HandleClientAsync(fakeClient.Object, CancellationToken.None);

        //    // Assert
        //    _loggerMock.Verify(l => l.Error(It.Is<string>(s => s.Contains("fake fail"))), Times.Once);
        //}
    }
}
