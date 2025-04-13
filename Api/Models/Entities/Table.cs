namespace backend.Api.Models.Entities
{
    public class Table
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Number { get; set; }
        public int Capacity { get; set; }
        public TableStatus Status { get; set; }
        public string Location { get; set; }
        public bool IsActive { get; set; }
    }

    public enum TableStatus
    {
        Available,
        Occupied,
        Reserved,
        Unavailable
    }
}