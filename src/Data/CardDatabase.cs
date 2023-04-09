namespace OpenCCG.Data;

public static class CardDatabase
{
    public static readonly CardRecord[] Cards =
    {
        new("A tiny dragon", 1, 2, 2, "res://img/cards/dragon.png"),
        new("A small dragon", 2, 3, 2, "res://img/cards/dragon2.png"),
        new("A regular dragon", 3, 3, 3, "res://img/cards/dragon3.png"),
        new("A big dragon", 4, 4, 3, "res://img/cards/dragon4.png"),
        new("A large dragon", 5, 4, 4, "res://img/cards/dragon5.png")
    };
}