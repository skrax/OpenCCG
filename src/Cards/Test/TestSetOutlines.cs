using System.Collections.Generic;
using System.Linq;

namespace OpenCCG.Cards.Test;

public static class TestSetOutlines
{
    public static readonly ICardOutline[] Creatures = new CreatureOutline[]
    {
        new("TEST-C-001", "Tiny Dragon",
            "",
            1, 2, 2,
            "res://img/cards/dragon.png"),
        new("TEST-C-002", "Small Dragon",
            "",
            2, 3, 3,
            "res://img/cards/dragon2.png"),
        new("TEST-C-003", "Regular Dragon",
            "",
            3, 5, 4,
            "res://img/cards/dragon3.png"),
        new("TEST-C-004", "Big Dragon",
            "",
            4, 6, 5,
            "res://img/cards/dragon4.png"),
        new("TEST-C-005", "Large Dragon",
            "",
            5, 7, 6,
            "res://img/cards/dragon5.png"),
        new("TEST-C-006", "Orestes, Half-Blind",
            "\n\n[center][b]Exposed[/b][/center]",
            1, 3, 2,
            "res://img/cards/orestes_half_blind.png"),
        new("TEST-C-007", "Ymir, Winter Soldier",
            "\n\n[center][b]Haste[/b][/center]",
            3, 3, 3,
            "res://img/cards/ymir_winter_soldier.png"),
        new("TEST-C-008", "Mornehold Spectre",
            "\n[center][b]Arcane[/b]\n[b]Exit:[/b] Deal 3 damage to a random enemy creature[/center]",
            2, 2, 1,
            "res://img/cards/mornehold_spectre.png"),
        new("TEST-C-009", "Black Leech",
            "\n\n[center][b]Drain[/b][/center]",
            1, 1, 3,
            "res://img/cards/black_leech.png"),
        new("TEST-C-010", "Towering Giant",
            "\n\n[center][b]Defender[/b][/center]",
            6, 4, 8,
            "res://img/cards/towering_giant.png"),
        new("TEST-C-011", "Mornehold Assassin",
            "\n\n[center][b]Play:[/b] Deal 2 damage[/center]",
            2, 2, 2,
            "res://img/cards/mornehold_assassin.png"),
        new("TEST-C-012", "Beren Morne",
            "\n\n[center][b]End of Turn:[/b] Deal 5 damage[/center]",
            8, 5, 5,
            "res://img/cards/beren_morne.png"),
        new("TEST-C-013", "Bounty Hunter",
            "\n[center][b]Arcane[/b]\n[b]Play:[/b] Expose a creature[/center]",
            3, 4, 3,
            "res://img/cards/bounty_hunter.png"),
        new("TEST-C-014", "Equites",
            "\n[center][b]Haste[/b]\n[b]Start Combat:[/b] Gain +1 ATK for each friendly creature[/center]",
            3,1,3,
            "res://img/cards/equites.png")
    };

    public static readonly ICardOutline[] Spells = new SpellOutline[]
    {
        new("TEST-S-001", "Throwing Knife",
            "\n\n[center]Deal 2 damage[/center]",
            1,
            "res://img/cards/throwing_knife.png"),
        new("TEST-S-002", "Fell the Mighty",
            "\n\n[center]Destroy the highest attack creature(s)[/center]",
            4,
            "res://img/cards/fell_the_mighty.png"),
        new("TEST-S-003", "Squish the Wimpy",
            "\n\n[center]Destroy the lowest attack creatures(s)[/center]",
            3,
            "res://img/cards/squish_the_wimpy.png"),
        new("TEST-S-004", "Firebomb",
            "\n\n[center]Deal 5 damage[/center]",
            3,
            "res://img/cards/firebomb.png"),
        new("TEST-S-005", "Imminent Catastrophe",
            "\n\n[center]Deal 7 damage to all creatures[/center]",
            7,
            "res://img/cards/imminent_catastrophe.png"),
        new("TEST-S-006", "Deep Analysis",
            "\n\n[center]Draw 2 cards[/center]",
            4,
            "res://img/cards/deep_analysis.png"),
        new("TEST-S-007", "Heed the Call",
            "\n\n[center]Summon 2 Mornehold Spectres[/center]",
            5,
            "res://img/cards/heed_the_call.png"),
        new("TEST-S-008", "String up",
            "\n\n[center]Destroy a creature[/center]",
            4,
            "res://img/cards/string_up.png")
    };

    public static readonly ICardOutline[] All = Creatures.Concat(Spells).ToArray();

    public static readonly Dictionary<string, ICardOutline> Cards = All.ToDictionary(x => x.Id);
}