using Enamel.Enums;

namespace Enamel.Components;

public readonly record struct AnimationComponent(
    AnimationSet AnimationSetId,
    double MillisBetweenFrames,
    double MillisSinceLastFrame = 0,
    int CurrentFrame = 0
);