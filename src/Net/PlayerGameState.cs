using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenCCG.Core;
using OpenCCG.Data;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG.Net;

public class PlayerGameState
{
    public enum ControllingEntity
    {
        Self,
        Enemy
    }

    public long PeerId { get; init; }

    public long EnemyPeerId { get; set; }

    public PlayerGameState Enemy { get; set; }

    public string PlayerName { get; init; }

    public RpcNodes Nodes { get; init; }

    public int Health { get; set; }

    public int Energy { get; set; }

    public int MaxEnergy { get; set; }

    public LinkedList<CardGameState> Deck { get; private set; } = new();

    public LinkedList<CardGameState> Hand { get; } = new();

    public LinkedList<CardGameState> Board { get; } = new();

    public LinkedList<CardGameState> Pit { get; } = new();

    public List<CardRecord> DeckList { get; private set; }

    public bool IsTurn { get; set; }


    public void Init(List<CardRecord> deckList)
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
                                   var card = new CardGameState
                                   {
                                       Record = x,
                                       Zone = CardZone.Deck,
                                       AttacksAvailable = 1,
                                       MaxAttacksPerTurn = 1
                                   };

                                   card.ResetStats();

                                   return card;
                               })
                               .Shuffle();

        Deck = new LinkedList<CardGameState>(shuffledDeckList);
        Nodes.MidPanel.EndTurnButtonSetActive(PeerId, new(false, null));
    }

    public LinkedList<CardGameState> GetListByZone(CardZone zone)
    {
        return zone switch
        {
            CardZone.Deck => Deck,
            CardZone.Hand => Hand,
            CardZone.Board => Board,
            CardZone.Pit => Pit,
            _ => throw new ArgumentOutOfRangeException(nameof(zone), zone, null)
        };
    }

    public void Draw(int count = 1)
    {
        for (var i = 0; i < count; ++i)
        {
            if (Deck.First == null) break;
            var card = Deck.First.Value;

            Deck.RemoveFirst();
            Hand.AddLast(card);

            var dto = card.AsDto();

            Nodes.Hand.DrawCard(PeerId, dto);
            Nodes.EnemyHand.DrawCard(EnemyPeerId);
            UpdateCardCountRpc();
        }
    }

    private void UpdateCardCountRpc()
    {
        Nodes.StatusPanel.SetCardCount(PeerId, Hand.Count);
        Nodes.EnemyStatusPanel.SetCardCount(EnemyPeerId, Hand.Count);
    }

    private void UpdateEnergyRpc()
    {
        Nodes.StatusPanel.SetEnergy(PeerId, Energy);
        Nodes.EnemyStatusPanel.SetEnergy(EnemyPeerId, Energy);
    }

    public async Task PlayCardAsync(Guid id)
    {
        if (!IsTurn)
        {
            Nodes.Hand.FailPlayCard(PeerId);
            return;
        }
        
        var card = Hand.FirstOrDefault(x => x.Id == id);

        if (card == null) throw new ApplicationException();
        if (Energy - card.Cost < 0)
        {
            Nodes.Hand.FailPlayCard(PeerId);
            return;
        }

        Energy -= card.Cost;
        UpdateEnergyRpc();

        Hand.Remove(card);

        if (card.Record.Type is CardRecordType.Spell)
        {
            Nodes.Hand.RemoveCard(PeerId, id);
            Nodes.EnemyHand.RemoveCard(EnemyPeerId);

            await card.OnSpellAsync(this);

            Pit.AddLast(card);
            card.Zone = CardZone.Pit;
        }
        else if (card.Record.Type is CardRecordType.Creature)
        {
            card.Zone = CardZone.Board;
            Board.AddLast(card);

            card.IsSummoningProtectionOn = card.Record.Abilities is { Exposed: false, Defender: false };
            card.IsSummoningSicknessOn = !card.Record.Abilities.Haste;

            await card.OnEnterAsync(this);

            var dto = card.AsDto();

            Nodes.Board.PlaceCard(PeerId, dto);
            Nodes.Hand.RemoveCard(PeerId, id);

            Nodes.EnemyBoard.PlaceCard(EnemyPeerId, dto);
            Nodes.EnemyHand.RemoveCard(EnemyPeerId);
        }

        UpdateCardCountRpc();
    }

    public async Task CombatPlayerAsync(Guid attackerId)
    {
        if (!IsTurn) return;

        var attacker = Board.First(x => x.Id == attackerId);

        if (attacker.IsSummoningSicknessOn) return;
        if (attacker.AttacksAvailable <= 0) return;
        if (Enemy.Board.Any(x => x.Record.Abilities.Defender)) return;

        var t1 = Nodes.Board.PlayCombatAvatarAnimAsync(PeerId, attackerId);
        var t2 = Nodes.EnemyBoard.PlayCombatAvatarAnimAsync(EnemyPeerId, attackerId);

        await Task.WhenAll(t1, t2);

        --attacker.AttacksAvailable;
        attacker.IsSummoningProtectionOn = false;
        UpdateSelfCreature(attacker);
        var atk = Math.Max(0, attacker.Atk);
        Enemy.Health -= atk;

        Nodes.EnemyStatusPanel.SetHealth(PeerId, Enemy.Health);
        Nodes.StatusPanel.SetHealth(EnemyPeerId, Enemy.Health);

        if (attacker.Record.Abilities.Drain)
        {
            Nodes.StatusPanel.SetHealth(PeerId, Health + atk);
            Nodes.EnemyStatusPanel.SetHealth(EnemyPeerId, Health + atk);
        }
    }

    public void ResolveDamage(CardGameState card, int damage, ControllingEntity controllingEntity)
    {
        card.Def -= damage;

        if (controllingEntity is ControllingEntity.Self)
        {
            UpdateSelfCreature(card);
            if (card.Def <= 0)
            {
                DestroySelfCreature(card);
            }
        }
        else
        {
            UpdateEnemyCreature(card);
            if (card.Def <= 0)
            {
                DestroyEnemyCreature(card);
            }
        }
    }

    private void UpdateEnemyCreature(CardGameState card)
    {
        var dto = card.AsDto();
        Nodes.EnemyBoard.UpdateCard(PeerId, dto);
        Nodes.Board.UpdateCard(EnemyPeerId, dto);
    }

    private void UpdateSelfCreature(CardGameState card)
    {
        var dto = card.AsDto();
        Nodes.Board.UpdateCard(PeerId, dto);
        Nodes.EnemyBoard.UpdateCard(EnemyPeerId, dto);
    }

    public async Task CombatAsync(Guid attackerId, Guid targetId)
    {
        // TODO feedback
        if (!IsTurn) return;

        var attacker = Board.First(x => x.Id == attackerId);
        var target = Enemy.Board.First(x => x.Id == targetId);

        // TODO feedback
        if (attacker.IsSummoningSicknessOn) return;
        if (target.IsSummoningProtectionOn) return;
        if (attacker.AttacksAvailable <= 0) return;
        if (Enemy.Board.Any(x => x.Record.Abilities.Defender) && !target.Record.Abilities.Defender) return;

        await attacker.OnStartCombatAsync(this);

        var t1 = Nodes.Board.PlayCombatAnimAsync(PeerId, attackerId, targetId);
        var t2 = Nodes.EnemyBoard.PlayCombatAnimAsync(EnemyPeerId, attackerId, targetId);

        await Task.WhenAll(t1, t2);

        --attacker.AttacksAvailable;
        attacker.IsSummoningProtectionOn = false;
        UpdateSelfCreature(attacker);
        ResolveDamage(attacker, target.Atk, ControllingEntity.Self);
        ResolveDamage(target, attacker.Atk, ControllingEntity.Enemy);

        if (attacker.Record.Abilities.Drain)
        {
            Nodes.StatusPanel.SetHealth(PeerId, Health + attacker.Atk);
            Nodes.EnemyStatusPanel.SetHealth(EnemyPeerId, Health + attacker.Atk);
        }

        if (target.Record.Abilities.Drain)
        {
            Nodes.StatusPanel.SetHealth(EnemyPeerId, Health + target.Atk);
            Nodes.EnemyStatusPanel.SetHealth(PeerId, Health + target.Atk);
        }

        if (attacker.Zone == CardZone.Board)
        {
            await attacker.OnEndCombatAsync(this);
        }
        else
        {
            await attacker.OnExitAsync(this);
        }
        
        if(target.Zone != CardZone.Board)
        {
            await target.OnExitAsync(Enemy);
        }
    }

    public void Start()
    {
        Draw(Rules.InitialCardsDrawn);
        UpdateEnergyRpc();

        Nodes.StatusPanel.SetHealth(PeerId, Health);
        Nodes.EnemyStatusPanel.SetHealth(EnemyPeerId, Health);
    }

    public async Task EndTurnAsync()
    {
        if (!IsTurn) return;

        foreach (var cardGameState in Board)
        {
            await cardGameState.OnEndTurnAsync(this);
        }

        IsTurn = false;
        Nodes.MidPanel.EndTurnButtonSetActive(PeerId, new(false, null));

        await Enemy.StartTurnAsync();
    }

    public async Task StartTurnAsync()
    {
        IsTurn = true;
        Nodes.MidPanel.EndTurnButtonSetActive(PeerId, new(true, null));

        foreach (var cardGameState in Board)
        {
            cardGameState.IsSummoningProtectionOn = false;
            cardGameState.IsSummoningSicknessOn = false;
            cardGameState.AttacksAvailable = cardGameState.MaxAttacksPerTurn;

            var dto = cardGameState.AsDto();
            Nodes.Board.UpdateCard(PeerId, dto);
            Nodes.EnemyBoard.UpdateCard(EnemyPeerId, dto);

            await cardGameState.OnStartTurnAsync(this);
        }

        Energy = MaxEnergy = Math.Min(Rules.MaxEnergy, MaxEnergy + Rules.EnergyGainedPerTurn);
        UpdateEnergyRpc();
        Draw();
    }

    public void Disconnect()
    {
        Enemy.NotifyDisconnected();
    }

    private void NotifyDisconnected()
    {
        Nodes.MidPanel.SetStatusMessage(PeerId, "Opponent disconnected");
    }

    public void DestroyEnemyCreature(CardGameState cardGameState)
    {
        Enemy.Board.Remove(cardGameState);
        Enemy.Pit.AddLast(cardGameState);
        cardGameState.Zone = CardZone.Pit;

        Nodes.EnemyBoard.RemoveCard(PeerId, cardGameState.Id);
        Nodes.Board.RemoveCard(EnemyPeerId, cardGameState.Id);
    }

    public void DestroySelfCreature(CardGameState cardGameState)
    {
        Board.Remove(cardGameState);
        Pit.AddLast(cardGameState);
        cardGameState.Zone = CardZone.Pit;

        Nodes.Board.RemoveCard(PeerId, cardGameState.Id);
        Nodes.EnemyBoard.RemoveCard(EnemyPeerId, cardGameState.Id);
    }
}