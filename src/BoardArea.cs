using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Core;
using OpenCCG.Net;
using OpenCCG.Net.Dto;
using OpenCCG.Net.Rpc;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG;

public partial class BoardArea : HBoxContainer, IMessageReceiver<MessageType>
{
    [Export] public bool IsEnemy;
    [Export] public BoardArea EnemyBoardArea;
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
            MessageType.UpdateCard => Executor.Make<CardGameStateDto>(UpdateCardAsync, Executor.ResponseMode.Respond),
            MessageType.RemoveCard => Executor.Make<RemoveCardDto>(RemoveCard),
            MessageType.PlayCombatAnim => Executor.Make<PlayCombatDto>(PlayCombatAnimAsync,
                Executor.ResponseMode.Respond),
            _ => throw new NotImplementedException()
        };
    }

    private async Task PlayCombatAnimAsync(PlayCombatDto dto)
    {
#if false
        var card = _cards.FirstOrDefault(x => x.CardGameState.Id == dto.From);

        if (card == null)
        {
            Logger.Error<BoardArea>($"IsEnemy: {IsEnemy} play combat: ${dto.From} not found");
            return;
        }

        if (dto.IsAvatar)
        {
            await card.AttackAsync(EnemyAvatar);
        }
        else
        {
            var other = EnemyBoardArea._cards.FirstOrDefault(x => x.CardGameState.Id == dto.To);
            if (other == null)
            {
                Logger.Error<BoardArea>($"IsEnemy: {IsEnemy} play combat: ${dto.To} not found");
                return;
            }

            await card.AttackAsync(other);
        }
#endif
    }

    private void PlaceCard(CardGameStateDto cardGameStateDto)
    {
        var card = CardBoardScene.Make<CardBoard, CardGameStateDto>(cardGameStateDto, this);
        Logger.Info<BoardArea>($"IsEnemy: {IsEnemy} placed: ${cardGameStateDto.Id} ${cardGameStateDto.Record.Name}");

        _cards.Add(card);
    }

    private async Task UpdateCardAsync(CardGameStateDto cardGameStateDto)
    {
        Logger.Info<BoardArea>($"IsEnemy: {IsEnemy} update: ${cardGameStateDto.Id}");

        var card = _cards.FirstOrDefault(x => x.CardGameState.Id == cardGameStateDto.Id);

        if (card == null)
        {
            Logger.Error<BoardArea>($"IsEnemy: {IsEnemy} update: ${cardGameStateDto.Id} not found");
            return;
        }

        if (card.IsQueuedForDeletion()) return;

        await card.UpdateAsync(cardGameStateDto);
    }

    private void RemoveCard(RemoveCardDto removeCardDto)
    {
        Logger.Info<BoardArea>($"IsEnemy: {IsEnemy} remove: ${removeCardDto.Id}");
        var card = _cards.FirstOrDefault(x => x.CardGameState.Id == removeCardDto.Id);

        if (card == null)
        {
            Logger.Error<BoardArea>($"IsEnemy: {IsEnemy} remove: ${removeCardDto.Id} not found");
            return;
        }

        _cards.Remove(card);
        card.Destroy(() => { });
    }
}