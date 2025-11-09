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

            // Створюємо CancellationTokenSource, який буде скасовано через 2 секунди
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var token = cts.Token;

            // Act
            // Запускаємо обробку клієнта у фоновому режимі
            var handleTask = _handler.HandleClientAsync(serverClient, token);

            var stream = clientTask.GetStream();
            var message = Encoding.UTF8.GetBytes("ping");

            // 🎯 Виправлення: Виклик WriteAsync з ReadOnlyMemory<byte> та CancellationToken
            await stream.WriteAsync(message.AsMemory(), token);


            byte[] buffer = new byte[1024];

            // Використовуємо ReadAsync з CancellationToken для кращої сумісності
            int bytesRead = await stream.ReadAsync(buffer, token);

            // Assert
            Assert.That(Encoding.UTF8.GetString(buffer, 0, bytesRead), Is.EqualTo("ping"));
            _loggerMock.Verify(l => l.Info(It.Is<string>(s => s.Contains("Echoed"))), Times.AtLeastOnce);

            // Очікуємо завершення обробки клієнта (HandleClientAsync), 
            // оскільки ми відправили та отримали дані.
            // Примітка: HandleClientAsync має завершитися після закриття потоку,
            // або коли токен буде скасовано.
            await handleTask;

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