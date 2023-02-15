using Enamel.Enums;
using Microsoft.Xna.Framework;

namespace Enamel;

public static class Constants
{
    public static readonly Color HighlightColour = new(200, 200, 255);
    public static readonly int MapWidth = 8;
    public static readonly int MapHeight = 8;
    public static readonly int MoveSpeed = 20;
    public static readonly int DefaultMovesPerTurn = 3;
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