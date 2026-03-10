using Autofac;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.Input;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using Platformer.Components;
using Platformer.Systems;

namespace Platformer
{
    public class GameMain : GameBase
    {
        private TiledMap _map;
        private TiledMapRenderer _renderer;
        private EntityFactory _entityFactory;
        private OrthographicCamera _camera;
        private World _world;

        private SpriteBatch _debugBatch;
        private Texture2D _debugPixel;

        private SettingsMenu _settingsMenu;


        public GameMain()
        {
        }

        protected override void RegisterDependencies(ContainerBuilder builder)
        {
            _camera = new OrthographicCamera(GraphicsDevice);

            builder.RegisterInstance(new SpriteBatch(GraphicsDevice));
            builder.RegisterInstance(_camera);
        }

        protected override void LoadContent()
        {
            _world = new WorldBuilder()
                .AddSystem(new WorldSystem())
                .AddSystem(new PlayerSystem())
                .AddSystem(new EnemySystem())
                .AddSystem(new DoorSystem())
                .AddSystem(new RenderSystem(new SpriteBatch(GraphicsDevice), _camera))
                .Build();

            Components.Add(_world);

            _entityFactory = new EntityFactory(_world, Content);

            // TOOD: Load maps and collision data more nicely :)
            _map = Content.Load<TiledMap>("test-map");
            _renderer = new TiledMapRenderer(GraphicsDevice, _map);

            foreach (var tileLayer in _map.TileLayers)
            {
                for (var x = 0; x < tileLayer.Width; x++)
                {
                    for (var y = 0; y < tileLayer.Height; y++)
                    {
                        var tile = tileLayer.GetTile((ushort)x, (ushort)y);

                        if (tile.GlobalIdentifier == 1)
                        {
                            var tileWidth = _map.TileWidth;
                            var tileHeight = _map.TileHeight;
                            _entityFactory.CreateTile(x, y, tileWidth, tileHeight);
                        }
                    }
                }
            }

            // Read object layers from Tiled (doors, spawn points, etc.)
            foreach (var objectLayer in _map.ObjectLayers)
            {
                //System.Diagnostics.Debug.WriteLine($"[Tiled] Found object layer: '{objectLayer.Name}' with {objectLayer.Objects.Count} objects");

                foreach (var obj in objectLayer.Objects)
                {
                    System.Diagnostics.Debug.WriteLine($"[Tiled]   Object: name='{obj.Name}' type='{obj.Type}' pos={obj.Position}");

                    if (obj.Name == "door")
                    {
                        var pos = new Vector2(obj.Position.X, obj.Position.Y);

                        // Read the targetMap custom property from Tiled
                        if (obj.Properties.TryGetValue("targetMap", out var targetMap))
                        {
                            System.Diagnostics.Debug.WriteLine($"[Tiled]   -> Creating door to '{targetMap}' at {pos}");
                            _entityFactory.CreateDoor(pos, targetMap, new Vector2(100, 240));
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[Tiled]   -> WARNING: door object has no 'targetMap' property!");
                        }
                    }
                }
            }

            //_entityFactory.CreateBlue(new Vector2(600, 240));
            //_entityFactory.CreateBlue(new Vector2(700, 100));
            _entityFactory.CreatePlayer(new Vector2(100, 240));

            _debugBatch = new SpriteBatch(GraphicsDevice);
            _debugPixel = new Texture2D(GraphicsDevice, 1, 1);
            _debugPixel.SetData(new[] { Color.White });

            var font = Content.Load<SpriteFont>("DefaultFont"); // make sure this exists in Content
            _settingsMenu = new SettingsMenu(this, font, _debugBatch);

        }

