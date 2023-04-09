using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using OpenCCG.Core;
using OpenCCG.Data;
using OpenCCG.Net.Dto;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG.Net;

public class PlayerGameState
{
    public int PeerId { get; init; }

    public int[] EnemyPeerIds { get; init; }

    public string PlayerName { get; init; }

    public RpcNodes Nodes { get; init; }

    public int Energy { get; set; }

    public int MaxEnergy { get; set; }

    public LinkedList<CardGameState> Deck { get; private set; } = new();

    public LinkedList<CardGameState> Hand { get; private set; } = new();

    public LinkedList<CardGameState> Board { get; private set; } = new();

    public LinkedList<CardGameState> Pit { get; private set; } = new();

    public List<CardRecord> DeckList { get; private set; }


    public void Init(List<CardRecord> deckList)
    {
        Energy = MaxEnergy = 0;
        Deck.Clear();
        Hand.Clear();
        Board.Clear();
        Pit.Clear();
        DeckList = deckList;

        var shuffledDeckList = DeckList
                               .Select(x => new CardGameState
                               {
                                   Record = x,
                                   Zone = CardZone.Deck
                               })
                               .Shuffle();

        Deck = new(shuffledDeckList);
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

            var dto = new CardGameStateDto(card.Id, card.Record, CardZone.Hand, CardZone.Deck);
            var json = JsonSerializer.Serialize(dto);

            Nodes.Hand.RpcId(PeerId, "DrawCard", json);
            foreach (var enemyPeerId in EnemyPeerIds) Nodes.EnemyHand.RpcId(enemyPeerId, "DrawCard");
        }
    }

    public void PlayCard(Guid id)
    {
        var card = Hand.FirstOrDefault(x => x.Id == id);

        if (card == null) throw new ApplicationException();

        Hand.Remove(card);
        card.Zone = CardZone.Board;
        Board.AddLast(card);

        var dtoJson = JsonSerializer.Serialize(new CardGameStateDto(id, card.Record, CardZone.Board, CardZone.Hand));

        Nodes.Board.RpcId(PeerId, "PlaceCard", dtoJson);
        Nodes.Hand.RpcId(PeerId, "RemoveCard", id.ToString());

        foreach (var enemyPeerId in EnemyPeerIds)
        {
            Nodes.EnemyBoard.RpcId(enemyPeerId, "PlaceCard", dtoJson);
            Nodes.EnemyHand.RpcId(enemyPeerId, "RemoveCard");
        }
    }
}