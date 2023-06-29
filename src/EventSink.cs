using System;

namespace OpenCCG;

public static class EventSink
{
    public static Action? OnDragCardStart;
    public static Action? OnDragCardStop;
    public static Action<ulong>? OnDragForCombatStart;
    public static Action<ulong>? OnDragForCombatStop;
}