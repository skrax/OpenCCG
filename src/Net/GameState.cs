using System.Collections.Generic;

namespace OpenCCG.Net;

public class GameState
{
    public readonly Dictionary<long, PlayerGameState> PlayerGameStates = new();
}