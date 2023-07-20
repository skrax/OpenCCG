namespace OpenCCG.Net.Messaging;

public interface IMessageController
{
    public void Configure(IMessageBroker broker);
}