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
    private static HighlightSystem? _highlightSystem;
    private static SelectionPreviewSystem? _selectionPreviewSystem;
    private static MoveSystem? _moveSystem;
    private static TurnSystem? _turnSystem;
    private static SpellManagementSystem? _spellManagementSystem;
    private static PlayerButtonsSystem? _playerButtonsSystem;
    private static SpellCastingSystem? _spellCastingSystem;

    private static GroundRenderer? _groundRenderer;
    private static SpriteIndexRenderer? _mapRenderer;
    private static TextRenderer? _textRenderer;

    private SpriteBatch _spriteBatch;
    private FontSystem _fontSystem;

    private Texture2D[] _textures;

    private const int ScreenWidth = 1024;
    private const int ScreenHeight = 768;
    private const int TileWidth = 40;
    private const int TileHeight = 20;
    private const int UpscaleFactor = 2;
    private RenderTarget2D _renderTarget;
    //private Entity[,] GroundGrid;
    //private Entity[,] EntityGrid;


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

        // Not actually populated yet since entities are tracked by World anyway
        //GroundGrid = new Entity[MapWidth, MapHeight];
        //EntityGrid = new Entity[MapWidth, MapHeight];

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
        _textures = new Texture2D[10];

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

        _textures[(int)Sprite.RedPixel] = redPixel;
        _textures[(int)Sprite.GreenRectangle] = greenRectangle;
        _textures[(int)Sprite.YellowSquare] = yellowSquare;
        _textures[(int)Sprite.Tile] = Content.Load<Texture2D>("Tile");
        _textures[(int)Sprite.GreenCube] = Content.Load<Texture2D>("greenCube");
        _textures[(int)Sprite.RedCube] = Content.Load<Texture2D>("redCube");
        _textures[(int)Sprite.YellowCube] = Content.Load<Texture2D>("yellowCube");
        _textures[(int)Sprite.Selection] = Content.Load<Texture2D>("Selection");
        _textures[(int)Sprite.SelectPreview] = Content.Load<Texture2D>("SelectPreview");
        _textures[(int)Sprite.Fireball] = Content.Load<Texture2D>("fireball");

        /*
        SYSTEMS
        */

        /*
        here we set up all our systems. 
        you can pass in information that these systems might need to their constructors.
        it doesn't matter what order you create the systems in, we'll specify in what order they run later.
        */
        // I think these only work if the map is square but it probably will be
        var mapHeightInPixels = TileHeight * Constants.MAP_HEIGHT * UpscaleFactor;
        var xOffset = ScreenWidth / 2 / UpscaleFactor - TileWidth/2;
        var yOffset = (ScreenHeight - mapHeightInPixels) / 2 / UpscaleFactor;
        _gridToScreenCoordSystem = new GridToScreenCoordSystem(World, TileWidth, TileHeight, xOffset, yOffset);
        _inputSystem = new InputSystem(World, UpscaleFactor, TileWidth, TileHeight, xOffset, yOffset);
        _unitSelectionSystem = new UnitSelectionSystem(World);
        _highlightSystem = new HighlightSystem(World);
        _selectionPreviewSystem = new SelectionPreviewSystem(World);
        _moveSystem = new MoveSystem(World);
        _turnSystem = new TurnSystem(World);
        _spellManagementSystem = new SpellManagementSystem(World);
        _playerButtonsSystem = new PlayerButtonsSystem(World);
        _spellCastingSystem = new SpellCastingSystem(World);

        /*
        RENDERERS
        */

        //same as above, but for the renderer
        _groundRenderer = new GroundRenderer(World, _spriteBatch, _textures);
        _mapRenderer = new SpriteIndexRenderer(World, _spriteBatch, _textures);
        _textRenderer = new TextRenderer(World, _spriteBatch, _fontSystem);

        /*
        ENTITIES
        */
        CreatePlayer(PlayerNumber.One, Sprite.GreenCube, 1, 1);
        CreatePlayer(PlayerNumber.One, Sprite.GreenCube, 2, 1);
        CreatePlayer(PlayerNumber.Two, Sprite.RedCube, 2, 4);
        CreatePlayer(PlayerNumber.Three, Sprite.YellowCube, 5, 7);

        var endTurnButton = World.CreateEntity();
        World.Set(endTurnButton, new PositionComponent(400, 300));
        World.Set(endTurnButton, new DimensionsComponent(40, 20));
        World.Set(endTurnButton, new TextureIndexComponent(Sprite.GreenRectangle));
        World.Set(endTurnButton, new OnClickComponent(ClickEvent.EndTurn));

        var learnFireballButton = World.CreateEntity();
        World.Set(learnFireballButton, new PositionComponent(50, 200));
        World.Set(learnFireballButton, new DimensionsComponent(40, 20));
        World.Set(learnFireballButton, new TextureIndexComponent(Sprite.GreenRectangle));
        World.Set(learnFireballButton, new OnClickComponent(ClickEvent.LearnSpell));
        World.Set(learnFireballButton, new SpellToLearnOnClickComponent(SpellId.Fireball));

        var learnArcaneBlockButton = World.CreateEntity();
        World.Set(learnArcaneBlockButton, new PositionComponent(50, 230));
        World.Set(learnArcaneBlockButton, new DimensionsComponent(40, 20));
        World.Set(learnArcaneBlockButton, new TextureIndexComponent(Sprite.GreenRectangle));
        World.Set(learnArcaneBlockButton, new OnClickComponent(ClickEvent.LearnSpell));
        World.Set(learnArcaneBlockButton, new SpellToLearnOnClickComponent(SpellId.ArcaneBlock));

        for(var x = 7; x >= 0; x--){
            for(var y = 7;  y >= 0; y--){
                var tile = World.CreateEntity();
                World.Set(tile, new TextureIndexComponent(Sprite.Tile));
                World.Set(tile, new GridCoordComponent(x, y));
                World.Set(tile, new DebugCoordComponent(x, y));
                World.Set(tile, new DisabledFlag());
                World.Set(tile, new GroundTileFlag());
            }
        }

        var turnTracker = World.CreateEntity();
        World.Set(turnTracker, new TurnIndexComponent(-1));
        World.Set(turnTracker, new PlayerCountComponent(3));
        World.Set(turnTracker, new PositionComponent(200, 10));

        // Spells
        var fireball = World.CreateEntity();
        World.Set(fireball, new SpellIdComponent(SpellId.Fireball));
        World.Set(fireball, new CastRangeComponent(1));
        World.Set(fireball, new TextureIndexOfSpawnedEntityComponent(Sprite.Fireball));
        World.Set(fireball, new SpawnsProjectileSpellFlag());
        World.Set(fireball, new SpawnedProjectileMoveRateComponent(ProjectileMoveRate.Immediate));
        World.Set(fireball, new SpawnedProjectileDamageComponent(1));

        var arcaneBlock = World.CreateEntity();
        World.Set(arcaneBlock, new SpellIdComponent(SpellId.ArcaneBlock));
        World.Set(arcaneBlock, new CastRangeComponent(1));

        World.Send(new EndTurnMessage());

        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        var elapsedTime = gameTime.ElapsedGameTime;
        _inputSystem?.Update(elapsedTime);
        _unitSelectionSystem?.Update(elapsedTime); // Must run before the selectionPreview system so that the PlayerUnitSelectedMessage can be received in the selectionPreviewSystem
        _highlightSystem?.Update(elapsedTime);
        _turnSystem?.Update(elapsedTime);
        _moveSystem?.Update(elapsedTime); // Must run after the unitSelectionSystem so the unit has the SelectedFlag by the time the MoveSystem runs
        _spellManagementSystem?.Update(elapsedTime);
        _playerButtonsSystem?.Update(elapsedTime);
        _spellCastingSystem?.Update(elapsedTime);
        _selectionPreviewSystem?.Update(elapsedTime); // This has to run after the move system so that it doesn't delete the MovementPreviews before the Move system has a chance to get them
        _gridToScreenCoordSystem?.Update(elapsedTime); // This needs to run near the end so entities can have their PositionComponent attached before the renderer tries to access it
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
        _groundRenderer?.Draw();
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

    private void CreatePlayer(PlayerNumber playerNumber, Sprite sprite, int x, int y)
    {
        var playerEntity = World.CreateEntity();
        World.Set(playerEntity, new TextureIndexComponent(sprite));
        World.Set(playerEntity, new SpriteOriginComponent(
            _textures[(int)sprite].Width/2 - TileWidth/2,
            (int)(_textures[(int)sprite].Height*0.45 - TileHeight/2))
        );
        World.Set(playerEntity, new GridCoordComponent(x, y));
        World.Set(playerEntity, new MovesPerTurnComponent(Constants.DEFAULT_MOVES_PER_TURN));
        World.Set(playerEntity, new ImpassableFlag());
        World.Set(playerEntity, new ControlledByPlayerComponent(playerNumber));
        World.Set(playerEntity, new HealthComponent(2));
    }
}