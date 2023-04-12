using Enamel.Components;
using Enamel.Components.Spells.SpawnedEntities;
using Enamel.Enums;
using MoonTools.ECS;

namespace Enamel.Spawners;

public class SpellCastSpawner : Spawner
{
    private readonly World _world;

    public SpellCastSpawner(World world) : base(world)
    {
        _world = world;
    }

    public void SpawnFireball(int x, int y, Direction direction)
    {
        var fireball = _world.CreateEntity();
        _world.Set(fireball, new GridCoordComponent(x, y));
        _world.Set(fireball, new TextureIndexComponent(Sprite.Fireball));
        SetProjectileComponents(fireball, 1, ProjectileMoveRate.Immediate, direction, Constants.DEFAULT_PROJECTILE_SPEED);
    }

    public void SpawnArcaneBlock(int x, int y)
    {
        var arcaneBlock = _world.CreateEntity();
        _world.Set(arcaneBlock, new GridCoordComponent(x, y));
        _world.Set(arcaneBlock, new TextureIndexComponent(Sprite.ArcaneBlock));
        _world.Set(arcaneBlock, new SpriteOriginComponent(-6, 9));
        _world.Set(arcaneBlock, new ImpassableFlag());
    }

    public void SpawnArcaneBubble(int x, int y, Direction direction)
    {
        var arcaneBubble = _world.CreateEntity();
        _world.Set(arcaneBubble, new GridCoordComponent(x, y));
        _world.Set(arcaneBubble, new TextureIndexComponent(Sprite.ArcaneBubble));
        SetProjectileComponents(arcaneBubble, 2, ProjectileMoveRate.PerStep, direction, Constants.DEFAULT_PROJECTILE_SPEED/10);
    }

    private void SetProjectileComponents(Entity entity, int damage, ProjectileMoveRate moveRate, Direction direction, int speed)
    {
        _world.Set(entity, new ProjectileDamageComponent(damage));
        _world.Set(entity, new ProjectileMoveRateComponent(moveRate));
        _world.Set(entity, new MovingInDirectionComponent(direction));
        _world.Set(entity, new SpeedComponent(speed));
    }
}