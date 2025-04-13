namespace backend.Api.Models.Entities
{
    public class OrderItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string OrderId { get; set; }
        public Order Order { get; set; }
        public string MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        public List<string> Customizations { get; set; } = new List<string>();
        public string SpecialInstructions { get; set; }
    }
}