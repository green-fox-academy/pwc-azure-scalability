using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace InvoiceProcessor.Functions.Activities
{
    public class SaveInvoiceSetEntityActivity
    {
        [FunctionName(nameof(SaveInvoiceSetEntityActivity))]
        public async Task Run(
            [ActivityTrigger] InvoiceSetEntity entity,
            [Table("IncomingInvoiceSets")] CloudTable cloudTable,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var tableOperation = TableOperation.InsertOrReplace(entity);
            await cloudTable.ExecuteAsync(tableOperation, cancellationToken);
        }
    }
}
