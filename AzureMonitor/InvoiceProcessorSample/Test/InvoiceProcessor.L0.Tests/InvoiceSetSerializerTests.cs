using System.IO;
using FluentAssertions;
using InvoiceProcessor.Common.Core;
using InvoiceProcessor.Common.Services;
using Xunit;

namespace InvoiceProcessor.L0.Tests
{
    public class InvoiceSetSerializerTests
    {
        private readonly InvoiceSetSerializer _sut;
        private readonly InvoiceSet _invoicePayload;
        private readonly string _sampleXml;

        public InvoiceSetSerializerTests()
        {
            _sut = new InvoiceSetSerializer();
            _sampleXml = File.ReadAllText("./TestFiles/CoreInvoicesSample.xml");
            _invoicePayload = new InvoiceSet
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
        public void TestInvoicePayloadSerialization()
        {
            var xml = _sut.SerializeToXml(_invoicePayload);
            xml.Should().Be(_sampleXml);
        }

        [Fact]
        public void TestInvoicePayloadDeserialization()
        {
            var invoicePayload = _sut.DeserializeFromXml(_sampleXml);
            invoicePayload.Should().BeEquivalentTo(_invoicePayload);
        }
    }
}
