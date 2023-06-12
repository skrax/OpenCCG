using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenCCG.Data;

public enum CardRecordType
{
    Creature,
    Spell
}

public record CardEffectRecord(string Id, string initJson);

public record CardRecord(
    string Id,
    string Name,
    CardEffectRecord[] Effects,
    CardRecordType Type,
    int Atk,
    int Def,
    int Cost,
    string ImgPath
)
{
    public string Description => GetEffectText();

    private List<ICardEffect>? _cachedEffects;

    private string GetEffectText()
    {
        var sb = new StringBuilder();
        if (_cachedEffects == null)
        {
            _cachedEffects = new List<ICardEffect>(Effects.Length);

            foreach (var cardEffectRecord in Effects)
            {
                var effect = Database.CardEffects[cardEffectRecord.Id](cardEffectRecord.initJson);
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