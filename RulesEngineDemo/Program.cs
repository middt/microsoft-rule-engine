using RulesEngine.Models;

namespace RulesEngineDemo
{
    public class Customer
    {
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public int LoyaltyFactor { get; set; }
        public decimal TotalPurchasesToDate { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string Email { get; set; } = string.Empty;
    }

    // External functions using instance classes (non-static)
    public class BusinessLogic
    {
        private readonly DateTime _currentDate;
        
        public BusinessLogic() : this(DateTime.Now) { }
        
        public BusinessLogic(DateTime currentDate)
        {
            _currentDate = currentDate;
        }

        public bool IsVipCustomer(decimal totalPurchases, int loyaltyFactor)
        {
            return totalPurchases >= 5000 && loyaltyFactor >= 5;
        }

        public bool IsValidEmail(string email)
        {
            return !string.IsNullOrEmpty(email) && email.Contains("@") && email.Contains(".");
        }

        public int GetCustomerAge(DateTime registrationDate)
        {
            return _currentDate.Year - registrationDate.Year;
        }

        public string GetDiscountCategory(decimal totalPurchases)
        {
            return totalPurchases switch
            {
                >= 10000 => "Platinum",
                >= 5000 => "Gold",
                >= 1000 => "Silver",
                _ => "Bronze"
            };
        }

        public bool IsWeekend()
        {
            var dayOfWeek = _currentDate.DayOfWeek;
            return dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday;
        }
    }

    // Alternative: Using interface for dependency injection
    public interface IDiscountService
    {
        bool IsEligibleForDiscount(Customer customer);
        decimal GetDiscountPercentage(Customer customer);
    }

    public class DiscountService : IDiscountService
    {
        public bool IsEligibleForDiscount(Customer customer)
        {
            return customer.TotalPurchasesToDate >= 1000 && customer.LoyaltyFactor >= 2;
        }

        public decimal GetDiscountPercentage(Customer customer)
        {
            return customer.TotalPurchasesToDate switch
            {
                >= 10000 => 30m,
                >= 5000 => 25m,
                >= 2500 => 15m,
                >= 1000 => 10m,
                _ => 0m
            };
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Microsoft Rules Engine Demo - Instance Methods ===");
            Console.WriteLine();

            // Create a simple customer
            var customer = new Customer
            {
                Name = "John Doe",
                Country = "USA",
                LoyaltyFactor = 5,
                TotalPurchasesToDate = 6500,
                RegistrationDate = DateTime.Now.AddYears(-3),
                Email = "john.doe@email.com"
            };

            // Create instance objects for external function calls
            var businessLogic = new BusinessLogic();
            var discountService = new DiscountService();

            // Create rules with instance method calls
            var rules = new List<Rule>
            {
                new Rule
                {
                    RuleName = "VipCustomerDiscount",
                    SuccessEvent = "25% VIP discount applied",
                    ErrorMessage = "Customer not eligible for VIP discount",
                    Expression = "businessLogic.IsVipCustomer(input1.TotalPurchasesToDate, input1.LoyaltyFactor)",
                    RuleExpressionType = RuleExpressionType.LambdaExpression
                },
                new Rule
                {
                    RuleName = "EmailValidationRule",
                    SuccessEvent = "Email is valid",
                    ErrorMessage = "Invalid email format",
                    Expression = "businessLogic.IsValidEmail(input1.Email)",
                    RuleExpressionType = RuleExpressionType.LambdaExpression
                },
                new Rule
                {
                    RuleName = "LoyalCustomerDiscount",
                    SuccessEvent = "15% loyal customer discount applied",
                    ErrorMessage = "Customer not eligible for loyal customer discount",
                    Expression = "businessLogic.GetCustomerAge(input1.RegistrationDate) >= 2 AND businessLogic.GetDiscountCategory(input1.TotalPurchasesToDate) != \"Bronze\"",
                    RuleExpressionType = RuleExpressionType.LambdaExpression
                },
                new Rule
                {
                    RuleName = "WeekendBonus",
                    SuccessEvent = "Weekend bonus 5% applied",
                    ErrorMessage = "Weekend bonus not applicable",
                    Expression = "businessLogic.IsWeekend() AND input1.LoyaltyFactor >= 3",
                    RuleExpressionType = RuleExpressionType.LambdaExpression
                },
                new Rule
                {
                    RuleName = "ServiceBasedDiscount",
                    SuccessEvent = "Service-based discount applied",
                    ErrorMessage = "Not eligible for service-based discount",
                    Expression = "discountService.IsEligibleForDiscount(input1) AND discountService.GetDiscountPercentage(input1) >= 15",
                    RuleExpressionType = RuleExpressionType.LambdaExpression
                }
            };

            // Create workflow
            var workflow = new Workflow
            {
                WorkflowName = "DiscountWorkflow",
                Rules = rules
            };

            // Configure Rules Engine settings to allow external functions
            var reSettings = new ReSettings
            {
                CustomTypes = new[] { typeof(BusinessLogic), typeof(DiscountService) }
            };

            // Initialize Rules Engine with settings and pass instance objects as parameters
            var rulesEngine = new RulesEngine.RulesEngine(new[] { workflow }, reSettings);

            // Display customer info
            Console.WriteLine($"Customer: {customer.Name}");
            Console.WriteLine($"Country: {customer.Country}");
            Console.WriteLine($"Loyalty Factor: {customer.LoyaltyFactor}");
            Console.WriteLine($"Total Purchases: ${customer.TotalPurchasesToDate:N2}");
            Console.WriteLine($"Registration Date: {customer.RegistrationDate:yyyy-MM-dd}");
            Console.WriteLine($"Email: {customer.Email}");
            Console.WriteLine();

            // Display external function results using instances
            Console.WriteLine("External Function Results (Instance Methods):");
            Console.WriteLine($"Is VIP Customer: {businessLogic.IsVipCustomer(customer.TotalPurchasesToDate, customer.LoyaltyFactor)}");
            Console.WriteLine($"Email Valid: {businessLogic.IsValidEmail(customer.Email)}");
            Console.WriteLine($"Customer Age (years): {businessLogic.GetCustomerAge(customer.RegistrationDate)}");
            Console.WriteLine($"Discount Category: {businessLogic.GetDiscountCategory(customer.TotalPurchasesToDate)}");
            Console.WriteLine($"Is Weekend: {businessLogic.IsWeekend()}");
            Console.WriteLine($"Service Eligible: {discountService.IsEligibleForDiscount(customer)}");
            Console.WriteLine($"Service Discount %: {discountService.GetDiscountPercentage(customer)}");
            Console.WriteLine();

            // Execute rules - pass instance objects as named parameters using RuleParameter
            Console.WriteLine("Executing discount rules...");
            
            var ruleParams = new[]
            {
                new RuleParameter("input1", customer),
                new RuleParameter("businessLogic", businessLogic),
                new RuleParameter("discountService", discountService)
            };
            
            var results = await rulesEngine.ExecuteAllRulesAsync("DiscountWorkflow", ruleParams);

            // Display results
            Console.WriteLine("\nRule Execution Results:");
            Console.WriteLine(new string('-', 40));

            foreach (var result in results)
            {
                var status = result.IsSuccess ? "✓ PASSED" : "✗ FAILED";
                var message = result.IsSuccess ? result.Rule.SuccessEvent : result.Rule.ErrorMessage;
                
                Console.WriteLine($"{status} - {result.Rule.RuleName}");
                Console.WriteLine($"   {message}");
                Console.WriteLine();
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}