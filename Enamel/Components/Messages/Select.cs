using MoonTools.ECS;

namespace Enamel.Components.Messages;

public readonly record struct Select(Entity Entity) : IHasEntity;