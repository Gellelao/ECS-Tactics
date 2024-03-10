using Enamel.Enums;

namespace Enamel.Components.Messages;

public readonly record struct SetStartingOrbsForPlayerMessage(PlayerId PlayerId, CharacterId CharacterId);