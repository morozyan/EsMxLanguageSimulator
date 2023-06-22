using EsMxSimulator.Core.Models;

namespace EsMxSimulator.Core.Services;

public interface INumberBlobClient
{
    Task<Number> DownloadAsync(int newNumber, string voiceName);
    Task UploadAsync(int newNumber, string voiceName, byte[] number);
}