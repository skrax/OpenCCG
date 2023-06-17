using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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

public record CardRecord(
    string Id,
    string Name,
    CardEffectRecord[] PlayEffects,
    CardEffectRecord[] ExitEffects,
    CardEffectRecord[] StartTurnEffects,
    CardEffectRecord[] EndTurnEffects,
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

    private CardEffectRecord[] Effects => PlayEffects.Concat(ExitEffects)
                                                     .Concat(StartTurnEffects)
                                                     .Concat(EndTurnEffects)
                                                     .ToArray();

    private string GetEffectText()
    {
        var sb = new StringBuilder();
        if (_cachedEffects == null)
        {
            _cachedEffects = new List<ICardEffect>(Effects.Length);

            foreach (var cardEffectRecord in Effects)
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