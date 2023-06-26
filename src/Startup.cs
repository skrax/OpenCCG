using System.Linq;
using Godot;

namespace OpenCCG;

public partial class Startup : Node
{
    [Export] private PackedScene _menuScene, _serverScene;

    public override void _Ready()
    {
        if (OS.HasFeature("server") || OS.GetCmdlineArgs().Contains("--listen"))
        {
            GetTree().ChangeSceneToPacked(_serverScene);
        }
        else
        {
            GetTree().ChangeSceneToPacked(_menuScene);
        }
    }
}