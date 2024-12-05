using System.Collections.Generic;
using System.ComponentModel;
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
    public class Item : INotifyPropertyChanged
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        private string _id = "";
        public string ID
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(ID));
                }
            }
        }

        private string _profile = "";
        public string Profile
        {
            get => _profile;
            set
            {
                if (_profile != value)
                {
                    _profile = value;
                    OnPropertyChanged(nameof(Profile));
                }
            }
        }

        private string _name = "";
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private string _current = "";
        public string Current
        {
            get => _current;
            set
            {
                if (_current != value)
                {
                    _current = value;
                    OnPropertyChanged(nameof(Current));
                }
            }
        }

        private string _status = "";
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        private string _description = "";
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        private string _defaultValue = "";
        public string DefaultValue
        {
            get => _defaultValue;
            set
            {
                if (_defaultValue != value)
                {
                    _defaultValue = value;
                    OnPropertyChanged(nameof(DefaultValue));
                }
            }
        }

        private Policy? _policy;
        public Policy? Policy
        {
            get => _policy;
            set
            {
                if (_policy != value)
                {
                    _policy = value;
                    OnPropertyChanged(nameof(Policy));
                }
            }
        }

        // Implement INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}