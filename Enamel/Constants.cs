using Enamel.Enums;
using Microsoft.Xna.Framework;

namespace Enamel;

public static class Constants
{
    public static readonly Color HighlightColour = new(200, 200, 255);
    public const int MAP_WIDTH = 8;
    public const int MAP_HEIGHT = 8;
    public const int TILE_WIDTH = 28;
    public const int TILE_HEIGHT = 14;
    public const int DEFAULT_MOVE_SPEED = 20;
    public const int DEFAULT_PROJECTILE_SPEED = 100;
    public const int DEFAULT_MOVES_PER_TURN = 3;
    public const int CURRENT_TURN_TEXT_SIZE = 20;
    public const int SPELL_CARD_TEXT_SIZE = 10;
    public static readonly Color CurrentTurnTextColour = Color.White;
    public static readonly Color SpellCardTextColour = Color.Black;
    public static readonly PlayerNumber[] TwoPlayerTurnOrder = {PlayerNumber.One, PlayerNumber.Two, PlayerNumber.Two, PlayerNumber.One};

    public static readonly PlayerNumber[] ThreePlayerTurnOrder = {PlayerNumber.One, PlayerNumber.Two, PlayerNumber.Three,
                                                                  PlayerNumber.Two, PlayerNumber.Three, PlayerNumber.One,
                                                                  PlayerNumber.Three, PlayerNumber.One, PlayerNumber.Two
    };
    public static readonly PlayerNumber[] FourPlayerTurnOrder = {PlayerNumber.One, PlayerNumber.Two, PlayerNumber.Three, PlayerNumber.Four,
                                                                 PlayerNumber.Four, PlayerNumber.Three, PlayerNumber.Two, PlayerNumber.One,
                                                                 PlayerNumber.Three, PlayerNumber.Four, PlayerNumber.One, PlayerNumber.Two,
                                                                 PlayerNumber.Two, PlayerNumber.One, PlayerNumber.Four, PlayerNumber.Three
    };
}