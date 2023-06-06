using System;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Core;

namespace OpenCCG;

public partial class CardTempArea : Sprite2D
{
    [Export] private InputEventSystem _inputEventSystem;
    private string? _requestId;

    [Rpc]
    public void ShowPermanent(string imgPath)
    {
        Texture = GD.Load<Texture2D>(imgPath);
    }

    [Rpc]
    public async void Show(string imgPath, string timeSpan)
    {
        Texture = GD.Load<Texture2D>(imgPath);
        await Task.Delay(TimeSpan.Parse(timeSpan));
        Reset();
    }

    [Rpc]
    public void Reset()
    {
        Texture = null;
    }

    [Rpc]
    public void RequireTargets(string requestId, string imgPath)
    {
        if (_requestId is not null)
        {
            Logger.Error<CardTempArea>("There is already a request being processed");
            return;
        }

        _requestId = requestId;

        ShowPermanent(imgPath);
        _inputEventSystem.OnRequireTarget();
    }

    public bool TryUpstreamTarget<T>(T target)
    {
        if (_requestId is null)
        {
            Logger.Error<CardTempArea>($"No request id set to use {nameof(TryUpstreamTarget)}");
            return false;
        } 
        
        // TODO
        if (target is CardBoard cardBoard)
        {
            //GetNode("/root/Main").RpcId(1, "UpstreamTargetCard", _requestId, cardBoard.CardGameState.Id.ToString());
            Reset();
            return true;
        }
        else if (target is EnemyAvatar)
        {
            //GetNode("/root/Main").RpcId(1, "UpstreamTargetEnemyAvatar", _requestId);
            Reset();
            return true;
        }

        return false;
    }
}