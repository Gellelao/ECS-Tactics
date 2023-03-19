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

            var spellToCastComponent = Get<SpellToCastOnClickComponent>(spellPreview);
            var spell = GetSpell(spellToCastComponent.SpellId);

            ResolveSpell(spell, casterEntity, targetX, targetY);

            Send(casterEntity, new SpellWasCastMessage(casterEntity, spellToCastComponent.SpellId));
        }
    }

    private void ResolveSpell(Entity spell, Entity casterEntity, int targetX, int targetY)
    {
        if (Has<SpawnedEntityTemplateComponent>(spell))
        {
            var spawnedEntity = _world.Instantiate(Get<SpawnedEntityTemplateComponent>(spell).Template);
            _world.Set(spawnedEntity, new GridCoordComponent(targetX, targetY));

            if (Has<ProjectileMoveRateComponent>(spawnedEntity))
            {
                var (originX, originY) = Get<GridCoordComponent>(casterEntity);
                var direction = GetDirectionOfCast(originX, originY, targetX, targetY);
                Set(spawnedEntity, new MovingInDirectionComponent(direction));
            }
        }
        if(Has<MovesCasterToTargetFlag>(spell))
        {
            Set(casterEntity, new MovingToPositionComponent(targetX, targetY));
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