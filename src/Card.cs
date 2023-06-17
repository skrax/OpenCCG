using System;
using System.Collections.Generic;
using Godot;
using OpenCCG.Core;
using OpenCCG.Data;
using OpenCCG.Net.Dto;

namespace OpenCCG;

public partial class Card : Sprite2D, INodeInit<CardGameStateDto>
{
    private readonly HashSet<CardZone> _hoverZones = new();
    [Export] private Area2D _area2D;
    [Export] private CardStatPanel _costPanel, _atkPanel, _defPanel;
    private Vector2 _dragOffset;
    [Export] private CardInfoPanel _infoPanel, _namePanel;
    [Export] private PackedScene _cardPreviewScene;

    public Action? OnDragFailed;
    private bool _hovering, _canHover = true;
    private CardGameStateDto _cardGameState;
    private CardPreview? _preview;

    public Guid Id { get; private set; }

    public void Init(CardGameStateDto card)
    {
        _cardGameState = card;
        var record = card.Record;
        Id = card.Id;
        _infoPanel.Value = record.Description;
        _costPanel.Value = card.Cost;
        _atkPanel.Value = card.Atk;
        _defPanel.Value = card.Def;
        _namePanel.Value = record.Name;
        if (record.Type is not CardRecordType.Creature)
        {
            _atkPanel.Visible = false;
            _defPanel.Visible = false;
        }

        Logger.Info<Card>($"Init Card: {card.Atk}/{card.Def}/{card.Cost}");
        Texture = GD.Load<Texture2D>(record.ImgPath);

    }

    public override void _Ready()
    {
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

        if (inputEvent is InputEventMouseMotion mouseMotion)
        {
            switch (_hovering)
            {
                case false when rect.HasPoint(ToLocal(mouseMotion.Position)):
                    if (!_canHover) return;

                    _hovering = true;
                    EventSink.ReportPointerEnter(this);
                    break;
                case true when !rect.HasPoint(ToLocal(mouseMotion.Position)):
                    _hovering = false;

                    EventSink.ReportPointerExit(this);
                    break;
            }
        }
    }

    public void ShowPreview()
    {
        Visible = false;
        _preview ??= _cardPreviewScene.Make<CardPreview>(GetParent());
        _preview.Init(_cardGameState);
        var pos = Position;
        pos.Y -= 256;
        _preview.Position = pos;
        _preview.Visible = true;
    }

    public void DisablePreview()
    {
        Visible = true;
        _preview!.Visible = false;
    }

    public void PlayOrInvokeDragFailure()
    {
        if (!_hoverZones.Contains(CardZone.Hand))
        {
            GetNode<Main>("/root/Main").PlayCard(Id);

            return;
        }

        OnDragFailed?.Invoke();
    }

    public void DrawOutline(bool enabled)
    {
        var shader = Material as ShaderMaterial;
        shader?.SetShaderParameter("drawOutline", true);
        
        _preview?.DrawOutline(enabled);
    }
}