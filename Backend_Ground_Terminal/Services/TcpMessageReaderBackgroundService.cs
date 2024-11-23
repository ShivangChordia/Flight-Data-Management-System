using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Ground_Terminal_Management_System.Services
{
    public class TcpMessageReaderBackgroundService : BackgroundService
    {
        private readonly TcpMessageReaderService _tcpMessageReaderService;

        public TcpMessageReaderBackgroundService(TcpMessageReaderService tcpMessageReaderService)
        {
            _tcpMessageReaderService = tcpMessageReaderService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _tcpMessageReaderService.StartAsync(stoppingToken);
        }
    }
}
