using Enamel.Enums;

namespace Enamel.Components;

public readonly record struct SocketComponent(bool Required, OrbType ExpectedOrbType);