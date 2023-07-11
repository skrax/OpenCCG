using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenCCG.Cards;
using OpenCCG.Cards.Test;
using OpenCCG.Core;
using OpenCCG.Net.Dto;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG.Net;

public class PlayerGameState
{
    public long PeerId { get; init; }

    public long EnemyPeerId { get; set; }

    public PlayerGameState Enemy { get; set; }

    public string PlayerName { get; init; }

    public RpcNodes Nodes { get; init; }

    public int Health { get; set; }

    public int Energy { get; set; }

    public int MaxEnergy { get; set; }

    public LinkedList<CardImplementation> Deck { get; private set; } = new();

    public LinkedList<CardImplementation> Hand { get; } = new();

    public LinkedList<CardImplementation> Board { get; } = new();

    public LinkedList<CardImplementation> Pit { get; } = new();

    public List<ICardOutline> DeckList { get; private set; }

    public bool IsTurn { get; set; }

    public void Init(List<ICardOutline> deckList)
    {
        Nodes.MidPanel.SetStatusMessage(PeerId, "");
        Energy = MaxEnergy = 0;
        Deck.Clear();
        Hand.Clear();
        Board.Clear();
        Pit.Clear();
        DeckList = deckList;
        Health = Rules.InitialHealth;

        var shuffledDeckList = DeckList
                               .Select(x =>
                               {
                                   var card = TestSetImplementations.GetImplementation(x.Id, this);
                                   card.MoveToZone(CardZone.Deck);
                                   return card;
                               })
                               .Shuffle();

        Deck = new LinkedList<CardImplementation>(shuffledDeckList);

        Nodes.MidPanel.EndTurnButtonSetActive(PeerId, new EndTurnButtonSetActiveDto(false, null));
    }

    public async Task StartAsync()
    {
        await DrawAsync(Rules.InitialCardsDrawn);
        UpdateEnergy();
        UpdateHealth();
    }

    public async Task StartTurnAsync()
    {
        if (IsTurn) return;
        IsTurn = true;
        Energy = MaxEnergy = Math.Min(Rules.MaxEnergy, MaxEnergy + Rules.EnergyGainedPerTurn);
        UpdateEnergy();
        await DrawAsync();

        foreach (CreatureImplementation creature in Board) await creature.OnUpkeepAsync();

        Nodes.MidPanel.EndTurnButtonSetActive(PeerId, new EndTurnButtonSetActiveDto(true, null));

        foreach (CreatureImplementation creature in Board) await creature.OnStartTurnAsync();
    }

    public async Task EndTurnAsync()
    {
        if (!IsTurn) return;

        Nodes.MidPanel.EndTurnButtonSetActive(PeerId, new EndTurnButtonSetActiveDto(false, "End Step"));

        foreach (CreatureImplementation creature in Board) await creature.OnEndStepAsync();

        foreach (CreatureImplementation creature in Board) await creature.OnEndTurnAsync();

        IsTurn = false;
        Nodes.MidPanel.EndTurnButtonSetActive(PeerId, new EndTurnButtonSetActiveDto(false, null));

        await Enemy.StartTurnAsync();
    }

    public async Task<bool> PlayCardAsync(Guid id)
    {
        if (!IsTurn) return false;

        var card = Hand.SingleOrDefault(x => x.Id == id);
        if (card == null) return false;
        if (Energy - card.State.Cost < 0) return false;

        Energy -= card.State.Cost;
        UpdateEnergy();

        if (card is CreatureImplementation creatureImplementation)
        {
            creatureImplementation.RemoveFromHand();
            creatureImplementation.MoveToZone(CardZone.None);
            await creatureImplementation.OnPlayAsync();
            creatureImplementation.MoveToZone(CardZone.Board);
            await creatureImplementation.EnterBoardAsync();
            UpdateCardCount();
            await creatureImplementation.OnEnterAsync();
        }
        else if (card is SpellImplementation spellImplementation)
        {
            spellImplementation.RemoveFromHand();
            spellImplementation.MoveToZone(CardZone.None);
            await spellImplementation.OnPlayAsync();
            spellImplementation.MoveToZone(CardZone.Pit);
            UpdateCardCount();
        }

        return true;
    }

