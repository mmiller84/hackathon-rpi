namespace Baywatch
{
    public class BayService
    {
        private readonly IList<Bay> bays;

        public BayService()
        {
            this.bays = new List<Bay>();

            for (int i = 0; i < 4; i++)
            {
                this.AddOrUpdate(new Bay() { Id = i });
            }
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

            if(match == null)
            {
                this.bays.Add(bay);
                match = bay;
            }

            match.CarId = bay.CarId;
            match.LastUpdated = DateTime.Now;
        }
    }
}
