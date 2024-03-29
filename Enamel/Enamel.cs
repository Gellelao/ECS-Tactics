using System;
using System.Collections.Generic;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Spells;
using Enamel.Enums;
using Enamel.Renderers;
using Enamel.Renderers.ImGui;
using Enamel.Spawners;
using Enamel.Systems;
using Enamel.Systems.UI;
using Enamel.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoonTools.ECS;
using DebugSystem = Enamel.Systems.DebugSystem;

namespace Enamel;

public class Enamel : Game
{
    GraphicsDeviceManager GraphicsDeviceManager { get; }

    /*
    the World is the place where all our entities go.
    */
    private static World World { get; } = new();
    private static ScreenUtils? _screenUtils;
    private static GridToScreenCoordSystem? _gridToScreenCoordSystem;
    private static InputSystem? _inputSystem;
    private static HighlightSystem? _highlightSystem;
    private static UnitSelectionSystem? _unitSelectionSystem;
    private static SelectionPreviewSystem? _selectionPreviewSystem;
    private static GridMoveSystem? _gridMoveSystem;
    private static TurnSystem? _turnSystem;
    private static SpellManagementSystem? _spellManagementSystem;
    private static TomesSystem? _tomesSystem;
    private static SpellCastingSystem? _spellCastingSystem;
    private static ProjectileSystem? _projectileSystem;
    private static DamageSystem? _damageSystem;
    private static DisablingSystem? _disablingSystem;
    private static PushSystem? _pushSystem;
    private static AnimationSystem? _animationSystem;
    private static DestroyAfterDurationSystem? _destroyAfterDurationSystem;
    private static ScreenMoveSystem? _screenMoveSystem;
    private static MainMenuSystem? _mainMenuSystem;
    private static CharSelectMenuSystem? _charSelectMenuSystem;
    private static CenterChildrenSystem? _centerChildrenSystem;
    private static RelativePositionSystem? _relativePositionSystem;
    private static ToggleFrameSystem? _toggleFrameSystem;
    private static DeploymentSystem? _deploymentSystem;
    private static InGameUiSystem? _inGameUiSystem;
    private static DragSystem? _dragSystem;
    private static HandSystem? _handSystem;

    private static SpriteRenderer? _mapRenderer;
    private static TextRenderer? _textRenderer;

#if DEBUG
    private ImGuiRenderer? _imGuiRenderer;
    private DebugSystem? _debugSystem;
#endif

    private SpriteBatch? _spriteBatch;
    //private FontSystem? _fontSystem;

    private RenderTarget2D? _renderTarget;
    private Rectangle _finalRenderRectangle;


    [STAThread]
    internal static void Main()
    {
        using var game = new Enamel();
        game.Run();
    }

    private Enamel()
    {
        GraphicsDeviceManager = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";

        GraphicsDeviceManager.PreferredBackBufferWidth = Constants.PIXEL_SCREEN_WIDTH * 5;
        GraphicsDeviceManager.PreferredBackBufferHeight = Constants.PIXEL_SCREEN_HEIGHT * 5;
        GraphicsDeviceManager.SynchronizeWithVerticalRetrace = true;
        GraphicsDeviceManager.PreferMultiSampling = false;
        GraphicsDeviceManager.IsFullScreen = false;
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnWindowClientSizeChanged;

        IsFixedTimeStep = false;
        IsMouseVisible = true;
    }

