using System.Collections.Generic;
using System.Linq;
using Godot;

namespace OpenCCG.Core;

public static class EnumerableExtensions
{
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> enumerable) => enumerable.OrderBy(_ => GD.Randi());
}