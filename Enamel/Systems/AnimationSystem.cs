using System;
using Enamel.Components;
using MoonTools.ECS;

namespace Enamel.Systems;

public class AnimationSystem : MoonTools.ECS.System
{
    private readonly (int X, int Y)[][] _animations;
    
    private Filter AnimationFilter { get; }

    public AnimationSystem(World world, (int X, int Y)[][] animations) : base(world)
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
                var animation = _animations[(int)animationComponent.AnimationId];
                var currentFrame = animationComponent.CurrentFrame;
                currentFrame++;
                if (currentFrame >= animation.Length)
                {
                    currentFrame = 0;
                }

                var nextSprite = animation[currentFrame];
                
                // TODO - need to put the width and height in the animation data to make this flexible
                Set(entity, new SpriteRectComponent(nextSprite.X*Constants.PLAYER_FRAME_WIDTH, nextSprite.Y*Constants.PLAYER_FRAME_WIDTH, Constants.PLAYER_FRAME_WIDTH, Constants.PLAYER_FRAME_WIDTH));

                Set(entity,
                    animationComponent with {CurrentFrame = currentFrame, MillisSinceLastFrame = 0});
            }
            else
            {
                Set(entity, animationComponent with {MillisSinceLastFrame = timeSinceLastFrame});
            }
        }
    }
}