using InvoiceProcessor.Common.Core;

namespace InvoiceProcessor.Common.Services
{
    public interface IInvoiceSetSerializer
    {
        InvoiceSet DeserializeFromXml(string xml);
        string SerializeToXml(InvoiceSet invoiceSet);
    }
}
