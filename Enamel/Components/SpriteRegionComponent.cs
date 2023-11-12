using Microsoft.Xna.Framework;

namespace Enamel.Components;

/// <summary>
/// Represents a subsection of a sprite sheet, for rendering
/// </summary>
/// <param name="X">The X coord of the CELL in the spritesheet to render, not the pixel coord!</param>
/// <param name="Y">The Y coord of the CELL in the spritesheet to render, not the pixel coord!</param>
/// <param name="Width">The width in pixels of cells in the spritesheet</param>
/// <param name="Height">The height in pixels of cells in the spritesheet</param>
public readonly record struct SpriteRegionComponent(int X, int Y, int Width, int Height)
{
    public Rectangle ToRectangle()
    {
        return new Rectangle(X*Width, Y*Height, Width, Height);
    }
}