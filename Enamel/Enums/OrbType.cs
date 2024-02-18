using System;

namespace Enamel.Enums;

[Flags]
public enum OrbType
{
    None = 1,
    Arcane = 2,
    Solar = 4,
    Any = None | Arcane | Solar
}