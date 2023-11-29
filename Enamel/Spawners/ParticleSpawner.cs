using System;
using Enamel.Components;
using Enamel.Enums;
using Microsoft.Xna.Framework;
using MoonTools.ECS;

namespace Enamel.Spawners;

public class ParticleSpawner(World world) : Manipulator(world)
{
    public void SpawnSmokePuff(float screenX, float screenY, ScreenDirection moveDirection, int moveSpeed, int lifeTimeMillis)
    {
        var smoke = world.CreateEntity();
        world.Set(smoke, new ScreenPositionComponent(screenX, screenY));
        world.Set(smoke, new TextureIndexComponent(Sprite.Smoke));
        world.Set(smoke, new DrawLayerComponent(DrawLayer.Units));
        World.Set(smoke, new AnimationSetComponent(AnimationSet.Smoke));
        world.Set(smoke, new AnimationStatusComponent(AnimationType.Idle, Constants.DEFAULT_MILLIS_BETWEEN_FRAMES));
        world.Set(smoke, new DestroyAfterMillisComponent(lifeTimeMillis));
        if (moveDirection != ScreenDirection.None)
        {
            var targetPosition = GetPositionForDirection(screenX, screenY, moveDirection);
            world.Set(smoke, new MovingToScreenPositionComponent(targetPosition.X, targetPosition.Y, moveSpeed));
        }
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
                throw new ArgumentOutOfRangeException(nameof(screenDirection), screenDirection, "No position to get for this direction");
        }
    }
}