using System.Collections.Generic;

namespace OpenCCG.Net;

public class GameState
{
    public readonly Dictionary<int, PlayerGameState> PlayerGameStates = new();
}