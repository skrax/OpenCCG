using System.Threading.Tasks;
using OpenCCG.Net;

namespace OpenCCG.Data;

public interface ICardEffect
{
    public string GetText();

    public Task Execute(CardGameState card, PlayerGameState playerGameState);
}