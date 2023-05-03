using System.Linq;
using Godot;
using OpenCCG.Core;
using OpenCCG.Data;

namespace OpenCCG;

public partial class CardBrowser : Control
{
    private static readonly PackedScene CardUIScene = GD.Load<PackedScene>("res://scenes/card-ui.tscn");
    private ScrollContainer _scrollContainer;
    private FlowContainer _flowContainer;
    private HBoxContainer _bottomPanelContainer;
    private Button _clearTextButton;
    private TextEdit _textEdit;

    public override void _Ready()
    {
        _scrollContainer = GetChild<ScrollContainer>(0);
        _flowContainer = _scrollContainer.GetChild<FlowContainer>(0);
        _bottomPanelContainer = GetChild<HBoxContainer>(1);
        _clearTextButton = _bottomPanelContainer.GetChild<Button>(0);
        _textEdit = _bottomPanelContainer.GetChild<TextEdit>(1);

        _clearTextButton.Pressed += () =>
        {
            Logger.Info("text cleared");
            _textEdit.Text = "";

            foreach (var child in _flowContainer.GetChildren())
            {
                if (!child.IsQueuedForDeletion()) child.QueueFree();
            }

            foreach (var cardRecord in CardDatabase.Cards)
            {
                CardUIScene.Make<CardUI, CardRecord>(cardRecord, _flowContainer);
            }
        };
        _textEdit.TextChanged += () =>
        {
            var text = _textEdit.Text;
            foreach (var child in _flowContainer.GetChildren())
            {
                if (!child.IsQueuedForDeletion()) child.QueueFree();
            }

            foreach (var cardRecord in CardDatabase.Cards.Where(x => x.Description.Contains(text)))
            {
                CardUIScene.Make<CardUI, CardRecord>(cardRecord, _flowContainer);
            }
        };

        foreach (var cardRecord in CardDatabase.Cards)
        {
            CardUIScene.Make<CardUI, CardRecord>(cardRecord, _flowContainer);
        }
    }
}