using Enamel.Enums;

namespace Enamel.Components;

public readonly record struct AnimationStatusComponent(
    AnimationType CurrentAnimation,
    AnimationType AnimationOnceFinished,
    double MillisBetweenFrames,
    double MillisSinceLastFrame = 0,
    int CurrentFrame = 0
)
{
    public AnimationStatusComponent(AnimationType CurrentAnimation, double MillisBetweenFrames, double MillisSinceLastFrame = 0, int CurrentFrame = 0) : this(CurrentAnimation, CurrentAnimation, MillisBetweenFrames, MillisSinceLastFrame, CurrentFrame)
    {
    }
}