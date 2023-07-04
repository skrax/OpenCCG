using System;
using System.Collections.Generic;
using OpenCCG.Net;

namespace OpenCCG.Cards.Test;

public class TestSetImplementations
{
    private static readonly Dictionary<string, Func<PlayerGameState, CardImplementation>> Mappings = new();

    public static void Init()
    {
        RegisterCreatures();

        RegisterSpells();
    }

    private static void RegisterSpells()
    {
        RegisterSpell("TEST-S-001", (outline, state) => new ThrowingKnife(outline, state));
    }

    private static void RegisterCreatures()
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
        RegisterCreature("TEST-C-009", (outline, state) => new VanillaCreature(outline, new CreatureAbilities
        {
            Drain = true
        }, state));

        RegisterCreature("TEST-C-010", (outline, state) => new VanillaCreature(outline, new CreatureAbilities
        {
            Defender = true
        }, state));
        RegisterCreature("TEST-C-011", (outline, state) => new MorneholdAssassin(outline, state));
    }

    public static CardImplementation GetImplementation(string key, PlayerGameState playerGameState)
        => Mappings[key](playerGameState);

    private static void Register(string key, Func<PlayerGameState, CardImplementation> mapping)
    {
        Mappings.Add(key, mapping);
    }

    private static void RegisterCreature(string key,
        Func<CreatureOutline, PlayerGameState, CardImplementation> mapping)
    {
        Mappings.Add(key, x => mapping((TestSetOutlines.Cards[key] as CreatureOutline)!, x));
    }

    private static void RegisterSpell(string key, Func<SpellOutline, PlayerGameState, CardImplementation> mapping)
    {
        Mappings.Add(key, x => mapping((TestSetOutlines.Cards[key] as SpellOutline)!, x));
    }
}