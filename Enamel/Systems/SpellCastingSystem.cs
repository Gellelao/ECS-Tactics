using System;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Spells.SpawnedEntities;
using Enamel.Components.TempComponents;
using Enamel.Components.UI;
using Enamel.Enums;
using Enamel.Spawners;
using Enamel.Utils;
using MoonTools.ECS;
using static Enamel.Utils.Utils;

namespace Enamel.Systems;

public class SpellCastingSystem : MoonTools.ECS.System
{
    private readonly SpellUtils _spellUtils;
    private readonly SpellCastSpawner _spawner;
    private Filter SpellPreviewFilter { get; }

    public SpellCastingSystem(World world, SpellUtils spellUtils, SpellCastSpawner spawner) : base(world)
    {
        _spellUtils = spellUtils;
        _spawner = spawner;
        SpellPreviewFilter = FilterBuilder.Include<SpellToCastOnSelectComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        if (!SomeMessage<GridCoordSelectedMessage>()) return;
        var (clickX, clickY) = ReadMessage<GridCoordSelectedMessage>();
        foreach (var spellPreview in SpellPreviewFilter.Entities)
        {
            var (targetX, targetY) = Get<GridCoordComponent>(spellPreview);
            if (clickX != targetX || clickY != targetY) continue;

            var spellToCastComponent = Get<SpellToCastOnSelectComponent>(spellPreview);
            
            // For spells that require a caster, resolve them here
            // For other cases (such as DeployWizard, resolve in a dedicated system and skip this section
            if (Some<SelectedFlag>())
            {
                var casterEntity = GetSingletonEntity<SelectedFlag>();

                var spell = _spellUtils.GetSpell(spellToCastComponent.SpellId);

                ResolveSpell(spell, casterEntity, targetX, targetY);
            }

            Send(new SpellWasCastMessage(spellToCastComponent.SpellId));
        }
    }

    private void ResolveSpell(Entity spell, Entity casterEntity, int targetX, int targetY)
    {
        var spellId = Get<SpellIdComponent>(spell).SpellId;
        var (originX, originY) = Get<GridCoordComponent>(casterEntity);
        var direction = GetDirection(originX, originY, targetX, targetY);
        
        // If targeting self we get None back, so just leave them facing as they are
        if (direction != GridDirection.None)
        {
            Set(casterEntity, new FacingDirectionComponent(direction));
        }
        
        // Force the animation to update now that we've (possibly) changed direction
        var animationStatus = Get<AnimationStatusComponent>(casterEntity);
        Set(casterEntity, animationStatus with {MillisSinceLastFrame = animationStatus.MillisBetweenFrames});

        switch (spellId)
        {
            case SpellId.DeployWizard:
                // Resolving this is handled in the DeploymentSystem
                break;
            case SpellId.StepOnce:
                Set(casterEntity, new MovingToGridCoordComponent(targetX, targetY));
                Set(casterEntity, new TempAnimationComponent(AnimationType.Walk, AnimationType.Idle));
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