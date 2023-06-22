using EsMxSimulator.Core.Models;

namespace EsMxSimulator.Core.Services;

public interface INumberSpeechService
{
    Task<Number> Generate(int newNumber);
}