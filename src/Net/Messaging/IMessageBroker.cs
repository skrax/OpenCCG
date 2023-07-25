using System;
using System.Threading.Tasks;

namespace OpenCCG.Net.Messaging;

public interface IMessageBroker
{
    public void Map(string route, MessageResolver resolver);

    public void MapAwaitableResponse(string route);

    public void MapResponse(string route, MessageResolver resolver);

    public void SendResult(MessageContext messageContext, MessageControllerResult result);
    public void EnqueueMessage(long peerId, Message message);

    public bool TryEnqueueMessageWithResponse(long peerId, Message message);

    public Task<MessageContext?> EnqueueMessageAndGetResponseAsync(long peerId, Message message);
    public Task<MessageContext?> EnqueueMessageAndGetResponseAsync(long peerId, Message message, TimeSpan timeout);
}