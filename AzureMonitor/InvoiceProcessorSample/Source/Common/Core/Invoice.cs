using System.Collections.Generic;
using System.Xml.Serialization;

namespace Common.Core
{
    public class Invoice
    {
        [XmlAttribute(nameof(InvoiceNumber))]
        public string InvoiceNumber { get; set; }

        [XmlAttribute(nameof(ContactName))]
        public string ContactName { get; set; }

        [XmlAttribute(nameof(TotalAmount))]
        public decimal TotalAmount { get; set; }

        [XmlArray(nameof(LineItems))]
        [XmlArrayItem(nameof(InvoiceLineItem), Type = typeof(InvoiceLineItem))]
        public List<InvoiceLineItem> LineItems { get; set; }
    }
}
