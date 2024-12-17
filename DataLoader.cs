using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace Techolics_
{
    public class DataLoader
    {
        public CISBenchmark LoadBenchmarkValues(string resourceName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CISBenchmark));

            // Load the XML content from the embedded resource
            using (var stream = GetResourceStream(resourceName))
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

            // Load the XML content from the embedded resource
            using (var stream = GetResourceStream(resourceName))
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
            var resourcePath = $"{assembly.GetName().Name}.{resourceName.Replace('/', '.')}"; // Format for embedded resource

            var stream = assembly.GetManifestResourceStream(resourcePath);
            if (stream == null)
            {
                throw new FileNotFoundException($"Resource not found: {resourcePath}");
            }

            return stream;
        }
    }
}
