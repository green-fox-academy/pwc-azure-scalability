using System.Xml.Serialization;

namespace Common.Core
{
    public class InvoiceLineItem
    {
        [XmlAttribute(nameof(Name))]
        public string Name { get; set; }

        [XmlAttribute(nameof(Quantity))]
        public int Quantity { get; set; }

        [XmlAttribute(nameof(UnitAmount))]
        public decimal UnitAmount { get; set; }
    }
}
