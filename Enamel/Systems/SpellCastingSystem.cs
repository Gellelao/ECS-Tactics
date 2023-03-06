using System;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Spells;
using Enamel.Components.Spells.SpawnedEntities;
using Enamel.Components.UI;
using Enamel.Enums;
using MoonTools.ECS;

namespace Enamel.Systems;

public class SpellCastingSystem : SpellSystem
{
    private readonly World _world;
    private Filter SpellPreviewFilter { get; }

    public SpellCastingSystem(World world) : base(world)
    {
        _world = world;
        SpellPreviewFilter = FilterBuilder.Include<SpellPreviewFlag>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var spellPreview in SpellPreviewFilter.Entities)
        {
            if (!SomeMessageWithEntity<SelectMessage>(spellPreview)) continue;

            var (targetX, targetY)= Get<GridCoordComponent>(spellPreview);
            
            var casterEntity = GetSingletonEntity<SelectedFlag>();
            var (originX, originY) = Get<GridCoordComponent>(casterEntity);

            var spellToCastComponent = Get<SpellToCastOnClickComponent>(spellPreview);
            var spell = GetSpell(spellToCastComponent.SpellId);

            ResolveSpell(spell, originX, originY, targetX, targetY);
        }
    }

    private void ResolveSpell(Entity spell, int originX, int originY, int targetX, int targetY)
    {
        if (Has<SpawnsProjectileSpellFlag>(spell))
        {
            var projectileTexture = Get<TextureIndexOfSpawnedEntityComponent>(spell).Index;
            var projectileDamage = Has<SpawnedProjectileDamageComponent>(spell) ? Get<SpawnedProjectileDamageComponent>(spell).Damage : 0;
            var projectileMoveRate = Get<SpawnedProjectileMoveRateComponent>(spell).Rate;

            var direction = GetDirectionOfCast(originX, originY, targetX, targetY);

            var projectile = _world.CreateEntity();
            _world.Set(projectile, new ProjectileDamageComponent(projectileDamage));
            _world.Set(projectile, new ProjectileMoveRateComponent(projectileMoveRate));
            _world.Set(projectile, new TextureIndexComponent(projectileTexture));
            _world.Set(projectile, new GridCoordComponent(originX, originY));
            _world.Set(projectile, new MovingInDirectionComponent(direction));
            _world.Set(projectile, new SpeedComponent(Constants.DEFAULT_PROJECTILE_SPEED));
        }
    }

    private Direction GetDirectionOfCast(int originX, int originY, int targetX, int targetY)
    {
        var yDiff = originY - targetY;
        var xDiff = originX - targetX;
        if (xDiff != 0 && yDiff != 0)
        {
            throw new NotImplementedException("Diagonal projectiles not implemented");
        }

        if (xDiff < 0) return Direction.East;
        if (xDiff > 0) return Direction.West;
        if (yDiff < 0) return Direction.South;
        if (yDiff > 0) return Direction.North;

        return Direction.None;
    }
}