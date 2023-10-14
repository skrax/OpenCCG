using Celnet.Domain;
using Celnet.Domain.Events;
using Celnet.Infrastructure.ENet;
using Celnet.Infrastructure.Protobuf;
using Godot;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using OpenCCG.Core;
using OpenCCG.Net.Controllers;
using OpenCCG.Proto;
using Serilog;

namespace OpenCCG.Net;

public partial class CelnetClient : Node
{
    private readonly Client _client;
    private readonly Backend<IMessage, IMessage> _backend;
    public readonly ProtobufPeerTransport Transport;
    [Export] private string _ip = "localhost";
    [Export] private ushort _port = 5777;


    private readonly MetricsController _metricsController;

    public CelnetClient()
    {
        _backend = Backend<IMessage, IMessage>.Make();
        _metricsController = new MetricsController(_backend);

        _client = new Client(new ENetService());
        _client.Connect(_ip, _port, ProtobufConfig.ChannelLimit);
        _client.OnPeerConnected += OnPeerConnected;
        _client.OnPeerDisconnected += OnPeerDisconnected;

        Transport = ProtobufPeerTransport.AsServer(_backend, _client,
            TypeRegistry.FromFiles(ProtocolReflection.Descriptor), false);

        Log.Information(Logging.Templates.ServiceIsRunning, nameof(CelnetClient));
    }

    private void OnPeerDisconnected(PeerDisconnectEvent ev)
    {
        Log.Information("{PeerId} disconnected", ev.PeerId);
    }

    private void OnPeerConnected(PeerConnectEvent ev)
    {
        Log.Information("{PeerId} connected", ev.PeerId);
    }

    public override void _Process(double delta)
    {
        _client.Poll();
    }
}