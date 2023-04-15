using System;
using System.Collections.Generic;

namespace OpenCCG;

public static class EventSink
{
    public static readonly List<CardBoard> PointerDownCardBoard = new();
    public static readonly List<CardBoard> PointerUpCardBoard = new();
    public static readonly List<Card> PointerDownCards = new();
    public static readonly List<Card> PointerUpCards = new();
    public static readonly List<Avatar> PointerDownAvatars = new();
    public static readonly List<Avatar> PointerUpAvatars = new();
    public static readonly List<EnemyAvatar> PointerDownEnemyAvatars = new();
    public static readonly List<EnemyAvatar> PointerUpEnemyAvatars = new();

    public static void Drain()
    {
        PointerDownCards.Clear();
        PointerUpCards.Clear();
        PointerDownCardBoard.Clear();
        PointerUpCardBoard.Clear();
        PointerDownAvatars.Clear();
        PointerUpAvatars.Clear();
        PointerDownEnemyAvatars.Clear();
        PointerUpEnemyAvatars.Clear();
    }

    public static void ReportPointerUp<T>(T entity)
    {
        switch (entity)
        {
            case CardBoard x:
                PointerUpCardBoard.Add(x);
                break;
            case Card x:
                PointerUpCards.Add(x);
                break;
            case Avatar x:
                PointerUpAvatars.Add(x);
                break;
            case EnemyAvatar x:
                PointerUpEnemyAvatars.Add(x);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(entity));
        }
    }

    public static void ReportPointerDown<T>(T entity)
    {
        switch (entity)
        {
            case CardBoard x:
                PointerDownCardBoard.Add(x);
                break;
            case Card x:
                PointerDownCards.Add(x);
                break;
            case Avatar x:
                PointerDownAvatars.Add(x);
                break;
            case EnemyAvatar x:
                PointerDownEnemyAvatars.Add(x);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(entity));
        }
    }
}