using System;
using System.Diagnostics;
using System.IO;
using Techolics_.Logging;

namespace Techolics_.PolicyManagement
{
    public class SeceditManager
    {
        private PolicyExplorerWindow policyExplorer;

        public SeceditManager(PolicyExplorerWindow policyExplorer)
        {
            Logger.Instance.WriteLog("SeceditManager initialized.");
            this.policyExplorer = policyExplorer;
        }

        public string? GetCurrentPolicyValue(Policy policy)
        {
            if (
                policy.Implementation?.Secedit == null
                || string.IsNullOrEmpty(policy.Implementation.Secedit.TemplateSetting)
            )
            {
                Logger.Instance.WriteLog($"No secedit template setting for policy {policy.Id}.");
                return null;
            }

            string templateSetting = policy.Implementation.Secedit.TemplateSetting;
            string seceditPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                "secedit.exe"
            );
            if (!File.Exists(seceditPath))
            {
                Logger.Instance.WriteLog("secedit.exe not found.");
                return null;
            }

            string tempFile = Path.GetTempFileName();
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = seceditPath,
                    Arguments = $"/export /cfg \"{tempFile}\" /areas SECURITYPOLICY USER_RIGHTS",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        Logger.Instance.WriteLog("Failed to start secedit process.");
                        return null;
                    }

                    process.WaitForExit();
                    if (process.ExitCode != 0)
                    {
                        string error = process.StandardError.ReadToEnd().Trim();
                        Logger.Instance.WriteLog($"secedit export error: {error}");
                        return null;
                    }
                }

                var lines = File.ReadAllLines(tempFile);
                var settingName = templateSetting.Split('=')[0].Trim();

                foreach (var line in lines)
                {
                    if (line.StartsWith(settingName, StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            string rawValue = parts[1].Trim();
                            return PolicyValueConverter.ConvertForDisplay(
                                rawValue,
                                policy.ValueType
                            );
                        }
                    }
                }

                // If not found and default is "No one", return "No one"
                if (
                    string.Equals(
                        policy.DefaultValue?.Value,
                        "No one",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return "No one";
                }

                return "Not Configured";
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLog($"Error in GetCurrentPolicyValue: {ex.Message}");
                return null;
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    try
                    {
                        File.Delete(tempFile);
                    }
                    catch { }
                }
            }
        }

        public bool ConfigurePolicy(Policy policy, bool isRevert = false)
        {
            if (
                policy.Implementation?.Secedit == null
                || string.IsNullOrEmpty(policy.Implementation.Secedit.TemplateSetting)
            )
            {
                Logger.Instance.WriteLog($"No secedit template for policy {policy.Id}.");
                return false;
            }

            string templateSetting = policy.Implementation.Secedit.TemplateSetting;
            string section = policy.Implementation.Secedit.Section;
            if (string.IsNullOrEmpty(section))
                section = "System Access";
            if (section.Equals("Priviledge Rights", StringComparison.OrdinalIgnoreCase))
                section = "Privilege Rights";

            string? value = isRevert ? GetPolicyDefaultValue(policy) : GetRequiredValue(policy);
            if (value == null)
            {
                Logger.Instance.WriteLog($"No suitable value found for policy {policy.Id}.");
                return false;
            }

            try
            {
                string convertedValue = PolicyValueConverter.ConvertForConfiguration(
                    value,
                    policy.ValueType
                );
                templateSetting = templateSetting.Replace("%Value%", convertedValue);

                string tempTemplateFile = Path.GetTempFileName();
                try
                {
                    var templateLines = new System.Collections.Generic.List<string>
                    {
                        "[Unicode]",
                        "Unicode=yes",
                        "[Version]",
                        "signature=\"$CHICAGO$\"",
                        "Revision=1",
                    };

                    if (section.Equals("Privilege Rights", StringComparison.OrdinalIgnoreCase))
                    {
                        templateLines.Add("[Privilege Rights]");
                        templateLines.Add(templateSetting);
                    }
                    else
                    {
                        templateLines.Add("[System Access]");
                        templateLines.Add(templateSetting);
                    }

                    File.WriteAllLines(tempTemplateFile, templateLines);

                    string seceditPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.System),
                        "secedit.exe"
                    );
                    if (!File.Exists(seceditPath))
                    {
                        Logger.Instance.WriteLog("secedit.exe not found.");
                        return false;
                    }

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
                            Logger.Instance.WriteLog("Failed to start secedit configure process.");
                            return false;
                        }

                        process.WaitForExit();
                        if (process.ExitCode != 0)
                        {
                            string error = process.StandardError.ReadToEnd().Trim();
                            Logger.Instance.WriteLog($"secedit configure error: {error}");
                            return false;
                        }

                        Logger.Instance.WriteLog($"Policy {policy.Id} configured successfully.");
                        return true;
                    }
                }
                finally
                {
                    CleanUpTempFiles(tempTemplateFile);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLog($"Error configuring policy {policy.Id}: {ex.Message}");
                return false;
            }
        }

        private void CleanUpTempFiles(string tempTemplateFile)
        {
            string tempSdbFile = Path.Combine(
                Path.GetTempPath(),
                Path.GetFileName(tempTemplateFile) + ".sdb"
            );
            if (File.Exists(tempTemplateFile))
            {
                try
                {
                    File.Delete(tempTemplateFile);
                }
                catch { }
            }
            if (File.Exists(tempSdbFile))
            {
                try
                {
                    File.Delete(tempSdbFile);
                }
                catch { }
            }
        }

        private string? GetPolicyDefaultValue(Policy policy)
        {
            if (policy.DefaultValue != null)
            {
                if (!string.IsNullOrEmpty(policy.DefaultValue.Value))
                {
                    return policy.DefaultValue.Value;
                }
                else
                {
                    bool isStandalone = IsStandalone();
                    return isStandalone
                        ? policy.DefaultValue.Standalone
                        : policy.DefaultValue.Domain;
                }
            }
            return null;
        }

        private string? GetRequiredValue(Policy policy)
        {
            if (policy.ValueConstraints?.RequiredValues != null)
            {
                var requiredValue = policy.ValueConstraints.RequiredValues.FirstOrDefault();
                if (requiredValue != null)
                {
                    return requiredValue.Value;
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
