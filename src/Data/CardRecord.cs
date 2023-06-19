using System.Linq;
using System.Reflection;
using System.Text;

namespace OpenCCG.Data;

public enum CardRecordType
{
    Creature,
    Spell
}

public record CardEffectRecord(string Id, string? InitJson = null)
{
    public string GetText() => Database.CardEffects[Id](InitJson).GetText();
}

public class CardAbilities
{
    public bool Exposed { get; init; }

    public bool Haste { get; init; }

    public bool Drain { get; init; }
    
    public bool Defender { get; init; }

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
    public CardEffectRecord? Spell { get; init; }
    public CardEffectRecord? Enter { get; init; }
    public CardEffectRecord? Exit { get; init; }
    public CardEffectRecord? StartTurn { get; init; }
    public CardEffectRecord? EndTurn { get; init; }
    public CardEffectRecord? StartCombat { get; init; }
    public CardEffectRecord? EndCombat { get; init; }

    public string GetText()
    {
        var sb = new StringBuilder();

        if (Spell != null) sb.AppendLine($"{Spell.GetText()}");
        if (Enter != null) sb.AppendLine($"Enter: {Enter.GetText()}");
        if (Exit != null) sb.AppendLine($"Exit: {Exit.GetText()}");
        if (StartTurn != null) sb.AppendLine($"Start of Turn: {StartTurn.GetText()}");
        if (EndTurn != null) sb.AppendLine($"End of Turn: {EndTurn.GetText()}");
        if (StartCombat != null) sb.AppendLine($"Start of Combat: {StartCombat.GetText()}");
        if (EndCombat != null) sb.AppendLine($"End of Combat: {EndCombat.GetText()}");

        return sb.ToString();
    }
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
        var effectsText = CardEffects.GetText();

        if (abilitiesText.Length > 0) sb.AppendLine(Abilities.GetText());
        if (effectsText.Length > 0) sb.AppendLine(effectsText);

        return sb.ToString();
    }
}