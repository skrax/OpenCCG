using System;
using Godot;

namespace OpenCCG.Core;

public static class Logger
{
    public static void Info<T>(string message)
    {
        GD.Print($"{MessageHeader<T>()}{message}");
    }

    public static void Info<T>(params object[] o)
    {
        GD.Print(MessageHeader<T>, o);
    }

    public static void Info(string message)
    {
        GD.Print(MessageHeader(), message);
    }

    public static void Info(params object[] o)
    {
        GD.Print(MessageHeader(), o);
    }

    public static void Warning<T>(string message)
    {
        GD.PushWarning($"{MessageHeader<T>()}{message}");
    }

    public static void Warning<T>(params object[] o)
    {
        GD.PushWarning(MessageHeader<T>(), o);
    }


    public static void Warning(string message)
    {
        GD.PushWarning(MessageHeader(), message);
    }

    public static void Warning(params object[] o)
    {
        GD.PushWarning(MessageHeader(), o);
    }

    public static void Error<T>(string message)
    {
        GD.PushError($"{MessageHeader<T>()}{message}");
    }

    public static void Error<T>(params object[] o)
    {
        GD.PushError(MessageHeader<T>(), o);
    }

    public static void Error(string message)
    {
        GD.PushError(MessageHeader(), message);
    }

    public static void Error(params object[] o)
    {
        GD.PushError(MessageHeader(), o);
    }

    private static string MessageHeader<T>()
    {
        return $"[{DateTime.Now.TimeOfDay}][{typeof(T).Name}] ";
    }

    private static string MessageHeader()
    {
        return $"[{DateTime.Now.TimeOfDay}] ";
    }
}