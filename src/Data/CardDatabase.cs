using System;

namespace OpenCCG.Data;

public static class CardDatabase
{
    public static readonly CardRecord[] Cards =
    {
        new("TEST-001", "Tiny Dragon",
            Array.Empty<ICardEffect>(), CardRecordType.Creature,
            2, 2, 1, "res://img/cards/dragon.png"),
        new("TEST-002", "Small Dragon",
            Array.Empty<ICardEffect>(), CardRecordType.Creature,
            3, 2, 2, "res://img/cards/dragon2.png"),
        new("TEST-003", "Regular Dragon",
            Array.Empty<ICardEffect>(), CardRecordType.Creature,
            3, 3, 3, "res://img/cards/dragon3.png"),
        new("TEST-004", "Big Dragon",
            Array.Empty<ICardEffect>(), CardRecordType.Creature,
            4, 3, 4, "res://img/cards/dragon4.png"),
        new("TEST-005", "Large Dragon",
            Array.Empty<ICardEffect>(), CardRecordType.Creature,
            4, 4, 5, "res://img/cards/dragon5.png"),
        new("TEST-006", "Throwing Knife",
            new ICardEffect[] { new DealDamageCardEffect(1) }, CardRecordType.Spell,
            0, 0, 1, "res://img/cards/throwing_knife.png")
    };
}