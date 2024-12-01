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

        public PolicyAuditor(PolicyExplorerWindow policyExplorer, CISBenchmark benchmarkValues, CISBenchmarkDocumentation benchmarkDocumentation)
        {
            this.policyExplorer = policyExplorer;
            this.benchmarkValues = benchmarkValues;
            this.benchmarkDocumentation = benchmarkDocumentation;
        }

        public void AuditPolicies()
        {
            // Clear previous logs
            policyExplorer.logsTextBox.Text = "";

            // Get the items from the DataGrid
            var items = policyExplorer.myDataGrid.ItemsSource as IEnumerable<Item>;
            if (items == null)
            {
                policyExplorer.logsTextBox.Text = "No policies to audit.";
                return;
            }

            foreach (var item in items)
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
                    policyExplorer.logsTextBox.Text += $"Policy {item.ID}: {item.Name} - {item.Status}\n";

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
                // Check if secedit.exe exists
                string seceditPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "secedit.exe");
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
                        CreateNoWindow = true
                        // Ensure the application runs as administrator
                    };

                    using (var process = Process.Start(startInfo))
                    {
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
                    string templateSetting = policy.Implementation.Secedit.TemplateSetting;
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
                            if (!string.Equals(currentValue, required, StringComparison.OrdinalIgnoreCase))
                                return false;
                        }
                        else if (op == "not_equal")
                        {
                            if (string.Equals(currentValue, required, StringComparison.OrdinalIgnoreCase))
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
