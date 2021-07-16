using System;
using System.IO;
using System.Xml;
using System.Xml.Xsl;

namespace InvoiceProcessor.Common.Transformations
{
    public class XsltTransformationService
    {
        public string TransformClient1XmlToCoreXml(string sourceXml)
        {
            var xslt = GetResource("Client1Transformation.xslt");

            var xslCompiledTransform = new XslCompiledTransform();
            using (var stringReader = new StringReader(xslt))
            {
                using var xmlReader = XmlReader.Create(stringReader);
                xslCompiledTransform.Load(xmlReader);
            }

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

        private static string GetResource(string resourceName)
        {
            var manifestName = $"{typeof(XsltTransformationService).Namespace}.{resourceName}";
            using var stream = typeof(XsltTransformationService).Assembly.GetManifestResourceStream(manifestName) ?? throw new InvalidOperationException($"Resource not found: {manifestName}");
            using var reader = new StreamReader(stream);
            var resource = reader.ReadToEnd();
            return resource;
        }
    }
}
