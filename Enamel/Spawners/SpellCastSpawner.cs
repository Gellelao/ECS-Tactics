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
        var fireballEntity = _world.CreateEntity();
        _world.Set(fireballEntity, new TextureIndexComponent(Sprite.Fireball));
        _world.Set(fireballEntity, new ProjectileDamageComponent(1));
        _world.Set(fireballEntity, new ProjectileMoveRateComponent(ProjectileMoveRate.Immediate));
        _world.Set(fireballEntity, new MovingInDirectionComponent(direction));
        _world.Set(fireballEntity, new GridCoordComponent(x, y));
        _world.Set(fireballEntity, new SpeedComponent(Constants.DEFAULT_PROJECTILE_SPEED));
    }


}