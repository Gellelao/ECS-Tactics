using System;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Spells.SpawnedEntities;
using Enamel.Components.UI;
using Enamel.Enums;
using Enamel.Spawners;
using MoonTools.ECS;
using static Enamel.Utils;

namespace Enamel.Systems;

public class SpellCastingSystem : SpellSystem
{
    private readonly SpellCastSpawner _spawner;
    private Filter SpellPreviewFilter { get; }

    public SpellCastingSystem(World world, SpellCastSpawner spawner) : base(world)
    {
        _spawner = spawner;
        SpellPreviewFilter = FilterBuilder.Include<SpellToCastOnClickComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        if (!SomeMessage<GridCoordSelectedMessage>()) return;
        var (clickX, clickY) = ReadMessage<GridCoordSelectedMessage>();
        foreach (var spellPreview in SpellPreviewFilter.Entities)
        {
            var (targetX, targetY) = Get<GridCoordComponent>(spellPreview);
            if (clickX != targetX || clickY != targetY) continue;
            
            var casterEntity = GetSingletonEntity<SelectedFlag>();

            var spellToCastComponent = Get<SpellToCastOnClickComponent>(spellPreview);
            var spell = GetSpell(spellToCastComponent.SpellId);

            ResolveSpell(spell, casterEntity, targetX, targetY);

            Send(new SpellWasCastMessage(spellToCastComponent.SpellId));
        }
    }

    private void ResolveSpell(Entity spell, Entity casterEntity, int targetX, int targetY)
    {
        var spellId = Get<SpellIdComponent>(spell).SpellId;
        var (originX, originY) = Get<GridCoordComponent>(casterEntity);
        var direction = GetDirection(originX, originY, targetX, targetY);

        switch (spellId)
        {
            case SpellId.StepOnce:
                Set(casterEntity, new MovingToCoordComponent(targetX, targetY));
                Set(casterEntity, new FacingDirectionComponent(direction));
                var animationSpeedComponent = Get<AnimationSpeedComponent>(casterEntity);
                // Force the animation to update now that we've (possibly) changed direction
                Set(casterEntity, animationSpeedComponent with {MillisSinceLastFrame = animationSpeedComponent.MillisBetweenFrames});
                Remove<GridCoordComponent>(casterEntity);
                break;
            case SpellId.Fireball:
                _spawner.SpawnFireball(originX, originY, direction);
                break;
            case SpellId.ArcaneBlock:
                _spawner.SpawnArcaneBlock(targetX, targetY);
                break;
            case SpellId.ArcaneBubble:
                _spawner.SpawnArcaneBubble(originX, originY, direction);
                break;
            case SpellId.RockCharge:
                Set(casterEntity, new MovingInDirectionComponent(direction));
                Set(casterEntity, new ProjectileMoveRateComponent(ProjectileMoveRate.Immediate));
                Set(casterEntity, new SpeedComponent(150));
                Set(casterEntity, new OnCollisionComponent(CollisionBehaviour.StopAndPush));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}