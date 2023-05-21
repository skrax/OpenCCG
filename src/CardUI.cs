using Godot;
using OpenCCG.Core;
using OpenCCG.Data;

namespace OpenCCG;

public partial class CardUI : TextureRect, INodeInit<CardRecord>
{
    [Export] private CardStatPanel _costPanel, _atkPanel, _defPanel;

    [Export] private CardInfoPanel _infoPanel, _namePanel;

    public CardRecord Record { get; private set; }

    public void Init(CardRecord record)
    {
        _infoPanel.Value = record.Description;
        _costPanel.Value = record.Cost;
        _atkPanel.Value = record.Atk;
        _defPanel.Value = record.Def;
        _namePanel.Value = record.Name;
        if (record.Type is not CardRecordType.Creature)
        {
            _atkPanel.Visible = false;
            _defPanel.Visible = false;
        }
        Texture = GD.Load<Texture2D>(record.ImgPath);
        Record = record;
    }
}