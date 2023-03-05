using MoonTools.ECS;

namespace Enamel.Components.Messages;

public readonly record struct DamageMessage(Entity Entity, int AmountOfDamage) : IHasEntity;