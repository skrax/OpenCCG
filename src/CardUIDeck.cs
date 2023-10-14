using System;
using System.IO;
using Godot;
using OpenCCG.Core;
using OpenCCG.Proto;
using Serilog;

namespace OpenCCG;

public partial class CardUIDeck : ColorRect, INodeInit<CreatureOutline>, INodeInit<SpellOutline>
{
    [Export] private Label _costLabel = null!, _countLabel = null!, _nameLabel = null!;
    [Export] private TextureRect _image = null!;

    public int Count { get; private set; }
    public SpellOutline? SpellOutline { get; private set; }
    public CreatureOutline? CreatureOutline { get; private set; }
    public CardType CardType { get; private set; }

    public string Id => CardType switch
    {
        CardType.CreatureType => CreatureOutline!.Id,
        CardType.SpellType => SpellOutline!.Id,
        _ => throw new ArgumentOutOfRangeException()
    };

    public void Init(CreatureOutline creatureOutline)
    {
        _costLabel.Text = creatureOutline.Cost.ToString();
        _countLabel.Text = "1x";
        Count = 1;
        _nameLabel.Text = creatureOutline.Name;
        var fileName = Path.GetFileNameWithoutExtension(creatureOutline.ImgPath);
        _image.Texture = GD.Load<AtlasTexture>($"res://img/cards/deck/{fileName}.tres");
        CreatureOutline = creatureOutline;
        CardType = CardType.CreatureType;
    }

    public void Init(SpellOutline spellOutline)
    {
        _costLabel.Text = spellOutline.Cost.ToString();
        _countLabel.Text = "1x";
        Count = 1;
        _nameLabel.Text = spellOutline.Name;
        var bla = Path.GetFileNameWithoutExtension(spellOutline.ImgPath);
        _image.Texture = GD.Load<AtlasTexture>($"res://img/cards/deck/{bla}.tres");
        SpellOutline = spellOutline;
        CardType = CardType.SpellType;
    }

    public void SetCount(int count)
    {
        Count = count;
        _countLabel.Text = $"{count}x";
    }
}