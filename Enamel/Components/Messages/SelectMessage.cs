using MoonTools.ECS;

namespace Enamel.Components.Messages;

public readonly record struct SelectMessage(Entity Entity) : IHasEntity;