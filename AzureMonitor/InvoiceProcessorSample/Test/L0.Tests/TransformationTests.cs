using Common.Transformations;
using FluentAssertions;
using System.IO;
using Xunit;

namespace L0.Tests
{
    public class XsltTransformationServiceTests
    {
        private readonly string _coreInvoicesSample;
        private readonly XsltTransformationService _sut;

        public XsltTransformationServiceTests()
        {
            _coreInvoicesSample = File.ReadAllText("./TestFiles/CoreInvoicesSample.xml");
            _sut = new XsltTransformationService();
        }

        [Fact]
        public void Client1TransformationTest()
        {
            var result = _sut.TransformClient1XmlToCoreXml(File.ReadAllText("./TestFiles/Client1InvoicesSample.xml"));
            result.Should().Be(_coreInvoicesSample);
        }
    }
}
