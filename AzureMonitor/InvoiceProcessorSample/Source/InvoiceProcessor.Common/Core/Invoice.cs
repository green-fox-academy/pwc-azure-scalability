using System;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace InvoiceProcessor.Common.Core
{
    public class Invoice
    {
        public const string PartitionKey = "/customer";

        [XmlIgnore]
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [XmlIgnore]
        [JsonProperty(PropertyName = "customer")]
        public string Customer { get; set; }

        [XmlIgnore]
        [JsonProperty(PropertyName = "status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public InvoiceStatus Status { get; set; }

        [XmlIgnore]
        [JsonProperty(PropertyName = "createdAt")]
        public DateTime CreatedAt { get; set; }

        [XmlIgnore]
        [JsonProperty(PropertyName = "sentAt")]
        public DateTime? SentAt { get; set; }

        [XmlIgnore]
        [JsonProperty(PropertyName = "verifiedAt")]
        public DateTime? VerifiedAt { get; set; }

        [XmlAttribute(nameof(InvoiceNumber))]
        [JsonProperty(PropertyName = "invoiceNumber")]
        public string InvoiceNumber { get; set; }

        [XmlAttribute(nameof(ContactName))]
        [JsonProperty(PropertyName = "contactName")]
        public string ContactName { get; set; }

        [XmlAttribute(nameof(TotalAmount))]
        [JsonProperty(PropertyName = "totalAmount")]
        public decimal TotalAmount { get; set; }

        [XmlArray(nameof(LineItems))]
        [XmlArrayItem(nameof(InvoiceLineItem), Type = typeof(InvoiceLineItem))]
        [JsonProperty(PropertyName = "lineItems")]
        public InvoiceLineItem[] LineItems { get; set; }
    }
}
