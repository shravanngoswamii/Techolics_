using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace Techolics_
{
    public class DataLoader
    {
        public CISBenchmark LoadBenchmarkValues(string resourceName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CISBenchmark));
            using (Stream stream = GetResourceStream(resourceName))
            {
                var result = serializer.Deserialize(stream) as CISBenchmark;
                if (result == null)
                {
                    throw new InvalidOperationException("Deserialization returned null.");
                }
                return result;
            }
        }

        public CISBenchmarkDocumentation LoadBenchmarkDocumentation(string resourceName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CISBenchmarkDocumentation));
            using (Stream stream = GetResourceStream(resourceName))
            {
                var result = serializer.Deserialize(stream) as CISBenchmarkDocumentation;
                if (result == null)
                {
                    throw new InvalidOperationException("Deserialization returned null.");
                }
                return result;
            }
        }

        private Stream GetResourceStream(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string? resourcePath = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase));

            if (resourcePath == null)
                throw new FileNotFoundException($"Resource not found: {resourceName}");

            // Ensure the stream is not null
            return assembly.GetManifestResourceStream(resourcePath)
                ?? throw new FileNotFoundException($"Resource stream could not be loaded for: {resourceName}");
        }
    }
}
