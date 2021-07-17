using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using InvoiceProcessor.Common.Core;

namespace InvoiceProcessor.Common.Services
{
    public class InvoiceSetSerializer : IInvoiceSetSerializer
    {
        public string SerializeToXml(InvoiceSet invoiceSet)
        {
            if (invoiceSet is null)
            {
                throw new ArgumentNullException(nameof(invoiceSet));
            }

            return SerializeToXmlInternal(invoiceSet);
        }

        public InvoiceSet DeserializeFromXml(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                throw new ArgumentException($"'{nameof(xml)}' cannot be null or empty.", nameof(xml));
            }

            return DeserializeFromXmlInternal<InvoiceSet>(xml);
        }

        private static string SerializeToXmlInternal(object model)
        {
            using var writer = new StringWriter();
            using var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true
            });
            var serializer = new XmlSerializer(model.GetType());
            serializer.Serialize(xmlWriter, model);
            return writer.ToString();
        }

        private static TType DeserializeFromXmlInternal<TType>(string xml)
        {
            using var stringReader = new StringReader(xml);
            using var xmlReader = XmlReader.Create(stringReader);
            var serializer = new XmlSerializer(typeof(TType));
            return (TType)serializer.Deserialize(xmlReader);
        }
    }
}
