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

        /// <summary>
        /// Configure policies based on isRevert and whether this call is from the edit window or not.
        /// When fromEditWindow = true and not reverting, use the user-chosen value.
        /// When fromEditWindow = false, use the original CIS recommended values from the benchmark.
        /// </summary>
        public void ConfigurePolicies(List<Item> selectedItems, bool isRevert = false, bool fromEditWindow = false)
        {
            if (selectedItems == null || !selectedItems.Any())
            {
                Logger.Instance.WriteLog(isRevert ? "No policies selected for reverting." : "No policies selected for configuration.");
                return;
            }

            foreach (var item in selectedItems)
            {
                if (item.Policy == null)
                {
                    Logger.Instance.WriteLog($"Item {item.ID} has no associated policy.");
                    continue;
                }

                Logger.Instance.WriteLog($"{(isRevert ? "Reverting" : "Configuring")} policy {item.ID}: {item.Name}");

                bool success = false;

                if (!isRevert && fromEditWindow)
                {
                    // Use user-provided value
                    if (!string.IsNullOrWhiteSpace(item.Current) &&
                        !string.Equals(item.Current, "N/A", System.StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(item.Current, "Not Configured", System.StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(item.Current, "Error", System.StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.Instance.WriteLog($"Using user-provided value '{item.Current}' for policy {item.ID}.");

                        // Temporarily override ValueConstraints.RequiredValues with user-provided value
                        var originalRequiredValues = item.Policy.ValueConstraints?.RequiredValues?.ToList();

                        // Ensure ValueConstraints and RequiredValues exist
                        if (item.Policy.ValueConstraints == null)
                        {
                            item.Policy.ValueConstraints = new ValueConstraints { RequiredValues = new List<RequiredValue>() };
                        }
                        if (item.Policy.ValueConstraints.RequiredValues == null)
                        {
                            item.Policy.ValueConstraints.RequiredValues = new List<RequiredValue>();
                        }

                        // Clear existing required values and set one with user-provided value
                        item.Policy.ValueConstraints.RequiredValues.Clear();
                        item.Policy.ValueConstraints.RequiredValues.Add(new RequiredValue { Operator = "equal", Value = item.Current });

                        // Attempt configuration
                        success = TryConfigure(item.Policy, isRevert);

                        // Restore original required values
                        if (originalRequiredValues != null)
                        {
                            item.Policy.ValueConstraints.RequiredValues = originalRequiredValues;
                            Logger.Instance.WriteLog($"Restored original ValueConstraints for policy {item.ID}.");
                        }
                        else
                        {
                            item.Policy.ValueConstraints.RequiredValues = null;
                            Logger.Instance.WriteLog($"Cleared ValueConstraints for policy {item.ID}.");
                        }

                        if (!success)
                        {
                            Logger.Instance.WriteLog($"Configuration failed for policy {item.ID} using user-provided value.");
                        }
                    }
                    else
                    {
                        Logger.Instance.WriteLog($"No valid user-provided value for policy {item.ID}. Using default logic.");
                        success = TryConfigure(item.Policy, isRevert);
                        if (!success)
                        {
                            Logger.Instance.WriteLog($"Configuration failed for policy {item.ID} using default logic.");
                        }
                    }
                }
                else
                {
                    // Use recommended values from ValueConstraints or default
                    success = TryConfigure(item.Policy, isRevert);
                    if (!success)
                    {
                        Logger.Instance.WriteLog($"Configuration failed for policy {item.ID} using recommended/default values.");
                    }
                }
            }

            // Re-audit the policies to update the status
            var auditor = new PolicyAuditor(policyExplorer, benchmarkValues, benchmarkDocumentation);
            auditor.AuditPolicies(selectedItems);
        }

        /// <summary>
        /// Attempts to configure the policy using available methods.
        /// </summary>
        private bool TryConfigure(Policy policy, bool isRevert)
        {
            bool success = false;

            // Try secedit method if available
            if (policy.Implementation?.Secedit != null)
            {
                success = seceditManager.ConfigurePolicy(policy, isRevert);
                Logger.Instance.WriteLog($"Secedit configuration {(success ? "succeeded" : "failed")} for policy {policy.Id}.");
            }

            // If secedit method failed or not available, try Group Policy method
            if (!success && policy.Implementation?.GroupPolicy != null)
            {
                success = groupPolicyManager.ConfigurePolicy(policy, isRevert);
                Logger.Instance.WriteLog($"Group Policy configuration {(success ? "succeeded" : "failed")} for policy {policy.Id}.");
            }

            // If Group Policy method failed or not available, try registry method
            if (!success && policy.Implementation?.Registry != null)
            {
                success = registryManager.ConfigurePolicy(policy, isRevert);
                Logger.Instance.WriteLog($"Registry configuration {(success ? "succeeded" : "failed")} for policy {policy.Id}.");
            }

            // If all methods failed
            if (!success)
            {
                Logger.Instance.WriteLog($"Configuration failed for policy {policy.Id} using all available methods.");
            }

            return success;
        }
    }
}
