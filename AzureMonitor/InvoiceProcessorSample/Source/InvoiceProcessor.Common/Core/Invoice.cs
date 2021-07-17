using System;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace InvoiceProcessor.Common.Core
{
    public class Invoice
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "customer")]
        public string Customer { get; set; }

        [JsonProperty(PropertyName = "status")]
        public InvoiceStatus Status { get; set; }

        [JsonProperty(PropertyName = "createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty(PropertyName = "sentAt")]
        public DateTime? SentAt { get; set; }

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
