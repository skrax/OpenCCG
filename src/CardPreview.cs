using Godot;
using OpenCCG.Core;
using OpenCCG.Data;
using OpenCCG.Net.Dto;

namespace OpenCCG;

public partial class CardPreview : TextureRect, INodeInit<CardGameStateDto>
{
    [Export] private CardStatPanel _costPanel, _atkPanel, _defPanel;
    [Export] private CardInfoPanel _infoPanel, _namePanel;

    public void Init(CardGameStateDto outline)
    {
        var record = outline.Record;
        _infoPanel.Value = record.Description;
        _costPanel.Value = outline.Cost;
        _atkPanel.Value = outline.Atk;
        _defPanel.Value = outline.Def;
        _namePanel.Value = record.Name;
        if (record.Type is not CardRecordType.Creature)
        {
            _atkPanel.Visible = false;
            _defPanel.Visible = false;
        }

        Texture = GD.Load<Texture2D>(record.ImgPath);
    }

    public void DrawOutline(bool enabled)
    {
        var shader = Material as ShaderMaterial;

        shader?.SetShaderParameter("drawOutline", enabled);
    }
}