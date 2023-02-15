using MoonTools.ECS;

namespace Enamel.Components.Messages;

public readonly record struct EndTurnMessage(Entity Entity) : IHasEntity;