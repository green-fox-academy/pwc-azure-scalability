using Common.Core;
using FluentAssertions;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Xunit;

namespace L0.Tests
{
    public class InvoicePlayloadSerializationTests
    {
        private readonly Root _invoicePayload;
        private readonly string _sampleXml;

        public InvoicePlayloadSerializationTests()
        {
            _sampleXml = File.ReadAllText("./TestFiles/CoreInvoicesSample.xml");
            _invoicePayload = new Root
            {
                Invoices = new List<Invoice>
                {
                    new Invoice
                    {
                        ContactName = "Contact",
                        InvoiceNumber = "#111",
                        TotalAmount = 100,
                        LineItems = new List<InvoiceLineItem>
                        {
                            new InvoiceLineItem
                            {
                                Name = "item1",
                                Quantity = 1,
                                UnitAmount = 80
                            },
                            new InvoiceLineItem
                            {
                                Name = "item2",
                                Quantity = 1,
                                UnitAmount = 20
                            }
                        }
                    },
                    new Invoice
                    {
                        ContactName = "Contact",
                        InvoiceNumber = "#222",
                        TotalAmount = 100,
                        LineItems = new List<InvoiceLineItem>
                        {
                            new InvoiceLineItem
                            {
                                Name = "item1",
                                Quantity = 1,
                                UnitAmount = 60
                            },
                            new InvoiceLineItem
                            {
                                Name = "item2",
                                Quantity = 1,
                                UnitAmount = 40
                            }
                        }
                    }
                }
            };
        }

        [Fact]
        public void TestInvoicePlayloadSerialization()
        {
            var xml = SerializeToXml(_invoicePayload);
            xml.Should().Be(_sampleXml);
        }

        [Fact]
        public void TestInvoicePlayloadDeserialization()
        {
            var invoicePayload = DeserializeFromXml<Root>(_sampleXml);
            invoicePayload.Should().BeEquivalentTo(_invoicePayload);
        }

        private static string SerializeToXml(object model)
        {
            using var writer = new StringWriter();
            var serializer = new XmlSerializer(model.GetType());
            serializer.Serialize(writer, model);
            return writer.ToString();
        }

        private static TType DeserializeFromXml<TType>(string xml)
        {
            using var stringReader = new StringReader(xml);
            var serializer = new XmlSerializer(typeof(TType));
            return (TType)serializer.Deserialize(stringReader);
        }
    }
}
