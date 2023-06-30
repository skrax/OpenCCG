using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Cards;
using OpenCCG.Core;
using OpenCCG.Net;
using OpenCCG.Net.Dto;
using OpenCCG.Net.Rpc;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG;

public partial class BoardArea : HBoxContainer, IMessageReceiver<MessageType>
{
    private static readonly PackedScene CardBoardScene = GD.Load<PackedScene>("res://scenes/card-board.tscn");

    public readonly List<CardBoard> _cards = new();
    [Export] public StatusPanel _StatusPanel, _EnemyStatusPanel;
    [Export] public BoardArea EnemyBoardArea;
    [Export] public bool IsEnemy;

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
            MessageType.PlaceCard => Executor.Make<CardImplementationDto>(PlaceCard, Executor.ResponseMode.Respond),
            MessageType.UpdateCard => Executor.Make<CardImplementationDto>(UpdateCardAsync, Executor.ResponseMode.Respond),
            MessageType.RemoveCard => Executor.Make<RemoveCardDto>(RemoveCard, Executor.ResponseMode.Respond),
            MessageType.PlayCombatAnim => Executor.Make<PlayCombatDto>(PlayCombatAnimAsync,
                Executor.ResponseMode.Respond),
            _ => throw new NotImplementedException()
        };
    }

    private async Task PlayCombatAnimAsync(PlayCombatDto dto)
    {
        var card = _cards.FirstOrDefault(x => x.CardImplementationDto.Id == dto.From);

        if (card == null)
        {
            Logger.Error<BoardArea>($"IsEnemy: {IsEnemy} play combat: ${dto.From} not found");
            return;
        }

        if (dto.IsAvatar)
        {
            await card.AttackAsync(_EnemyStatusPanel._avatar);
        }
        else
        {
            var other = EnemyBoardArea._cards.FirstOrDefault(x => x.CardImplementationDto.Id == dto.To);
            if (other == null)
            {
                Logger.Error<BoardArea>($"IsEnemy: {IsEnemy} play combat: ${dto.To} not found");
                return;
            }

            await card.AttackAsync(other);
        }
    }

    private void PlaceCard(CardImplementationDto dto)
    {
        var card = CardBoardScene.Make<CardBoard, CardImplementationDto>(dto, this);
        Logger.Info<BoardArea>($"IsEnemy: {IsEnemy} placed: ${dto.Id} ${dto.Outline.Name}");

        _cards.Add(card);
    }

    private async Task UpdateCardAsync(CardImplementationDto dto)
    {
        Logger.Info<BoardArea>($"IsEnemy: {IsEnemy} update: ${dto.Id}");

        var card = _cards.FirstOrDefault(x => x.CardImplementationDto.Id == dto.Id);

        if (card == null)
        {
            Logger.Error<BoardArea>($"IsEnemy: {IsEnemy} update: ${dto.Id} not found");
            return;
        }

        if (card.IsQueuedForDeletion()) return;

        await card.UpdateAsync(dto);
    }

    private void RemoveCard(RemoveCardDto removeCardDto)
    {
        Logger.Info<BoardArea>($"IsEnemy: {IsEnemy} remove: ${removeCardDto.Id}");
        var card = _cards.FirstOrDefault(x => x.CardImplementationDto.Id == removeCardDto.Id);

        if (card == null)
        {
            Logger.Error<BoardArea>($"IsEnemy: {IsEnemy} remove: ${removeCardDto.Id} not found");
            return;
        }

        _cards.Remove(card);
        card.Destroy(() => { });
    }
}