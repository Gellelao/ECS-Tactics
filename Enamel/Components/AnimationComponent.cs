using Enamel.Enums;

namespace Enamel.Components;

public readonly record struct AnimationComponent(
    Animation AnimationId,
    double MillisBetweenFrames,
    double MillisSinceLastFrame = 0,
    int CurrentFrame = 0
);