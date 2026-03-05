using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.Graphics;
using Platformer.Collisions;
using Platformer.Components;
using World = MonoGame.Extended.ECS.World;

namespace Platformer
{
    public class EntityFactory
    {
        private readonly World _world;
        private readonly ContentManager _contentManager;

        private readonly List<int> _entityIds = new List<int>();

        public EntityFactory(World world, ContentManager contentManager)
        {
            _world = world;
            _contentManager = contentManager;
        }

        public Entity CreatePlayer(Vector2 position)
        {
            var dudeTexture = _contentManager.Load<Texture2D>("hero");
            var dudeAtlas = Texture2DAtlas.Create("TextureAtlas//hero", dudeTexture, 16, 16);


            var entity = _world.CreateEntity();
            _entityIds.Add(entity.Id);
            var spriteSheet = new SpriteSheet("SpriteSheet//hero", dudeAtlas);
            //var spriteSheet = new SpriteSheet {TextureAtlas = dudeAtlas};

            AddAnimationCycle(spriteSheet, "idle", new[] { 0, 1, 2, 1 });
            AddAnimationCycle(spriteSheet, "walk", new[] { 6, 7, 8, 9, 10, 11 });
            AddAnimationCycle(spriteSheet, "jump", new[] { 10, 12 }, false);
            AddAnimationCycle(spriteSheet, "fall", new[] { 13, 14 }, false);
            AddAnimationCycle(spriteSheet, "swim", new[] { 18, 19, 20, 21, 22, 23 });
            AddAnimationCycle(spriteSheet, "kick", new[] { 15 }, false, 0.3f);
            AddAnimationCycle(spriteSheet, "punch", new[] { 26 }, false, 0.3f);
            AddAnimationCycle(spriteSheet, "cool", new[] { 17 }, false, 0.3f);
            entity.Attach(new AnimatedSprite(spriteSheet, "idle"));
            entity.Attach(new Transform2(position, 0, Vector2.One * 4));
            var body = new Body { Position = position, Size = new Vector2(32, 64), BodyType = BodyType.Dynamic };
            entity.Attach(body);
            entity.Attach(new Player());

            PlayerRegistry.PlayerBody = body;

            return entity;
        }

        public Entity CreateBlue(Vector2 position)
        {
            var dudeTexture = _contentManager.Load<Texture2D>("blueguy");
            var dudeAtlas = Texture2DAtlas.Create("TextureAtlas//blueguy", dudeTexture, 16, 16);
            //var dudeAtlas = TextureAtlas.Create("blueguyAtlas", dudeTexture, 16, 16);

            var entity = _world.CreateEntity();
            _entityIds.Add(entity.Id);
            var spriteSheet = new SpriteSheet("SpriteSheet//blueguy", dudeAtlas);
            //var spriteSheet = new SpriteSheet {TextureAtlas = dudeAtlas};
            AddAnimationCycle(spriteSheet, "idle", new[] { 0, 1, 2, 3, 2, 1 });
            AddAnimationCycle(spriteSheet, "walk", new[] { 6, 7, 8, 9, 10, 11 });
            AddAnimationCycle(spriteSheet, "jump", new[] { 10, 12 }, false, 1.0f);
            entity.Attach(new AnimatedSprite(spriteSheet, "idle"));
            entity.Attach(new Transform2(position, 0, Vector2.One * 4));
            entity.Attach(new Body { Position = position, Size = new Vector2(32, 64), BodyType = BodyType.Dynamic });
            entity.Attach(new Enemy());
            return entity;
        }

        private void AddAnimationCycle(SpriteSheet spriteSheet, string name, int[] frames, bool isLooping = true, float frameDuration = 0.1f)
        {
            spriteSheet.DefineAnimation(name, builder =>
            {
                builder.IsLooping(isLooping);
                for (int i = 0; i < frames.Length; i++)
                {
                    builder.AddFrame(frames[i], TimeSpan.FromSeconds(frameDuration));
                }
            });
            //var cycle = new SpriteSheetAnimationCycle();
            //foreach (var f in frames)
            //{
            //    cycle.Frames.Add(new SpriteSheetAnimationFrame(f, frameDuration));
            //}

            //cycle.IsLooping = isLooping;

            //spriteSheet.Cycles.Add(name, cycle);
        }

        public void CreateTile(int x, int y, int width, int height)
        {
            var entity = _world.CreateEntity();
            _entityIds.Add(entity.Id);
            entity.Attach(new Body
            {
                Position = new Vector2(x * width - width * 0.5f, y * height - height * 0.5f),
                Size = new Vector2(width, height),
                BodyType = BodyType.Static
            });
        }

        public Entity CreateDoor(Vector2 position, string targetMap, Vector2 spawnPosition)
        {
            var bounds = new RectangleF(position.X, position.Y, 32, 64);

            var entity = _world.CreateEntity();
            _entityIds.Add(entity.Id);
            entity.Attach(new Transform2(position));
            entity.Attach(new Door(targetMap, spawnPosition, bounds));
            // NO Body attached — door is a trigger, not a solid wall

            DoorRegistry.DoorBounds = bounds;

            return entity;
        }

        public void DestroyAllEntities()
        {
            // Collect all active entity IDs and destroy them
            foreach (var id in _entityIds)
                _world.DestroyEntity(id);

            _entityIds.Clear();

            // Reset registries
            PlayerRegistry.PlayerBody = null;
            DoorRegistry.DoorBounds = null;
        }
    }
}
