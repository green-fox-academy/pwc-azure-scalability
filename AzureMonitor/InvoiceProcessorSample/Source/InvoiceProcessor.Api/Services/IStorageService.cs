using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace InvoiceProcessor.Api.Services
{
    public interface IStorageService
    {
        Task UploadAsync(Stream content, string containerName, string path, CancellationToken cancellationToken);
    }
}
