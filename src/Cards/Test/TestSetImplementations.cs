using System;
using System.Collections.Generic;
using OpenCCG.Net;
using OpenCCG.Net.Gameplay.Test;

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
        RegisterSpell("TEST-S-002", (outline, state) => new FellTheMighty(outline, state));
        RegisterSpell("TEST-S-003", (outline, state) => new SquishTheWimpy(outline, state));
        RegisterSpell("TEST-S-004", (outline, state) => new Firebomb(outline, state));
        RegisterSpell("TEST-S-005", (outline, state) => new ImminentCatastrophe(outline, state));
        RegisterSpell("TEST-S-006", (outline, state) => new DeepAnalysis(outline, state));
        RegisterSpell("TEST-S-007", (outline, state) => new HeedTheCall(outline, state));
        RegisterSpell("TEST-S-008", (outline, state) => new StringUp(outline, state));
        RegisterSpell("TEST-S-009", (outline, state) => new CallToArms(outline, state));
        RegisterSpell("TEST-S-010", (outline, state) => new RiseToTheOccasion(outline, state));
    }

    private static void RegisterCreatures()
    {
        RegisterCreature("TEST-C-001",
            (outline, state) => new VanillaCreature(outline, new CreatureAbilities(), state));
        RegisterCreature("TEST-C-002",
            (outline, state) => new VanillaCreature(outline, new CreatureAbilities(), state));
        RegisterCreature("TEST-C-003",
            (outline, state) => new VanillaCreature(outline, new CreatureAbilities(), state));
        RegisterCreature("TEST-C-004",
            (outline, state) => new VanillaCreature(outline, new CreatureAbilities(), state));
        RegisterCreature("TEST-C-005",
            (outline, state) => new VanillaCreature(outline, new CreatureAbilities(), state));
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
        RegisterCreature("TEST-C-012", (outline, state) => new BerenMorne(outline, state));
        RegisterCreature("TEST-C-013", (outline, state) => new BountyHunter(outline, state));
        RegisterCreature("TEST-C-014", (outline, state) => new Equites(outline, state));
        RegisterCreature("TEST-C-015", (outline, state) => new Peltast(outline, state));
        RegisterCreature("TEST-C-016", (outline, state) => new VanillaCreature(outline, new CreatureAbilities()
        {
            Defender = true
        }, state));
        RegisterCreature("TEST-C-017", (outline, state) => new FieldMedic(outline, state));
        RegisterCreature("TEST-C-018", (outline, state) => new ChampionOfNargaeya(outline, state));
        RegisterCreature("TEST-C-019", (outline, state) => new Hastatus(outline, state));
    }

    public static CardImplementation GetImplementation(string key, PlayerGameState playerGameState)
    {
        return Mappings[key](playerGameState);
    }

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