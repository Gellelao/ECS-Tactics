using Enamel.Enums;

namespace Enamel;

public class AnimationData
{
    public int SpriteWidth { get; private set; }
    public int SpriteHeight { get; private set; }
    public int[][] Frames { get; private set; }

    /// <summary>
    /// Defines all the animations for a single entity
    /// </summary>
    /// <param name="spriteWidth">Width in pixels that one sprite takes up on the sheet</param>
    /// <param name="spriteHeight">Height in pixels that one sprite takes up on the sheet</param>
    /// <param name="frames">2D array, where the first index is the <see cref="AnimationType"/>,
    /// and the second index is the frame of the animation</param>
    public AnimationData(int spriteWidth, int spriteHeight, int[][] frames)
    {
        SpriteWidth = spriteWidth;
        SpriteHeight = spriteHeight;
        Frames = frames;
    }
}