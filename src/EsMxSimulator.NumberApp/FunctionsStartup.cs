using EsMxSimulator.Core.Options;
using EsMxSimulator.Core.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Reflection;

[assembly: FunctionsStartup(typeof(EsMxSimulator.NumberApp.Startup))]

namespace EsMxSimulator.NumberApp;

public class Startup : FunctionsStartup
{
    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
        var context = builder.GetContext();

        builder.ConfigurationBuilder
            .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), optional: true, reloadOnChange: false)
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
            .AddEnvironmentVariables();
    }

    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddMemoryCache((options) =>
        {
            options.SizeLimit = 100 * 1024 * 1024;
        });

        builder.Services.AddTransient<INumberSimulator, NumberSimulator>();
        builder.Services.AddTransient<INumberSpeechService, NumberSpeechService>();
        builder.Services.AddTransient<INumberBlobClient, NumberBlobClient>();

        builder.Services.AddOptions<SpeechOptions>().Configure<IConfiguration>((option, configuration) =>
        {
            configuration.GetSection(nameof(SpeechOptions)).Bind(option);
        });

        builder.Services.AddOptions<VoiceOptions>().Configure<IConfiguration>((option, configuration) =>
        {
            configuration.GetSection(nameof(VoiceOptions)).Bind(option);
        });

        builder.Services.AddOptions<NumberBlobClientOptions>().Configure<IConfiguration>((option, configuration) =>
        {
            configuration.GetSection(nameof(NumberBlobClientOptions)).Bind(option);
        });

        builder.Services.AddAzureClients(clientBuilder =>
        {
            var configuration = builder.GetContext().Configuration;
            clientBuilder.AddBlobServiceClient(configuration.GetValue<string>("AzureBlobStorage"));
        });
    }
}
