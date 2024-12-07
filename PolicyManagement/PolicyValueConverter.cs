using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Techolics_.Logging;

namespace Techolics_.PolicyManagement
{
    public static class PolicyValueConverter
    {
        private static Dictionary<string, string> SidMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, string> ReverseSidMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        static PolicyValueConverter()
        {
            try
            {
                string mappingFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "SIDMappings.xml");
                Logger.Instance.WriteLog($"PolicyValueConverter: Loading SIDMappings from {mappingFile}");
                if (File.Exists(mappingFile))
                {
                    var doc = XDocument.Load(mappingFile);
                    foreach (var elem in doc.Descendants("Mapping"))
                    {
                        var key = (string?)elem.Attribute("key");
                        var val = (string?)elem.Attribute("value");
                        if (!string.IsNullOrEmpty(key))
                        {
                            key = key.Trim();
                        }
                        if (!string.IsNullOrEmpty(val))
                        {
                            val = val.Trim();
                        }

                        if (!string.IsNullOrEmpty(key) && val != null)
                        {
                            SidMappings[key] = val;
                        }
                    }

                    // Build reverse mappings
                    foreach (var kvp in SidMappings)
                    {
                        if (!string.IsNullOrEmpty(kvp.Value))
                        {
                            ReverseSidMappings[kvp.Value] = kvp.Key;
                        }
                    }
                }
                else
                {
                    Logger.Instance.WriteLog("PolicyValueConverter: SIDMappings.xml not found!");
                }

                Logger.Instance.WriteLog($"PolicyValueConverter: Total mappings loaded: {SidMappings.Count}");
                Logger.Instance.WriteLog($"PolicyValueConverter: Total reverse mappings loaded: {ReverseSidMappings.Count}");
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLog($"PolicyValueConverter: Error loading SIDMappings: {ex.Message}");
            }
        }

        public static string ConvertForConfiguration(string value, string valueType)
        {
            value = value.Trim();
            if (string.Equals(valueType, "Boolean", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(value, "enabled", StringComparison.OrdinalIgnoreCase))
                {
                    return "1";
                }
                else if (string.Equals(value, "false", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(value, "disabled", StringComparison.OrdinalIgnoreCase))
                {
                    return "0";
                }
                else
                {
                    throw new ArgumentException($"Invalid Boolean value: {value}");
                }
            }
            else if (string.Equals(valueType, "Integer", StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }
            else if (string.Equals(valueType, "String", StringComparison.OrdinalIgnoreCase))
            {
                // Handle multiple comma-separated values
                var parts = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(p => p.Trim())
                                 .ToList();

                for (int i = 0; i < parts.Count; i++)
                {
                    if (SidMappings.TryGetValue(parts[i], out var mappedSid))
                    {
                        parts[i] = mappedSid;
                    }
                    // If no mapping found, use as-is
                }

                return string.Join(", ", parts);
            }

            return value;
        }

        public static string ConvertForDisplay(string value, string valueType)
        {
            if (string.Equals(valueType, "Boolean", StringComparison.OrdinalIgnoreCase))
            {
                if (value == "1") return "Enabled";
                else if (value == "0") return "Disabled";
                else return value;
            }
            else if (string.Equals(valueType, "String", StringComparison.OrdinalIgnoreCase))
            {
                // Handle multiple comma-separated SIDs
                var parts = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(p => p.Trim())
                                 .ToList();

                for (int i = 0; i < parts.Count; i++)
                {
                    var part = parts[i];
                    if (!string.IsNullOrEmpty(part) && part.StartsWith("*") && ReverseSidMappings.TryGetValue(part, out var friendlyName))
                    {
                        parts[i] = friendlyName;
                    }
                    // If no reverse mapping found, keep as-is
                }

                return string.Join(", ", parts);
            }

            return value;
        }

        public static string ConvertSIDToFriendlyNameIfPossible(string value)
        {
            // Handle multiple comma-separated values here as well
            var parts = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(p => p.Trim())
                             .ToList();

            for (int i = 0; i < parts.Count; i++)
            {
                var part = parts[i];
                if (!string.IsNullOrEmpty(part) && part.StartsWith("*") && ReverseSidMappings.TryGetValue(part, out var friendlyName))
                {
                    parts[i] = friendlyName;
                }
            }

            return string.Join(", ", parts);
        }
    }
}
