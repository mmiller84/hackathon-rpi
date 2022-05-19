namespace Baywatch
{
    public class BayMonitor : BackgroundService
    {
        private const int READ_INTERVAL_MS = 50;
        private const int CLEAN_INTERVAL_MS = 2000;
        private const int IR_ID_SCAN_CODE = 4;

        private readonly BayService _bayService;
        private readonly ILogger<BayMonitor> _logger;

        public BayMonitor(BayService bayService, ILogger<BayMonitor> logger)
        {
            _bayService = bayService ?? throw new ArgumentNullException(nameof(bayService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var tasks = new List<Task>()
            {
                this.ReadDevice("/dev/input/event0", stoppingToken),
                this.ReadDevice("/dev/input/event1", stoppingToken),
                this.ReadDevice("/dev/input/event2", stoppingToken),
                this.ReadDevice("/dev/input/event3", stoppingToken),
                this.Clean(stoppingToken),
            };

            await Task.WhenAny(tasks);
        }

        private async Task Clean(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach(var bay in _bayService.Get())
                {
                    var timeSinceLastUpdate = DateTime.Now - bay.LastUpdated;
                    if(bay.CarId > 0 && timeSinceLastUpdate.TotalMilliseconds > CLEAN_INTERVAL_MS)
                    {
                        _logger.LogInformation($"Car {bay.CarId} has left bay {bay.Id}");
                        bay.CarId = 0;
                    }
                }

                await Task.Delay(500);
            }
        }

        private async Task ReadDevice(string file, CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Monitoring device {file}...");

            var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            byte[] buffer = new byte[24];
            int value = 0;
            int code = 0;

            while(!stoppingToken.IsCancellationRequested) {
                var result = await stream.ReadAsync(buffer, 0, buffer.Length, stoppingToken);
                if(result == buffer.Length) { 

                    // start after byte 16 to skip timeval since we don't actually need it
                    //short type = BitConverter.ToInt16(new byte[] { buffer[offset], buffer[++offset] }, 0);
                    int offset = 17;

                    code = BitConverter.ToInt16(new byte[] { buffer[++offset], buffer[++offset] }, 0);
                    value = BitConverter.ToInt32(new byte[] { buffer[++offset], buffer[++offset], buffer[++offset], buffer[++offset] }, 0);


                    if (code == IR_ID_SCAN_CODE && value != 2147483647)
                    {
                        //_logger.LogInformation($"Read code {code} and value {value} on device {file}");

                        var bay = new Bay()
                        {
                            Id = int.Parse(file.LastOrDefault().ToString()),
                            CarId = value, //???
                        };

                        _logger.LogInformation($"Car {bay.CarId} is in Bay {bay.Id}");
                        _bayService.AddOrUpdate(bay);
                    }
                }
                await Task.Delay(READ_INTERVAL_MS);
            }
        }
    }
}
 