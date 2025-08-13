# Microsoft Rules Engine Demo with Instance Methods

A .NET Core console application demonstrating the [Microsoft Rules Engine](https://github.com/microsoft/RulesEngine) with external function calls using **instance methods** (non-static classes).

## Overview

This demo shows how to:
- Create rules with hardcoded expressions
- Use **instance methods** in rule expressions (no static classes required)
- Pass instance objects as named parameters to Rules Engine
- Configure Rules Engine with custom types
- Use dependency injection patterns with Rules Engine
- Execute rules against input data
- Display rule results

## What it does

The demo creates a customer with sample data and tests several rules using external functions:
1. **VIP Customer Discount**: 25% discount using `BusinessLogic.IsVipCustomer()` function
2. **Email Validation**: Validates email format using `BusinessLogic.IsValidEmail()` function
3. **Loyal Customer Discount**: 15% discount using multiple external functions
4. **Weekend Bonus**: 5% bonus using `BusinessLogic.IsWeekend()` function

## Running the Demo

```bash
cd RulesEngineDemo
dotnet run
```

## Sample Output

```
=== Microsoft Rules Engine Demo - Instance Methods ===

Customer: John Doe
Country: USA
Loyalty Factor: 5
Total Purchases: $6,500.00
Registration Date: 2021-01-15
Email: john.doe@email.com

External Function Results (Instance Methods):
Is VIP Customer: True
Email Valid: True
Customer Age (years): 3
Discount Category: Gold
Is Weekend: False
Service Eligible: True
Service Discount %: 25

Executing discount rules...

Rule Execution Results:
----------------------------------------
✓ PASSED - VipCustomerDiscount
   25% VIP discount applied

✓ PASSED - EmailValidationRule
   Email is valid

✓ PASSED - LoyalCustomerDiscount
   15% loyal customer discount applied

✗ FAILED - WeekendBonus
   Weekend bonus not applicable

✓ PASSED - ServiceBasedDiscount
   Service-based discount applied

Press any key to exit...
```

## Key Components

### Customer Model
```csharp
public class Customer
{
    public string Name { get; set; }
    public string Country { get; set; }
    public int LoyaltyFactor { get; set; }
    public decimal TotalPurchasesToDate { get; set; }
    public DateTime RegistrationDate { get; set; }
    public string Email { get; set; }
}
```

### Instance-Based Business Logic (Non-Static)
```csharp
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

// Service with interface for dependency injection
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
```

### Rules with External Function Calls
```csharp
var rules = new List<Rule>
{
    new Rule
    {
        RuleName = "VipCustomerDiscount",
        Expression = "BusinessLogic.IsVipCustomer(input1.TotalPurchasesToDate, input1.LoyaltyFactor)",
        SuccessEvent = "25% VIP discount applied",
        ErrorMessage = "Customer not eligible for VIP discount",
        RuleExpressionType = RuleExpressionType.LambdaExpression
    },
    new Rule
    {
        RuleName = "LoyalCustomerDiscount",
        Expression = "BusinessLogic.GetCustomerAge(input1.RegistrationDate) >= 2 AND BusinessLogic.GetDiscountCategory(input1.TotalPurchasesToDate) != \"Bronze\"",
        SuccessEvent = "15% loyal customer discount applied",
        ErrorMessage = "Customer not eligible for loyal customer discount",
        RuleExpressionType = RuleExpressionType.LambdaExpression
    }
};
```

### Instance Creation and Rules Engine Configuration
```csharp
// Create instance objects
var businessLogic = new BusinessLogic();
var discountService = new DiscountService();

// Configure Rules Engine with custom types
var reSettings = new ReSettings
{
    CustomTypes = new[] { typeof(BusinessLogic), typeof(DiscountService) }
};

var rulesEngine = new RulesEngine.RulesEngine(new[] { workflow }, reSettings);

// Pass instances as named parameters using RuleParameter
var ruleParams = new[]
{
    new RuleParameter("input1", customer),
    new RuleParameter("businessLogic", businessLogic),
    new RuleParameter("discountService", discountService)
};

var results = await rulesEngine.ExecuteAllRulesAsync("DiscountWorkflow", ruleParams);
```

## Key Features Demonstrated

### External Function Calls
- **Simple Functions**: `IsVipCustomer()`, `IsValidEmail()`, `IsWeekend()`
- **Complex Logic**: `GetDiscountCategory()` with switch expressions
- **Date Operations**: `GetCustomerAge()` with DateTime calculations
- **Combined Expressions**: Using multiple functions in single rule expressions

### Benefits of Instance Methods (vs Static Classes)
- **Dependency Injection**: Support for DI containers and IoC patterns
- **State Management**: Can maintain instance state and configuration
- **Testability**: Easier mocking and unit testing with interfaces
- **Flexibility**: Constructor injection for configuration and dependencies
- **Thread Safety**: Each instance can maintain its own state
- **Reusable Logic**: Methods can be used across multiple rules
- **Complex Calculations**: Move sophisticated logic out of rule expressions
- **Maintainable Rules**: Keep rule expressions clean and readable

### Key Differences from Static Methods
1. **Instance Creation**: Must create instances before passing to Rules Engine
2. **Parameter Passing**: Use `RuleParameter` objects with named parameters
3. **Dependency Injection**: Can inject dependencies through constructors
4. **State Preservation**: Instance variables persist across rule evaluations
5. **Interface Support**: Can implement interfaces for better abstraction

## Learn More

- [Microsoft Rules Engine Repository](https://github.com/microsoft/RulesEngine)
- [Official Documentation](https://microsoft.github.io/RulesEngine/)
- [NuGet Package](https://www.nuget.org/packages/RulesEngine/)