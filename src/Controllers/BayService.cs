namespace Baywatch
{
    public class BayService
    {
        private readonly IList<Bay> bays;

        public BayService()
        {
            this.bays = new List<Bay>();
        }

        public IEnumerable<Bay> Get()
        {
            return bays;
        }

        public void Delete(Bay bay)
        {
            this.bays.Remove(bay);
        }

        public void AddOrUpdate(Bay bay)
        {
            var match = this.bays.FirstOrDefault(x => x.Id == bay.Id);

            if (match != null)
            {
                match.CarId = bay.CarId;
            }
            else
            {
                this.bays.Add(bay);
            }
        }
    }

    public class BayMonitor : BackgroundService
    {
        private const int REFRESH_INTERVAL_MS = 100;
        private readonly BayService _bayService;
        private readonly ILogger<BayMonitor> _logger;

        public BayMonitor(BayService bayService, ILogger<BayMonitor> logger)
        {
            _bayService = bayService ?? throw new ArgumentNullException(nameof(bayService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                this.UpdateState();

                Thread.Sleep(REFRESH_INTERVAL_MS);
            }

            return Task.CompletedTask;
        }

        private void UpdateState()
        {
            const string INPUT_FOLDER = "/dev/input";
            const string INPUT_FORMAT = "event*";

            // check for files at /dev/input/eventX where X is 0,1,2,etc
            if(Directory.Exists(INPUT_FOLDER))
            {
                var files = Directory.GetFiles(INPUT_FOLDER, INPUT_FORMAT);
                foreach (var file in files)
                {
                    var bay = this.Parse(file);
                    _bayService.AddOrUpdate(bay);
                }
            }
        }
        private Bay Parse(string file)
        {

            var input = File.ReadAllBytes(file);

            // const int TIME_OFFSET = 0;
            //var time = BitConverter.ToInt16(input.Skip(TIME_OFFSET).Take(16).ToArray());

            // const int TYPE_OFFSET = 16;
            //var type = BitConverter.ToInt16(input.Skip(TYPE_OFFSET).Take(2).ToArray());

            // const int CODE_OFFSET = 18;
            //var code = BitConverter.ToInt16(input.Skip(CODE_OFFSET).Take(2).ToArray());

            const int VALUE_OFFSET = 20;
            var value = BitConverter.ToInt32(input.Skip(VALUE_OFFSET).Take(4).ToArray());

            var bay = new Bay()
            {
                Id = int.Parse(file.LastOrDefault().ToString()),
                CarId = value, //???
            };

            return bay;
        }
    }
}
