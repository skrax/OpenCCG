namespace OpenCCG.Cards;

public interface ICardOutline
{
    public string Id { get; }
    public string Name { get; }

    public string Description { get; }
    public int Cost { get; }

    public string ImgPath { get; }
}