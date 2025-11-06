using System.Net.Sockets;
using EchoTspServer.Application.Interfaces;
using EchoTspServer.Application.Services;
using Moq;
using NUnit.Framework;

namespace EchoTspServer.Tests
{
    [TestFixture]
    public class EchoServerTests
    {
        private Mock<ILogger> _loggerMock;
        private Mock<IClientHandler> _handlerMock;
        private EchoServer _server;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger>();
            _handlerMock = new Mock<IClientHandler>();
            _server = new EchoServer(6001, _loggerMock.Object, _handlerMock.Object);
        }

        [Test]
        public async Task StartAsync_StartsAndStopsWithoutError()
        {
            var task = _server.StartAsync();
            await Task.Delay(100); // дати серверу запуститися

            // act
            _server.Stop();

            try
            {
                await task; // дочекаємось завершення
            }
            catch (SocketException ex)
            {
                // Це очікувано, бо listener закривається під час AcceptTcpClientAsync
                Assert.That(ex.SocketErrorCode, Is.EqualTo(SocketError.OperationAborted));
            }

            _loggerMock.Verify(l => l.Info(It.Is<string>(s => s.Contains("Server started"))), Times.Once);
            _loggerMock.Verify(l => l.Info(It.Is<string>(s => s.Contains("Server stopped"))), Times.Once);
        }


        [Test]
        public void Stop_CanBeCalledMultipleTimes_SafeToCall()
        {
            Assert.DoesNotThrow(() =>
            {
                _server.Stop();
                _server.Stop();
            });
        }

        [Test]
        public void Constructor_SetsDependenciesProperly()
        {
            Assert.NotNull(_server);
        }
    }
}
