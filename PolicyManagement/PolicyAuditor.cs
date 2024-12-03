using System;
using System.Collections.Generic;
using System.Linq;
using Techolics_.Logging;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

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
                try
                {
                    if (item.Policy != null)
                    {
                        // Skip policies without any implementation
                        if (item.Policy.Implementation == null)
                        {
                            Logger.Instance.WriteLog(
                                $"Skipping policy {item.ID} as it has no implementation."
                            );
                            item.Current = "Not Implemented";
                            item.Status = "N/A";
                            continue;
                        }

                        Logger.Instance.WriteLog(
                            $"Starting audit for policy {item.ID}: {item.Name}"
                        );
                        string? currentValue = null;

                        // Try secedit method if available
                        if (item.Policy.Implementation.Secedit != null)
                        {
                            currentValue = seceditManager.GetCurrentPolicyValue(item.Policy);
                            if (currentValue != null)
                            {
                                Logger.Instance.WriteLog(
                                    $"Secedit audit successful for policy {item.ID}. Value: {currentValue}"
                                );
                            }
                            else
                            {
                                Logger.Instance.WriteLog(
                                    $"Secedit audit failed for policy {item.ID}."
                                );
                            }
                        }

                        // If secedit method failed or not available, try Group Policy method
                        if (currentValue == null && item.Policy.Implementation.GroupPolicy != null)
                        {
                            currentValue = groupPolicyManager.GetCurrentPolicyValue(item.Policy);
                            if (currentValue != null)
                            {
                                Logger.Instance.WriteLog(
                                    $"Group Policy audit successful for policy {item.ID}. Value: {currentValue}"
                                );
                            }
                            else
                            {
                                Logger.Instance.WriteLog(
                                    $"Group Policy audit failed for policy {item.ID}."
                                );
                            }
                        }

                        // If Group Policy method failed or not available, try registry method
                        if (currentValue == null && item.Policy.Implementation.Registry != null)
                        {
                            currentValue = registryManager.GetCurrentPolicyValue(item.Policy);
                            if (currentValue != null)
                            {
                                Logger.Instance.WriteLog(
                                    $"Registry audit successful for policy {item.ID}. Value: {currentValue}"
                                );
                            }
                            else
                            {
                                Logger.Instance.WriteLog(
                                    $"Registry audit failed for policy {item.ID}."
                                );
                            }
                        }

                        // If all methods failed
                        if (currentValue == null)
                        {
                            Logger.Instance.WriteLog(
                                $"Failed to audit policy {item.ID} using all available methods."
                            );
                            currentValue = "Error";
                        }

                        // Update the Current value with display conversion
                        if (
                            !string.Equals(
                                currentValue,
                                "Error",
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                        {
                            currentValue = PolicyValueConverter.ConvertForDisplay(
                                currentValue,
                                item.Policy.ValueType
                            );
                        }

                        item.Current = currentValue;

                        // Check if the current value meets the ValueConstraints
                        bool isCompliant = CheckPolicyCompliance(item.Policy, currentValue);

                        // Update the Status
                        item.Status = isCompliant ? "Pass" : "Fail";

                        // Log the result
                        Logger.Instance.WriteLog(
                            $"Audit result for policy {item.ID}: {item.Status}"
                        );
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.WriteLog(
                        $"Unexpected error auditing policy {item.ID ?? "Unknown"}: {ex.Message}"
                    );
                    item.Status = "Error";
                }
            }

            // Refresh the DataGrid after all audits are complete
            policyExplorer.myDataGrid.Items.Refresh();
        }

        private bool CheckPolicyCompliance(Policy policy, string currentValue)
        {
            // If currentValue indicates an error, consider it non-compliant
            if (string.Equals(currentValue, "Error", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (policy.ValueConstraints != null && policy.ValueConstraints.RequiredValues != null)
            {
                foreach (var requiredValue in policy.ValueConstraints.RequiredValues)
                {
                    try
                    {
                        // Implement comparison based on operator
                        string op = requiredValue.Operator;
                        string required = requiredValue.Value;

                        // Handle value conversion for comparison
                        string convertedCurrent = currentValue;
                        string convertedRequired = required;

                        if (
                            string.Equals(
                                policy.ValueType,
                                "Boolean",
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                        {
                            convertedCurrent = PolicyValueConverter.ConvertForConfiguration(
                                currentValue,
                                policy.ValueType
                            );
                            convertedRequired = PolicyValueConverter.ConvertForConfiguration(
                                required,
                                policy.ValueType
                            );
                        }

                        // Attempt to parse as integer
                        bool isInt = int.TryParse(convertedCurrent, out int currentInt);
                        bool isRequiredInt = int.TryParse(convertedRequired, out int requiredInt);

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
                                    Logger.Instance.WriteLog(
                                        $"Unsupported operator '{op}' for policy {policy.Id}."
                                    );
                                    return false;
                            }
                        }
                        else
                        {
                            // Handle non-integer values
                            switch (op)
                            {
                                case "equal":
                                    if (
                                        !string.Equals(
                                            currentValue,
                                            required,
                                            StringComparison.OrdinalIgnoreCase
                                        )
                                    )
                                        return false;
                                    break;
                                case "not_equal":
                                    if (
                                        string.Equals(
                                            currentValue,
                                            required,
                                            StringComparison.OrdinalIgnoreCase
                                        )
                                    )
                                        return false;
                                    break;
                                // For other operators, you can define additional cases or default behavior
                                default:
                                    Logger.Instance.WriteLog(
                                        $"Unsupported operator '{op}' for non-integer comparison in policy {policy.Id}."
                                    );
                                    return false;
                            }
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        Logger.Instance.WriteLog(
                            $"Value conversion error for policy {policy.Id}: {ex.Message}"
                        );
                        return false;
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.WriteLog(
                            $"Unexpected error while checking compliance for policy {policy.Id}: {ex.Message}"
                        );
                        return false;
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
