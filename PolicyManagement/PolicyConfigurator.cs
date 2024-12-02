using System.Collections.Generic;
using System.Linq;
using Techolics_.Logging;

namespace Techolics_.PolicyManagement
{
    public class PolicyConfigurator
    {
        private PolicyExplorerWindow policyExplorer;
        private CISBenchmark benchmarkValues;
        private CISBenchmarkDocumentation benchmarkDocumentation;

        private SeceditManager seceditManager;
        private RegistryManager registryManager;
        private GroupPolicyManager groupPolicyManager;

        public PolicyConfigurator(
            PolicyExplorerWindow policyExplorer,
            CISBenchmark benchmarkValues,
            CISBenchmarkDocumentation benchmarkDocumentation
        )
        {
            this.policyExplorer = policyExplorer;
            this.benchmarkValues = benchmarkValues;
            this.benchmarkDocumentation = benchmarkDocumentation;

            // Initialize managers
            seceditManager = new SeceditManager(policyExplorer);
            registryManager = new RegistryManager(policyExplorer);
            groupPolicyManager = new GroupPolicyManager(policyExplorer);
        }

        public void ConfigurePolicies(List<Item> selectedItems, bool isRevert = false)
        {
            if (selectedItems == null || !selectedItems.Any())
            {
                Logger.Instance.WriteLog(isRevert ? "No policies selected for reverting." : "No policies selected for configuration.");
                return;
            }

            foreach (var item in selectedItems)
            {
                if (item.Policy != null)
                {
                    Logger.Instance.WriteLog($"{(isRevert ? "Reverting" : "Configuring")} policy {item.ID}: {item.Name}");
                    bool success = false;

                    // Try secedit method if available
                    if (item.Policy.Implementation?.Secedit != null)
                    {
                        success = seceditManager.ConfigurePolicy(item.Policy, isRevert);
                        Logger.Instance.WriteLog($"Secedit configuration {(success ? "succeeded" : "failed")} for policy {item.ID}.");
                    }

                    // If secedit method failed or not available, try Group Policy method
                    if (!success && item.Policy.Implementation?.GroupPolicy != null)
                    {
                        success = groupPolicyManager.ConfigurePolicy(item.Policy, isRevert);
                        Logger.Instance.WriteLog($"Group Policy configuration {(success ? "succeeded" : "failed")} for policy {item.ID}.");
                    }

                    // If Group Policy method failed or not available, try registry method
                    if (!success && item.Policy.Implementation?.Registry != null)
                    {
                        success = registryManager.ConfigurePolicy(item.Policy, isRevert);
                        Logger.Instance.WriteLog($"Registry configuration {(success ? "succeeded" : "failed")} for policy {item.ID}.");
                    }

                    // If all methods failed
                    if (!success)
                    {
                        Logger.Instance.WriteLog($"Configuration failed for policy {item.ID} using all available methods.");
                    }
                }
            }

            // Re-audit the policies to update the status
            var auditor = new PolicyAuditor(policyExplorer, benchmarkValues, benchmarkDocumentation);
            auditor.AuditPolicies(selectedItems);
        }
    }
}
