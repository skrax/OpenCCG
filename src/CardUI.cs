using System;
using Godot;
using OpenCCG.Cards;
using OpenCCG.Core;

namespace OpenCCG;

public partial class CardUI : TextureRect, INodeInit<ICardOutline>
{
    [Export] private CardStatPanel _costPanel, _atkPanel, _defPanel;

    [Export] private CardInfoPanel _infoPanel, _namePanel;

    public ICardOutline Outline { get; private set; }

    public void Init(ICardOutline outline)
    {
        _infoPanel.Value = outline.Description;
        _costPanel.Value = outline.Cost;
        switch (outline)
        {
            case ICreatureOutline creatureOutline:
                _atkPanel.Value = creatureOutline.Atk;
                _defPanel.Value = creatureOutline.Def;
                break;
            case ISpellOutline spellOutline:
                _atkPanel.Visible = false;
                _defPanel.Visible = false;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(outline));
        }

        _namePanel.Value = outline.Name;

        Texture = GD.Load<Texture2D>(outline.ImgPath);
        Outline = outline;
    }
}