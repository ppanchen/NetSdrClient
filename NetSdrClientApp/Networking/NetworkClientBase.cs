using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetSdrClientApp.Networking
{
    public abstract class NetworkClientBase
    {
        protected CancellationTokenSource? _cts;
        public event EventHandler<byte[]>? MessageReceived;

        protected void OnMessageReceived(byte[] data)
        {
            MessageReceived?.Invoke(this, data);
        }

        protected void Log(string message)
        {
            Console.WriteLine(message);
        }

        public virtual void Stop()
        {
            try
            {
                _cts?.Cancel();
                Log("Stopped network client.");
            }
            catch (Exception ex)
            {
                Log($"Error while stopping: {ex.Message}");
            }
        }

        protected void LogBytes(string prefix, byte[] data)
        {
            var hex = BitConverter.ToString(data).Replace("-", " ");
            Log($"{prefix}: {hex}");
        }
    }
}
