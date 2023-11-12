using System;
using Enamel.Components;
using MoonTools.ECS;

namespace Enamel.Systems;

public class AnimationSystem : MoonTools.ECS.System
{
    private readonly AnimationData[] _animations;
    
    private Filter AnimationFilter { get; }

    public AnimationSystem(World world, AnimationData[] animations) : base(world)
    {
        _animations = animations;
        AnimationFilter = FilterBuilder.Include<AnimationComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in AnimationFilter.Entities)
        {
            var animationComponent = Get<AnimationComponent>(entity);
            var timeSinceLastFrame = animationComponent.MillisSinceLastFrame;
            timeSinceLastFrame += delta.TotalMilliseconds;

            if (timeSinceLastFrame > animationComponent.MillisBetweenFrames)
            {
                var animationData = _animations[(int)animationComponent.AnimationId];
                var currentFrame = animationComponent.CurrentFrame;
                currentFrame++;
                if (currentFrame >= animationData.Frames.Length)
                {
                    currentFrame = 0;
                }

                var nextSprite = animationData.Frames[currentFrame];
                
                // TODO - need to put the width and height in the animation data to make this flexible
                Set(entity, new SpriteRegionComponent(
                    nextSprite.X,
                    nextSprite.Y,
                    animationData.SpriteWidth,
                    animationData.SpriteHeight)
                );

                Set(entity, animationComponent with {CurrentFrame = currentFrame, MillisSinceLastFrame = 0});
            }
            else
            {
                Set(entity, animationComponent with {MillisSinceLastFrame = timeSinceLastFrame});
            }
        }
    }
}