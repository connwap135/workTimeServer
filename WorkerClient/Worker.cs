using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerClient
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly Socket mySocket;
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                IPEndPoint ipep = new IPEndPoint(IPAddress.Loopback, 39169);

                var sendbuf = new byte[] { 0xf3 };
                mySocket.SendTo(sendbuf, sendbuf.Length, SocketFlags.None, ipep);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
