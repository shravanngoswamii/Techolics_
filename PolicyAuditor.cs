using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Techolics_
{
    public class PolicyAuditor
    {
        private PolicyExplorerWindow policyExplorer;
        private CISBenchmark benchmarkValues;
        private CISBenchmarkDocumentation benchmarkDocumentation;

        public PolicyAuditor(
            PolicyExplorerWindow policyExplorer,
            CISBenchmark benchmarkValues,
            CISBenchmarkDocumentation benchmarkDocumentation
        )
        {
            this.policyExplorer = policyExplorer;
            this.benchmarkValues = benchmarkValues;
            this.benchmarkDocumentation = benchmarkDocumentation;
        }

        public void AuditPolicies(List<Item> selectedItems)
        {
            // Clear previous logs
            policyExplorer.logsTextBox.Text = "";

            if (selectedItems == null || selectedItems.Count == 0)
            {
                policyExplorer.logsTextBox.Text = "No policies to audit.";
                return;
            }

            foreach (var item in selectedItems)
            {
                if (item.Policy != null)
                {
                    // Get the current value using the Implementation
                    string currentValue = GetCurrentPolicyValue(item.Policy);

                    // Update the Current value
                    item.Current = currentValue;

                    // Check if the current value meets the ValueConstraints
                    bool isCompliant = CheckPolicyCompliance(item.Policy, currentValue);

                    // Update the Status
                    item.Status = isCompliant ? "Pass" : "Fail";

                    // Log the result
                    policyExplorer.logsTextBox.Text +=
                        $"Policy {item.ID}: {item.Name} - {item.Status}\n";

                    // Log any errors
                    if (currentValue.StartsWith("Error"))
                    {
                        policyExplorer.logsTextBox.Text += $"{currentValue}\n";
                    }
                }
            }

            // Refresh the DataGrid
            policyExplorer.myDataGrid.Items.Refresh();
        }

        private string GetCurrentPolicyValue(Policy policy)
        {
            if (policy.Implementation != null && policy.Implementation.Secedit != null)
            {
                // Check if TemplateSetting is null
                if (string.IsNullOrEmpty(policy.Implementation.Secedit.TemplateSetting))
                {
                    return "Error: TemplateSetting is null or empty.";
                }

                string templateSetting = policy.Implementation.Secedit.TemplateSetting;

                // Rest of your code...
                // Check if secedit.exe exists
                string seceditPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.System),
                    "secedit.exe"
                );
                if (!File.Exists(seceditPath))
                {
                    return "Error executing secedit: secedit.exe not found.";
                }

                // Use secedit to export the security settings to a file
                string tempFile = Path.GetTempFileName();
                try
                {
                    // Execute secedit command
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = seceditPath,
                        Arguments = $"/export /cfg \"{tempFile}\" /areas SECURITYPOLICY",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        // Ensure the application runs as administrator
                    };

                    using (var process = Process.Start(startInfo))
                    {
                        if (process == null)
                        {
                            return "Error executing secedit: Process could not be started.";
                        }

                        process.WaitForExit();

                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();

                        if (process.ExitCode != 0)
                        {
                            // Log the error details
                            return $"Error executing secedit: {error.Trim()}";
                        }
                    }

                    // Read the exported security settings
                    var lines = File.ReadAllLines(tempFile);

                    // Find the setting for the policy
                    // string templateSetting = policy.Implementation.Secedit.TemplateSetting; // Already defined above
                    // Example: "PasswordHistorySize = %Value%"

                    // Extract the setting name
                    var settingName = templateSetting.Split('=')[0].Trim();

                    foreach (var line in lines)
                    {
                        if (line.StartsWith(settingName, StringComparison.OrdinalIgnoreCase))
                        {
                            var parts = line.Split('=');
                            if (parts.Length == 2)
                            {
                                return parts[1].Trim();
                            }
                        }
                    }

                    // Setting not found
                    return "Not Configured";
                }
                catch (Exception ex)
                {
                    return $"Error executing secedit: {ex.Message}";
                }
                finally
                {
                    // Clean up temporary file
                    if (File.Exists(tempFile))
                    {
                        try
                        {
                            File.Delete(tempFile);
                        }
                        catch
                        {
                            // Ignore any errors during cleanup
                        }
                    }
                }
            }

            // If no implementation or unhandled method, return "N/A"
            return "N/A";
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
                            if (
                                !string.Equals(
                                    currentValue,
                                    required,
                                    StringComparison.OrdinalIgnoreCase
                                )
                            )
                                return false;
                        }
                        else if (op == "not_equal")
                        {
                            if (
                                string.Equals(
                                    currentValue,
                                    required,
                                    StringComparison.OrdinalIgnoreCase
                                )
                            )
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

        public void ConfigurePolicies(List<Item> selectedItems)
        {
            // Clear previous logs
            policyExplorer.logsTextBox.Text = "";

            if (selectedItems == null || !selectedItems.Any())
            {
                policyExplorer.logsTextBox.Text = "No policies selected for configuration.";
                return;
            }

            // Create a temporary security template file
            string tempTemplateFile = Path.GetTempFileName();

            try
            {
                // Build the security template content
                List<string> templateLines = new List<string>
                {
                    "[Unicode]",
                    "Unicode=yes",
                    "[Version]",
                    "signature=\"$CHICAGO$\"",
                    "Revision=1",
                    "[System Access]",
                };

                foreach (var item in selectedItems)
                {
                    var policy = item.Policy;
                    if (
                        policy != null
                        && policy.Implementation != null
                        && policy.Implementation.Secedit != null
                    )
                    {
                        // Check if TemplateSetting is null
                        if (string.IsNullOrEmpty(policy.Implementation.Secedit.TemplateSetting))
                        {
                            policyExplorer.logsTextBox.Text += $"TemplateSetting is null or empty for policy {policy.Id}\n";
                            continue;
                        }

                        string templateSetting = policy.Implementation.Secedit.TemplateSetting;

                        // Replace %Value% with the required value
                        if (
                            policy.ValueConstraints != null
                            && policy.ValueConstraints.RequiredValues != null
                        )
                        {
                            var requiredValue =
                                policy.ValueConstraints.RequiredValues.FirstOrDefault();
                            if (requiredValue != null)
                            {
                                string value = requiredValue.Value;
                                templateSetting = templateSetting.Replace("%Value%", value);

                                // Add to template lines
                                templateLines.Add(templateSetting);
                            }
                            else
                            {
                                policyExplorer.logsTextBox.Text +=
                                    $"No required value for policy {policy.Id}\n";
                            }
                        }
                        else
                        {
                            policyExplorer.logsTextBox.Text +=
                                $"No value constraints for policy {policy.Id}\n";
                        }
                    }
                }

                // Write the template file
                File.WriteAllLines(tempTemplateFile, templateLines);

                // Apply the template using secedit
                string seceditPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.System),
                    "secedit.exe"
                );
                if (!File.Exists(seceditPath))
                {
                    policyExplorer.logsTextBox.Text = "Error: secedit.exe not found.";
                    return;
                }

                // Apply the template
                string logFile = Path.GetTempFileName();

                var startInfo = new ProcessStartInfo
                {
                    FileName = seceditPath,
                    Arguments =
                        $"/configure /db \"{tempTemplateFile}.sdb\" /cfg \"{tempTemplateFile}\" /log \"{logFile}\" /quiet",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        policyExplorer.logsTextBox.Text += "Error executing secedit: Process could not be started.\n";
                        return;
                    }

                    process.WaitForExit();

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    if (process.ExitCode != 0)
                    {
                        // Log the error details
                        policyExplorer.logsTextBox.Text +=
                            $"Error executing secedit: {error.Trim()}\n";
                        return;
                    }
                    else
                    {
                        policyExplorer.logsTextBox.Text += "Configuration applied successfully.\n";
                    }
                }

                // Re-audit the policies to update the status
                AuditPolicies(selectedItems);
            }
            catch (Exception ex)
            {
                policyExplorer.logsTextBox.Text += $"Error configuring policies: {ex.Message}\n";
            }
            finally
            {
                // Clean up temporary files
                if (File.Exists(tempTemplateFile))
                {
                    try
                    {
                        File.Delete(tempTemplateFile);
                    }
                    catch
                    {
                        // Ignore errors during cleanup
                    }
                }
            }
        }

        public void RevertPolicies(List<Item> selectedItems)
        {
            // Clear previous logs
            policyExplorer.logsTextBox.Text = "";

            if (selectedItems == null || !selectedItems.Any())
            {
                policyExplorer.logsTextBox.Text = "No policies selected for reverting.";
                return;
            }

            // Determine if the system is standalone or domain
            bool isStandalone = IsStandalone();

            // Create a temporary security template file
            string tempTemplateFile = Path.GetTempFileName();

            try
            {
                // Build the security template content
                List<string> templateLines = new List<string>
                {
                    "[Unicode]",
                    "Unicode=yes",
                    "[Version]",
                    "signature=\"$CHICAGO$\"",
                    "Revision=1",
                    "[System Access]"
                };

                foreach (var item in selectedItems)
                {
                    var policy = item.Policy;
                    if (policy != null && policy.Implementation != null && policy.Implementation.Secedit != null)
                    {
                        // Check if TemplateSetting is null
                        if (string.IsNullOrEmpty(policy.Implementation.Secedit.TemplateSetting))
                        {
                            policyExplorer.logsTextBox.Text += $"TemplateSetting is null or empty for policy {policy.Id}\n";
                            continue;
                        }

                        string templateSetting = policy.Implementation.Secedit.TemplateSetting;

                        // Get the default value
                        string? defaultValue = GetPolicyDefaultValue(policy, isStandalone);

                        if (defaultValue != null)
                        {
                            // Replace %Value% with the default value
                            templateSetting = templateSetting.Replace("%Value%", defaultValue);

                            // Add to template lines
                            templateLines.Add(templateSetting);
                        }
                        else
                        {
                            policyExplorer.logsTextBox.Text += $"No default value for policy {policy.Id}\n";
                        }
                    }
                }

                // Write the template file
                File.WriteAllLines(tempTemplateFile, templateLines);

                // Apply the template using secedit
                string seceditPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "secedit.exe");
                if (!File.Exists(seceditPath))
                {
                    policyExplorer.logsTextBox.Text = "Error: secedit.exe not found.";
                    return;
                }

                // Apply the template
                string logFile = Path.GetTempFileName();

                var startInfo = new ProcessStartInfo
                {
                    FileName = seceditPath,
                    Arguments = $"/configure /db \"{tempTemplateFile}.sdb\" /cfg \"{tempTemplateFile}\" /log \"{logFile}\" /quiet",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        policyExplorer.logsTextBox.Text += "Error executing secedit: Process could not be started.\n";
                        return;
                    }

                    process.WaitForExit();

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    if (process.ExitCode != 0)
                    {
                        // Log the error details
                        policyExplorer.logsTextBox.Text += $"Error executing secedit: {error.Trim()}\n";
                        return;
                    }
                    else
                    {
                        policyExplorer.logsTextBox.Text += "Policies reverted to default successfully.\n";
                    }
                }

                // Re-audit the policies to update the status
                AuditPolicies(selectedItems);
            }
            catch (Exception ex)
            {
                policyExplorer.logsTextBox.Text += $"Error reverting policies: {ex.Message}\n";
            }
            finally
            {
                // Clean up temporary files
                if (File.Exists(tempTemplateFile))
                {
                    try
                    {
                        File.Delete(tempTemplateFile);
                    }
                    catch
                    {
                        // Ignore errors during cleanup
                    }
                }
            }
        }

        private bool IsStandalone()
        {
            return Environment.MachineName == Environment.UserDomainName;
        }

        private string? GetPolicyDefaultValue(Policy policy, bool isStandalone)
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
    }
}
