using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenCCG.Core;
using OpenCCG.Net.Api;

namespace OpenCCG;

public partial class EnemyHandArea : Area2D, IEnemyHandRpc
{
    private static readonly PackedScene CardHiddenScene = GD.Load<PackedScene>("res://scenes/card-hidden.tscn");
    private readonly List<Sprite2D> _cards = new();

    [Rpc]
    public void RemoveCard()
    {
        var cardEntity = _cards.LastOrDefault();
        if (cardEntity == null) return;

        _cards.Remove(cardEntity);
        cardEntity.QueueFree();
        SetCardPositions();
    }

    [Rpc]
    public void DrawCard()
    {
        var entity = CardHiddenScene.Make<Sprite2D>(this);
        _cards.Add(entity);
        SetCardPositions();
    }

    private void SetCardPositions()
    {
        SpriteHelpers.OrderHorizontally(_cards.ToArray());
    }
}