using EsMxSimulator.Core.Models;

namespace EsMxSimulator.Core.Services;

public class NumberSimulator : INumberSimulator
{
    private readonly INumberSpeechService _speechService;

    public NumberSimulator(INumberSpeechService speechService)
    {
        _speechService = speechService;
    }

    public async Task<Turn> GuessNumber(int start, int end)
    {
        var newNumber = GenerateNumber(start, end);

        var number = await _speechService.Generate(newNumber);

        return new Turn
        {
            Number = newNumber,
            Name = number.Name,
            Voice = number.Voice
        };
    }

    private static int GenerateNumber(int start, int end)
    {
        var r = new Random();

        var newNumber = r.Next(start, end+1);
        return newNumber;
    }
}