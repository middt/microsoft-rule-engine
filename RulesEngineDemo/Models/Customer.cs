namespace RulesEngineDemo.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public int Age { get; set; }
        public decimal TotalPurchasesToDate { get; set; }
        public int LoyaltyFactor { get; set; }
        public string MembershipLevel { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public bool IsEmailVerified { get; set; }
        public int YearsAsCustomer => DateTime.Now.Year - RegistrationDate.Year;
    }
}
