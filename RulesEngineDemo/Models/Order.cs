namespace RulesEngineDemo.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public Customer Customer { get; set; } = new Customer();
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public decimal TotalAmount => Items.Sum(item => item.TotalPrice);
        public DateTime OrderDate { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public bool IsUrgentDelivery { get; set; }
        public int ItemCount => Items.Count;
    }

    public class OrderItem
    {
        public Product Product { get; set; } = new Product();
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}
