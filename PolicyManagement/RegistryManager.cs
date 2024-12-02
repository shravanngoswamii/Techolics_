using System;
using System.Linq;
using Microsoft.Win32;
using Techolics_.Logging;

namespace Techolics_.PolicyManagement
{
    public class RegistryManager
    {
        private PolicyExplorerWindow policyExplorer;

        public RegistryManager(PolicyExplorerWindow policyExplorer)
        {
            this.policyExplorer = policyExplorer;
        }

        public string? GetCurrentPolicyValue(Policy policy)
        {
            if (policy.Implementation?.Registry == null)
            {
                Logger.Instance.WriteLog($"Policy {policy.Id} has no registry implementation.");
                return null;
            }

            var registryInfo = policy.Implementation.Registry;

            try
            {
                // Parse the 'Key' to get 'Hive' and 'Path'
                string key = registryInfo.Key;
                (string? hiveName, string? subKeyPath) = ParseRegistryKey(key);

                if (hiveName == null || subKeyPath == null)
                {
                    Logger.Instance.WriteLog($"Invalid registry key format: {key}");
                    return null;
                }

                RegistryKey? baseKey = GetRegistryHive(hiveName);
                if (baseKey == null)
                {
                    Logger.Instance.WriteLog($"Invalid registry hive: {hiveName}");
                    return null;
                }

                using (RegistryKey? subKey = baseKey.OpenSubKey(subKeyPath))
                {
                    if (subKey == null)
                    {
                        Logger.Instance.WriteLog($"Registry key not found: {subKeyPath}");
                        return null;
                    }

                    object? value = subKey.GetValue(registryInfo.ValueName);
                    if (value != null)
                    {
                        return value.ToString();
                    }
                    else
                    {
                        Logger.Instance.WriteLog(
                            $"Registry value not found: {registryInfo.ValueName}"
                        );
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLog($"Error accessing registry: {ex.Message}");
                return null;
            }
        }

        public bool ConfigurePolicy(Policy policy, bool isRevert = false)
        {
            if (policy.Implementation?.Registry == null)
            {
                return false;
            }

            var registryInfo = policy.Implementation.Registry;

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

            try
            {
                // Parse the 'Key' to get 'Hive' and 'Path'
                string key = registryInfo.Key;
                (string? hiveName, string? subKeyPath) = ParseRegistryKey(key);

                if (hiveName == null || subKeyPath == null)
                {
                    Logger.Instance.WriteLog($"Invalid registry key format: {key}");
                    return false;
                }

                RegistryKey? baseKey = GetRegistryHive(hiveName);
                if (baseKey == null)
                {
                    Logger.Instance.WriteLog($"Invalid registry hive: {hiveName}");
                    return false;
                }

                using (RegistryKey? subKey = baseKey.OpenSubKey(subKeyPath, true))
                {
                    if (subKey == null)
                    {
                        Logger.Instance.WriteLog($"Registry key not found: {subKeyPath}");
                        return false;
                    }

                    RegistryValueKind valueKind = GetRegistryValueKind(registryInfo.ValueType);

                    // Convert the value to the correct type based on ValueType
                    object convertedValue = ConvertValue(value, valueKind);

                    subKey.SetValue(registryInfo.ValueName, convertedValue, valueKind);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLog($"Error writing to registry: {ex.Message}");
                return false;
            }
        }

        private (string? hiveName, string? subKeyPath) ParseRegistryKey(string key)
        {
            int firstBackslashIndex = key.IndexOf('\\');
            if (firstBackslashIndex <= 0)
            {
                return (null, null);
            }

            string hiveName = key.Substring(0, firstBackslashIndex);
            string subKeyPath = key.Substring(firstBackslashIndex + 1);

            return (hiveName, subKeyPath);
        }

        private RegistryKey? GetRegistryHive(string hiveName)
        {
            switch (hiveName.ToUpper())
            {
                case "HKEY_LOCAL_MACHINE":
                case "HKLM":
                    return Registry.LocalMachine;
                case "HKEY_CURRENT_USER":
                case "HKCU":
                    return Registry.CurrentUser;
                case "HKEY_CLASSES_ROOT":
                case "HKCR":
                    return Registry.ClassesRoot;
                case "HKEY_USERS":
                case "HKU":
                    return Registry.Users;
                case "HKEY_CURRENT_CONFIG":
                case "HKCC":
                    return Registry.CurrentConfig;
                default:
                    return null;
            }
        }

        private RegistryValueKind GetRegistryValueKind(string valueType)
        {
            switch (valueType.ToUpper())
            {
                case "REG_SZ":
                case "STRING":
                    return RegistryValueKind.String;
                case "REG_DWORD":
                case "DWORD":
                    return RegistryValueKind.DWord;
                case "REG_QWORD":
                case "QWORD":
                    return RegistryValueKind.QWord;
                case "REG_BINARY":
                case "BINARY":
                    return RegistryValueKind.Binary;
                case "REG_MULTI_SZ":
                case "MULTI_SZ":
                    return RegistryValueKind.MultiString;
                case "REG_EXPAND_SZ":
                case "EXPAND_SZ":
                    return RegistryValueKind.ExpandString;
                default:
                    return RegistryValueKind.String;
            }
        }

        private object ConvertValue(string value, RegistryValueKind valueKind)
        {
            try
            {
                switch (valueKind)
                {
                    case RegistryValueKind.DWord:
                        return int.Parse(value);
                    case RegistryValueKind.QWord:
                        return long.Parse(value);
                    case RegistryValueKind.Binary:
                        // Assuming value is in hex format separated by commas
                        return value.Split(',').Select(b => Convert.ToByte(b.Trim(), 16)).ToArray();
                    case RegistryValueKind.MultiString:
                        // Assuming values are separated by semicolons
                        return value.Split(';');
                    default:
                        return value;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLog(
                    $"Error converting value '{value}' to type '{valueKind}': {ex.Message}"
                );
                return value;
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
