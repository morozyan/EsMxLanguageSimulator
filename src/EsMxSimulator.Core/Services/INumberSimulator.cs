using EsMxSimulator.Core.Models;

namespace EsMxSimulator.Core.Services;

public interface INumberSimulator
{
    Task<Turn> GuessNumber(int start, int end);
}