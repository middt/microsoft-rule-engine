namespace RulesEngineDemo.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsInStock { get; set; }
        public int QuantityInStock { get; set; }
        public DateTime LaunchDate { get; set; }
        public bool IsDiscountEligible { get; set; }
        public string Brand { get; set; } = string.Empty;
    }
}
