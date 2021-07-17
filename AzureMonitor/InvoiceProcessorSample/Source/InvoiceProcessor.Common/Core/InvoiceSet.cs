using System.Xml.Serialization;

namespace InvoiceProcessor.Common.Core
{
    [XmlRoot("Root")]
    public class InvoiceSet
    {
        [XmlArray(nameof(Invoices))]
        [XmlArrayItem(nameof(Invoice), Type = typeof(Invoice))]
        public Invoice[] Invoices { get; set; }
    }
}
