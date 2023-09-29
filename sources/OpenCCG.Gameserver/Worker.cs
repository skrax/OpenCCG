using Celnet.Domain.Events;
using Celnet.Infrastructure.ENet;

namespace OpenCCG.Gameserver;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly Server _server;

    public Worker(
        ILogger<Worker> logger,
        Server server
        )
    {
        _logger = logger;
        _server = server;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _server.Create(5777, 2);
        _server.OnPeerConnected += OnPeerConnected;
        _server.OnPeerDisconnected += OnPeerDisconnected;
        _server.OnPeerTimeout += OnPeerTimeout;
        if (!_server.IsRunning)
        {
            throw new InvalidOperationException("Failed to create server");
        }
        
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000/ 16, stoppingToken);
            _server.Poll();
        }
    }

    private void OnPeerTimeout(PeerTimeoutEvent ev)
    {
        _logger.LogInformation("{PeerId} timeout", ev.PeerId);
    }

    private void OnPeerDisconnected(PeerDisconnectEvent ev)
    {
        _logger.LogInformation("{PeerId} disconnected", ev.PeerId);
    }

    private void OnPeerConnected(PeerConnectEvent ev)
    {
        _logger.LogInformation("{PeerId} connected", ev.PeerId);
    }
}