using System;
using System.Threading.Tasks;

namespace NetSdrClientApp.Networking
{﻿
    public interface IUdpClient
    {
        event EventHandler<byte[]>? MessageReceived;
    
        Task StartListeningAsync();
    
        void StopListening();
        void Exit();
    }
}
