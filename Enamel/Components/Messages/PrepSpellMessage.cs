using Enamel.Enums;

namespace Enamel.Components.Messages;

public readonly record struct PrepSpellMessage(SpellId SpellId, int OriginGridX, int OriginGridY);