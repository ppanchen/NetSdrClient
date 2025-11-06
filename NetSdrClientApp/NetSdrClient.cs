using NetSdrClientApp.Messages;
using NetSdrClientApp.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NetSdrClientApp
{
    public class NetSdrClient
    {
        private readonly ITcpClient _tcpClient;
        private readonly IUdpClient _udpClient;
        private TaskCompletionSource<byte[]>? responseTaskSource;

        public bool IQStarted { get; private set; }

        public NetSdrClient(ITcpClient tcpClient, IUdpClient udpClient)
        {
            _tcpClient = tcpClient;
            _udpClient = udpClient;

            _tcpClient.MessageReceived += _tcpClient_MessageReceived;
            _udpClient.MessageReceived += _udpClient_MessageReceived;
        }

        public async Task ConnectAsync()
        {
            if (!_tcpClient.Connected)
            {
                _tcpClient.Connect();

                var sampleRate = BitConverter.GetBytes((long)100000).Take(5).ToArray();
                var automaticFilterMode = BitConverter.GetBytes((ushort)0).ToArray();
                var adMode = new byte[] { 0x00, 0x03 };

                // Host pre setup
                var msgs = new List<byte[]>
                {
                    NetSdrMessageHelper.GetControlItemMessage(MsgTypes.SetControlItem, ControlItemCodes.IQOutputDataSampleRate, sampleRate),
                    NetSdrMessageHelper.GetControlItemMessage(MsgTypes.SetControlItem, ControlItemCodes.RFFilter, automaticFilterMode),
                    NetSdrMessageHelper.GetControlItemMessage(MsgTypes.SetControlItem, ControlItemCodes.ADModes, adMode),
                };

                foreach (var msg in msgs)
                {
                    await SendTcpRequest(msg);
                }

                Console.WriteLine("Connected to SDR host.");
            }
        }

        public void Disconnect()
        {
            _tcpClient.Disconnect();
            Console.WriteLine("Disconnected from SDR host.");
        }

        public async Task StartIQAsync()
        {
            if (!_tcpClient.Connected)
            {
                Console.WriteLine("No active connection.");
                return;
            }

            var iqDataMode = (byte)0x80;
            var start = (byte)0x02;
            var fifo16bitCaptureMode = (byte)0x01;
            var n = (byte)1;

            var args = new[] { iqDataMode, start, fifo16bitCaptureMode, n };

            var msg = NetSdrMessageHelper.GetControlItemMessage(
                MsgTypes.SetControlItem,
                ControlItemCodes.ReceiverState,
                args);

            await SendTcpRequest(msg);

            IQStarted = true;
            _ = _udpClient.StartListeningAsync();

            Console.WriteLine("IQ stream started.");
        }

        public async Task StopIQAsync()
        {
            if (!_tcpClient.Connected)
            {
                Console.WriteLine("No active connection.");
                return;
            }

            var stop = (byte)0x01;
            var args = new byte[] { 0, stop, 0, 0 };

            var msg = NetSdrMessageHelper.GetControlItemMessage(
                MsgTypes.SetControlItem,
                ControlItemCodes.ReceiverState,
                args);

            await SendTcpRequest(msg);

            IQStarted = false;
            _udpClient.StopListening();

            Console.WriteLine("IQ stream stopped.");
        }

        public async Task ChangeFrequencyAsync(long hz, int channel)
        {
            var channelArg = (byte)channel;
            var frequencyArg = BitConverter.GetBytes(hz).Take(5);
            var args = new[] { channelArg }.Concat(frequencyArg).ToArray();

            var msg = NetSdrMessageHelper.GetControlItemMessage(
                MsgTypes.SetControlItem,
                ControlItemCodes.ReceiverFrequency,
                args);

            await SendTcpRequest(msg);
            Console.WriteLine($"Frequency changed to {hz} Hz (channel {channel}).");
        }

        private void _udpClient_MessageReceived(object? sender, byte[] e)
        {
            NetSdrMessageHelper.TranslateMessage(e, out MsgTypes type, out ControlItemCodes code, out ushort sequenceNum, out byte[] body);
            var samples = NetSdrMessageHelper.GetSamples(16, body);

            Console.WriteLine($"Samples received: {string.Join(' ', body.Select(b => b.ToString("X2")))}");

            using (FileStream fs = new FileStream("samples.bin", FileMode.Append, FileAccess.Write, FileShare.Read))
            using (BinaryWriter sw = new BinaryWriter(fs))
            {
                foreach (var sample in samples)
                {
                    sw.Write((short)sample); // write 16-bit samples
                }
            }
        }

        private async Task<byte[]> SendTcpRequest(byte[] msg)
        {
            if (!_tcpClient.Connected)
            {
                Console.WriteLine("No active connection.");
                return Array.Empty<byte>();
            }

            responseTaskSource = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
            await _tcpClient.SendMessageAsync(msg);

            try
            {
                var resp = await responseTaskSource.Task;
                return resp ?? Array.Empty<byte>();
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("TCP request timed out.");
                return Array.Empty<byte>();
            }
        }

        private void _tcpClient_MessageReceived(object? sender, byte[] e)
        {
            // Handle only expected responses
            if (responseTaskSource != null && !responseTaskSource.Task.IsCompleted)
            {
                responseTaskSource.SetResult(e);
                responseTaskSource = null;
            }

            Console.WriteLine("Response received: " + string.Join(' ', e.Select(b => b.ToString("X2"))));
        }
    }
}
