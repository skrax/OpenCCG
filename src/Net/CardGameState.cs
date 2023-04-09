using System;
using OpenCCG.Data;

namespace OpenCCG.Net;

public class CardGameState
{
    public readonly Guid Id = Guid.NewGuid();
    public CardRecord Record { get; init; }

    public CardZone Zone { get; set; }
}