    //you'll want to do most setup in LoadContent() rather than your constructor.
    protected override void LoadContent()
    {
        _renderTarget = new RenderTarget2D(GraphicsDevice, Constants.PIXEL_SCREEN_WIDTH, Constants.PIXEL_SCREEN_HEIGHT);
        var rectangle = GetFinalRenderRectangle();
        _finalRenderRectangle = rectangle;

        /*
        CONTENT
        */
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        var textures = ContentUtils.LoadTextures(Content, GraphicsDevice, _spriteBatch);
        var animations = ContentUtils.LoadAnimations();
        var fonts = ContentUtils.LoadFonts(Content, GraphicsDevice);

        /*
        SYSTEMS
        */
        // Can use these to make a basic camera system - moving the offsets should just move the camera?
        var cameraX = Constants.PIXEL_SCREEN_WIDTH / 2;
        var cameraY = Constants.PIXEL_SCREEN_HEIGHT / 2;

        _screenUtils = new ScreenUtils(World, cameraX, cameraY);
        var spellUtils = new SpellUtils(World);
        var menuUtils = new MenuUtils(World);
        var orbSpawner = new OrbSpawner(World);
        var spellCastSpawner = new SpellCastSpawner(World);
        var particleSpawner = new ParticleSpawner(World, animations);
        var characterSpawner = new CharacterSpawner(World, spellUtils);

        _gridToScreenCoordSystem = new GridToScreenCoordSystem(World, cameraX, cameraY);
        _inputSystem = new InputSystem(World, _screenUtils);
        _highlightSystem = new HighlightSystem(World, _screenUtils);
        _unitSelectionSystem = new UnitSelectionSystem(World);
        _selectionPreviewSystem = new SelectionPreviewSystem(World, spellUtils);
        _gridMoveSystem = new GridMoveSystem(World, cameraX, cameraY);
        _turnSystem = new TurnSystem(World);
        _spellManagementSystem = new SpellManagementSystem(World, spellUtils);
        _tomesSystem = new TomesSystem(World, menuUtils);
        _spellCastingSystem = new SpellCastingSystem(World, spellUtils, spellCastSpawner);
        _projectileSystem = new ProjectileSystem(World);
        _damageSystem = new DamageSystem(World, particleSpawner);
        _disablingSystem = new DisablingSystem(World);
        _pushSystem = new PushSystem(World);
        _animationSystem = new AnimationSystem(World, animations);
        _destroyAfterDurationSystem = new DestroyAfterDurationSystem(World);
        _screenMoveSystem = new ScreenMoveSystem(World);
        _mainMenuSystem = new MainMenuSystem(World, menuUtils);
        _charSelectMenuSystem = new CharSelectMenuSystem(World, menuUtils);
        _centerChildrenSystem = new CenterChildrenSystem(World);
        _relativePositionSystem = new RelativePositionSystem(World);
        _toggleFrameSystem = new ToggleFrameSystem(World, _screenUtils, animations);
        _deploymentSystem = new DeploymentSystem(World, characterSpawner, menuUtils);
        _inGameUiSystem = new InGameUiSystem(World, menuUtils);
        _dragSystem = new DragSystem(World, _screenUtils);
        _handSystem = new HandSystem(World, orbSpawner);
        
        /*
         * Debug
         */
#if DEBUG
        _imGuiRenderer = new ImGuiRenderer(this);
        _imGuiRenderer.RebuildFontAtlas();
        var imGuiTextures = new Dictionary<int, (IntPtr, Vector2)>();
        for (var i = 0; i < textures.Length; i++)
        {
            var texture = textures[i];
            if (texture == null) continue;
            imGuiTextures.Add(i, (_imGuiRenderer.BindTexture(texture), new Vector2(texture.Width, texture.Height)));
        }

        _debugSystem = new DebugSystem(World, imGuiTextures, orbSpawner);
#endif
        
        /*
        RENDERERS
        */
        _mapRenderer = new SpriteRenderer(World, _spriteBatch, textures);
        _textRenderer = new TextRenderer(World, _spriteBatch, fonts);

        /*
        ENTITIES
        */
        // Had a weird issue where the first entity would cause errors, so create a throwaway entity here for now
        //var zeroEntity = World.CreateEntity();

        // Spells
        // For whatever reason, putting the spell template creation after the ground tiles caused an exception when drawing.
        // So just putting spells first until I decide to figure out why that happens
        CreateSpells();

        for (var x = 7; x >= 0; x--)
        {
            for (var y = 7; y >= 0; y--)
            {
                var tile = World.CreateEntity();
                World.Set(tile, new TextureIndexComponent(Sprite.Tile));
                World.Set(tile, new GridCoordComponent(x, y));
                World.Set(tile, new DebugCoordComponent(x, y));
                World.Set(tile, new DisabledFlag());
                World.Set(tile, new DrawLayerComponent(DrawLayer.Tiles));
            }
        }

        World.Send(new GoToMainMenuMessage());

        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        var elapsedTime = gameTime.ElapsedGameTime;
        _screenUtils?.Update(elapsedTime);
        _destroyAfterDurationSystem?.Update(elapsedTime);
        _screenMoveSystem?.Update(elapsedTime);
        _inputSystem?.Update(elapsedTime);
        _dragSystem?.Update(elapsedTime);
        _highlightSystem?.Update(elapsedTime);
        _mainMenuSystem?.Update(elapsedTime);
        _charSelectMenuSystem?.Update(elapsedTime);
        _centerChildrenSystem?.Update(elapsedTime);
        _relativePositionSystem?.Update(elapsedTime);
        // Must run before the selectionPreview system so that the PlayerUnitSelectedMessage can be received in the selectionPreviewSystem
        _unitSelectionSystem?.Update(elapsedTime);
        // Must run before TurnSystem so the currentPlayer can have their SelectedCharacterComponent removed before the TurnSystem changes the current player. Should run before spellManagement system to spells can be learned on the frame characters are spawned.
        _deploymentSystem?.Update(elapsedTime);
        _turnSystem?.Update(elapsedTime);
        // Must run after both the TurnSystem and CharSelectMenuSystem, since they both send messages to the HandSystem
        _handSystem?.Update(elapsedTime);
        // Must run after turnSystem so that the currentPlayer has been updated when the portraits are reordered
        _inGameUiSystem?.Update(elapsedTime);
        // Must run before the projectileSystem because the spellPreviewSystem runs as soon as a spell is cast, and if the spell kills a unit that unit needs to be deleted by the DamageMessage in ProjectileSystem before the movements previews are displayed
        _spellCastingSystem?.Update(elapsedTime);
        // Yikes, running a system twice... this is needed because the entity spawned by the spell needs a position for the _gridMoveSystem to update it, and we can't wait for the _gridToScreenCoordSystem. Nor can we replicate the behaviour in that system, because its the only thing with access to the _offsets...
        _gridToScreenCoordSystem?.Update(elapsedTime);
        // Must run after the unitSelectionSystem so the unit has the SelectedFlag by the time the _gridMoveSystem runs  (is this true?)
        _gridMoveSystem?.Update(elapsedTime);
        // Must run after the _gridMoveSystem because it listens for UnitMoveCompletedMessages to know when to move the PerStep projectiles
        _projectileSystem?.Update(elapsedTime);
        // Listens for PushMessages from the Projectile system
        _pushSystem?.Update(elapsedTime);
        // Should run after pushSystem because the pushSystem sends DamageMessages
        _damageSystem?.Update(elapsedTime);
        _spellManagementSystem?.Update(elapsedTime);
        _tomesSystem?.Update(elapsedTime);
        // Put before the disablingSystem so the buttons can "untoggle" before they get disabled
        _toggleFrameSystem?.Update(elapsedTime);
        _disablingSystem?.Update(elapsedTime);
        _animationSystem?.Update(elapsedTime);
        // Must run after the move system so that it doesn't delete the MovementPreviews before the Move system has a chance to get them
        _selectionPreviewSystem?.Update(elapsedTime);
        // Must run near the end so entities can have their PositionComponent attached before the renderer tries to access it
        _gridToScreenCoordSystem?.Update(elapsedTime);
        World.FinishUpdate();
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.Black);

