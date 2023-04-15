using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using OpenCCG.Core;
using OpenCCG.Data;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG.Net;

public class PlayerGameState
{
    public int PeerId { get; init; }

    public int EnemyPeerId { get; set; }

    public PlayerGameState Enemy { get; set; }

    public string PlayerName { get; init; }

    public RpcNodes Nodes { get; init; }

    public int Health { get; set; }

    public int Energy { get; set; }

    public int MaxEnergy { get; set; }

    public LinkedList<CardGameState> Deck { get; private set; } = new();

    public LinkedList<CardGameState> Hand { get; private set; } = new();

    public LinkedList<CardGameState> Board { get; private set; } = new();

    public LinkedList<CardGameState> Pit { get; private set; } = new();

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

        Deck = new(shuffledDeckList);
        Nodes.MidPanel.RpcId(PeerId, "EndTurnButtonSetActive", false);
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

            var json = JsonSerializer.Serialize(card.AsDto());

            Nodes.Hand.RpcId(PeerId, "DrawCard", json);
            Nodes.EnemyHand.RpcId(EnemyPeerId, "DrawCard");
            UpdateCardCountRpc();
        }
    }

    private void UpdateCardCountRpc()
    {
        Nodes.StatusPanel.RpcId(PeerId, "SetCardCount", Hand.Count);
        Nodes.EnemyStatusPanel.RpcId(EnemyPeerId, "SetCardCount", Hand.Count);
    }

    private void UpdateEnergyRpc()
    {
        Nodes.StatusPanel.RpcId(PeerId, "SetEnergy", Energy);
        Nodes.EnemyStatusPanel.RpcId(EnemyPeerId, "SetEnergy", Energy);
    }

    public void PlayCard(Guid id)
    {
        var card = Hand.FirstOrDefault(x => x.Id == id);

        if (card == null) throw new ApplicationException();
        if (Energy - card.Cost < 0)
        {
            Nodes.Hand.RpcId(PeerId, "FailPlayCard");
            return;
        }

        Energy -= card.Cost;
        UpdateEnergyRpc();

        Hand.Remove(card);
        card.Zone = CardZone.Board;
        Board.AddLast(card);

        card.IsSummoningProtectionOn = true;
        card.IsSummoningSicknessOn = true;

        var dtoJson = JsonSerializer.Serialize(card.AsDto());

        Nodes.Board.RpcId(PeerId, "PlaceCard", dtoJson);
        Nodes.Hand.RpcId(PeerId, "RemoveCard", id.ToString());

        Nodes.EnemyBoard.RpcId(EnemyPeerId, "PlaceCard", dtoJson);
        Nodes.EnemyHand.RpcId(EnemyPeerId, "RemoveCard");

        UpdateCardCountRpc();
    }

    public void CombatPlayer(Guid attackerId)
    {
        if (!isTurn) return;

        var attacker = Board.First(x => x.Id == attackerId);

        if (attacker.IsSummoningSicknessOn) return;
        if (attacker.AttacksAvailable <= 0) return;

        --attacker.AttacksAvailable;
        Enemy.Health -= Math.Min(0, attacker.Atk);

        Nodes.EnemyStatusPanel.RpcId(PeerId, "SetHealth", Enemy.Health);
        Nodes.StatusPanel.RpcId(EnemyPeerId, "SetHealth", Enemy.Health);
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
        attacker.Def -= target.Atk;
        target.Def -= attacker.Atk;

        if (attacker.Def <= 0)
        {
            Board.Remove(attacker);
            Pit.AddLast(attacker);

            Nodes.Board.RpcId(PeerId, "RemoveCard", attacker.Id.ToString());
            Nodes.EnemyBoard.RpcId(EnemyPeerId, "RemoveCard", attacker.Id.ToString());
        }
        else
        {
            var json = JsonSerializer.Serialize(attacker.AsDto());
            Nodes.Board.RpcId(PeerId, "UpdateCard", json);

            Nodes.EnemyBoard.RpcId(EnemyPeerId, "UpdateCard", json);
        }

        if (target.Def <= 0)
        {
            Enemy.Board.Remove(target);
            Enemy.Pit.AddLast(target);

            Nodes.EnemyBoard.RpcId(PeerId, "RemoveCard", target.Id.ToString());

            Nodes.Board.RpcId(EnemyPeerId, "RemoveCard", target.Id.ToString());
        }
        else
        {
            var json = JsonSerializer.Serialize(target.AsDto());
            Nodes.EnemyBoard.RpcId(PeerId, "UpdateCard", json);
            Nodes.Board.RpcId(EnemyPeerId, "UpdateCard", json);
        }
    }

    public void Start()
    {
        Draw(Rules.InitialCardsDrawn);
        UpdateEnergyRpc();

        Nodes.StatusPanel.RpcId(PeerId, "SetHealth", Health);
        Nodes.EnemyStatusPanel.RpcId(EnemyPeerId, "SetHealth", Health);
    }

    public void EndTurn()
    {
        if (!isTurn) return;

        isTurn = false;
        Nodes.MidPanel.RpcId(PeerId, "EndTurnButtonSetActive", false);
        Enemy.StartTurn();
    }

    public void StartTurn()
    {
        isTurn = true;
        Nodes.MidPanel.RpcId(PeerId, "EndTurnButtonSetActive", true);
        foreach (var cardGameState in Board)
        {
            cardGameState.IsSummoningProtectionOn = false;
            cardGameState.IsSummoningSicknessOn = false;
            cardGameState.AttacksAvailable = cardGameState.MaxAttacksPerTurn;

            var json = JsonSerializer.Serialize(cardGameState.AsDto());
            Nodes.Board.RpcId(PeerId, "UpdateCard", json);
            Nodes.EnemyBoard.RpcId(EnemyPeerId, "UpdateCard", json);
        }

        Energy = MaxEnergy = Math.Min(Rules.MaxEnergy, MaxEnergy + Rules.EnergyGainedPerTurn);
        UpdateEnergyRpc();
        Draw(Rules.CardsDrawnPerTurn);
    }
}