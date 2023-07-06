using Godot;
using OpenCCG.Cards;
using OpenCCG.Core;

namespace OpenCCG;

public partial class CardPreview : TextureRect, INodeInit<CardImplementationDto>
{
    [Export] private CardStatPanel _costPanel, _atkPanel, _defPanel;
    [Export] private CardInfoPanel _infoPanel, _namePanel;

    public void Init(CardImplementationDto dto)
    {
        _infoPanel.Value = dto.Outline.Description;
        _costPanel.Value = dto.Outline.Cost;
        _namePanel.Value = dto.Outline.Name;
        Texture = GD.Load<Texture2D>(dto.Outline.ImgPath);

        if (dto.IsCreature)
        {
            _atkPanel.Value = dto.CreatureOutline!.Atk;
            _defPanel.Value = dto.CreatureOutline.Def;
        }
        else if (dto.IsSpell)
        {
            _atkPanel.Visible = false;
            _defPanel.Visible = false;
        }
    }

    public void DrawOutline(bool enabled)
    {
        var shader = Material as ShaderMaterial;

        shader?.SetShaderParameter("drawOutline", enabled);
    }
}