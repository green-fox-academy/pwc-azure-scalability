using System.IO;
using FluentAssertions;
using InvoiceProcessor.Common.Transformations;
using Xunit;

namespace InvoiceProcessor.L0.Tests
{
    public class XslTransformationServiceTests
    {
        private readonly string _coreInvoicesSample;
        private readonly string _client1InvoicesSamplePath;
        private readonly XslTransformationService _sut;

        public XslTransformationServiceTests()
        {
            _coreInvoicesSample = File.ReadAllText("./TestFiles/CoreInvoicesSample.xml");
            _client1InvoicesSamplePath = "./TestFiles/Client1InvoicesSample.xml";
            _sut = new XslTransformationService();
        }

        [Fact]
        public void Client1TransformationTest()
        {
            var result = _sut.TransformClient1XmlToCoreXml(File.ReadAllText(_client1InvoicesSamplePath));
            result.Should().Be(_coreInvoicesSample);
        }

        [Fact]
        public void Client1TransformationTest2()
        {
            using var memoryStream = new MemoryStream();
            using var fileStream = File.OpenRead(_client1InvoicesSamplePath);
            _sut.TransformClient1XmlToCoreXml(fileStream, memoryStream);

            using var reader = new StreamReader(memoryStream);
            var result = reader.ReadToEnd();

            result.Should().Be(_coreInvoicesSample);
        }
    }
}
