using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Techolics_.Models;
using Techolics_.Logging;

namespace Techolics_.PolicyManagement
{
    public class GPOCreator
    {
        private readonly string gpoBasePath;
        private readonly string gpoName;
        private readonly string description;

        public GPOCreator(string basePath, string name, string description = "")
        {
            this.gpoBasePath = Path.Combine(basePath, name);
            this.gpoName = name;
            this.description = description;
        }

        public void CreateGPOStructure()
        {
            try
            {
                // Create main GPO directory
                Directory.CreateDirectory(gpoBasePath);

                // Create Machine configuration directories
                Directory.CreateDirectory(Path.Combine(gpoBasePath, "Machine"));
                Directory.CreateDirectory(Path.Combine(gpoBasePath, "Machine", "Microsoft"));
                Directory.CreateDirectory(Path.Combine(gpoBasePath, "Machine", "Microsoft", "Windows NT"));
                Directory.CreateDirectory(Path.Combine(gpoBasePath, "Machine", "Microsoft", "Windows NT", "SecEdit"));

                // Create User configuration directory
                Directory.CreateDirectory(Path.Combine(gpoBasePath, "User"));

                // Create GPO info file
                CreateGPTIni();

                Logger.Instance.WriteLog($"Created GPO directory structure at {gpoBasePath}");
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLog($"Error creating GPO structure: {ex.Message}");
                throw;
            }
        }

        private void CreateGPTIni()
        {
            var iniPath = Path.Combine(gpoBasePath, "GPT.INI");
            var content = new StringBuilder();
            content.AppendLine("[General]");
            content.AppendLine("Version=1");
            content.AppendLine($"displayName={gpoName}");

            if (!string.IsNullOrEmpty(description))
            {
                content.AppendLine($"description={description}");
            }

            File.WriteAllText(iniPath, content.ToString());
            Logger.Instance.WriteLog($"Created GPT.INI file at {iniPath}");
        }

        public void AddRegistryPolicies(List<Policy> policies)
        {
            if (!policies.Any()) return;

            try
            {
                var machineRegistryPath = Path.Combine(gpoBasePath, "Machine", "Registry.pol");
                var userRegistryPath = Path.Combine(gpoBasePath, "User", "Registry.pol");

                // For now, treat all registry policies as machine policies
                // TODO: Add proper user/machine targeting when the model supports it
                var machinePolicies = policies;
                var userPolicies = new List<Policy>();

                if (machinePolicies.Any())
                {
                    CreateLGPOTextFile(machinePolicies, machineRegistryPath, "Machine");
                }

                if (userPolicies.Any())
                {
                    CreateLGPOTextFile(userPolicies, userRegistryPath, "User");
                }

                Logger.Instance.WriteLog($"Added registry policies to GPO at {gpoBasePath}");
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLog($"Error adding registry policies: {ex.Message}");
                throw;
            }
        }

        private void CreateLGPOTextFile(List<Policy> policies, string outputPath, string target)
        {
            var lgpoTextPath = Path.Combine(Path.GetDirectoryName(outputPath)!, $"{Path.GetFileNameWithoutExtension(outputPath)}.txt");

            using (var writer = new StreamWriter(lgpoTextPath))
            {
                foreach (var policy in policies)
                {
                    if (policy.Implementation?.Registry == null) continue;

                    writer.WriteLine($"{target}");
                    writer.WriteLine($"Software\\Policies\\{policy.Implementation.Registry.Key}");
                    writer.WriteLine(policy.Implementation.Registry.ValueName);

                    string valueType = policy.Implementation.Registry.ValueType.ToUpper();
                    string value = policy.ValueConstraints?.RequiredValues?.FirstOrDefault()?.Value ?? "";

                    writer.WriteLine($"{valueType}:{value}");
                    writer.WriteLine();
                }
            }

            Logger.Instance.WriteLog($"Created LGPO text file at {lgpoTextPath}");
        }

        public void AddSeceditPolicies(List<Policy> policies)
        {
            if (!policies.Any()) return;

            try
            {
                var infPath = Path.Combine(gpoBasePath, "Machine", "Microsoft", "Windows NT", "SecEdit", "GptTmpl.inf");
                CreateSeceditInfFile(policies, infPath);
                Logger.Instance.WriteLog($"Added secedit policies to GPO at {infPath}");
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLog($"Error adding secedit policies: {ex.Message}");
                throw;
            }
        }

        private void CreateSeceditInfFile(List<Policy> policies, string outputPath)
        {
            using (var writer = new StreamWriter(outputPath))
            {
                writer.WriteLine("[Version]");
                writer.WriteLine("signature=\"$CHICAGO$\"");
                writer.WriteLine("Revision=1");
                writer.WriteLine();

                // Group policies by section
                var sections = policies
                    .Where(p => p.Implementation?.Secedit != null)
                    .GroupBy(p => p.Implementation.Secedit.Section)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var section in sections)
                {
                    writer.WriteLine($"[{section.Key}]");
                    foreach (var policy in section.Value)
                    {
                        var value = policy.ValueConstraints?.RequiredValues?.FirstOrDefault()?.Value ?? "";
                        writer.WriteLine($"{policy.Implementation.Secedit.TemplateSetting}={value}");
                    }
                    writer.WriteLine();
                }
            }
        }
    }
}
