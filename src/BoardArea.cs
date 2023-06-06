using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenCCG.Core;
using OpenCCG.Net;
using OpenCCG.Net.Dto;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG;

public partial class BoardArea : Area2D, IMessageReceiver<MessageType>
{
    private static readonly PackedScene CardBoardScene = GD.Load<PackedScene>("res://scenes/card-board.tscn");

    private readonly List<CardBoard> _cards = new();

    public Dictionary<string, IObserver>? Observers => null;

    [Rpc]
    public void HandleMessage(string messageJson)
    {
        IMessageReceiver<MessageType>.HandleMessage(this, messageJson);
    }

    private void SetCardPositions()
    {
        SpriteHelpers.OrderHorizontally(_cards.Cast<Sprite2D>().ToArray());
    }

    private void PlaceCard(CardGameStateDto cardGameStateDto)
    {
        var card = CardBoardScene.Make<CardBoard, CardGameStateDto>(cardGameStateDto, this);

        _cards.Add(card);

        SetCardPositions();
    }

    private void UpdateCard(CardGameStateDto cardGameStateDto)
    {
        _cards
            .First(x => x.CardGameState.Id == cardGameStateDto.Id)
            .Update(cardGameStateDto);
    }

    private void RemoveCard(RemoveCardDto removeCardDto)
    {
        var card = _cards.First(x => x.CardGameState.Id == Guid.Parse(removeCardDto.Id));
        _cards.Remove(card);
        card.QueueFree();

        SetCardPositions();
    }

    public Func<int, string?, string?> GetExecutor(MessageType messageType) => messageType switch
    {
        MessageType.PlaceCard => IMessageReceiver<MessageType>.MakeExecutor<CardGameStateDto>(PlaceCard),
        MessageType.UpdateCard => IMessageReceiver<MessageType>.MakeExecutor<CardGameStateDto>(UpdateCard),
        MessageType.RemoveCard => IMessageReceiver<MessageType>.MakeExecutor<RemoveCardDto>(RemoveCard),
        _ => throw new NotImplementedException()
    };
}