        protected override void Update(GameTime gameTime)
        {
            // TODO: Using global shared input state is really bad!

            KeyboardExtended.Update();
            MouseExtended.Update();

            if (PendingMap != null)
            {
                LoadMap(PendingMap, PendingSpawn);
                PendingMap = null;
            }

            //var keyboardState = KeyboardExtended.GetState();

            //if (keyboardState.IsKeyDown(Keys.Escape))
            //    Exit();

            _renderer.Update(gameTime);
            //_camera.LookAt(_playerEntity.Get<Transform2>().Position);

            //_world.Update(gameTime);


            var keyboard = KeyboardExtended.GetState();
            if (keyboard.IsKeyDown(Keys.Tab)) // or F1, Escape, etc.
                if (_settingsMenu.IsOpen) _settingsMenu.Close();
                else _settingsMenu.Open();

            _settingsMenu.Update();


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _renderer.Draw(_camera.GetViewMatrix());
            //_world.Draw(gameTime);


            _debugBatch.Begin(transformMatrix: _camera.GetViewMatrix());
            DebugDrawBodies();
            _debugBatch.End();

            _settingsMenu.Draw();

            base.Draw(gameTime);
        }

        public static string PendingMap { get; private set; }
        public static Vector2 PendingSpawn { get; private set; }


        public static void RequestLevelChange(string mapName, Vector2 spawnPosition)
        {
            PendingMap = mapName;
            PendingSpawn = spawnPosition;
        }



        private void LoadMap(string mapName, Vector2 playerSpawn)
        {
            // 1. Clear old physics bodies by destroying all entities
            // (simplest approach — rebuild everything from scratch)
            _entityFactory.DestroyAllEntities();

            // 2. Load the new Tiled map
            _map = Content.Load<TiledMap>(mapName);
            _renderer = new TiledMapRenderer(GraphicsDevice, _map);

            // 3. Recreate tile collision bodies
            foreach (var tileLayer in _map.TileLayers)
            {
                for (var x = 0; x < tileLayer.Width; x++)
                {
                    for (var y = 0; y < tileLayer.Height; y++)
                    {
                        var tile = tileLayer.GetTile((ushort)x, (ushort)y);
                        if (tile.GlobalIdentifier == 1)
                        {
                            _entityFactory.CreateTile(x, y, _map.TileWidth, _map.TileHeight);
                        }
                    }
                }
            }

            // 4. Recreate doors from object layer
            foreach (var objectLayer in _map.ObjectLayers)
            {
                foreach (var obj in objectLayer.Objects)
                {
                    if (obj.Name == "door")
                    {
                        var pos = new Vector2(obj.Position.X, obj.Position.Y);
                        if (obj.Properties.TryGetValue("targetMap", out var targetMap))
                        {
                            _entityFactory.CreateDoor(pos, targetMap, new Vector2(100, 240));
                        }
                    }
                }
            }

            // 5. Spawn player at new position
            _entityFactory.CreatePlayer(playerSpawn);
        }



        private void DebugDrawBodies()
        {
            // Draw player body (green)
            var playerBody = PlayerRegistry.PlayerBody;
            if (playerBody != null)
                DrawRectangleOutline(_debugBatch, playerBody.Position, playerBody.Size, Color.LimeGreen);

            // Draw door bounds (red) — you'll need a DoorRegistry similar to PlayerRegistry
            var doorBounds = DoorRegistry.DoorBounds; // see step 2 below
            if (doorBounds.HasValue)
                DrawRectangleOutline(_debugBatch,
                    new Vector2(doorBounds.Value.X, doorBounds.Value.Y),
                    new Vector2(doorBounds.Value.Width, doorBounds.Value.Height),
                    Color.Red);
        }

        private void DrawRectangleOutline(SpriteBatch batch, Vector2 position, Vector2 size, Color color, int thickness = 2)
        {
            // Top
            batch.Draw(_debugPixel, new Rectangle((int)position.X, (int)position.Y, (int)size.X, thickness), color);
            // Bottom
            batch.Draw(_debugPixel, new Rectangle((int)position.X, (int)(position.Y + size.Y), (int)size.X, thickness), color);
            // Left
            batch.Draw(_debugPixel, new Rectangle((int)position.X, (int)position.Y, thickness, (int)size.Y), color);
            // Right
            batch.Draw(_debugPixel, new Rectangle((int)(position.X + size.X), (int)position.Y, thickness, (int)size.Y), color);
        }
    }
}