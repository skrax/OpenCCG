using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenCCG.Core;
using OpenCCG.Net;
using OpenCCG.Net.Dto;
using OpenCCG.Net.Rpc;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG;

public partial class BoardArea : Area2D, IMessageReceiver<MessageType>
{
    private static readonly PackedScene CardBoardScene = GD.Load<PackedScene>("res://scenes/card-board.tscn");

    private readonly List<CardBoard> _cards = new();

    public Dictionary<string, IObserver>? Observers => null;

    [Rpc]
    public async void HandleMessageAsync(string messageJson)
    {
        await IMessageReceiver<MessageType>.HandleMessageAsync(this, messageJson);
    }

    public Executor GetExecutor(MessageType messageType)
    {
        return messageType switch
        {
            MessageType.PlaceCard => Executor.Make<CardGameStateDto>(PlaceCard),
            MessageType.UpdateCard => Executor.Make<CardGameStateDto>(UpdateCard),
            MessageType.RemoveCard => Executor.Make<RemoveCardDto>(RemoveCard),
            _ => throw new NotImplementedException()
        };
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
}