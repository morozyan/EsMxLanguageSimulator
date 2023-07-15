using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using EsMxSimulator.Core.Models;
using EsMxSimulator.Core.Options;
using Microsoft.Extensions.Caching.Memory;

namespace EsMxSimulator.Core.Services;

public class NumberBlobClient : INumberBlobClient
{
    private readonly BlobContainerClient _containerClient;
    private readonly IMemoryCache _memoryCache;

    public NumberBlobClient(IOptions<NumberBlobClientOptions> options, BlobServiceClient blobServiceClient, IMemoryCache memoryCache)
    {
        _containerClient = blobServiceClient.GetBlobContainerClient(options.Value.ContainerName);
        _memoryCache = memoryCache;
    }


    public async Task<Number> DownloadAsync(int newNumber, string voiceName, CancellationToken cancellationToken)
    {
        if (_memoryCache.TryGetValue<byte[]>(GetFileName(newNumber, voiceName), out var blobData))
        {
            return new Number
            {
                Name = voiceName,
                Voice = blobData
            };
        }

        BlobClient blobClient = await GetBlobClient(newNumber, voiceName);

        if (!await blobClient.ExistsAsync())
        {
            return null;
        }

        var response = await blobClient.DownloadContentAsync(cancellationToken);

        blobData = response.Value.Content.ToArray();

        _memoryCache.Set(GetFileName(newNumber, voiceName), blobData, new MemoryCacheEntryOptions 
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            Size = blobData.Length
        });

        return new Number
        {
            Name = voiceName,
            Voice = blobData
        };
    }

    public async Task UploadAsync(int newNumber, string voiceName, byte[] number, CancellationToken cancellationToken)
    {
        BlobClient blobClient = await GetBlobClient(newNumber, voiceName);

        await blobClient.UploadAsync(new BinaryData(number), true, cancellationToken);
    }

    private async Task<BlobClient> GetBlobClient(int newNumber, string voiceName)
    {
        var fileName = GetFileName(newNumber, voiceName);

        return _containerClient.GetBlobClient(fileName);
    }

    private static string GetFileName(int newNumber, string voiceName)
    {
        return $"{newNumber:D8}_{voiceName}.wav";
    }
}