using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using NetSdrClientApp;
using NetSdrClientApp.Networking;

namespace AppTests
{
    [TestFixture]
    public class NetSdrClientUserTests
    {
        private Mock<ITcpClient> _tcpMock;
        private Mock<IUdpClient> _udpMock;
        private NetSdrClient _client;

        [SetUp]
        public void Setup()
        {
            _tcpMock = new Mock<ITcpClient>();
            _udpMock = new Mock<IUdpClient>();

            _tcpMock.Setup(t => t.Connected).Returns(true);

            _client = new NetSdrClient(_tcpMock.Object, _udpMock.Object);
        }

        [Test]
        public async Task ConnectAsync_ShouldCallTcpConnect()
        {
            _tcpMock.Setup(t => t.Connected).Returns(false);

            await _client.ConnectAsync();

            _tcpMock.Verify(t => t.Connect(), Times.Once);
        }

        [Test]
        public async Task ConnectAsync_WhenAlreadyConnected_ShouldNotReconnect()
        {
            _tcpMock.Setup(t => t.Connected).Returns(true);

            await _client.ConnectAsync();
            await _client.ConnectAsync();

            _tcpMock.Verify(t => t.Connect(), Times.Never);
        }

        [Test]
        public async Task ChangeFrequencyAsync_ShouldSendTcpMessage()
        {
            await _client.ConnectAsync();
            await _client.ChangeFrequencyAsync(10_000_000, 1);

            _tcpMock.Verify(t => t.SendMessageAsync(It.IsAny<byte[]>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task StartIQ_ShouldStartUdpListening()
        {
            await _client.ConnectAsync();
            await _client.StartIQAsync();

            _udpMock.Verify(u => u.StartListeningAsync(), Times.Once);
        }

        [Test]
        public async Task StopIQ_ShouldStopUdpListening()
        {
            await _client.ConnectAsync();
            await _client.StartIQAsync();
            await _client.StopIQAsync();

            _udpMock.Verify(u => u.StopListening(), Times.Once);
        }

        [Test]
        public async Task Disconnect_ShouldCloseTcpAndUdp()
        {
            await _client.ConnectAsync();
            _client.Disconect();

            _tcpMock.Verify(t => t.Disconnect(), Times.Once);
            _udpMock.Verify(u => u.StopListening(), Times.Once);
        }
    }
}
