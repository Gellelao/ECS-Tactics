using System;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.TempComponents;
using Enamel.Enums;
using MoonTools.ECS;

namespace Enamel.Systems;

public class AnimationSystem : MoonTools.ECS.System
{
    private readonly AnimationData[] _animations;
    
    private Filter AnimationFilter { get; }

    public Filter TempAnimationFilter { get; }

    public AnimationSystem(World world, AnimationData[] animations) : base(world)
    {
        _animations = animations;
        AnimationFilter = FilterBuilder
            .Include<AnimationSetComponent>()
            .Include<AnimationStatusComponent>().Build();
        TempAnimationFilter = FilterBuilder.Include<TempAnimationComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        // See if there were any messages that might cause an animation update
        HandleMessages();
        // Just a helper to simplify repeated code
        HandleTempAnimations();
        // Update frames of existing animations
        foreach (var entity in AnimationFilter.Entities)
        {
            var animationSetId = Get<AnimationSetComponent>(entity).AnimationSetId;
            var animationStatusComponent = Get<AnimationStatusComponent>(entity);
            var direction = Has<FacingDirectionComponent>(entity)
                ? Get<FacingDirectionComponent>(entity).GridDirection
                : GridDirection.North;
            
            var timeSinceLastFrame = animationStatusComponent.MillisSinceLastFrame;
            timeSinceLastFrame += delta.TotalMilliseconds;

            var currentAnimation = animationStatusComponent.CurrentAnimation;
                
            // get the information about all the animations for this entity
            var animationData = _animations[(int)animationSetId];
            // get the information about the specific type (e.g idle, walk, etc) that we want to play here
            var animationFrames = animationData.Frames[(int)currentAnimation];
                
            // get the frame to display
            var currentFrame = animationStatusComponent.CurrentFrame;
            if (currentFrame >= animationFrames.Length)
            {
                currentFrame = 0;
                // If we've reached the end of the current animation switch to the next one (it may well be the same as the existing one though!)
                var nextAnimation = animationStatusComponent.AnimationOnceFinished;
                animationFrames = animationData.Frames[(int)nextAnimation];
                currentAnimation = nextAnimation;
            }
                
            var nextSpriteY = animationFrames[currentFrame];
            
            // check if we need to update to the next frame and/or animation
            if (timeSinceLastFrame > animationStatusComponent.MillisBetweenFrames)
            {
                currentFrame++;
                Set(entity, animationStatusComponent with {CurrentAnimation = currentAnimation, CurrentFrame = currentFrame, MillisSinceLastFrame = 0});
            }
            else
            {
                Set(entity, animationStatusComponent with {MillisSinceLastFrame = timeSinceLastFrame});
            }
                            
            // the direction enum identifies the x column in the spritesheet
            // the y value is determined by the animation frame
            Set(entity, new SpriteRegionComponent(
                (int)direction,
                nextSpriteY,
                animationData.SpriteWidth,
                animationData.SpriteHeight)
            );
        }
    }

    private void HandleMessages()
    {
        var selectedEntity = GetSingletonEntity<SelectedFlag>();
        if (SomeMessage<PrepSpellMessage>())
        {
            var spell = ReadMessage<PrepSpellMessage>().SpellId;
            if (spell != SpellId.StepOnce)
            {
                Set(selectedEntity, Get<AnimationStatusComponent>(selectedEntity) with
                {
                    // We want to hold this animation until the player makes an input
                    CurrentAnimation = AnimationType.Raise,
                    AnimationOnceFinished = AnimationType.Raise
                });
            }
        }
        if (SomeMessage<SpellWasCastMessage>())
        {
            var spell = ReadMessage<PrepSpellMessage>().SpellId;
            if (spell != SpellId.StepOnce)
            {
                Set(selectedEntity, new TempAnimationComponent(AnimationType.Throw, AnimationType.Idle));
            }
        }
        if (SomeMessage<CancelMessage>())
        {
            Set(selectedEntity, Get<AnimationStatusComponent>(selectedEntity) with {AnimationOnceFinished = AnimationType.Idle});
        }
    }

    private void HandleTempAnimations()
    {
        foreach (var entity in TempAnimationFilter.Entities)
        {
            var tempAnimation = Get<TempAnimationComponent>(entity);
            var animationStatus = Get<AnimationStatusComponent>(entity);
            Set(entity, animationStatus with
            {
                CurrentAnimation = tempAnimation.NewAnimation,
                AnimationOnceFinished = tempAnimation.AnimationOnceFinished,
                MillisSinceLastFrame = 0,
                CurrentFrame = 0
            });
            Remove<TempAnimationComponent>(entity);
        }
    }
}