using Microsoft.Xna.Framework;
using MoonTools.Structs;

namespace Enamel.Components;

public readonly struct PositionComponent
{
    private Position2D Position { get;}
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

    public PositionComponent(Position2D position)
    {
        Position = position;
    }
    
    public PositionComponent(float x, float y)
    {
        Position = new Position2D(x, y);
    }
}