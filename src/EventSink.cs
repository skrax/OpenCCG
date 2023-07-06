using System;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG;

public static class EventSink
{
    public static Action? OnDragCardStart;
    public static Action? OnDragCardStop;
    public static Action<ulong>? OnDragForCombatStart;
    public static Action<ulong>? OnDragForCombatStop;
    public static Action<RequireTargetInputDto>? OnDragSelectTargetStart;
    public static Action? OnDragSelectTargetStop;
}