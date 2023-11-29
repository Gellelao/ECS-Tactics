using Enamel.Enums;

namespace Enamel.Components.TempComponents;

public readonly record struct BeingPushedComponent(GridDirection GridDirection, bool EntityMustBePushable);