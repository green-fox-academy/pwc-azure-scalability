using Microsoft.Azure.Cosmos.Table;

namespace InvoiceProcessor.Functions.Activities
{
    public class InvoiceSetEntity : TableEntity
    {
        public string Customer { get; set; }
        public string OriginalFileUri { get; set; }
        public string TransformedFileUri { get; set; }
    }
}
