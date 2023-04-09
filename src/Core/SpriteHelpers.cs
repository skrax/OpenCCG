using Godot;

namespace OpenCCG.Core;

public static class SpriteHelpers
{
    public static void OrderHorizontally(Sprite2D[] sprites)
    {
        if (sprites.Length == 0) return;

        foreach (var card in sprites) card.Position = new Vector2(0, 0);

        var width = sprites[0].GetRect().Size.X * sprites[0].Scale.X + 20f;
        var entireWidth = (sprites.Length - 1) * width;

        for (var i = 1; i < sprites.Length; ++i)
        {
            var translation = new Vector2(width * i, 0);

            sprites[i].Transform = sprites[i].Transform.Translated(translation);
        }

        foreach (var card in sprites) card.Transform = card.Transform.Translated(new Vector2(-entireWidth / 2, 0));
    }
}