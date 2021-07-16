using System.Collections.Generic;
using System.Xml.Serialization;

namespace Common.Core
{
    [XmlRoot(nameof(Root))]
    public class Root
    {
        [XmlArray(nameof(Invoices))]
        [XmlArrayItem(nameof(Invoice), Type = typeof(Invoice))]
        public List<Invoice> Invoices { get; set; }
    }
}
