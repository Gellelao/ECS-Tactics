using Microsoft.Xna.Framework;

namespace Enamel;

public static class Constants
{
    public const int PIXEL_SCREEN_WIDTH = 320;
    public const int PIXEL_SCREEN_HEIGHT = 180;
    public const float PIXEL_RATIO = (float) PIXEL_SCREEN_WIDTH / PIXEL_SCREEN_HEIGHT;

    public static readonly Color HighlightColour = new(200, 200, 255);

    public const int MAP_WIDTH = 8;
    public const int MAP_HEIGHT = 8;
    public const int TILE_WIDTH = 28;
    public const int TILE_HEIGHT = 14;

    public const int DEFAULT_WALK_SPEED = 10;
    public const int DEFAULT_PROJECTILE_SPEED = 100;

    public const int PLAYER_FRAME_WIDTH = 22;
    public const int PLAYER_FRAME_HEIGHT = 32;

    public const int DEFAULT_MILLIS_BETWEEN_FRAMES = 180;

    public static readonly Color CurrentTurnTextColour = Color.White;
    public static readonly Color TomeTextColour = Color.Black;
}