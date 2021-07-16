using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using FluentAssertions;
using InvoiceProcessor.Common.Core;
using Xunit;

namespace InvoiceProcessor.L0.Tests
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
                Invoices = new[]
                {
                    new Invoice
                    {
                        ContactName = "Contact",
                        InvoiceNumber = "#111",
                        TotalAmount = 100,
                        LineItems = new[]
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
                        LineItems = new[]
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
            using var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true
            });
            var serializer = new XmlSerializer(model.GetType());
            serializer.Serialize(xmlWriter, model);
            return writer.ToString();
        }

        private static TType DeserializeFromXml<TType>(string xml)
        {
            using var stringReader = new StringReader(xml);
            using var xmlReader = XmlReader.Create(stringReader);
            var serializer = new XmlSerializer(typeof(TType));
            return (TType)serializer.Deserialize(xmlReader);
        }
    }
}
