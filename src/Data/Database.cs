using System;
using System.Collections.Generic;
using System.Linq;
using OpenCCG.Net.ServerNodes;

namespace OpenCCG.Data;

public static class Database
{
    public static readonly Dictionary<string, CardRecord> Cards =
        new CardRecord[]
        {
            new("TEST-001", "Tiny Dragon",
                Array.Empty<CardEffectRecord>(),
                CardRecordType.Creature,
                2, 2, 1, "res://img/cards/dragon.png"),
            new("TEST-002", "Small Dragon",
                Array.Empty<CardEffectRecord>(),
                CardRecordType.Creature,
                3, 2, 2, "res://img/cards/dragon2.png"),
            new("TEST-003", "Regular Dragon",
                Array.Empty<CardEffectRecord>(),
                CardRecordType.Creature,
                3, 3, 3, "res://img/cards/dragon3.png"),
            new("TEST-004", "Big Dragon",
                Array.Empty<CardEffectRecord>(),
                CardRecordType.Creature,
                4, 3, 4, "res://img/cards/dragon4.png"),
            new("TEST-005", "Large Dragon",
                Array.Empty<CardEffectRecord>(),
                CardRecordType.Creature,
                4, 4, 5, "res://img/cards/dragon5.png"),
            new("TEST-006", "Throwing Knife",
                new[] { DealDamageCardEffect.MakeRecord(2, RequireTargetSide.Enemy, RequireTargetType.All) },
                CardRecordType.Spell,
                0, 0, 1, "res://img/cards/throwing_knife.png")
        }.ToDictionary(x => x.Id);

    public static readonly Dictionary<string, Func<string?, ICardEffect>> CardEffects = new(new[]
    {
        new KeyValuePair<string, Func<string?, ICardEffect>>(DealDamageCardEffect.Id, s =>
        {
            if (s == null) throw new ArgumentNullException();
            return new DealDamageCardEffect(s);
        })
    });
}