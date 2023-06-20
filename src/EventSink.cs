using System;
using System.Collections.Generic;

namespace OpenCCG;

public static class EventSink
{
    public static readonly List<CardBoard> PointerDownCardBoard = new();
    public static readonly List<CardBoard> PointerUpCardBoard = new();
    public static readonly List<CardBoard> PointerEnterCardBoard = new();
    public static readonly List<CardBoard> PointerExitCardBoard = new();
    public static readonly List<Card> PointerDownCards = new();
    public static readonly List<Card> PointerUpCards = new();
    public static readonly List<Card> PointerEnterCards = new();
    public static readonly List<Card> PointerExitCards = new();
    public static readonly List<Avatar> PointerDownAvatars = new();
    public static readonly List<Avatar> PointerUpAvatars = new();
    public static readonly List<Avatar> PointerEnterAvatars = new();
    public static readonly List<Avatar> PointerExitAvatars = new();

    public static void Drain()
    {
        PointerDownCards.Clear();
        PointerUpCards.Clear();
        PointerEnterCards.Clear();
        PointerExitCards.Clear();
        PointerDownCardBoard.Clear();
        PointerUpCardBoard.Clear();
        PointerEnterCardBoard.Clear();
        PointerExitCardBoard.Clear();
        PointerDownAvatars.Clear();
        PointerUpAvatars.Clear();
        PointerEnterAvatars.Clear();
        PointerExitAvatars.Clear();
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
            default:
                throw new ArgumentOutOfRangeException(nameof(entity));
        }
    }

    public static void ReportPointerEnter<T>(T entity)
    {
        switch (entity)
        {
            case CardBoard x:
                PointerEnterCardBoard.Add(x);
                break;
            case Card x:
                PointerEnterCards.Add(x);
                break;
            case Avatar x:
                PointerEnterAvatars.Add(x);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(entity));
        }
    }

    public static void ReportPointerExit<T>(T entity)
    {
        switch (entity)
        {
            case CardBoard x:
                PointerExitCardBoard.Add(x);
                break;
            case Card x:
                PointerExitCards.Add(x);
                break;
            case Avatar x:
                PointerExitAvatars.Add(x);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(entity));
        }
    }
}