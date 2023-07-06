namespace OpenCCG.Cards;

public record SpellOutline(
    string Id,
    string Name,
    string Description,
    int Cost,
    string ImgPath
) : ISpellOutline;