using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Core;
using OpenCCG.Net;
using OpenCCG.Net.Rpc;

namespace OpenCCG.GameBoard;

public partial class EnemyHand : HBoxContainer
{
    [Export] private PackedScene _cardHiddenScene = null!;
    [Export] private Curve _heightCurve = null!, _rotationCurve = null!, _separationCurve = null!;
    private readonly List<CardHidden> _cards = new();

    public override void _Ready()
    {
        SortChildren += CustomSort;
        PreSortChildren += PreCustomSort;
    }

    public void RemoveCard()
    {
        var cardEntity = _cards.LastOrDefault();
        if (cardEntity == null) return;

        _cards.Remove(cardEntity);
        cardEntity.QueueFree();
    }

    public void AddCard()
    {
        var entity = _cardHiddenScene.Make<CardHidden>(this);
        _cards.Add(entity);
    }

    private void PreCustomSort()
    {
        if (!_cards.Any()) return;
        var separation = (int)_separationCurve.Sample((_cards.Count - 0f) / 12f) * 40;

        if (HasThemeConstantOverride("separation"))
            AddThemeConstantOverride("separation", separation);
    }

    private void CustomSort()
    {
        for (var index = 0; index < _cards.Count; index++)
        {
            var c = _cards[index];

            if (_cards.Count > 3)
            {
                var sampleIndex = (float)(index - 0) / (_cards.Count - 1 - 0);
                var pos = c.GlobalPosition;
                pos.Y += _heightCurve.Sample(sampleIndex) * 18;
                c.GlobalPosition = pos;
                c.RotationDegrees = _rotationCurve.Sample(sampleIndex) * -4f;
            }

            c.PlayDrawAnimAsync(new Vector2(1921, c.GlobalPosition.Y), c.GlobalPosition);

            c.ZIndex = _cards.Count - index;
        }
    }
}