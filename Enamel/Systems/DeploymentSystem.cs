using System;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Relations;
using Enamel.Components.UI;
using Enamel.Enums;
using Enamel.Spawners;
using Enamel.Utils;
using MoonTools.ECS;

namespace Enamel.Systems;

public class DeploymentSystem : MoonTools.ECS.System
{
    private readonly CharacterSpawner _characterSpawner;
    private readonly MenuUtils _menuUtils;
    private Entity _redeployButton;
    private Entity _deployingPlayer;
    private Filter PlayersWaitingToDeploy { get; }

    public DeploymentSystem(World world, CharacterSpawner characterSpawner, MenuUtils menuUtils) : base(world)
    {
        _characterSpawner = characterSpawner;
        _menuUtils = menuUtils;
        PlayersWaitingToDeploy = FilterBuilder.Include<SelectedCharacterComponent>().Build();
    }

    public override void Update(TimeSpan delta)
    {
        if (SomeMessage<DeployWizardsMessage>())
        {
            Send(new PrepSpellMessage(SpellId.DeployWizard, 0, 0));

            // Would be really nice to put all UI stuff for each "screen" in one system but we need a way to delete this button from this system
            // and simplest way I could think of was store it in a field... also it is a deployment-specific button so maybe its fine
            _redeployButton = _menuUtils.CreateUiEntity(300, 140, 20, 20);
            Set(_redeployButton, new TextureIndexComponent(Sprite.RedeployWizardButton));
            Set(_redeployButton, new AnimationSetComponent(AnimationSet.RedeployWizardButton));
            Set(_redeployButton, new ToggleFrameOnMouseHoverComponent(1));
            Set(_redeployButton, new ToggleFrameOnMouseDownComponent(2));
            Set(_redeployButton, new OnClickComponent(ClickEvent.PrepSpell, (int) SpellId.DeployWizard));
        }

        if (PlayersWaitingToDeploy.Empty) return;

        if (SomeMessage<EndTurnMessage>())
        {
            _deployingPlayer = GetSingletonEntity<CurrentPlayerFlag>();
            Remove<SelectedCharacterComponent>(_deployingPlayer);
            if (PlayersWaitingToDeploy.Count > 0)
            {
                Send(new PrepSpellMessage(SpellId.DeployWizard, 0, 0));
            }
            else
            {
                Destroy(_redeployButton);
            }

            return;
        }

        if (!SomeMessage<GridCoordSelectedMessage>()) return;

        // If the current player clicks, delete any existing characters and place their selected char
        _deployingPlayer = GetSingletonEntity<CurrentPlayerFlag>();
        DestroyCharactersOfPlayer(_deployingPlayer);
        var characterToDeploy = Get<SelectedCharacterComponent>(_deployingPlayer).Character;

        var coords = ReadMessage<GridCoordSelectedMessage>();
        Entity character = _characterSpawner.SpawnCharacter(characterToDeploy, coords.X, coords.Y);
        Relate(_deployingPlayer, character, new ControlsRelation());
        Set(character, new DisabledFlag());
    }

    private void DestroyCharactersOfPlayer(Entity deployingPlayer)
    {
        foreach (var character in OutRelations<ControlsRelation>(deployingPlayer))
        {
            Destroy(character);
        }
    }
}