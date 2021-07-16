using System;
using System.IO;
using System.Xml;
using System.Xml.Xsl;

namespace InvoiceProcessor.Common.Transformations
{
    public class XslTransformationService : IXslTransformationService
    {
        public string TransformClient1XmlToCoreXml(string sourceXml)
        {
            var xslCompiledTransform = CreateXslCompiledTransformForClient1();
            using var writer = new StringWriter();
            using var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings { OmitXmlDeclaration = true });
            using (var stringReader = new StringReader(sourceXml))
            {
                using var xmlReader = XmlReader.Create(stringReader);
                xslCompiledTransform.Transform(xmlReader, null, xmlWriter);
            }

            var result = writer.ToString();
            return result;
        }

        public void TransformClient1XmlToCoreXml(Stream source, Stream target)
        {
            var xslCompiledTransform = CreateXslCompiledTransformForClient1();
            using var xmlWriter = XmlWriter.Create(target, new XmlWriterSettings { OmitXmlDeclaration = true });
            using var xmlReader = XmlReader.Create(source);
            xslCompiledTransform.Transform(xmlReader, null, xmlWriter);

            target.Position = 0;
        }

        private static XslCompiledTransform CreateXslCompiledTransformForClient1()
        {
            var xslt = GetResource("Client1Transformation.xslt");

            var xslCompiledTransform = new XslCompiledTransform();
            using (var stringReader = new StringReader(xslt))
            {
                using var xmlReader = XmlReader.Create(stringReader);
                xslCompiledTransform.Load(xmlReader);
            }

            return xslCompiledTransform;
        }

        private static string GetResource(string resourceName)
        {
            var manifestName = $"{typeof(XslTransformationService).Namespace}.{resourceName}";
            using var stream = typeof(XslTransformationService).Assembly.GetManifestResourceStream(manifestName) ?? throw new InvalidOperationException($"Resource not found: {manifestName}");
            using var reader = new StreamReader(stream);
            var resource = reader.ReadToEnd();
            return resource;
        }
    }
}
