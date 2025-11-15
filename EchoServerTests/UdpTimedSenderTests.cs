using EchoTspServer.Application.Interfaces;
using EchoTspServer.Application.Services;
using Moq;
using NUnit.Framework;

namespace EchoTspServer.Tests
{
    [TestFixture]
    public class UdpTimedSenderTests
    {
        private Mock<ILogger> _loggerMock;
        private UdpTimedSender _sender;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger>();
            _sender = new UdpTimedSender("127.0.0.1", 9999, _loggerMock.Object);
        }

        [TearDown]
        public void Cleanup()
        {
            _sender.Dispose();
        }

        [Test]
        public void StartSending_StartsAndStopsCorrectly()
        {
            _sender.StartSending(100);
            Assert.DoesNotThrow(() => _sender.StopSending());
        }

        [Test]
        public void StartSending_WhenAlreadyRunning_Throws()
        {
            _sender.StartSending(100);
            Assert.Throws<InvalidOperationException>(() => _sender.StartSending(100));
        }

        [Test]
        public void Dispose_StopsAndDisposesUdpClient()
        {
            Assert.DoesNotThrow(() => _sender.Dispose());
        }

        [Test]
        public void SendMessage_LogsInfoOnSuccess()
        {
            // simulate one interval
            _sender.StartSending(10);
            Thread.Sleep(30);
            _sender.StopSending();

            _loggerMock.Verify(l => l.Info(It.Is<string>(s => s.Contains("Sent UDP packet"))), Times.AtLeastOnce);
        }
    }
}
