using System.Collections.Generic;
using System.Xml.Serialization;

namespace InvoiceProcessor.Common.Core
{
    [XmlRoot(nameof(Root))]
    public class Root
    {
        [XmlArray(nameof(Invoices))]
        [XmlArrayItem(nameof(Invoice), Type = typeof(Invoice))]
        public Invoice[] Invoices { get; set; }
    }
}
