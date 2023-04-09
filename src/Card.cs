using System;
using System.Collections.Generic;
using Godot;
using OpenCCG.Core;
using OpenCCG.Net.Dto;

namespace OpenCCG;

public partial class Card : Sprite2D, INodeInit<CardGameStateDto>
{
    private static ulong? _dragInstanceId;
    private Area2D _area2D;
    private CardStatPanel _costPanel, _atkPanel, _defPanel;
    private Vector2 _dragOffset;

    private readonly HashSet<CardZone> _hoverZones = new();
    private CardInfoPanel _infoPanel;

    public Action? OnDragFailed;

    public Guid Id { get; private set; }

    public void Init(CardGameStateDto card)
    {
        var record = card.Record;
        Id = card.Id;
        _infoPanel.Description = record.Description;
        _costPanel.Value = card.Cost;
        _atkPanel.Value = card.Atk;
        _defPanel.Value = card.Def;
        Logger.Info<Card>($"Init Card: {card.Atk}/{card.Def}/{card.Cost}");
        Texture = GD.Load<Texture2D>(record.ImgPath);
    }

    public override void _Ready()
    {
        _infoPanel = GetChild<CardInfoPanel>(0);
        _atkPanel = GetChild<CardStatPanel>(1);
        _defPanel = GetChild<CardStatPanel>(2);
        _costPanel = GetChild<CardStatPanel>(3);
        _area2D = GetChild<Area2D>(4);

        _area2D.AreaEntered += OnAreaEntered;
        _area2D.AreaExited += OnAreaExited;
    }


    private void OnAreaEntered(Area2D area)
    {
        switch (area)
        {
            case HandArea:
                _hoverZones.Add(CardZone.Hand);
                break;
            case BoardArea:
                _hoverZones.Add(CardZone.Board);
                break;
        }
    }

    private void OnAreaExited(Area2D area)
    {
        switch (area)
        {
            case HandArea:
                _hoverZones.Remove(CardZone.Hand);
                break;
            case BoardArea:
                _hoverZones.Remove(CardZone.Board);
                break;
        }
    }


    public override void _ExitTree()
    {
        if (_dragInstanceId == GetInstanceId())
            _dragInstanceId = null;
    }

    public override void _Input(InputEvent inputEvent)
    {
        var rect = GetRect();
        if (inputEvent.IsActionPressed(InputActions.SpriteClick))
        {
            if (inputEvent is not InputEventMouseButton mouseEvent ||
                !rect.HasPoint(ToLocal(mouseEvent.Position))) return;

            EventSink.ReportPointerDown(this);
        }

        if (inputEvent.IsActionReleased(InputActions.SpriteClick))
        {
            if (inputEvent is not InputEventMouseButton mouseEvent ||
                !rect.HasPoint(ToLocal(mouseEvent.Position))) return;

            EventSink.ReportPointerUp(this);
        }
    }

    public void PlayOrInvokeDragFailure()
    {
        if (!_hoverZones.Contains(CardZone.Hand))
        {
            GetNode("/root/Main").RpcId(1, "PlayCard", Id.ToString());

            return;
        }

        OnDragFailed?.Invoke();
    }
}