    public async Task CombatAsync(CombatPlayerCardDto dto)
    {
        if (!IsTurn) return;

        if (Board.SingleOrDefault(x => x.Id == dto.AttackerId) is not CreatureImplementation attacker ||
            Enemy.Board.SingleOrDefault(x => x.Id == dto.TargetId) is not CreatureImplementation target) return;
        if (attacker.CreatureState.AttacksAvailable <= 0) return;
        if (!target.CreatureState.IsExposed) return;
        if (!target.Abilities.Defender && Enemy.Board.Cast<CreatureImplementation>().Any(x => x.Abilities.Defender))
            return;

        await attacker.OnStartCombatAsync();

        await Task.WhenAll(Nodes.Board.PlayCombatAnimAsync(PeerId, attacker.Id, target.Id),
            Nodes.EnemyBoard.PlayCombatAnimAsync(EnemyPeerId, attacker.Id, target.Id)
        );

        --attacker.CreatureState.AttacksAvailable;
        attacker.CreatureState.IsExposed = true;

        await Task.WhenAll(attacker.TakeDamageAsync(target.CreatureState.Atk),
            target.TakeDamageAsync(attacker.CreatureState.Atk));

        if (attacker.Abilities.Drain && attacker.CreatureState.Atk > 0)
        {
            Health += attacker.CreatureState.Atk;
            UpdateHealth();
        }

        if (target.Abilities.Drain && target.CreatureState.Atk > 0)
        {
            Enemy.Health += target.CreatureState.Atk;
            Enemy.UpdateHealth();
        }

        if (attacker.CreatureState.Def > 0)
        {
            await attacker.OnEndCombatAsync();
            if (target.CreatureState.Def <= 0)
            {
                target.MoveToZone(CardZone.None);
                await target.RemoveFromBoardAsync();
                await target.OnExitAsync();
                target.MoveToZone(CardZone.Pit);
            }
        }
        else
        {
            if (target.CreatureState.Def <= 0)
            {
                target.MoveToZone(CardZone.None);
                attacker.MoveToZone(CardZone.None);
                await Task.WhenAll(attacker.RemoveFromBoardAsync(), target.RemoveFromBoardAsync());
                await Task.WhenAll(attacker.OnExitAsync(), target.OnExitAsync());
                target.MoveToZone(CardZone.Pit);
                attacker.MoveToZone(CardZone.Pit);
            }
            else
            {
                attacker.MoveToZone(CardZone.None);
                await attacker.RemoveFromBoardAsync();
                await attacker.OnExitAsync();
                attacker.MoveToZone(CardZone.Pit);
            }
        }
    }

    public async Task CombatPlayerAsync(Guid attackerId)
    {
        if (!IsTurn) return;

        if (Board.SingleOrDefault(x => x.Id == attackerId) is not CreatureImplementation attacker) return;
        if (attacker.CreatureState.AttacksAvailable < 1) return;
        if (Enemy.Board.Cast<CreatureImplementation>().Any(x => x.Abilities.Defender)) return;

        await attacker.OnStartCombatAsync();

        await Task.WhenAll(Nodes.Board.PlayCombatAvatarAnimAsync(PeerId, attacker.Id),
            Nodes.EnemyBoard.PlayCombatAvatarAnimAsync(EnemyPeerId, attacker.Id)
        );

        --attacker.CreatureState.AttacksAvailable;
        attacker.CreatureState.IsExposed = true;

        await attacker.UpdateAsync();

        if (attacker.CreatureState.Atk > 0)
        {
            Enemy.Health -= attacker.CreatureState.Atk;
            Enemy.UpdateHealth();
        }

        if (attacker.Abilities.Drain && attacker.CreatureState.Atk > 0)
        {
            Health += attacker.CreatureState.Atk;
            UpdateHealth();
        }

        await attacker.OnEndCombatAsync();
    }

    public async Task DrawAsync(int count = 1)
    {
        for (var i = 0; i < count; ++i)
        {
            if (Deck.First == null) break;
            var card = Deck.First.Value;
            card.MoveToZone(CardZone.Hand);

            var dto = card.AsDto();

            await Task.WhenAll(Nodes.Hand.DrawCard(PeerId, dto), Nodes.EnemyHand.DrawCard(EnemyPeerId));

            UpdateCardCount();
        }
    }

    public void UpdateHealth()
    {
        Nodes.StatusPanel.SetHealth(PeerId, Health);
        Nodes.EnemyStatusPanel.SetHealth(EnemyPeerId, Health);
    }

    public void UpdateCardCount()
    {
        Nodes.StatusPanel.SetCardCount(PeerId, Hand.Count);
        Nodes.EnemyStatusPanel.SetCardCount(EnemyPeerId, Hand.Count);
    }

    public void UpdateEnergy()
    {
        Nodes.StatusPanel.SetEnergy(PeerId, Energy, MaxEnergy);
        Nodes.EnemyStatusPanel.SetEnergy(EnemyPeerId, Energy, MaxEnergy);
    }

    public void Disconnect()
    {
        Enemy.NotifyDisconnected();
    }

    private void NotifyDisconnected()
    {
        Nodes.MidPanel.SetStatusMessage(PeerId, "Opponent disconnected");
    }
}