using System.Collections.Generic;
using System.Xml.Serialization;

namespace Techolics_
{
    // Data classes

    [XmlRoot("CIS_Benchmark")]
    public class CISBenchmark
    {
        [XmlElement("Section")]
        public List<Section> Sections { get; set; } = new List<Section>();
    }

    public class Section
    {
        [XmlAttribute("id")]
        public string Id { get; set; } = "";

        [XmlAttribute("title")]
        public string Title { get; set; } = "";

        [XmlElement("Section")]
        public List<Section>? SubSections { get; set; }

        [XmlElement("Policy")]
        public List<Policy>? Policies { get; set; }
    }

    public class Policy
    {
        [XmlAttribute("id")]
        public string Id { get; set; } = "";

        [XmlAttribute("mode")]
        public string Mode { get; set; } = "";

        [XmlAttribute("profile")]
        public string Profile { get; set; } = "";

        [XmlAttribute("title")]
        public string Title { get; set; } = "";

        [XmlAttribute("value_type")]
        public string ValueType { get; set; } = "";

        [XmlElement("DefaultValue")]
        public DefaultValue? DefaultValue { get; set; }

        [XmlElement("ValueConstraints")]
        public ValueConstraints? ValueConstraints { get; set; }

        [XmlElement("Implementation")]
        public Implementation? Implementation { get; set; }
    }

    public class DefaultValue
    {
        [XmlAttribute("domain")]
        public string? Domain { get; set; }

        [XmlAttribute("standalone")]
        public string? Standalone { get; set; }

        [XmlAttribute("value")]
        public string? Value { get; set; }
    }

    public class ValueConstraints
    {
        [XmlElement("RequiredValue")]
        public List<RequiredValue>? RequiredValues { get; set; }
    }

    public class RequiredValue
    {
        [XmlAttribute("operator")]
        public string Operator { get; set; } = "";

        [XmlAttribute("value")]
        public string Value { get; set; } = "";
    }

    public class Implementation
    {
        [XmlElement("Registry")]
        public RegistryImplementation? Registry { get; set; }

        [XmlElement("Secedit")]
        public SeceditImplementation? Secedit { get; set; }

        [XmlElement("GroupPolicy")]
        public GroupPolicyImplementation? GroupPolicy { get; set; }
    }

    public class RegistryImplementation
    {
        [XmlElement("Key")]
        public string Key { get; set; } = "";

        [XmlElement("ValueName")]
        public string ValueName { get; set; } = "";

        [XmlElement("ValueType")]
        public string ValueType { get; set; } = "";
    }

    public class SeceditImplementation
    {
        [XmlElement("TemplateSetting")]
        public string TemplateSetting { get; set; } = "";
    }

    public class GroupPolicyImplementation
    {
        [XmlElement("PolicyPath")]
        public string PolicyPath { get; set; } = "";

        [XmlElement("PolicyName")]
        public string PolicyName { get; set; } = "";
    }

    [XmlRoot("CIS_Benchmark_Documentation")]
    public class CISBenchmarkDocumentation
    {
        [XmlElement("Policy")]
        public List<DocumentationPolicy> Policies { get; set; } = new List<DocumentationPolicy>();
    }

    public class DocumentationPolicy
    {
        [XmlAttribute("id")]
        public string Id { get; set; } = "";

        [XmlElement("Documentation")]
        public Documentation? Documentation { get; set; }
    }

    public class Documentation
    {
        [XmlElement("Title")]
        public Title? Title { get; set; }

        [XmlElement("ProfileApplicability")]
        public ProfileApplicability? ProfileApplicability { get; set; }

        [XmlElement("Description")]
        public Description? Description { get; set; }

        [XmlElement("Rationale")]
        public Rationale? Rationale { get; set; }

        [XmlElement("Impact")]
        public Impact? Impact { get; set; }

        [XmlElement("Audit")]
        public Audit? Audit { get; set; }

        [XmlElement("Remediation")]
        public Remediation? Remediation { get; set; }

        [XmlElement("DefaultValue")]
        public DefaultValueText? DefaultValue { get; set; }

        [XmlElement("References")]
        public References? References { get; set; }
    }

    public class Title
    {
        [XmlText]
        public string Text { get; set; } = "";
    }

    public class ProfileApplicability
    {
        [XmlElement("Text")]
        public string Text { get; set; } = "";
    }

    public class Description
    {
        [XmlElement("Text")]
        public string Text { get; set; } = "";
    }

    public class Rationale
    {
        [XmlElement("Text")]
        public string Text { get; set; } = "";
    }

    public class Impact
    {
        [XmlElement("Text")]
        public string Text { get; set; } = "";
    }

    public class Audit
    {
        [XmlElement("Text")]
        public string Text { get; set; } = "";
    }

    public class Remediation
    {
        [XmlElement("Text")]
        public string Text { get; set; } = "";

        [XmlElement("CodeBlock")]
        public CodeBlock? CodeBlock { get; set; }
    }

    public class CodeBlock
    {
        [XmlElement("Line")]
        public List<string>? Lines { get; set; }
    }

    public class DefaultValueText
    {
        [XmlElement("Text")]
        public string Text { get; set; } = "";
    }

    public class References
    {
        [XmlElement("Reference")]
        public List<Reference>? ReferenceList { get; set; }
    }

    public class Reference
    {
        [XmlAttribute("url")]
        public string Url { get; set; } = "";

        [XmlText]
        public string Text { get; set; } = "";
    }

    // Item class to represent data for the DataGrid
    public class Item
    {
        public bool IsSelected { get; set; }
        public string ID { get; set; } = "";
        public string Profile { get; set; } = "";
        public string Name { get; set; } = "";
        public string Current { get; set; } = "";
        public string Status { get; set; } = "";
        public string Description { get; set; } = "";
        public string DefaultValue { get; set; } = "";
        public Policy? Policy { get; set; } // Added property
    }
}
