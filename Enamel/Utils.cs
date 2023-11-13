using System;
using Enamel.Enums;
using Microsoft.Xna.Framework;

namespace Enamel;

public static class Utils
{
    public static Vector2 GridToScreenCoords(int gridX, int gridY){
        float screenX = Constants.TILE_WIDTH * gridX / 2 -
                        Constants.TILE_WIDTH * gridY / 2;
        float screenY = Constants.TILE_HEIGHT * gridX / 2 +
                        Constants.TILE_HEIGHT * gridY / 2;
        return new Vector2(screenX, screenY);
    }

    public static Direction GetDirection(int originX, int originY, int targetX, int targetY)
    {
        var yDiff = originY - targetY;
        var xDiff = originX - targetX;
        if (xDiff != 0 && yDiff != 0)
        {
            throw new NotImplementedException("Diagonal projectiles not implemented");
        }

        if (xDiff < 0) return Direction.East;
        if (xDiff > 0) return Direction.West;
        if (yDiff < 0) return Direction.South;
        if (yDiff > 0) return Direction.North;

        return Direction.None;
    }
}