using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoonTools.ECS;
using Enamel.Systems;
using Enamel.Components;
using Enamel.Renderers;
using FontStashSharp;
using Enamel.Enums;

namespace Enamel
{
    public class Enamel : Game
    {
        GraphicsDeviceManager GraphicsDeviceManager { get; }

        /*
        the World is the place where all our entities go.
        */
        private static World World { get; } = new();
        private static GridToScreenCoordSystem? _gridToScreenCoordSystem;
        private static InputSystem? _inputSystem;
        private static SelectionSystem? _selectionSystem;
        private static SpriteIndexRenderer? _mapRenderer;
        private static TextRenderer? _textRenderer;

        private SpriteBatch _spriteBatch;
        private FontSystem _fontSystem;

        private Texture2D[] _textures;

        private const int ScreenWidth = 1024;
        private const int ScreenHeight = 768;
        private const int TileWidth = 40;
        private const int TileHeight = 20;
        private const int MapWidth = 8;
        private const int MapHeight = 8;
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
            _textures = new Texture2D[5];

            var redPixel = new Texture2D(GraphicsDevice, 1, 1);
            redPixel.SetData(new[] { Color.Red });

            _textures[(int)Sprite.RedPixel] = redPixel;
            _textures[(int)Sprite.Tile] = Content.Load<Texture2D>("Tile");
            _textures[(int)Sprite.Player1] = Content.Load<Texture2D>("Wizard");
            _textures[(int)Sprite.Selection] = Content.Load<Texture2D>("Selection");

            /*
            SYSTEMS
            */

            /*
            here we set up all our systems. 
            you can pass in information that these systems might need to their constructors.
            it doesn't matter what order you create the systems in, we'll specify in what order they run later.
            */
            // I think these only work if the map is square but it probably will be
            var mapHeightInPixels = TileHeight * MapHeight * UpscaleFactor;
            var xOffset = ScreenWidth / 2 / UpscaleFactor - TileWidth/2;
            var yOffset = (ScreenHeight - mapHeightInPixels) / 2 / UpscaleFactor;
            _gridToScreenCoordSystem = new GridToScreenCoordSystem(World, TileWidth, TileHeight, xOffset, yOffset);
            _inputSystem = new InputSystem(World, UpscaleFactor, TileWidth, TileHeight, xOffset, yOffset);
            _selectionSystem = new SelectionSystem(World);

            /*
            RENDERERS
            */

            //same as above, but for the renderer
            _mapRenderer = new SpriteIndexRenderer(World, _spriteBatch, _textures);
            _textRenderer = new TextRenderer(World, _spriteBatch, _fontSystem);

            /*
            ENTITIES
            */
            
            var player1 = World.CreateEntity();
            World.Set(player1, new TextureIndexComponent((int)Sprite.Player1));
            World.Set(player1, new SpriteOriginComponent(
                _textures[(int)Sprite.Player1].Width/2 - TileWidth/2,
                (int)(_textures[(int)Sprite.Player1].Height*0.8 - TileHeight/2))
            );
            World.Set(player1, new GridCoordComponent(3, 2));
            World.Set(player1, new SelectableComponent());

            var player2 = World.CreateEntity();
            World.Set(player2, new TextureIndexComponent((int)Sprite.Player1));
            World.Set(player2, new SpriteOriginComponent(
                _textures[(int)Sprite.Player1].Width/2 - TileWidth/2,
                (int)(_textures[(int)Sprite.Player1].Height*0.8 - TileHeight/2))
            );
            World.Set(player2, new GridCoordComponent(4, 6));
            World.Set(player2, new SelectableComponent());


            for(var x = 7; x >= 0; x--){
                for(var y = 7;  y >= 0; y--){
                    var tile = World.CreateEntity();
                    World.Set(tile, new TextureIndexComponent((int)Sprite.Tile));
                    World.Set(tile, new GridCoordComponent(x, y));
                    World.Set(tile, new DebugCoordComponent(x, y));
                }
            }

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            _gridToScreenCoordSystem?.Update(gameTime.ElapsedGameTime);
            _inputSystem?.Update(gameTime.ElapsedGameTime);
            _selectionSystem?.Update(gameTime.ElapsedGameTime);
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
    }
}