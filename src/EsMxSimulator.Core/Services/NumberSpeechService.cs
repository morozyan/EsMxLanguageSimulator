using EsMxSimulator.Core.Models;
using EsMxSimulator.Core.Options;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Options;

namespace EsMxSimulator.Core.Services;

public class NumberSpeechService : INumberSpeechService
{
    private readonly SpeechOptions _options;
    private readonly VoiceOptions _voiceOptions;
    private readonly INumberBlobClient _numberBlobClient;

    public NumberSpeechService(IOptions<SpeechOptions> options, IOptions<VoiceOptions> voiceOptions, INumberBlobClient numberBlobClient)
    {
        _options = options.Value;
        _voiceOptions = voiceOptions.Value;

        _numberBlobClient = numberBlobClient;
    }

    public async Task<Number> Generate(int newNumber, CancellationToken cancellationToken)
    {
        var voice = ChooseVoice();

        var number = await _numberBlobClient.DownloadAsync(newNumber, voice, cancellationToken)
            ?? await GenerateNumber(newNumber, voice);

        await _numberBlobClient.UploadAsync(newNumber, voice, number.Voice, cancellationToken);
        return number;
    }

    private async Task<Number> GenerateNumber(int newNumber, string voice)
    {
        var config = CreateConfig(voice);

        using var synthesizer = new SpeechSynthesizer(config, AudioConfig.FromStreamOutput(AudioOutputStream.CreatePullStream()));
        using var result = await synthesizer.SpeakTextAsync($"{newNumber}");

        return new Number
        {
            Name = voice,
            Voice = result.AudioData
        };
    }

    private string ChooseVoice()
    {
        var r = new Random();

        var voiceIndex = r.Next(0, _voiceOptions.Voices.Length);

        return _voiceOptions.Voices[voiceIndex];
    }

    private SpeechConfig CreateConfig(string voiceName)
    {

        var config = SpeechConfig.FromSubscription(_options.SubscriptionKey, _options.SubscriptionRegion);
        config.SpeechSynthesisVoiceName = voiceName;
        return config;
    }
}
