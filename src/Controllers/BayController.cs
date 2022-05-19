using Microsoft.AspNetCore.Mvc;

namespace Baywatch.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class BayController : ControllerBase
    {
        private readonly BayService _bayService;

        private readonly ILogger<BayController> _logger;

        public BayController(ILogger<BayController> logger, BayService bayService)
        {
            _logger = logger;
            _bayService = bayService;
        }

        [HttpGet(Name = "Get")]
        public IEnumerable<Bay> Get()
        {
            return _bayService.Get();
        }

        //[HttpGet(Name = "Test")]
        //public IEnumerable<Bay> Test()
        //{
        //    _bayService.AddOrUpdate(new Bay() { Id = 0, CarId = 10 });
        //    _bayService.AddOrUpdate(new Bay() { Id = 1, CarId = 20 });
        //    _bayService.AddOrUpdate(new Bay() { Id = 2, CarId = 30 });
        //    _bayService.AddOrUpdate(new Bay() { Id = 2, CarId = 30 });
        //    _bayService.AddOrUpdate(new Bay() { Id = 2, CarId = 30 });
        //    _bayService.AddOrUpdate(new Bay() { Id = 2, CarId = 31 });
        //    return this.Get();
        //}
    }
}