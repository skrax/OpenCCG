using OpenCCG.Net;

namespace OpenCCG.Data;

public interface ICardEffect
{
    public string GetText();

    public void Execute(CardGameState card, PlayerGameState playerGameState);
}