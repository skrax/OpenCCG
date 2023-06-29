using System.Linq;
using Godot;
using OpenCCG.Core;

namespace OpenCCG;

public partial class Avatar : TextureRect
{
    [Export] public bool IsEnemy;

    public override void _Ready()
    {
        EventSink.OnDragForCombatStart += OnDragForCombatStart;
        EventSink.OnDragForCombatStop += OnDragForCombatStop;
    }

    private void OnDragForCombatStart(ulong instanceId)
    {
        if (!IsEnemy) return;
        var board = GetNode<BoardArea>("/root/Main/EnemyBoard");
        if (board._cards.Any(x => x.CardGameState.Record.Abilities.Defender)) return;

        var shader = Material as ShaderMaterial;
        shader?.SetShaderParameter("drawOutline", true);
    }

    private void OnDragForCombatStop(ulong instanceId)
    {
        if (!IsEnemy) return;

        var shader = Material as ShaderMaterial;
        shader?.SetShaderParameter("drawOutline", false);
    }

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