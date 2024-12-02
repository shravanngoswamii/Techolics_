using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Techolics_.Logging;

namespace Techolics_.PolicyManagement
{
    public class SeceditManager
    {
        private PolicyExplorerWindow policyExplorer;

        public SeceditManager(PolicyExplorerWindow policyExplorer)
        {
            this.policyExplorer = policyExplorer;
        }

        public string? GetCurrentPolicyValue(Policy policy)
        {
            if (policy.Implementation?.Secedit == null || string.IsNullOrEmpty(policy.Implementation.Secedit.TemplateSetting))
            {
                Logger.Instance.WriteLog($"Policy {policy.Id} has no secedit implementation or template setting.");
                return null;
            }

            string templateSetting = policy.Implementation.Secedit.TemplateSetting;

            // Check if secedit.exe exists
            string seceditPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                "secedit.exe"
            );
            if (!File.Exists(seceditPath))
            {
                Logger.Instance.WriteLog("Error executing secedit: secedit.exe not found.");
                return null;
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
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        Logger.Instance.WriteLog("Error executing secedit: Process could not be started.");
                        return null;
                    }

                    process.WaitForExit();

                    string error = process.StandardError.ReadToEnd();

                    if (process.ExitCode != 0)
                    {
                        // Log the error details
                        Logger.Instance.WriteLog($"Error executing secedit: {error.Trim()}");
                        return null;
                    }
                }

                // Read the exported security settings
                var lines = File.ReadAllLines(tempFile);

                // Find the setting for the policy
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
                Logger.Instance.WriteLog($"Error executing secedit: {ex.Message}");
                return null;
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

        public bool ConfigurePolicy(Policy policy, bool isRevert = false)
        {
            if (policy.Implementation?.Secedit == null || string.IsNullOrEmpty(policy.Implementation.Secedit.TemplateSetting))
            {
                Logger.Instance.WriteLog($"Policy {policy.Id} has no secedit implementation or template setting.");
                return false;
            }

            string templateSetting = policy.Implementation.Secedit.TemplateSetting;

            // Replace %Value% with the required value or default value
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

            templateSetting = templateSetting.Replace("%Value%", value);

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
                    templateSetting
                };

                // Write the template file
                File.WriteAllLines(tempTemplateFile, templateLines);

                // Apply the template using secedit
                string seceditPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.System),
                    "secedit.exe"
                );
                if (!File.Exists(seceditPath))
                {
                    Logger.Instance.WriteLog("Error: secedit.exe not found.");
                    return false;
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
                    CreateNoWindow = true,
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        Logger.Instance.WriteLog("Error executing secedit: Process could not be started.");
                        return false;
                    }

                    process.WaitForExit();

                    string error = process.StandardError.ReadToEnd();

                    if (process.ExitCode != 0)
                    {
                        // Log the error details
                        Logger.Instance.WriteLog($"Error executing secedit: {error.Trim()}");
                        return false;
                    }
                    else
                    {
                        Logger.Instance.WriteLog($"Configuration applied successfully using secedit for policy {policy.Id}.");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLog($"Error configuring policy {policy.Id} using secedit: {ex.Message}");
                return false;
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
