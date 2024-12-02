using System;
using System.Linq;
using System.Management.Automation;
using Techolics_.Logging;

namespace Techolics_.PolicyManagement
{
    public class GroupPolicyManager
    {
        private PolicyExplorerWindow policyExplorer;

        public GroupPolicyManager(PolicyExplorerWindow policyExplorer)
        {
            this.policyExplorer = policyExplorer;
        }

        public string? GetCurrentPolicyValue(Policy policy)
        {
            if (policy.Implementation?.GroupPolicy == null)
            {
                Logger.Instance.WriteLog($"Policy {policy.Id} has no Group Policy implementation.");
                return null;
            }

            try
            {
                string policyPath = policy.Implementation.GroupPolicy.PolicyPath;
                string policyName = policy.Implementation.GroupPolicy.PolicyName;

                using (PowerShell ps = PowerShell.Create())
                {
                    ps.AddCommand("Get-GPRegistryValue")
                      .AddParameter("Name", "Local Group Policy")
                      .AddParameter("Key", policyPath)
                      .AddParameter("ValueName", policyName);

                    var results = ps.Invoke();

                    if (ps.HadErrors || results.Count == 0)
                    {
                        foreach (var error in ps.Streams.Error)
                        {
                            Logger.Instance.WriteLog($"Error getting policy {policy.Id}: {error}");
                        }
                        Logger.Instance.WriteLog($"Policy {policy.Id} not configured.");
                        return "Not Configured";
                    }

                    var value = results[0].Properties["Value"].Value?.ToString();
                    string displayValue = PolicyValueConverter.ConvertForDisplay(value ?? "", policy.ValueType);
                    Logger.Instance.WriteLog($"Retrieved value for policy {policy.Id}: {displayValue}");
                    return displayValue;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLog($"Error accessing Group Policy for policy {policy.Id}: {ex.Message}");
                return null;
            }
        }

        public bool ConfigurePolicy(Policy policy, bool isRevert = false)
        {
            if (policy.Implementation?.GroupPolicy == null)
            {
                Logger.Instance.WriteLog($"Policy {policy.Id} has no Group Policy implementation.");
                return false;
            }

            try
            {
                string policyPath = policy.Implementation.GroupPolicy.PolicyPath;
                string policyName = policy.Implementation.GroupPolicy.PolicyName;

                string? value = null;
                if (isRevert)
                {
                    // Get default value
                    value = GetPolicyDefaultValue(policy);
                    if (value == null)
                    {
                        Logger.Instance.WriteLog($"No default value for policy {policy.Id}");
                        return false;
                    }
                }
                else
                {
                    // Get required value
                    if (policy.ValueConstraints?.RequiredValues != null)
                    {
                        var requiredValue = policy.ValueConstraints.RequiredValues.FirstOrDefault();
                        if (requiredValue != null)
                        {
                            value = requiredValue.Value;
                        }
                        else
                        {
                            Logger.Instance.WriteLog($"No required value for policy {policy.Id}");
                            return false;
                        }
                    }
                    else
                    {
                        Logger.Instance.WriteLog($"No value constraints for policy {policy.Id}");
                        return false;
                    }
                }

                // Convert value for configuration
                string convertedValue = PolicyValueConverter.ConvertForConfiguration(value, policy.ValueType);

                using (PowerShell ps = PowerShell.Create())
                {
                    ps.AddCommand("Set-GPRegistryValue")
                      .AddParameter("Name", "Local Group Policy")
                      .AddParameter("Key", policyPath)
                      .AddParameter("ValueName", policyName)
                      .AddParameter("Type", "DWORD") // Adjust the type as needed
                      .AddParameter("Value", convertedValue);

                    ps.Invoke();

                    if (ps.HadErrors)
                    {
                        foreach (var error in ps.Streams.Error)
                        {
                            Logger.Instance.WriteLog($"Error configuring policy {policy.Id}: {error}");
                        }
                        return false;
                    }
                    else
                    {
                        Logger.Instance.WriteLog($"Successfully configured policy {policy.Id} using Group Policy.");
                        return true;
                    }
                }
            }
            catch (ArgumentException ex)
            {
                Logger.Instance.WriteLog($"Value conversion error for policy {policy.Id}: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLog($"Error configuring Group Policy for policy {policy.Id}: {ex.Message}");
                return false;
            }
        }

        private string? GetPolicyDefaultValue(Policy policy)
        {
            if (policy.DefaultValue != null)
            {
                if (!string.IsNullOrEmpty(policy.DefaultValue.Value))
                {
                    // Global default value
                    return policy.DefaultValue.Value;
                }
                else
                {
                    // Domain or Standalone default values
                    bool isStandalone = IsStandalone();
                    if (isStandalone)
                    {
                        return policy.DefaultValue.Standalone;
                    }
                    else
                    {
                        return policy.DefaultValue.Domain;
                    }
                }
            }
            return null;
        }

        private bool IsStandalone()
        {
            return Environment.MachineName == Environment.UserDomainName;
        }
    }
}
