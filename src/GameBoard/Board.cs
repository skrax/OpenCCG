using System.Collections.Generic;
using Godot;

namespace OpenCCG.GameBoard;

public partial class Board : HBoxContainer
{
    [Export] private PackedScene _cardBoardScene = null!;
    [Export] public StatusPanel StatusPanel = null!, EnemyStatusPanel = null!;
    [Export] public Board EnemyBoard = null!;
    [Export] public bool IsEnemy;

    public readonly List<CardBoard> Cards = new();
    #if false

    public async Task PlayCombatAnimAsync(PlayCombatDto dto)
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
            var other = EnemyBoard.Cards.FirstOrDefault(x => x.CardImplementationDto.Id == dto.To);
            if (other == null)
            {
                Log.Error("IsEnemy: {IsEnemy} play combat: {To} not found", IsEnemy, dto.To);
                return;
            }

            await card.AttackAsync(other);
        }
    }

    public void PlaceCard(CardImplementationDto dto)
    {
        var card = _cardBoardScene.Make<CardBoard, CardImplementationDto>(dto, this);
        Log.Information("IsEnemy: {IsEnemy} placed: {Id} {Name}", IsEnemy, dto.Id, dto.Outline.Name);

        Cards.Add(card);
    }

    public async Task UpdateCardAsync(CardImplementationDto dto)
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

    public void RemoveCard(RemoveCardDto removeCardDto)
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
#endif
}