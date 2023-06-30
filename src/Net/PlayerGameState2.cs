using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenCCG.Cards;
using OpenCCG.Cards.Test;
using OpenCCG.Core;
using OpenCCG.Data;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG.Net;

public class PlayerGameState2
{
    public long PeerId { get; init; }

    public long EnemyPeerId { get; set; }

    public PlayerGameState2 Enemy { get; set; }

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
                                   card.Zone = CardZone.Deck;

                                   return card;
                               })
                               .Shuffle();

        Deck = new LinkedList<CardImplementation>(shuffledDeckList);

        Nodes.MidPanel.EndTurnButtonSetActive(PeerId, new EndTurnButtonSetActiveDto(false, null));
    }

    public void Start()
    {
        Draw(Rules.InitialCardsDrawn);
        UpdateEnergy();
        UpdateHealth();
    }

    public async Task StartTurnAsync()
    {
        if (IsTurn) return;
        IsTurn = true;
        Energy = MaxEnergy = Math.Min(Rules.MaxEnergy, MaxEnergy + Rules.EnergyGainedPerTurn);
        UpdateEnergy();
        Draw();

        foreach (CreatureImplementation creature in Board)
        {
            await creature.OnUpkeepAsync();
        }

        Nodes.MidPanel.EndTurnButtonSetActive(PeerId, new EndTurnButtonSetActiveDto(true, null));

        foreach (CreatureImplementation creature in Board)
        {
            await creature.OnStartTurnAsync();
        }
    }

    public async Task EndTurnAsync()
    {
        if (!IsTurn) return;

        Nodes.MidPanel.EndTurnButtonSetActive(PeerId, new EndTurnButtonSetActiveDto(false, "End Step"));

        foreach (CreatureImplementation creature in Board)
        {
            await creature.OnEndStepAsync();
        }

        foreach (CreatureImplementation creature in Board)
        {
            await creature.OnEndTurnAsync();
        }

        IsTurn = false;
        Nodes.MidPanel.EndTurnButtonSetActive(PeerId, new EndTurnButtonSetActiveDto(false, null));

        await Enemy.StartTurnAsync();
    }

    public async Task<bool> PlayCardAsync(Guid id)
    {
        if (!IsTurn) return false;

        var card = Hand.SingleOrDefault(x => x.Id == id);
        if (card == null) return false;
        if (Energy - card.Cost < 0) return false;

        Energy -= card.Cost;
        UpdateEnergy();

        if (card is CreatureImplementation creatureImplementation)
        {
            await creatureImplementation.OnPlayAsync();
            await creatureImplementation.OnEnterBoardAsync();
            UpdateCardCount();
            await creatureImplementation.OnEnterAsync();
        }
        else if (card is SpellImplementation spellImplementation)
        {
            await spellImplementation.OnPlayAsync();
            spellImplementation.MoveToZone(CardZone.Pit);
            UpdateCardCount();
        }
        
        return true;
    }

    public void Draw(int count = 1)
    {
        for (var i = 0; i < count; ++i)
        {
            if (Deck.First == null) break;
            var card = Deck.First.Value;
            card.MoveToZone(CardZone.Hand);

            var dto = card.AsDto();

            Nodes.Hand.DrawCard(PeerId, dto);
            Nodes.EnemyHand.DrawCard(EnemyPeerId);

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
}