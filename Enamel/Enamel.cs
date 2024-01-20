using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoonTools.ECS;
using Enamel.Systems;
using Enamel.Components;
using Enamel.Components.Messages;
using Enamel.Components.Spells;
using Enamel.Components.UI;
using Enamel.Renderers;
using FontStashSharp;
using Enamel.Enums;
using Enamel.Spawners;
using System.Net.Mail;
using Enamel.Components.Relations;

namespace Enamel;

public class Enamel : Game
{
    GraphicsDeviceManager GraphicsDeviceManager { get; }

    /*
    the World is the place where all our entities go.
    */
    private static World World { get; } = new();
    private static GridToScreenCoordSystem? _gridToScreenCoordSystem;
    private static InputSystem? _inputSystem;
    private static UnitSelectionSystem? _unitSelectionSystem;
    private static SelectionPreviewSystem? _selectionPreviewSystem;
    private static GridMoveSystem? _gridMoveSystem;
    private static TurnSystem? _turnSystem;
    private static SpellManagementSystem? _spellManagementSystem;
    private static PlayerButtonsSystem? _playerButtonsSystem;
    private static SpellCastingSystem? _spellCastingSystem;
    private static ProjectileSystem? _projectileSystem;
    private static DamageSystem? _damageSystem;
    private static UnitDisablingSystem? _unitDisablingSystem;
    private static PushSystem? _pushSystem;
    private static AnimationSystem? _animationSystem;
    private static DestroyAfterDurationSystem? _destroyAfterDurationSystem;
    private static ScreenMoveSystem? _screenMoveSystem;
    private static MenuSystem? _menuSystem;
    private static CenterChildrenSystem? _centerChildrenSystem;

    private static SpriteRenderer? _mapRenderer;
    private static TextRenderer? _textRenderer;

    private SpriteBatch _spriteBatch;
    private FontSystem _fontSystem;

    private RenderTarget2D _renderTarget;
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

        GraphicsDeviceManager.PreferredBackBufferWidth = Constants.PIXEL_SCREEN_WIDTH*2;
        GraphicsDeviceManager.PreferredBackBufferHeight = Constants.PIXEL_SCREEN_HEIGHT*2;
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
        var (rectangle, scale) = GetFinalRenderRectangle();
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
        var cameraX = Constants.PIXEL_SCREEN_WIDTH/2;
        var cameraY = Constants.PIXEL_SCREEN_HEIGHT/2;
        _gridToScreenCoordSystem = new GridToScreenCoordSystem(World, cameraX, cameraY);
        _inputSystem = new InputSystem(World, scale, cameraX, cameraY);
        _unitSelectionSystem = new UnitSelectionSystem(World);
        _selectionPreviewSystem = new SelectionPreviewSystem(World);
        _gridMoveSystem = new GridMoveSystem(World, cameraX, cameraY);
        _turnSystem = new TurnSystem(World);
        _spellManagementSystem = new SpellManagementSystem(World);
        _playerButtonsSystem = new PlayerButtonsSystem(World);

        var spellCastSpawner = new SpellCastSpawner(World);
        _spellCastingSystem = new SpellCastingSystem(World, spellCastSpawner);
        _projectileSystem = new ProjectileSystem(World);

        var particleSpawner = new ParticleSpawner(World, animations);
        _damageSystem = new DamageSystem(World, particleSpawner);
        _unitDisablingSystem = new UnitDisablingSystem(World);
        _pushSystem = new PushSystem(World);
        _animationSystem = new AnimationSystem(World, animations);
        _destroyAfterDurationSystem = new DestroyAfterDurationSystem(World);
        _screenMoveSystem = new ScreenMoveSystem(World);
        _menuSystem = new MenuSystem(World);
        _centerChildrenSystem = new CenterChildrenSystem(World);

        /*
        RENDERERS
        */
        _mapRenderer = new SpriteRenderer(World, _spriteBatch, textures);
        _textRenderer = new TextRenderer(World, _spriteBatch, fonts);

        /*
        ENTITIES
        */
        var zeroEntity = World.CreateEntity();
        //World.Destroy(zeroEntity);
        
        var turnTracker = World.CreateEntity();
        World.Set(turnTracker, new TurnIndexComponent(-1));
        World.Set(turnTracker, new ScreenPositionComponent(100, 10));
        
