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
    //ICardEffect[] Effects,
    //this.Effects = Effects;
    //Effects.Any()
    //? Effects.Select(x => x.Text).Aggregate((x, y) => $"{x}\n{y}")
    //     :
    //public ICardEffect[] Effects { get; }

    public string Description { get; } = "";
}