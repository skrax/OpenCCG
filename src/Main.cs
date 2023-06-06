using System;
using System.Collections.Generic;
using Godot;
using OpenCCG.Net;
using OpenCCG.Net.Dto;

namespace OpenCCG;

public partial class Main : Node, IMessageReceiver<MessageType>
{
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

    public Dictionary<string, IObserver>? Observers => null;

    [Rpc]
    public void HandleMessage(string messageJson)
    {
        IMessageReceiver<MessageType>.HandleMessage(this, messageJson);
    }

    public Func<int, string?, string?> GetExecutor(MessageType messageType) => messageType switch
    {
        _ => throw new NotImplementedException()
    };
}