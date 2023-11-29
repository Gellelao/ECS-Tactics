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
    private static AnimationSystem _animationSystem;
    private static DestroyAfterDurationSystem _destroyAfterDurationSystem;
    private static ScreenMoveSystem _screenMoveSystem;

    private static SpriteRenderer? _mapRenderer;
    private static TextRenderer? _textRenderer;

    private SpriteBatch _spriteBatch;
    private FontSystem _fontSystem;

    private const int ScreenWidth = 1920;
    private const int ScreenHeight = 1080;
    private const int UpscaleFactor = 4;
    private RenderTarget2D _renderTarget;


    [STAThread]
    internal static void Main()
    {
        using var game = new Enamel();
        game.Run();
    }

    private Enamel()
    {
        //setup our graphics device, default window size, etc
        //here is where i will make a plea to you, intrepid game developer:
        //please default your game to windowed mode.
        GraphicsDeviceManager = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";

        GraphicsDeviceManager.PreferredBackBufferWidth = ScreenWidth;
        GraphicsDeviceManager.PreferredBackBufferHeight = ScreenHeight;
        GraphicsDeviceManager.SynchronizeWithVerticalRetrace = true;
        GraphicsDeviceManager.PreferMultiSampling = false;

        IsFixedTimeStep = false;
        IsMouseVisible = true;
    }

    //you'll want to do most setup in LoadContent() rather than your constructor.
    protected override void LoadContent()
    {
        _renderTarget = new RenderTarget2D(GraphicsDevice, ScreenWidth/UpscaleFactor, ScreenHeight/UpscaleFactor);

        /*
        CONTENT
        */
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _fontSystem = new FontSystem();
        _fontSystem.AddFont(File.ReadAllBytes(
            Path.Combine(
                Content.RootDirectory, "opensans.ttf"
            )
        ));

        // Unsure if this is the way to do this but keep all textures in a dictionary and refer to them by index?
        var textures = new Texture2D[100];

        var redPixel = new Texture2D(GraphicsDevice, 1, 1);
        redPixel.SetData(new[] { Color.Red });

        var greenPixel = new Texture2D(GraphicsDevice, 1, 1);
        greenPixel.SetData(new[] { Color.ForestGreen });
        var greenRectangle = new RenderTarget2D(GraphicsDevice, 40, 20);
        GraphicsDevice.SetRenderTarget(greenRectangle);
        _spriteBatch.Begin();
        _spriteBatch.Draw(greenPixel, new Rectangle(0, 0, 40, 20), Color.White);
        _spriteBatch.End();
        GraphicsDevice.SetRenderTarget(null);

        var yellowPixel = new Texture2D(GraphicsDevice, 1, 1);
        yellowPixel.SetData(new[] { Color.Yellow });
        var yellowSquare = new RenderTarget2D(GraphicsDevice, 30, 30);
        GraphicsDevice.SetRenderTarget(yellowSquare);
        _spriteBatch.Begin();
        _spriteBatch.Draw(yellowPixel, new Rectangle(0, 0, 30, 30), Color.White);
        _spriteBatch.End();
        GraphicsDevice.SetRenderTarget(null);

        textures[(int)Sprite.RedPixel] = redPixel;
        textures[(int)Sprite.GreenRectangle] = greenRectangle;
        textures[(int)Sprite.YellowSquare] = yellowSquare;
        textures[(int)Sprite.Tile] = Content.Load<Texture2D>("GroundTile");
        textures[(int)Sprite.GreenCube] = Content.Load<Texture2D>("greenCube");
        textures[(int)Sprite.BlueWizard] = Content.Load<Texture2D>("blueWiz");
        textures[(int)Sprite.YellowCube] = Content.Load<Texture2D>("yellowCube");
        textures[(int)Sprite.SelectedTile] = Content.Load<Texture2D>("Selected");
        textures[(int)Sprite.TileSelectPreview] = Content.Load<Texture2D>("TilePreview");
        textures[(int)Sprite.Fireball] = Content.Load<Texture2D>("fireball");
        textures[(int)Sprite.ArcaneBlock] = Content.Load<Texture2D>("ArcaneCube");
        textures[(int)Sprite.ArcaneBubble] = Content.Load<Texture2D>("bubble");
        textures[(int)Sprite.Smoke] = Content.Load<Texture2D>("SmokePuff");
        
        // Animations
        var animations = new AnimationData[100];
        // X and Y are the coords of the segment of the sprite sheet we want to draw, if each sprite was a cell in an array
        // we'll multiply X and Y by the size of the sprite to get the pixel coords when rendering.
        // Here we are only defining arrays of Y values, because X is determined by the direction of the sprite (see sprite sheet, each column has all the sprites for once direction)
        var blueWizAnimations = new int[Enum.GetNames(typeof(AnimationType)).Length][];
        blueWizAnimations[(int)AnimationType.Idle] = [1];
        blueWizAnimations[(int)AnimationType.Walk] = [0, 1, 2, 1];
        blueWizAnimations[(int)AnimationType.Hurt] = [3];
        blueWizAnimations[(int)AnimationType.Raise] = [4];
        blueWizAnimations[(int)AnimationType.Throw] = [5];
        animations[(int) AnimationSet.BlueWiz] = new AnimationData(
            Constants.PLAYER_FRAME_WIDTH,
            Constants.PLAYER_FRAME_HEIGHT, 
            blueWizAnimations
        );
        
        animations[(int) AnimationSet.Smoke] = new AnimationData(15, 18, [[0, 1, 2, 3]]);

        /*
        SYSTEMS
        */
        // I think these only work if the map is square but it probably will be
        var mapHeightInPixels = Constants.TILE_HEIGHT * Constants.MAP_HEIGHT * UpscaleFactor;
        var xOffset = ScreenWidth / 2 / UpscaleFactor - Constants.TILE_WIDTH/2;
        var yOffset = (ScreenHeight - mapHeightInPixels) / 2 / UpscaleFactor;
        _gridToScreenCoordSystem = new GridToScreenCoordSystem(World, xOffset, yOffset);
        _inputSystem = new InputSystem(World, UpscaleFactor, xOffset, yOffset);
        _unitSelectionSystem = new UnitSelectionSystem(World);
        _selectionPreviewSystem = new SelectionPreviewSystem(World);
        _gridMoveSystem = new GridMoveSystem(World, xOffset, yOffset);
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

        /*
        RENDERERS
        */
        _mapRenderer = new SpriteRenderer(World, _spriteBatch, textures);
        _textRenderer = new TextRenderer(World, _spriteBatch, _fontSystem);

        /*
        ENTITIES
        */
        // Spells
        // For whatever reason, putting the spell template creation after the ground tiles caused an exception when drawing.
        // So just putting spells first until I decide to figure out why that happens
        CreateSpells();

        var player1 = CreatePlayer(PlayerNumber.One, Sprite.BlueWizard, 1, 1);
        World.Set(player1, new SelectedFlag()); // Just do this for dev, so this player can start with learned spells
        
        CreatePlayer(PlayerNumber.Two, Sprite.BlueWizard, 1, 6);

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

        var turnTracker = World.CreateEntity();
        World.Set(turnTracker, new TurnIndexComponent(-1));
        World.Set(turnTracker, new PlayerCountComponent(3));
        World.Set(turnTracker, new ScreenPositionComponent(200, 10));

        World.Send(new EndTurnMessage());
        // Set up player 1 for dev
        World.Send(new LearnSpellMessage(SpellId.StepOnce));
        World.Send(new LearnSpellMessage(SpellId.Fireball));

        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        var elapsedTime = gameTime.ElapsedGameTime;
        _destroyAfterDurationSystem.Update(elapsedTime);
        _screenMoveSystem.Update(elapsedTime);
        _inputSystem?.Update(elapsedTime);
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
        _animationSystem.Update(elapsedTime);
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

        _spriteBatch.Begin(SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullCounterClockwise,
            null,
            Matrix.Identity); // Only have to set all these here so I can change the default SamplerState
        _spriteBatch.Draw(_renderTarget, new Rectangle(0, 0, ScreenWidth, ScreenHeight), Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private Entity CreatePlayer(PlayerNumber playerNumber, Sprite sprite, int x, int y)
    {
        var playerEntity = World.CreateEntity();
        World.Set(playerEntity, new TextureIndexComponent(sprite));
        // Obviously make the animation set configurable once I have more than 1 to choose from
        World.Set(playerEntity, new AnimationSetComponent(AnimationSet.BlueWiz));
        World.Set(playerEntity, new AnimationStatusComponent(AnimationType.Idle, Constants.DEFAULT_MILLIS_BETWEEN_FRAMES));
        World.Set(playerEntity, new FacingDirectionComponent(GridDirection.South));
        World.Set(playerEntity, new SpriteOriginComponent(-4, 18));
        World.Set(playerEntity, new DrawLayerComponent(DrawLayer.Units));
        World.Set(playerEntity, new GridCoordComponent(x, y));
        World.Set(playerEntity, new MovesPerTurnComponent(Constants.DEFAULT_MOVES_PER_TURN)); // Can probably delete this along with the component and the TurnSystem logic for it
        World.Set(playerEntity, new ImpassableFlag());
        World.Set(playerEntity, new ControlledByPlayerComponent(playerNumber));
        World.Set(playerEntity, new HealthComponent(1));
        // Just for testing, I think players would normally only get this flag if they've had some effect applied to them by another player
        World.Set(playerEntity, new PushableFlag());
        return playerEntity;
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