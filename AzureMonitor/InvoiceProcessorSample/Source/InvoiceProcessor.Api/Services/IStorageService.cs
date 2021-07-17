using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace InvoiceProcessor.Api.Services
{
    public interface IStorageService
    {
        Task UploadAsync(IFormFile file, string containerName, string path, CancellationToken cancellationToken);
    }
}
