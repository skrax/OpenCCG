using Godot;
using OpenCCG.Core;

namespace OpenCCG;

public partial class Avatar : TextureRect
{
    [Export] public bool IsEnemy;

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        return IsEnemy;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        var attacker = InstanceFromId(data.As<ulong>()) as CardBoard;

        Logger.Info<CardBoard>($"{attacker!.CardGameState.Id} attacked avatar");

        GetNode<Main>("/root/Main").CombatPlayer(attacker.CardGameState.Id);
    }
}