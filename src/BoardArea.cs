using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Cards;
using OpenCCG.Core;
using OpenCCG.Net;
using OpenCCG.Net.Rpc;
using OpenCCG.Net.ServerNodes;
using Serilog;

namespace OpenCCG;

public partial class BoardArea : HBoxContainer, IMessageReceiver<MessageType>
{
    private static readonly PackedScene CardBoardScene = GD.Load<PackedScene>("res://scenes/card-board.tscn");

    public readonly List<CardBoard> Cards = new();
    [Export] public StatusPanel StatusPanel = null!, EnemyStatusPanel = null!;
    [Export] public BoardArea EnemyBoardArea = null!;
    [Export] public bool IsEnemy;

    public Dictionary<string, IObserver>? Observers => null;

    [Rpc]
    public async void HandleMessageAsync(string messageJson)
    {
        await IMessageReceiver<MessageType>.HandleMessageAsync(this, messageJson);
    }

    public Executor? GetExecutor(MessageType messageType)
    {
        return messageType switch
        {
            MessageType.PlaceCard => Executor.Make<CardImplementationDto>(PlaceCard, Executor.ResponseMode.Respond),
            MessageType.UpdateCard => Executor.Make<CardImplementationDto>(UpdateCardAsync,
                Executor.ResponseMode.Respond),
            MessageType.RemoveCard => Executor.Make<RemoveCardDto>(RemoveCard, Executor.ResponseMode.Respond),
            MessageType.PlayCombatAnim => Executor.Make<PlayCombatDto>(PlayCombatAnimAsync,
                Executor.ResponseMode.Respond),
            _ => null
        };
    }

    private async Task PlayCombatAnimAsync(PlayCombatDto dto)
    {
        var card = Cards.FirstOrDefault(x => x.CardImplementationDto.Id == dto.From);

        if (card == null)
        {
            Log.Error("IsEnemy: {IsEnemy} play combat: {From} not found", IsEnemy, dto.From);
            return;
        }

        if (dto.IsAvatar)
        {
            await card.AttackAsync(EnemyStatusPanel._avatar);
        }
        else
        {
            var other = EnemyBoardArea.Cards.FirstOrDefault(x => x.CardImplementationDto.Id == dto.To);
            if (other == null)
            {
                Log.Error("IsEnemy: {IsEnemy} play combat: {To} not found", IsEnemy, dto.To);
                return;
            }

            await card.AttackAsync(other);
        }
    }

    private void PlaceCard(CardImplementationDto dto)
    {
        var card = CardBoardScene.Make<CardBoard, CardImplementationDto>(dto, this);
        Log.Information("IsEnemy: {IsEnemy} placed: {Id} {Name}", IsEnemy, dto.Id, dto.Outline.Name);

        Cards.Add(card);
    }

    private async Task UpdateCardAsync(CardImplementationDto dto)
    {
        Log.Information("IsEnemy: {IsEnemy} update: {Id}", IsEnemy, dto.Id);

        var card = Cards.FirstOrDefault(x => x.CardImplementationDto.Id == dto.Id);

        if (card == null)
        {
            Log.Error("IsEnemy: {IsEnemy} update: {Id} not found", IsEnemy, dto.Id);
            return;
        }

        if (card.IsQueuedForDeletion()) return;

        await card.UpdateAsync(dto);
    }

    private void RemoveCard(RemoveCardDto removeCardDto)
    {
        Log.Information("IsEnemy: {IsEnemy} remove: {Id}", IsEnemy, removeCardDto.Id);
        var card = Cards.FirstOrDefault(x => x.CardImplementationDto.Id == removeCardDto.Id);

        if (card == null)
        {
            Log.Error("IsEnemy: {IsEnemy} remove: {Id} not found", IsEnemy, removeCardDto.Id);
            return;
        }

        Cards.Remove(card);
        card.Destroy(() => { });
    }
}