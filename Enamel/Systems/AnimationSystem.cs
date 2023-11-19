using System;
using Enamel.Components;
using Enamel.Enums;
using MoonTools.ECS;

namespace Enamel.Systems;

public class AnimationSystem : MoonTools.ECS.System
{
    private readonly AnimationData[] _animations;
    
    private Filter AnimationFilter { get; }

    public AnimationSystem(World world, AnimationData[] animations) : base(world)
    {
        _animations = animations;
        AnimationFilter = FilterBuilder
            .Include<AnimationSetComponent>()
            .Include<AnimationSpeedComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        // Update frames of existing animations
        foreach (var entity in AnimationFilter.Entities)
        {
            var animationSetId = Get<AnimationSetComponent>(entity).AnimationSetId;
            var animationSpeedComponent = Get<AnimationSpeedComponent>(entity);
            var direction = Has<FacingDirectionComponent>(entity)
                ? Get<FacingDirectionComponent>(entity).Direction
                : Direction.North;
            var animationType = Has<AnimationTypeComponent>(entity)
                ? Get<AnimationTypeComponent>(entity).AnimationType
                : AnimationType.Idle;
            
            var timeSinceLastFrame = animationSpeedComponent.MillisSinceLastFrame;
            timeSinceLastFrame += delta.TotalMilliseconds;

            // check if we need to update to the next frame
            if (timeSinceLastFrame > animationSpeedComponent.MillisBetweenFrames)
            {
                // get the information about all the animations for this entity
                var animationData = _animations[(int)animationSetId];
                // get the information about the specific type (e.g idle, walk, etc) that we want to play here
                var animationFrames = animationData.Frames[(int)animationType];
                
                // get the next frame to display
                var currentFrame = animationSpeedComponent.CurrentFrame;
                currentFrame++;
                if (currentFrame >= animationFrames.Length)
                {
                    currentFrame = 0;
                }
                var nextSpriteY = animationFrames[currentFrame];
                
                // the direction enum identifies the x column in the spritesheet
                // the y value is determined by the animation frame
                Set(entity, new SpriteRegionComponent(
                    (int)direction,
                    nextSpriteY,
                    animationData.SpriteWidth,
                    animationData.SpriteHeight)
                );

                Set(entity, animationSpeedComponent with {CurrentFrame = currentFrame, MillisSinceLastFrame = 0});
            }
            else
            {
                Set(entity, animationSpeedComponent with {MillisSinceLastFrame = timeSinceLastFrame});
            }
        }
    }
}