using System;
using System.IO;
using System.Xml.Serialization;

namespace Techolics_
{
    public class DataLoader
    {
        public CISBenchmark LoadBenchmarkValues(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CISBenchmark));
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                var result = serializer.Deserialize(fs) as CISBenchmark;
                if (result == null)
                {
                    throw new InvalidOperationException("Deserialization returned null.");
                }
                return result;
            }
        }

        public CISBenchmarkDocumentation LoadBenchmarkDocumentation(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CISBenchmarkDocumentation));
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                var result = serializer.Deserialize(fs) as CISBenchmarkDocumentation;
                if (result == null)
                {
                    throw new InvalidOperationException("Deserialization returned null.");
                }
                return result;
            }
        }
    }
}
