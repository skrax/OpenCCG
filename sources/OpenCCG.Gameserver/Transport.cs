using System.Collections.Concurrent;
using Celnet.Domain;
using Celnet.Infrastructure.ENet;
using Celnet.Infrastructure.Protobuf;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace OpenCCG.Gameserver;

public class Transport : ProtobufPeerTransport
{
    public Transport(Backend<IMessage, IMessage> backend, Server server, TypeRegistry typeRegistry)
        : base(backend,
            server,
            typeRegistry,
            new ConcurrentDictionary<string, TaskCompletionSource<IMessage>>())
    {
    }
}