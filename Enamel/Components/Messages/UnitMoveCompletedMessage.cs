using MoonTools.ECS;

namespace Enamel.Components.Messages;

public readonly record struct UnitMoveCompletedMessage(Entity UnitMoved);