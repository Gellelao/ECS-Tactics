using MoonTools.ECS;

namespace Enamel.Components.Messages;

public readonly record struct PlayerUnitSelectedMessage(Entity Entity) : IHasEntity;