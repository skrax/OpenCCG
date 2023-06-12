using Godot;

namespace OpenCCG.Core;

public static class PackedSceneExt
{
    public static T Make<T>(this PackedScene scene, Node parent) where T : Node
    {
        var instance = scene.Instantiate<T>();
        parent.AddChild(instance);

        return instance;
    }

    public static T Make<T, TParam>(this PackedScene packedScene, TParam param, Node parent)
        where T : Node, INodeInit<TParam>
    {
        var instance = packedScene.Instantiate<T>();
        parent.AddChild(instance);

        instance.Init(param);

        return instance;
    }


    public static T Make<T, TParam>(this PackedScene packedScene, TParam param)
        where T : Node, INodeInit<TParam>
    {
        var instance = packedScene.Instantiate<T>();

        instance.Init(param);

        return instance;
    }
}