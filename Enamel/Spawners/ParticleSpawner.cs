using System;
using Enamel.Components;
using Enamel.Enums;
using Microsoft.Xna.Framework;
using MoonTools.ECS;

namespace Enamel.Spawners;

public class ParticleSpawner(World world, AnimationData[] animations) : Manipulator(world)
{
    public void SpawnSmokePuff(float screenX, float screenY, ScreenDirection moveDirection, int moveSpeed)
    {
        const AnimationSet animationId = AnimationSet.Smoke;
        const int millisBetweenFrames = Constants.DEFAULT_MILLIS_BETWEEN_FRAMES;
        var smoke = CreateEntity();
        Set(smoke, new ScreenPositionComponent(screenX, screenY));
        Set(smoke, new TextureIndexComponent(Sprite.Smoke));
        Set(smoke, new DrawLayerComponent(DrawLayer.Units));
        Set(smoke, new AnimationSetComponent(animationId));
        Set(smoke, new AnimationStatusComponent(AnimationType.Idle, millisBetweenFrames));
        var totalMillis = GetTotalMillisOfAnimation(animations[(int) animationId], millisBetweenFrames);
        Set(smoke, new DestroyAfterMillisComponent(totalMillis));
        if (moveDirection != ScreenDirection.None)
        {
            var targetPosition = GetPositionForDirection(screenX, screenY, moveDirection);
            Set(smoke, new MovingToScreenPositionComponent(targetPosition.X, targetPosition.Y, moveSpeed));
        }
    }

    private static int GetTotalMillisOfAnimation(AnimationData animation, int millisBetweenFrames)
    {
        return animation.Frames[0].Length * millisBetweenFrames;
    }

    private Vector2 GetPositionForDirection(float x, float y, ScreenDirection screenDirection)
    {
        switch (screenDirection)
        {
            case ScreenDirection.Up:
                return new Vector2(x, -100);
            case ScreenDirection.Down:
                return new Vector2(x, 100);
            case ScreenDirection.Left:
                return new Vector2(100, y);
            case ScreenDirection.Right:
                return new Vector2(-100, y);
            case ScreenDirection.None:
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(screenDirection),
                    screenDirection,
                    "No position to get for this direction"
                );
        }
    }
}