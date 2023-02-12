using MoonTools.ECS;

namespace Enamel.Components.Messages;

public readonly record struct StartTurnMessage(Entity Entity) : IHasEntity;