using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NetSdrClientApp
{
    public interface ITcpClient
    {
        void Connect();
        void Disconnect();
        Task SendMessageAsync(byte[] data, CancellationToken cancellationToken);
        Task StartListeningAsync(CancellationToken cancellationToken);

        event EventHandler<byte[]> MessageReceived;
    }
}
