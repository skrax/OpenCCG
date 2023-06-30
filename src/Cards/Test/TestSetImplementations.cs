using System;
using System.Collections.Generic;
using OpenCCG.Net;

namespace OpenCCG.Cards.Test;

public class TestSetImplementations
{
    private static readonly Dictionary<string, Func<PlayerGameState2, CardImplementation>> Mappings = new();

    public static void Init()
    {
        RegisterCreature("TEST-C-001", (outline, state) => new VanillaCreature(outline, new(), state));
        RegisterCreature("TEST-C-002", (outline, state) => new VanillaCreature(outline, new(), state));
        RegisterCreature("TEST-C-003", (outline, state) => new VanillaCreature(outline, new(), state));
        RegisterCreature("TEST-C-004", (outline, state) => new VanillaCreature(outline, new(), state));
        RegisterCreature("TEST-C-005", (outline, state) => new VanillaCreature(outline, new(), state));
        RegisterCreature("TEST-C-006", (outline, state) => new VanillaCreature(outline, new CreatureAbilities
        {
            Exposed = true
        }, state));
        RegisterCreature("TEST-C-007", (outline, state) => new VanillaCreature(outline, new CreatureAbilities
        {
            Haste = true
        }, state));
        RegisterCreature("TEST-C-008", (outline, state) => new MorneholdSpectre(outline, state));
    }

    public static CardImplementation GetImplementation(string key, PlayerGameState2 playerGameState)
        => Mappings[key](playerGameState);

    private static void Register(string key, Func<PlayerGameState2, CardImplementation> mapping)
    {
        Mappings.Add(key, mapping);
    }

    private static void RegisterCreature(string key,
        Func<ICreatureOutline, PlayerGameState2, CardImplementation> mapping)
    {
        Mappings.Add(key, x => mapping((TestSetOutlines.Cards[key] as ICreatureOutline)!, x));
    }

    private static void RegisterSpell(string key, Func<ISpellOutline, PlayerGameState2, CardImplementation> mapping)
    {
        Mappings.Add(key, x => mapping((TestSetOutlines.Cards[key] as ISpellOutline)!, x));
    }
}