using Enamel.Enums;

namespace Enamel.Components;

public readonly record struct AnimationComponent(Animation AnimationId, int CurrentFrame, double MillisBetweenFrames, double MillisSinceLastFrame);