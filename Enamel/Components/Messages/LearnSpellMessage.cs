using Enamel.Enums;
using MoonTools.ECS;

namespace Enamel.Components.Messages;

public readonly record struct LearnSpellMessage(SpellId SpellId);