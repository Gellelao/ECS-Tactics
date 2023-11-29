using Microsoft.Xna.Framework;
using MoonTools.Structs;

namespace Enamel.Components;

public readonly struct ScreenPositionComponent(float x, float y)
{
    private Position2D Position { get;} = new(x, y);
    public int X => Position.X;
    public int Y => Position.Y;
    public Vector2 ToVector
    {
        get
        {
            var numericVector = Position.ToVector2();
            return new Vector2(numericVector.X, numericVector.Y);
        }
    }
}