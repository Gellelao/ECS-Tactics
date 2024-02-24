using System;
using Enamel.Enums;

namespace Enamel.Extensions;

public static class OrbTypeExtensions
{
    public static Sprite ToSocketSprite(this OrbType orbType)
    {
        switch (orbType)
        {
            case OrbType.Any:
            case OrbType.Colourless:
                return Sprite.GreySocket;
            case OrbType.Arcane:
                return Sprite.BlueSocket;
            case OrbType.None:
            case OrbType.Solar:
            default:
                throw new ArgumentOutOfRangeException(nameof(orbType), orbType, null);
        }
    }
}