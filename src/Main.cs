using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;
using OpenCCG.Core;
using OpenCCG.Net;
using OpenCCG.Net.Dto;
using OpenCCG.Net.Rpc;
using FileAccess = Godot.FileAccess;

namespace OpenCCG;

public partial class Main : Node, IMessageReceiver<MessageType>
{
    public Dictionary<string, IObserver>? Observers => null;

    [Rpc]
    public async void HandleMessageAsync(string messageJson)
    {
        await IMessageReceiver<MessageType>.HandleMessageAsync(this, messageJson);
    }

    public Executor GetExecutor(MessageType messageType)
    {
        return messageType switch
        {
            _ => throw new NotImplementedException()
        };
    }

    public void PlayCard(Guid id)
    {
        IMessageReceiver<MessageType>.FireAndForget(this, 1, MessageType.PlayCard, id);
    }

    public void CombatPlayerCard(Guid attackerId, Guid targetId)
    {
        IMessageReceiver<MessageType>.FireAndForget(this, 1, MessageType.CombatPlayerCard,
            new CombatPlayerCardDto(attackerId, targetId));
    }

    public void EndTurn()
    {
        IMessageReceiver<MessageType>.FireAndForget(this, 1, MessageType.EndTurn);
    }

    public void CombatPlayer(Guid cardId)
    {
        IMessageReceiver<MessageType>.FireAndForget(this, 1, MessageType.CombatPlayer, cardId);
    }

    public void Enqueue()
    {
        var runtimeData = this.GetAutoloaded<RuntimeData>();

        var password = runtimeData._useQueuePassword ? runtimeData._queuePassword : null;
        using var file = FileAccess.Open(runtimeData._deckPath, FileAccess.ModeFlags.Read);
        if (file == null) throw new FileLoadException();
        var deck = JsonSerializer.Deserialize<CardUIDeck.JsonRecord[]>(file.GetAsText())!;

        var dto = new QueuePlayerDto(deck, password);

        IMessageReceiver<MessageType>.FireAndForget(this, 1, MessageType.Queue, dto);
    }
}