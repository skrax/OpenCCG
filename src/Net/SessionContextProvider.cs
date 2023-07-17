using System.Collections.Generic;
using OpenCCG.Core;

namespace OpenCCG.Net;

public static class SessionContextProvider
{
    public static readonly Dictionary<long, SessionContext> SessionContextsByPeerId = new();
}