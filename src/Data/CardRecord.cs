namespace OpenCCG.Data;

public record CardRecord
(
    string Id,
    string Name,
    string Description,
    int Atk,
    int Def,
    int Cost,
    string ImgPath
);