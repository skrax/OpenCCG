using System;
using System.Collections.Generic;
using OpenCCG.Cards;

namespace OpenCCG.Net.Gameplay.Test;

public class TestSetImplementations
{
    private static readonly Dictionary<string, Func<ICard>> Mappings = new();

    public static void Init()
    {
        RegisterCreatures();

        RegisterSpells();
    }

    private static void RegisterSpells()
    {
        RegisterSpell("TEST-S-001", outline => new ThrowingKnife(outline));
        RegisterSpell("TEST-S-002", outline => new FellTheMighty(outline));
        RegisterSpell("TEST-S-003", outline => new SquishTheWimpy(outline));
        RegisterSpell("TEST-S-004", outline => new Firebomb(outline));
        RegisterSpell("TEST-S-005", outline => new ImminentCatastrophe(outline));
        RegisterSpell("TEST-S-006", outline => new DeepAnalysis(outline));
        RegisterSpell("TEST-S-007", outline => new HeedTheCall(outline));
        RegisterSpell("TEST-S-008", outline => new StringUp(outline));
        RegisterSpell("TEST-S-009", outline => new CallToArms(outline));
        RegisterSpell("TEST-S-010", outline => new RiseToTheOccasion(outline));
    }

    private static void RegisterCreatures()
    {
        RegisterCreature("TEST-C-001", outline => new VanillaCreature(outline, new CreatureAbilities()));
        RegisterCreature("TEST-C-002", outline => new VanillaCreature(outline, new CreatureAbilities()));
        RegisterCreature("TEST-C-003", outline => new VanillaCreature(outline, new CreatureAbilities()));
        RegisterCreature("TEST-C-004", outline => new VanillaCreature(outline, new CreatureAbilities()));
        RegisterCreature("TEST-C-005", outline => new VanillaCreature(outline, new CreatureAbilities()));
        RegisterCreature("TEST-C-006", outline => new VanillaCreature(outline, new CreatureAbilities
        {
            Exposed = true
        }));
        RegisterCreature("TEST-C-007", outline => new VanillaCreature(outline, new CreatureAbilities
        {
            Haste = true
        }));
        RegisterCreature("TEST-C-008", outline => new MorneholdSpectre(outline));
        RegisterCreature("TEST-C-009", outline => new VanillaCreature(outline, new CreatureAbilities
        {
            Drain = true
        }));

        RegisterCreature("TEST-C-010", outline => new VanillaCreature(outline, new CreatureAbilities
        {
            Defender = true
        }));
        RegisterCreature("TEST-C-011", outline => new MorneholdAssassin(outline));
        RegisterCreature("TEST-C-012", outline => new BerenMorne(outline));
        RegisterCreature("TEST-C-013", outline => new BountyHunter(outline));
        RegisterCreature("TEST-C-014", outline => new Equites(outline));
        RegisterCreature("TEST-C-015", outline => new Peltast(outline));
        RegisterCreature("TEST-C-016", outline => new VanillaCreature(outline, new CreatureAbilities()
        {
            Defender = true
        }));
        RegisterCreature("TEST-C-017", outline => new FieldMedic(outline));
        RegisterCreature("TEST-C-018", outline => new ChampionOfNargaeya(outline));
        RegisterCreature("TEST-C-019", outline => new Hastatus(outline));
    }

    public static ICard GetImplementation(string key)
    {
        return Mappings[key]();
    }

    private static void RegisterCreature(string key, Func<CreatureOutline, Creature> mapping)
    {
        Mappings.Add(key, () => mapping((TestSetOutlines.Cards[key] as CreatureOutline)!));
    }

    private static void RegisterSpell(string key, Func<SpellOutline, ICard> mapping)
    {
        Mappings.Add(key, () => mapping((TestSetOutlines.Cards[key] as SpellOutline)!));
    }
}