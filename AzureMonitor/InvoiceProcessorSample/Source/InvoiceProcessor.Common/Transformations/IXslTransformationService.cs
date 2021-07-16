using System.IO;

namespace InvoiceProcessor.Common.Transformations
{
    public interface IXslTransformationService
    {
        string TransformClient1XmlToCoreXml(string sourceXml);
        void TransformClient1XmlToCoreXml(Stream source, Stream target);
    }
}
