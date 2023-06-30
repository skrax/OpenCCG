namespace OpenCCG.Cards;

public record CreatureOutline(
    string Id,
    string Name,
    string Description,
    int Cost,
    int Atk,
    int Def,
    string ImgPath
) : ICreatureOutline;