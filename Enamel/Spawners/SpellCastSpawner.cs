using Enamel.Components;
using Enamel.Components.Spells.SpawnedEntities;
using Enamel.Enums;
using MoonTools.ECS;

namespace Enamel.Spawners;

public class SpellCastSpawner(World world) : Manipulator(world)
{
    public void SpawnFireball(int x, int y, GridDirection gridDirection)
    {
        var fireball = CreateEntity();
        Set(fireball, new GridCoordComponent(x, y));
        Set(fireball, new TextureIndexComponent(Sprite.Fireball));
        Set(fireball, new DrawLayerComponent(DrawLayer.Units));
        SetProjectileComponents(
            fireball,
            1,
            ProjectileMoveRate.Immediate,
            gridDirection,
            Constants.DEFAULT_PROJECTILE_SPEED
        );
    }

    public void SpawnArcaneBlock(int x, int y)
    {
        var arcaneBlock = CreateEntity();
        Set(arcaneBlock, new GridCoordComponent(x, y));
        Set(arcaneBlock, new TextureIndexComponent(Sprite.ArcaneBlock));
        Set(arcaneBlock, new SpriteOriginComponent(-6, 9));
        Set(arcaneBlock, new ImpassableFlag());
    }

    public void SpawnArcaneBubble(int x, int y, GridDirection gridDirection)
    {
        var arcaneBubble = CreateEntity();
        Set(arcaneBubble, new GridCoordComponent(x, y));
        Set(arcaneBubble, new TextureIndexComponent(Sprite.ArcaneBubble));
        SetProjectileComponents(
            arcaneBubble,
            2,
            ProjectileMoveRate.PerStep,
            gridDirection,
            Constants.DEFAULT_PROJECTILE_SPEED / 10
        );
    }

    private void SetProjectileComponents(
        Entity entity,
        int damage,
        ProjectileMoveRate moveRate,
        GridDirection gridDirection,
        int speed
    )
    {
        Set(entity, new ProjectileDamageComponent(damage));
        Set(entity, new ProjectileMoveRateComponent(moveRate));
        Set(entity, new MovingInDirectionComponent(gridDirection));
        Set(entity, new SpeedComponent(speed));
        Set(entity, new OnCollisionComponent(CollisionBehaviour.DestroySelf));
    }
}