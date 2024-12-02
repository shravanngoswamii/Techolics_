using System.Collections.Generic;
using System.Linq;
using Techolics_.Logging;

namespace Techolics_.PolicyManagement
{
    public class PolicyAuditor
    {
        private PolicyExplorerWindow policyExplorer;
        private CISBenchmark benchmarkValues;
        private CISBenchmarkDocumentation benchmarkDocumentation;

        private SeceditManager seceditManager;
        private RegistryManager registryManager;
        private GroupPolicyManager groupPolicyManager;

        public PolicyAuditor(
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

        public void AuditPolicies(List<Item> selectedItems)
        {
            if (selectedItems == null || selectedItems.Count == 0)
            {
                Logger.Instance.WriteLog("No policies to audit.");
                return;
            }

            foreach (var item in selectedItems)
            {
                if (item.Policy != null)
                {
                    Logger.Instance.WriteLog($"Starting audit for policy {item.ID}: {item.Name}");
                    string? currentValue = null;

                    // Try secedit method if available
                    if (item.Policy.Implementation?.Secedit != null)
                    {
                        currentValue = seceditManager.GetCurrentPolicyValue(item.Policy);
                        if (currentValue != null)
                        {
                            Logger.Instance.WriteLog($"Secedit audit successful for policy {item.ID}. Value: {currentValue}");
                        }
                        else
                        {
                            Logger.Instance.WriteLog($"Secedit audit failed for policy {item.ID}.");
                        }
                    }

                    // If secedit method failed or not available, try Group Policy method
                    if (currentValue == null && item.Policy.Implementation?.GroupPolicy != null)
                    {
                        currentValue = groupPolicyManager.GetCurrentPolicyValue(item.Policy);
                        if (currentValue != null)
                        {
                            Logger.Instance.WriteLog($"Group Policy audit successful for policy {item.ID}. Value: {currentValue}");
                        }
                        else
                        {
                            Logger.Instance.WriteLog($"Group Policy audit failed for policy {item.ID}.");
                        }
                    }

                    // If Group Policy method failed or not available, try registry method
                    if (currentValue == null && item.Policy.Implementation?.Registry != null)
                    {
                        currentValue = registryManager.GetCurrentPolicyValue(item.Policy);
                        if (currentValue != null)
                        {
                            Logger.Instance.WriteLog($"Registry audit successful for policy {item.ID}. Value: {currentValue}");
                        }
                        else
                        {
                            Logger.Instance.WriteLog($"Registry audit failed for policy {item.ID}.");
                        }
                    }

                    // If all methods failed
                    if (currentValue == null)
                    {
                        Logger.Instance.WriteLog($"Failed to audit policy {item.ID} using all available methods.");
                        currentValue = "Error";
                    }

                    // Update the Current value
                    item.Current = currentValue;

                    // Check if the current value meets the ValueConstraints
                    bool isCompliant = CheckPolicyCompliance(item.Policy, currentValue);

                    // Update the Status
                    item.Status = isCompliant ? "Pass" : "Fail";

                    // Log the result
                    Logger.Instance.WriteLog($"Audit result for policy {item.ID}: {item.Status}");
                }
            }

            // Refresh the DataGrid
            policyExplorer.myDataGrid.Items.Refresh();
        }

        private bool CheckPolicyCompliance(Policy policy, string currentValue)
        {
            if (policy.ValueConstraints != null && policy.ValueConstraints.RequiredValues != null)
            {
                foreach (var requiredValue in policy.ValueConstraints.RequiredValues)
                {
                    // Implement comparison based on operator
                    string op = requiredValue.Operator;
                    string required = requiredValue.Value;

                    // Attempt to parse as integer
                    bool isInt = int.TryParse(currentValue, out int currentInt);
                    bool isRequiredInt = int.TryParse(required, out int requiredInt);

                    if (isInt && isRequiredInt)
                    {
                        switch (op)
                        {
                            case "equal":
                                if (currentInt != requiredInt)
                                    return false;
                                break;
                            case "not_equal":
                                if (currentInt == requiredInt)
                                    return false;
                                break;
                            case "greater_or_equal":
                                if (currentInt < requiredInt)
                                    return false;
                                break;
                            case "less_or_equal":
                                if (currentInt > requiredInt)
                                    return false;
                                break;
                            // Add other operators as needed
                            default:
                                return false;
                        }
                    }
                    else
                    {
                        // Handle non-integer values
                        if (op == "equal")
                        {
                            if (!string.Equals(currentValue, required, System.StringComparison.OrdinalIgnoreCase))
                                return false;
                        }
                        else if (op == "not_equal")
                        {
                            if (string.Equals(currentValue, required, System.StringComparison.OrdinalIgnoreCase))
                                return false;
                        }
                        else
                        {
                            // For other operators, return false
                            return false;
                        }
                    }
                }

                // All constraints passed
                return true;
            }

            // No constraints, assume compliant
            return true;
        }
    }
}
