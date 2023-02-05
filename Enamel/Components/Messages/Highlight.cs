using MoonTools.ECS;

namespace Enamel.Components.Messages;

public readonly record struct Highlight(Entity Entity) : IHasEntity;