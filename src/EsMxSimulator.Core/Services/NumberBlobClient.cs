using Azure.Storage.Blobs;
using EsMxSimulator.Core.Models;
using EsMxSimulator.Core.Options;
using Microsoft.Extensions.Options;

namespace EsMxSimulator.Core.Services;

public class NumberBlobClient : INumberBlobClient
{
    private readonly NumberBlobClientOptions _options;

    public NumberBlobClient(IOptions<NumberBlobClientOptions> options)
    {
        _options = options.Value;
    }


    public async Task<Number> DownloadAsync(int newNumber, string voiceName)
    {
        BlobClient blobClient = await GetBlobClient(newNumber, voiceName);

        if (!await blobClient.ExistsAsync())
        {
            return null;
        }

        var response = await blobClient.DownloadContentAsync();

        return new Number
        {
            Name = voiceName,
            Voice = response.Value.Content.ToArray()
        };
    }

    public async Task UploadAsync(int newNumber, string voiceName, byte[] number)
    {
        BlobClient blobClient = await GetBlobClient(newNumber, voiceName);

        await blobClient.UploadAsync(new BinaryData(number), true);
    }

    private async Task<BlobClient> GetBlobClient(int newNumber, string voiceName)
    {
        var containerClient = await GetBlobContainerClient();

        var fileName = GetFileName(newNumber, voiceName);

        return containerClient.GetBlobClient(fileName);
    }

    private async Task<BlobContainerClient> GetBlobContainerClient()
    {
        var containerClient = new BlobContainerClient(_options.ConnectionString, _options.ContainerName);
        await containerClient.CreateIfNotExistsAsync();
        return containerClient;
    }

    private static string GetFileName(int newNumber, string voiceName)
    {
        return $"{newNumber:D8}_{voiceName}.wav";
    }
}