        //render to the smaller renderTarget, then upscale after
        _mapRenderer?.Draw();
        _textRenderer?.Draw();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch?.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullCounterClockwise,
            null,
            Matrix.Identity
        ); // Only have to set all these here so I can change the default SamplerState
        _spriteBatch?.Draw(_renderTarget, _finalRenderRectangle, Color.White);
        _spriteBatch?.End();

#if DEBUG
        _imGuiRenderer?.BeforeLayout(gameTime);
        _debugSystem?.ImGuiLayout();
        _imGuiRenderer?.AfterLayout();
#endif

        base.Draw(gameTime);
    }

    private void OnWindowClientSizeChanged(object? sender, EventArgs e)
    {
        _finalRenderRectangle = GetFinalRenderRectangle();
    }

    private Rectangle GetFinalRenderRectangle()
    {
        float windowWidth = Window.ClientBounds.Width;
        float windowHeight = Window.ClientBounds.Height;
        float windowAspectRatio = windowWidth / windowHeight;
        float scale;
        if (windowAspectRatio > Constants.PIXEL_RATIO)
        {
            // Window is wider than the target, scale based on height
            scale = windowHeight / Constants.PIXEL_SCREEN_HEIGHT;
        }
        else
        {
            // Window is narrower than the target, scale based on width
            scale = windowWidth / Constants.PIXEL_SCREEN_WIDTH;
        }

        var offsetX = (int) ((windowWidth - (Constants.PIXEL_SCREEN_WIDTH * scale)) / 2);
        var offsetY = (int) ((windowHeight - (Constants.PIXEL_SCREEN_HEIGHT * scale)) / 2);

        World.Send(new ScreenDetailsChangedMessage(scale, offsetX, offsetY));

        return new Rectangle(
            offsetX,
            offsetY,
            (int) (Constants.PIXEL_SCREEN_WIDTH * scale),
            (int) (Constants.PIXEL_SCREEN_HEIGHT * scale)
        );
    }

    private void CreateSpells()
    {
        var deployWizard = World.CreateEntity();
        World.Set(deployWizard, new SpellIdComponent(SpellId.DeployWizard));
        World.Set(deployWizard, new CastRangeComponent(100));
        World.Set(deployWizard, new CanTargetSelfFlag());

        var stepOnceSpell = World.CreateEntity();
        World.Set(stepOnceSpell, new SpellIdComponent(SpellId.StepOnce));
        World.Set(stepOnceSpell, new CastRangeComponent(1));
        World.Set(stepOnceSpell, new OrbRequirementComponent(1, 1, 0));

        var fireballSpell = World.CreateEntity();
        World.Set(fireballSpell, new SpellIdComponent(SpellId.Fireball));
        World.Set(fireballSpell, new CastRangeComponent(1));
        World.Set(fireballSpell, new CanTargetImpassableFlag());

        var arcaneBlockSpell = World.CreateEntity();
        World.Set(arcaneBlockSpell, new SpellIdComponent(SpellId.ArcaneBlock));
        World.Set(arcaneBlockSpell, new CastRangeComponent(1));

        var arcaneBubbleSpell = World.CreateEntity();
        World.Set(arcaneBubbleSpell, new SpellIdComponent(SpellId.ArcaneBubble));
        World.Set(arcaneBubbleSpell, new CastRangeComponent(1));
        World.Set(arcaneBubbleSpell, new CanTargetImpassableFlag());

        var rockChargeSpell = World.CreateEntity();
        World.Set(rockChargeSpell, new SpellIdComponent(SpellId.RockCharge));
        World.Set(rockChargeSpell, new CastRangeComponent(1));
        World.Set(rockChargeSpell, new CanTargetImpassableFlag());
    }
}