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

    public void Init(ICardOutline dto)
    {
        _infoPanel.Value = dto.Description;
        _costPanel.Value = dto.Cost;
        switch (dto)
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
                throw new ArgumentOutOfRangeException(nameof(dto));
        }

        _namePanel.Value = dto.Name;

        Texture = GD.Load<Texture2D>(dto.ImgPath);
        Outline = dto;
    }
}