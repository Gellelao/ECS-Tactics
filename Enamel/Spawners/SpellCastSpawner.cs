using System;
using Enamel.Components;
using Enamel.Components.Spells.SpawnedEntities;
using Enamel.Enums;
using MoonTools.ECS;

namespace Enamel.Spawners;

public class SpellCastSpawner(World world) : Manipulator(world)
{
    public void SpawnFireball(int x, int y, Direction direction)
    {
        Console.WriteLine("Spawning fireball");
        var fireball = world.CreateEntity();
        world.Set(fireball, new GridCoordComponent(x, y));
        world.Set(fireball, new TextureIndexComponent(Sprite.Fireball));
        world.Set(fireball, new DrawLayerComponent(DrawLayer.Units));
        SetProjectileComponents(fireball, 1, ProjectileMoveRate.Immediate, direction, Constants.DEFAULT_PROJECTILE_SPEED);
    }

    public void SpawnArcaneBlock(int x, int y)
    {
        var arcaneBlock = world.CreateEntity();
        world.Set(arcaneBlock, new GridCoordComponent(x, y));
        world.Set(arcaneBlock, new TextureIndexComponent(Sprite.ArcaneBlock));
        world.Set(arcaneBlock, new SpriteOriginComponent(-6, 9));
        world.Set(arcaneBlock, new ImpassableFlag());
    }

    public void SpawnArcaneBubble(int x, int y, Direction direction)
    {
        var arcaneBubble = world.CreateEntity();
        world.Set(arcaneBubble, new GridCoordComponent(x, y));
        world.Set(arcaneBubble, new TextureIndexComponent(Sprite.ArcaneBubble));
        SetProjectileComponents(arcaneBubble, 2, ProjectileMoveRate.PerStep, direction, Constants.DEFAULT_PROJECTILE_SPEED/10);
    }

    private void SetProjectileComponents(Entity entity, int damage, ProjectileMoveRate moveRate, Direction direction, int speed)
    {
        world.Set(entity, new ProjectileDamageComponent(damage));
        world.Set(entity, new ProjectileMoveRateComponent(moveRate));
        world.Set(entity, new MovingInDirectionComponent(direction));
        world.Set(entity, new SpeedComponent(speed));
        world.Set(entity, new OnCollisionComponent(CollisionBehaviour.DestroySelf));
    }
}