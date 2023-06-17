using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Godot.Collections;
using OpenCCG.Net;
using Array = System.Array;

namespace OpenCCG.Data;

public enum CardRecordType
{
    Creature,
    Spell
}

public record CardEffectRecord(string Id, string? InitJson = null);

public class CardAbilities
{
    public bool Exposed { get; init; }

    public bool Haste { get; init; }

    public bool Drain { get; init; }

    public string GetText()
    {
        var sb = new StringBuilder();
        var abilities = AllAbilities.Where(x => (bool)x.GetValue(this)!).Select(x => x.Name);
        sb.AppendJoin(", ", abilities);

        return sb.ToString();
    }

    private static readonly PropertyInfo[] AllAbilities = typeof(CardAbilities)
                                                          .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                                          .Where(x => x.PropertyType == typeof(bool))
                                                          .ToArray();
}

public class CardEffects
{
    public CardEffectRecord[] Enter { get; init; } = Array.Empty<CardEffectRecord>();
    public CardEffectRecord[] Exit { get; init; } = Array.Empty<CardEffectRecord>();
    public CardEffectRecord[] StartTurn { get; init; } = Array.Empty<CardEffectRecord>();
    public CardEffectRecord[] EndTurn { get; init; } = Array.Empty<CardEffectRecord>();
    public CardEffectRecord[] StartCombat { get; init; } = Array.Empty<CardEffectRecord>();
    public CardEffectRecord[] EndCombat { get; init; } = Array.Empty<CardEffectRecord>();

    public CardEffectRecord[] Effects => Enter.Concat(Exit)
                                              .Concat(StartTurn)
                                              .Concat(EndTurn)
                                              .Concat(StartCombat)
                                              .Concat(EndCombat)
                                              .ToArray();
}

public record CardRecord(
    string Id,
    string Name,
    CardEffects CardEffects,
    CardAbilities Abilities,
    CardRecordType Type,
    int Atk,
    int Def,
    int Cost,
    string ImgPath
)
{
    public string Description => GetDescription();

    private string GetDescription()
    {
        var sb = new StringBuilder();

        var abilitiesText = Abilities.GetText();
        if (abilitiesText.Length > 0) sb.AppendLine(Abilities.GetText());
        sb.AppendLine(GetEffectText());

        return sb.ToString();
    }

    private List<ICardEffect>? _cachedEffects;


    private string GetEffectText()
    {
        var sb = new StringBuilder();
        if (_cachedEffects == null)
        {
            _cachedEffects = new List<ICardEffect>(CardEffects.Effects.Length);

            foreach (var cardEffectRecord in CardEffects.Effects)
            {
                var effect = Database.CardEffects[cardEffectRecord.Id](cardEffectRecord.InitJson);
                _cachedEffects.Add(effect);

                sb.AppendLine(effect.GetText());
            }

            return sb.ToString();
        }

        foreach (var cachedEffect in _cachedEffects)
        {
            sb.AppendLine(cachedEffect.GetText());
        }

        return sb.ToString();
    }
}