using System;
using Enamel.Components;
using Enamel.Components.Messages;
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
            .Include<AnimationStatusComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        // See if there were any messages that might cause an animation update
        HandleMessages();
        // Update frames of existing animations
        foreach (var entity in AnimationFilter.Entities)
        {
            var animationSetId = Get<AnimationSetComponent>(entity).AnimationSetId;
            var animationStatusComponent = Get<AnimationStatusComponent>(entity);
            var direction = Has<FacingDirectionComponent>(entity)
                ? Get<FacingDirectionComponent>(entity).Direction
                : Direction.North;
            
            var timeSinceLastFrame = animationStatusComponent.MillisSinceLastFrame;
            timeSinceLastFrame += delta.TotalMilliseconds;

            // check if we need to update to the next frame
            if (timeSinceLastFrame > animationStatusComponent.MillisBetweenFrames)
            {
                var currentAnimation = animationStatusComponent.CurrentAnimation;
                
                // get the information about all the animations for this entity
                var animationData = _animations[(int)animationSetId];
                // get the information about the specific type (e.g idle, walk, etc) that we want to play here
                var animationFrames = animationData.Frames[(int)currentAnimation];
                
                // get the next frame to display
                var currentFrame = animationStatusComponent.CurrentFrame;
                currentFrame++;
                if (currentFrame >= animationFrames.Length)
                {
                    currentFrame = 0;
                    // If we've reached the end of the current animation switch to the next one
                    var nextAnimation = animationStatusComponent.AnimationOnceFinished;
                    animationFrames = animationData.Frames[(int)nextAnimation];
                    currentAnimation = nextAnimation;
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

                Set(entity, animationStatusComponent with {CurrentAnimation = currentAnimation, CurrentFrame = currentFrame, MillisSinceLastFrame = 0});
            }
            else
            {
                Set(entity, animationStatusComponent with {MillisSinceLastFrame = timeSinceLastFrame});
            }
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
                var animationStatus = Get<AnimationStatusComponent>(selectedEntity);
                Set(selectedEntity, animationStatus with
                {
                    CurrentAnimation = AnimationType.Throw,
                    AnimationOnceFinished = AnimationType.Idle,
                    MillisSinceLastFrame = animationStatus.MillisBetweenFrames,
                    CurrentFrame = -1 // This way the animationSystem increments it to zero and we get the expected frame...
                });
            }
        }
        if (SomeMessage<CancelMessage>())
        {
            Set(selectedEntity, Get<AnimationStatusComponent>(selectedEntity) with {AnimationOnceFinished = AnimationType.Idle});
        }
    }
}