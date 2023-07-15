using EsMxSimulator.Core.Models;

namespace EsMxSimulator.Core.Services;

public interface INumberBlobClient
{
    Task<Number> DownloadAsync(int newNumber, string voiceName, CancellationToken cancellationToken);
    Task UploadAsync(int newNumber, string voiceName, byte[] number, CancellationToken cancellationToken);
}