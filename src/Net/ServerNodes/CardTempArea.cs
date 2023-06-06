using System;
using Godot;

namespace OpenCCG.Net.ServerNodes;

public partial class CardTempArea : Node
{
    [Rpc]
    public void ShowPermanent(string imgPath)
    {
        throw new NotImplementedException();
    }

    [Rpc]
    public void Show(string imgPath, string timeSpan)
    {
        throw new NotImplementedException();
    }

    [Rpc]
    public void Reset()
    {
        throw new NotImplementedException();
    }

    [Rpc]
    public void RequireTargets(string requestId, string imgPath)
    {
        throw new NotImplementedException();
    }
}