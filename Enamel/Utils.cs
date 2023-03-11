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
}