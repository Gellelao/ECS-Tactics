using Enamel.Enums;

namespace Enamel.Components.Messages;

public readonly record struct PushMessage(Direction Direction, bool EntityMustBePushable);