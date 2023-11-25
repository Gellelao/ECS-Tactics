using Enamel.Enums;

namespace Enamel.Components.TempComponents;

public readonly record struct BeingPushedComponent(Direction Direction, bool EntityMustBePushable);