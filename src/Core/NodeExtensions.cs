using Godot;

namespace OpenCCG.Core;

public static class NodeExtensions
{
    public static T GetAutoloaded<T>(this Node node) where T : class
    {
        return node.GetNode<T>($"/root/{typeof(T).Name}");
    }
}