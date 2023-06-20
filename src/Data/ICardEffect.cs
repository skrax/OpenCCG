using System.Threading.Tasks;
using OpenCCG.Net;

namespace OpenCCG.Data;

public interface ICardEffect
{
    public string GetText();

    public Task ExecuteAsync(CardGameState card, PlayerGameState playerGameState);
}