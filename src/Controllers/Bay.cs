namespace Baywatch
{
    public class Bay
    {
        public int Id { get; set; }

        public int CarId { get; set; }
        public DateTime LastUpdated { get; internal set; }
    }
}