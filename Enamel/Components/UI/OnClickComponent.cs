using Enamel.Enums;

namespace Enamel.Components.UI;

public readonly record struct OnClickComponent(ClickEvent ClickEvent, int Id = -1);