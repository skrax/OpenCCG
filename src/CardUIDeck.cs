using Godot;
using OpenCCG.Core;
using OpenCCG.Data;

namespace OpenCCG;

public partial class CardUIDeck : ColorRect, INodeInit<CardRecord>
{
    [Export] private Label _costLabel, _countLabel, _nameLabel;
    [Export] private TextureRect _image;

    public int Count { get; private set; }
    public CardRecord Record { get; private set; }

    public void Init(CardRecord record)
    {
        _costLabel.Text = record.Cost.ToString();
        _countLabel.Text = "1x";
        Count = 1;
        _nameLabel.Text = record.Name;
        _image.Texture = GD.Load<Texture2D>(record.ImgPath);
        Record = record;
    }

    public void SetCount(int count)
    {
        Count = count;
        _countLabel.Text = $"{count}x";
    }

    public record JsonRecord(string Id, int Count);

    public JsonRecord ToJsonRecord() => new(Record.Id, Count);
}