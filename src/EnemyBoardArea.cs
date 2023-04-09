using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Godot;
using OpenCCG.Core;
using OpenCCG.Net.Api;
using OpenCCG.Net.Dto;

namespace OpenCCG;

public partial class EnemyBoardArea : Area2D, IBoardRpc
{
    private static readonly PackedScene CardBoardScene = GD.Load<PackedScene>("res://scenes/card-board.tscn");

    private readonly List<CardBoard> _cards = new();

    private void SetCardPositions()
    {
        SpriteHelpers.OrderHorizontally(_cards.Cast<Sprite2D>().ToArray());
    }

    [Rpc]
    public void PlaceCard(string cardGameStateDtoJson)
    {
        var obj = JsonSerializer.Deserialize<CardGameStateDto>(cardGameStateDtoJson);
        var card = CardBoardScene.Make<CardBoard, CardGameStateDto>(obj, this);

        _cards.Add(card);

        SetCardPositions();
    }

    [Rpc]
    public void UpdateCard(string cardGameStateDtoJson)
    {
        var obj = JsonSerializer.Deserialize<CardGameStateDto>(cardGameStateDtoJson);

        _cards.Find(x => x.CardGameState.Id == obj.Id)
              .Update(obj);
    }

    [Rpc]
    public void RemoveCard(string id)
    {
        var card = _cards.Find(x => x.CardGameState.Id == Guid.Parse(id));
        _cards.Remove(card);
        card.QueueFree();

        SetCardPositions();
    }
}