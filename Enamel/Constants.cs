using Enamel.Enums;
using Microsoft.Xna.Framework;
using StbImageSharp;

namespace Enamel;

public static class Constants
{
    public static readonly Color HighlightColour = new(200, 200, 255);
    public const int MAP_WIDTH = 8;
    public const int MAP_HEIGHT = 8;
    public const int MOVE_SPEED = 20;
    public const int DEFAULT_MOVES_PER_TURN = 3;
    public const int CURRENT_TURN_TEXT_SIZE = 20;
    public static readonly Color CURRENT_TURN_TEXT_COLOUR = Color.White;
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