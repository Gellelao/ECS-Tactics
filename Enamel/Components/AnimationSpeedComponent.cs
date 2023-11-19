namespace Enamel.Components;

public readonly record struct AnimationSpeedComponent(
    double MillisBetweenFrames,
    double MillisSinceLastFrame = 0,
    int CurrentFrame = 0
 );