        // Spells
        // For whatever reason, putting the spell template creation after the ground tiles caused an exception when drawing.
        // So just putting spells first until I decide to figure out why that happens
        CreateSpells();

        //var player1 = CreatePlayer(PlayerNumber.One, Sprite.BlueWizard, 1, 1);
        //World.Set(player1, new SelectedFlag()); // Just do this for dev, so this player can start with learned spells
        
        //CreatePlayer(PlayerNumber.Two, Sprite.BlueWizard, 1, 6);

        var endTurnButton = World.CreateEntity();
        World.Set(endTurnButton, new ScreenPositionComponent(400, 300));
        World.Set(endTurnButton, new DimensionsComponent(40, 20));
        World.Set(endTurnButton, new TextureIndexComponent(Sprite.GreenRectangle));
        World.Set(endTurnButton, new OnClickComponent(ClickEvent.EndTurn));

        var learnFireballButton = World.CreateEntity();
        World.Set(learnFireballButton, new ScreenPositionComponent(50, 200));
        World.Set(learnFireballButton, new DimensionsComponent(40, 20));
        World.Set(learnFireballButton, new TextureIndexComponent(Sprite.GreenRectangle));
        World.Set(learnFireballButton, new OnClickComponent(ClickEvent.LearnSpell));
        World.Set(learnFireballButton, new SpellToLearnOnClickComponent(SpellId.Fireball));

        var learnArcaneBlockButton = World.CreateEntity();
        World.Set(learnArcaneBlockButton, new ScreenPositionComponent(50, 230));
        World.Set(learnArcaneBlockButton, new DimensionsComponent(40, 20));
        World.Set(learnArcaneBlockButton, new TextureIndexComponent(Sprite.GreenRectangle));
        World.Set(learnArcaneBlockButton, new OnClickComponent(ClickEvent.LearnSpell));
        World.Set(learnArcaneBlockButton, new SpellToLearnOnClickComponent(SpellId.ArcaneBlock));

        var learnArcaneBubbleButton = World.CreateEntity();
        World.Set(learnArcaneBubbleButton, new ScreenPositionComponent(50, 260));
        World.Set(learnArcaneBubbleButton, new DimensionsComponent(40, 20));
        World.Set(learnArcaneBubbleButton, new TextureIndexComponent(Sprite.GreenRectangle));
        World.Set(learnArcaneBubbleButton, new OnClickComponent(ClickEvent.LearnSpell));
        World.Set(learnArcaneBubbleButton, new SpellToLearnOnClickComponent(SpellId.ArcaneBubble));

        for(var x = 7; x >= 0; x--){
            for(var y = 7;  y >= 0; y--){
                var tile = World.CreateEntity();
                World.Set(tile, new TextureIndexComponent(Sprite.Tile));
                World.Set(tile, new GridCoordComponent(x, y));
                World.Set(tile, new DebugCoordComponent(x, y));
                World.Set(tile, new DisabledFlag());
                World.Set(tile, new DrawLayerComponent(DrawLayer.Tiles));
            }
        }

        //World.Send(new EndTurnMessage());
        // Set up player 1 for dev
        //World.Send(new LearnSpellMessage(SpellId.StepOnce));
        //World.Send(new LearnSpellMessage(SpellId.Fireball));
        //World.Send(new LearnSpellMessage(SpellId.RockCharge));
        
