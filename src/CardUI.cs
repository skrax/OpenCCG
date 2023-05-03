using Godot;
using OpenCCG.Core;
using OpenCCG.Data;

namespace OpenCCG;

public partial class CardUI : TextureRect, INodeInit<CardRecord>
{
    private Area2D _area2D;
    
    private CardStatPanel _costPanel, _atkPanel, _defPanel;
    
    private CardInfoPanel _infoPanel;

    public void Init(CardRecord record)
    {
        _infoPanel.Description = record.Description;
        _costPanel.Value = record.Cost;
        _atkPanel.Value = record.Atk;
        _defPanel.Value = record.Def;
        Texture = GD.Load<Texture2D>(record.ImgPath);
    }

    public override void _Ready()
    {
        _infoPanel = GetChild<CardInfoPanel>(0);
        _atkPanel = GetChild<CardStatPanel>(1);
        _defPanel = GetChild<CardStatPanel>(2);
        _costPanel = GetChild<CardStatPanel>(3);
    }
}