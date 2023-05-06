namespace OpenCCG.Data;

public static class CardDatabase
{
    public static readonly CardRecord[] Cards =
    {
        new("TEST-001", "A tiny dragon", "", 2, 2, 1, "res://img/cards/dragon.png"),
        new("TEST-002", "A small dragon", "", 3, 2, 2, "res://img/cards/dragon2.png"),
        new("TEST-003", "A regular dragon", "", 3, 3, 3, "res://img/cards/dragon3.png"),
        new("TEST-004", "A big dragon", "", 4, 3, 4, "res://img/cards/dragon4.png"),
        new("TEST-005", "A large dragon", "", 4, 4, 5, "res://img/cards/dragon5.png")
    };
}