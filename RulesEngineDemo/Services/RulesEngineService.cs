using System.Text.Json;
using RulesEngine.Models;
using RulesEngineDemo.Models;

namespace RulesEngineDemo.Services
{
    public class RulesEngineService
    {
        private readonly RulesEngine.RulesEngine _rulesEngine;

        public RulesEngineService()
        {
            var workflows = LoadWorkflows();
            _rulesEngine = new RulesEngine.RulesEngine(workflows.ToArray());
        }

        private List<Workflow> LoadWorkflows()
        {
            var workflows = new List<Workflow>();
            var rulesPath = Path.Combine(Directory.GetCurrentDirectory(), "Rules");

            // Load all rule files
            var ruleFiles = new[]
            {
                "discount-rules.json",
                "eligibility-rules.json", 
                "validation-rules.json"
            };

            foreach (var file in ruleFiles)
            {
                var filePath = Path.Combine(rulesPath, file);
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    var workflowsFromFile = JsonSerializer.Deserialize<List<Workflow>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    if (workflowsFromFile != null)
                    {
                        workflows.AddRange(workflowsFromFile);
                    }
                }
            }

            return workflows;
        }

        public async Task<List<RuleResultTree>> ExecuteDiscountRulesAsync(Customer customer)
        {
            var inputs = new dynamic[] { customer };
            return await _rulesEngine.ExecuteAllRulesAsync("DiscountRules", inputs);
        }

        public async Task<List<RuleResultTree>> ExecuteEligibilityRulesAsync(object input)
        {
            var inputs = new dynamic[] { input };
            return await _rulesEngine.ExecuteAllRulesAsync("EligibilityRules", inputs);
        }

        public async Task<List<RuleResultTree>> ExecuteValidationRulesAsync(object input)
        {
            var inputs = new dynamic[] { input };
            return await _rulesEngine.ExecuteAllRulesAsync("ValidationRules", inputs);
        }
    }
}
