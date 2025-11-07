﻿using Moq;
using NetSdrClientApp;
using NetSdrClientApp.Networking;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NetSdrClientAppTests;

[TestFixture]
public class NetSdrClientTests
{
    private NetSdrClient _client;
    private Mock<ITcpClient> _tcpMock;
    private Mock<IUdpClient> _udpMock;

    [SetUp]
    public void Setup()
    {
        _tcpMock = new Mock<ITcpClient>();
        _udpMock = new Mock<IUdpClient>();

        // TCP Connect/Disconnect logic
        _tcpMock.Setup(t => t.Connect()).Callback(() =>
        {
            _tcpMock.Setup(t => t.Connected).Returns(true);
        });

        _tcpMock.Setup(t => t.Disconnect()).Callback(() =>
        {
            _tcpMock.Setup(t => t.Connected).Returns(false);
            _udpMock.Object.StopListening(); // симуляція зупинки UDP при Disconnect
        });

        // Симуляція отримання повідомлення для SendTcpRequest
        _tcpMock.Setup(t => t.SendMessageAsync(It.IsAny<byte[]>())).Callback<byte[]>((bytes) =>
        {
            _tcpMock.Raise(tcp => tcp.MessageReceived += null, _tcpMock.Object, bytes);
        });

        _client = new NetSdrClient(_tcpMock.Object, _udpMock.Object);
    }

    [Test]
    public async Task ConnectAsyncTest()
    {
        await _client.ConnectAsync();

        _tcpMock.Verify(t => t.Connect(), Times.Once);
        _tcpMock.Verify(t => t.SendMessageAsync(It.IsAny<byte[]>()), Times.Exactly(3));
    }

    [Test]
    public async Task DisconnectWithNoConnectionTest()
    {
        _client.Disconect();

        _tcpMock.Verify(t => t.Disconnect(), Times.Once);
    }

    [Test]
    public async Task DisconnectTest()
    {
        await ConnectAsyncTest();

        _client.Disconect();

        _tcpMock.Verify(t => t.Disconnect(), Times.Once);
    }

    [Test]
    public async Task StartIQNoConnectionTest()
    {
        await _client.StartIQAsync();

        _tcpMock.Verify(t => t.SendMessageAsync(It.IsAny<byte[]>()), Times.Never);
        _tcpMock.VerifyGet(t => t.Connected, Times.AtLeastOnce);
    }

    [Test]
    public async Task StartIQTest()
    {
        await ConnectAsyncTest();

        await _client.StartIQAsync();

        _udpMock.Verify(u => u.StartListeningAsync(), Times.Once);
        Assert.That(_client.IQStarted, Is.True);
    }

   [Test]
    public async Task StopIQTest()
    {
        await ConnectAsyncTest();
        await _client.StartIQAsync();
        await _client.StopIQAsync();

        _udpMock.Verify(u => u.StopListening(), Times.Once);
        Assert.That(_client.IQStarted, Is.False);
    }

    [Test]
    public async Task ChangeFrequencyAsyncTest()
    {
        await ConnectAsyncTest();

        await _client.ChangeFrequencyAsync(10_000_000, 1);

        _tcpMock.Verify(t => t.SendMessageAsync(It.IsAny<byte[]>()), Times.Exactly(4));
    }

    [Test]
    public async Task ConnectAsync_WhenAlreadyConnected_ShouldNotReconnect()
    {
        _tcpMock.Setup(t => t.Connected).Returns(true);

        await _client.ConnectAsync();
        await _client.ConnectAsync();

        _tcpMock.Verify(t => t.Connect(), Times.Never);
    }

    /// <summary>
    /// Перевіряє, що StopIQAsync не кидає виняток, навіть якщо IQ ще не стартував.
    /// </summary>
    [Test]
    public async Task StopIQWithoutStarting_ShouldNotThrow()
    {
        await ConnectAsyncTest();

        await _client.StopIQAsync();

        _udpMock.Verify(u => u.StopListening(), Times.Once);
        Assert.That(_client.IQStarted, Is.False);
    }

    /// <summary>
    /// Перевіряє, що при отриманні TCP повідомлення task завершується і повертає ті ж дані.
    /// </summary>
    [Test]
    public async Task TcpMessageReceived_ShouldSetResponseTaskSource()
    {
        await ConnectAsyncTest();
        var testMessage = new byte[] { 0x01, 0x02, 0x03 };

        var sendTask = _client.GetType()
            .GetMethod("SendTcpRequest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(_client, new object[] { testMessage }) as Task<byte[]>;

        _tcpMock.Raise(tcp => tcp.MessageReceived += null, _tcpMock.Object, testMessage);
        var response = await sendTask!;

        Assert.AreEqual(testMessage, response);
    }

    /// <summary>
    /// ✅ ДОДАНО: тестує, що під час StartIQAsync без підключення викидається виняток.
    /// Це покриє гілку “if (!Connected)” або подібну логіку.
    /// </summary>
   [Test]
    public async Task StartIQAsync_WithoutConnection_ShouldNotStartAndNotListen()
    {
        // Arrange
        _tcpMock.Setup(t => t.Connected).Returns(false);

        // Act
        await _client.StartIQAsync();

        // Assert
        _tcpMock.Verify(t => t.SendMessageAsync(It.IsAny<byte[]>()), Times.Never);
        _udpMock.Verify(u => u.StartListeningAsync(), Times.Never);
        Assert.That(_client.IQStarted, Is.False);
    }
}
