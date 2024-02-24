using System;

namespace Enamel.Enums;

[Flags]
public enum OrbType
{
    Colourless = 1,
    Arcane = 2,
    Solar = 4,
    None = 256, // Keep this one last and don't include it in "Any"
    Any = Colourless | Arcane | Solar
}