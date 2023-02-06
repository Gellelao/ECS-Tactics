using MoonTools.ECS;

namespace Enamel.Components.Messages;

public readonly record struct HighlightMessage(Entity Entity) : IHasEntity;