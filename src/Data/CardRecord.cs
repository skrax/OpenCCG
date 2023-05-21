using System.Linq;

namespace OpenCCG.Data;

public enum CardRecordType
{
    Creature,
    Spell
}

public record CardRecord
{
    public string Id { get; }
    public string Name { get; }
    public ICardEffect[] Effects { get; }
    public CardRecordType Type { get; }
    public int Atk { get; }
    public int Def { get; }
    public int Cost { get; }
    public string ImgPath { get; }

    public string Description { get; }

    public CardRecord
    (
        string Id,
        string Name,
        ICardEffect[] Effects,
        CardRecordType Type,
        int Atk,
        int Def,
        int Cost,
        string ImgPath
    )
    {
        this.Id = Id;
        this.Name = Name;
        this.Effects = Effects;
        this.Type = Type;
        this.Atk = Atk;
        this.Def = Def;
        this.Cost = Cost;
        this.ImgPath = ImgPath;
        Description = Effects.Any()
            ? Effects.Select(x => x.Text).Aggregate((x, y) => $"{x}\n{y}")
            : "";
    }
}