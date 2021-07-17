using System.Xml.Serialization;
using Newtonsoft.Json;

namespace InvoiceProcessor.Common.Core
{
    public class InvoiceLineItem
    {
        [XmlAttribute(nameof(Name))]
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [XmlAttribute(nameof(Quantity))]
        [JsonProperty(PropertyName = "quantity")]
        public int Quantity { get; set; }

        [XmlAttribute(nameof(UnitAmount))]
        [JsonProperty(PropertyName = "unitAmount")]
        public decimal UnitAmount { get; set; }
    }
}
