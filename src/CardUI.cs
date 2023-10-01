using Godot;
using OpenCCG.Core;
using OpenCCG.Proto;

namespace OpenCCG;

public partial class CardUI : TextureRect, INodeInit<CreatureOutline>, INodeInit<SpellOutline>
{
    [Export] private CardStatPanel _costPanel = null!, _atkPanel = null!, _defPanel = null!;
    [Export] private CardInfoPanel _infoPanel = null!, _namePanel = null!;

    public void Init(CreatureOutline creature)
    {
        _infoPanel.Value = creature.Description;
        _namePanel.Value = creature.Name;
        _costPanel.SetValue(creature.Cost, creature.Cost);
        _atkPanel.SetValue(creature.Atk, creature.Atk);
        _defPanel.SetValue(creature.Def, creature.Def);

        Texture = GD.Load<Texture2D>(creature.ImgPath);
    }

    public void Init(SpellOutline spell)
    {
        _infoPanel.Value = spell.Description;
        _namePanel.Value = spell.Name;
        _costPanel.SetValue(spell.Cost, spell.Cost);
        _atkPanel.Visible = false;
        _defPanel.Visible = false;

        Texture = GD.Load<Texture2D>(spell.ImgPath);
    }
}