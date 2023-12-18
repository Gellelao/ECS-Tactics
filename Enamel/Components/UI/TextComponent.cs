using Enamel.Enums;
using Microsoft.Xna.Framework;

namespace Enamel.Components.UI;

public readonly record struct TextComponent(int TextIndex, Font Font, Color Color);