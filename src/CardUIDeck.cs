using Godot;
using OpenCCG.Cards;
using OpenCCG.Core;

namespace OpenCCG;

public partial class CardUIDeck : ColorRect, INodeInit<ICardOutline>
{
    [Export] private Label _costLabel, _countLabel, _nameLabel;
    [Export] private TextureRect _image;

    public int Count { get; private set; }
    public ICardOutline Outline { get; private set; }

    public void Init(ICardOutline dto)
    {
        _costLabel.Text = dto.Cost.ToString();
        _countLabel.Text = "1x";
        Count = 1;
        _nameLabel.Text = dto.Name;
        _image.Texture = GD.Load<Texture2D>(dto.ImgPath);
        Outline = dto;
    }

    public void SetCount(int count)
    {
        Count = count;
        _countLabel.Text = $"{count}x";
    }

    public JsonRecord ToJsonRecord()
    {
        return new JsonRecord(Outline.Id, Count);
    }

    public record JsonRecord(string Id, int Count);
}