        World.Send(new GoToMainMenuMessage());

        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        var elapsedTime = gameTime.ElapsedGameTime;
        _destroyAfterDurationSystem?.Update(elapsedTime);
        _screenMoveSystem?.Update(elapsedTime);
        _inputSystem?.Update(elapsedTime);
        _menuSystem?.Update(elapsedTime);
        _centerChildrenSystem?.Update(elapsedTime);
        _unitSelectionSystem?.Update(elapsedTime); // Must run before the selectionPreview system so that the PlayerUnitSelectedMessage can be received in the selectionPreviewSystem
        _turnSystem?.Update(elapsedTime);
        _spellCastingSystem?.Update(elapsedTime); // Must run before the projectileSystem because the spellPreviewSystem runs as soon as a spell is cast, and if the spell kills a unit that unit needs to be deleted by the DamageMessage in ProjectileSystem before the movements previews are displayed
        _gridToScreenCoordSystem?.Update(elapsedTime); // Yikes, running a system twice... this is needed because the entity spawned by the spell needs a position for the _gridMoveSystem to update it, and we can't wait for the _gridToScreenCoordSystem. Nor can we replicate the behaviour in that system, because its the only thing with access to the _offsets...
        _gridMoveSystem?.Update(elapsedTime); // Must run after the unitSelectionSystem so the unit has the SelectedFlag by the time the _gridMoveSystem runs  (is this true?)
        _projectileSystem?.Update(elapsedTime); // Must run after the _gridMoveSystem because it listens for UnitMoveCompletedMessages to know when to move the PerStep projectiles
        _pushSystem?.Update(elapsedTime); // Listens for PushMessages from the Projectile system
        _damageSystem?.Update(elapsedTime); // Should run after pushSystem because the pushSystem sends DamageMessages
        _spellManagementSystem?.Update(elapsedTime);
        _playerButtonsSystem?.Update(elapsedTime);
        _unitDisablingSystem?.Update(elapsedTime);
        _animationSystem?.Update(elapsedTime);
        _selectionPreviewSystem?.Update(elapsedTime); // Must run after the move system so that it doesn't delete the MovementPreviews before the Move system has a chance to get them
        _gridToScreenCoordSystem?.Update(elapsedTime); // Must run near the end so entities can have their PositionComponent attached before the renderer tries to access it
        World.FinishUpdate(); //always call this at the end of your update function.
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.Black);

        /*
        renderers don't get passed the game time. 
        render to the smaller renderTarget, then upscale after
        */
        _mapRenderer?.Draw();
        _textRenderer?.Draw();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullCounterClockwise,
            null,
            Matrix.Identity); // Only have to set all these here so I can change the default SamplerState
        _spriteBatch.Draw(_renderTarget, _finalRenderRectangle, Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void OnWindowClientSizeChanged(object? sender, EventArgs e)
    {
        _finalRenderRectangle = GetFinalRenderRectangle().Item1;
    }

    private (Rectangle, float) GetFinalRenderRectangle()
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

        var offsetX = (int)((windowWidth - (Constants.PIXEL_SCREEN_WIDTH * scale)) / 2);
        var offsetY = (int)((windowHeight - (Constants.PIXEL_SCREEN_HEIGHT * scale)) / 2);

        World.Send(new ScreenDetailsChangedMessage(scale, offsetX, offsetY));

        return (new Rectangle(offsetX, offsetY, (int)(Constants.PIXEL_SCREEN_WIDTH * scale), (int)(Constants.PIXEL_SCREEN_HEIGHT * scale)), scale);
    }

    private Entity CreatePlayer(PlayerNumber playerNumber, Sprite sprite, int x, int y)
    {
        var player = World.CreateEntity();
        World.Set(player, new PlayerNumberComponent(playerNumber));
        
        var playerCharacter = World.CreateEntity();
        World.Relate(player, playerCharacter, new ControlsRelation());
        
        World.Set(playerCharacter, new TextureIndexComponent(sprite));
        World.Set(playerCharacter, new AnimationSetComponent(AnimationSet.BlueWiz));
        World.Set(playerCharacter, new AnimationStatusComponent(AnimationType.Idle, Constants.DEFAULT_MILLIS_BETWEEN_FRAMES));
        World.Set(playerCharacter, new FacingDirectionComponent(GridDirection.South));
        World.Set(playerCharacter, new SpriteOriginComponent(-4, 18));
        World.Set(playerCharacter, new DrawLayerComponent(DrawLayer.Units));
        World.Set(playerCharacter, new GridCoordComponent(x, y));
        World.Set(playerCharacter, new ImpassableFlag());
        World.Set(playerCharacter, new HealthComponent(1));
        // Just for testing, I think players would normally only get this flag if they've had some effect applied to them by another player
        World.Set(playerCharacter, new PushableFlag());
        return playerCharacter;
    }

    private void CreateSpells()
    {
        var stepOnceSpell = World.CreateEntity();
        World.Set(stepOnceSpell, new SpellIdComponent(SpellId.StepOnce));
        World.Set(stepOnceSpell, new CastRangeComponent(1));
        
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