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

    public int PeerId { get; init; }

    public int EnemyPeerId { get; set; }

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

    public bool isTurn { get; set; }


    public void Init(List<CardRecord> deckList)
    {
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
        Nodes.MidPanel.EndTurnButtonSetActive(PeerId, false);
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

    public async Task PlayCard(Guid id)
    {
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
            var effects = card.Record.Effects.Select(x => Database.CardEffects[x.Id](x.initJson));

            Nodes.Hand.RemoveCard(PeerId, id);
            Nodes.EnemyHand.RemoveCard(EnemyPeerId);

            foreach (var cardEffect in effects) await cardEffect.Execute(card, this);

            Pit.AddLast(card);
        }
        else if (card.Record.Type is CardRecordType.Creature)
        {
            card.Zone = CardZone.Board;
            Board.AddLast(card);

            card.IsSummoningProtectionOn = true;
            card.IsSummoningSicknessOn = true;

            var dto = card.AsDto();

            Nodes.Board.PlaceCard(PeerId, dto);
            Nodes.Hand.RemoveCard(PeerId, id);

            Nodes.EnemyBoard.PlaceCard(EnemyPeerId, dto);
            Nodes.EnemyHand.RemoveCard(EnemyPeerId);
        }

        UpdateCardCountRpc();
    }

    public void CombatPlayer(Guid attackerId)
    {
        if (!isTurn) return;

        var attacker = Board.First(x => x.Id == attackerId);

        if (attacker.IsSummoningSicknessOn) return;
        if (attacker.AttacksAvailable <= 0) return;

        --attacker.AttacksAvailable;
        Enemy.Health -= Math.Max(0, attacker.Atk);

        Nodes.EnemyStatusPanel.SetHealth(PeerId, Enemy.Health);
        Nodes.StatusPanel.SetHealth(EnemyPeerId, Enemy.Health);
    }

    public void ResolveDamage(CardGameState card, int damage, ControllingEntity controllingEntity)
    {
        card.Def -= damage;

        if (controllingEntity is ControllingEntity.Self)
        {
            if (card.Def <= 0)
            {
                Board.Remove(card);
                Pit.AddLast(card);

                Nodes.Board.RemoveCard(PeerId, card.Id);
                Nodes.EnemyBoard.RemoveCard(EnemyPeerId, card.Id);
            }
            else
            {
                var dto = card.AsDto();
                Nodes.Board.UpdateCard(PeerId, dto);
                Nodes.EnemyBoard.UpdateCard(EnemyPeerId, dto);
            }
        }
        else
        {
            if (card.Def <= 0)
            {
                Enemy.Board.Remove(card);
                Enemy.Pit.AddLast(card);

                Nodes.EnemyBoard.RemoveCard(PeerId, card.Id);
                Nodes.Board.RemoveCard(EnemyPeerId, card.Id);
            }
            else
            {
                var dto = card.AsDto();
                Nodes.EnemyBoard.UpdateCard(PeerId, dto);
                Nodes.Board.UpdateCard(EnemyPeerId, dto);
            }
        }
    }

    public void Combat(Guid attackerId, Guid targetId)
    {
        // TODO feedback
        if (!isTurn) return;

        var attacker = Board.First(x => x.Id == attackerId);
        var target = Enemy.Board.First(x => x.Id == targetId);

        // TODO feedback
        if (attacker.IsSummoningSicknessOn) return;
        if (target.IsSummoningProtectionOn) return;
        if (attacker.AttacksAvailable <= 0) return;

        --attacker.AttacksAvailable;
        ResolveDamage(attacker, target.Atk, ControllingEntity.Self);
        ResolveDamage(target, attacker.Atk, ControllingEntity.Enemy);
    }

    public void Start()
    {
        Draw(Rules.InitialCardsDrawn);
        UpdateEnergyRpc();

        Nodes.StatusPanel.SetHealth(PeerId, Health);
        Nodes.EnemyStatusPanel.SetHealth(EnemyPeerId, Health);
    }

    public void EndTurn()
    {
        if (!isTurn) return;

        isTurn = false;
        Nodes.MidPanel.EndTurnButtonSetActive(PeerId, false);
        Enemy.StartTurn();
    }

    public void StartTurn()
    {
        isTurn = true;
        Nodes.MidPanel.EndTurnButtonSetActive(PeerId, true);
        foreach (var cardGameState in Board)
        {
            cardGameState.IsSummoningProtectionOn = false;
            cardGameState.IsSummoningSicknessOn = false;
            cardGameState.AttacksAvailable = cardGameState.MaxAttacksPerTurn;

            var dto = cardGameState.AsDto();
            Nodes.Board.UpdateCard(PeerId, dto);
            Nodes.EnemyBoard.UpdateCard(EnemyPeerId, dto);
        }

        Energy = MaxEnergy = Math.Min(Rules.MaxEnergy, MaxEnergy + Rules.EnergyGainedPerTurn);
        UpdateEnergyRpc();
        Draw();
    }
}