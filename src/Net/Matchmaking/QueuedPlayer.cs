using System.Collections.Generic;
using OpenCCG.Cards;

namespace OpenCCG.Net.Matchmaking;

public record QueuedPlayer(long PeerId, List<ICardOutline> DeckList);