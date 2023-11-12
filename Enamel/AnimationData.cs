using System.Collections.Generic;

namespace Enamel;

public class AnimationData
{
    public int SpriteWidth { get; private set; }
    public int SpriteHeight { get; private set; }
    public (int X, int Y)[] Frames { get; private set; }

    public AnimationData(int spriteWidth, int spriteHeight, (int X, int Y)[] frames)
    {
        SpriteWidth = spriteWidth;
        SpriteHeight = spriteHeight;
        Frames = frames;
    }
}