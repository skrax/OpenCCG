using Godot;
using OpenCCG.Core;

namespace OpenCCG;

public partial class Avatar : TextureRect
{
    [Export] public bool IsEnemy;

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        var obj = InstanceFromId(data.As<ulong>());

        return obj switch
        {
            CardBoard => IsEnemy,
            CardEffectPreview => true,
            _ => false
        };
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        var obj = InstanceFromId(data.As<ulong>());

        switch (obj)
        {
            case CardBoard attacker:
            {
                Logger.Info<CardBoard>($"{attacker!.CardGameState.Id} attacked avatar");

                GetNode<Main>("/root/Main").CombatPlayer(attacker.CardGameState.Id);
                break;
            }
            case CardEffectPreview effect:
            {
                effect.TryUpstreamTarget(this);
                break;
            }
        }
    }
}