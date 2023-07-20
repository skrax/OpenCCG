namespace OpenCCG.Net.Messaging;

public abstract class MessageClient
{
    protected readonly IMessageBroker Broker;

    protected MessageClient(IMessageBroker broker)
    {
        Broker = broker;
    }

    public abstract void Configure();
}