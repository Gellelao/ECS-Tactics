using System;

namespace Enamel.Components
{
    public struct PositionComponent
    {
        public int X { get; init; }
        public int Y { get; init; }

        public PositionComponent(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}