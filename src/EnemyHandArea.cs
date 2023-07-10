using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OpenCCG.Core;
using OpenCCG.Net;
using OpenCCG.Net.Rpc;

namespace OpenCCG;

public partial class EnemyHandArea : HBoxContainer, IMessageReceiver<MessageType>
{
    private static readonly PackedScene CardHiddenScene = GD.Load<PackedScene>("res://scenes/card-hidden.tscn");
    private readonly List<CardHidden> _cards = new();
    [Export] private Curve _heightCurve, _rotationCurve, _separationCurve;

    public Dictionary<string, IObserver>? Observers => null;

    public override void _Ready()
    {
        SortChildren += CustomSort;
        PreSortChildren += PreCustomSort;
    }

    [Rpc]
    public async void HandleMessageAsync(string messageJson)
    {
        await IMessageReceiver<MessageType>.HandleMessageAsync(this, messageJson);
    }

    public Executor? GetExecutor(MessageType messageType)
    {
        return messageType switch
        {
            MessageType.RemoveCard => Executor.Make(RemoveCard, Executor.ResponseMode.NoResponse),
            MessageType.DrawCard => Executor.Make(DrawCard, Executor.ResponseMode.Respond),
            _ => null
        };
    }

    private void RemoveCard()
    {
        var cardEntity = _cards.LastOrDefault();
        if (cardEntity == null) return;

        _cards.Remove(cardEntity);
        cardEntity.QueueFree();
    }

    private async Task DrawCard()
    {
        var entity = CardHiddenScene.Make<CardHidden>(this);
        _cards.Add(entity);
        await entity.PlayDrawAnimAsync();
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