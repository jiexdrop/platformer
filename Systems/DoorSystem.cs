// Systems/DoorSystem.cs
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using Platformer.Collisions;
using Platformer.Components;

namespace Platformer.Systems
{
    public class DoorSystem : EntityProcessingSystem
    {
        private ComponentMapper<Door> _doorMapper;
        private ComponentMapper<Body> _bodyMapper;
        private ComponentMapper<Player> _playerMapper;

        // We'll track the player body separately
        private Body _playerBody;

        private double _debugTimer = 0;

        public DoorSystem()
            : base(Aspect.All(typeof(Door)))
        {
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _doorMapper = mapperService.GetMapper<Door>();
            _bodyMapper = mapperService.GetMapper<Body>();
            _playerMapper = mapperService.GetMapper<Player>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var playerBody = PlayerRegistry.PlayerBody;
            if (playerBody == null) return;

            var door = _doorMapper.Get(entityId);

            var playerRect = new RectangleF(
                playerBody.Position.X,
                playerBody.Position.Y,
                playerBody.Size.X,
                playerBody.Size.Y
            );

            // DEBUG: print distance to door every second
            _debugTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (_debugTimer > 1.0)
            {
                var doorCenter = new Vector2(door.Bounds.X + door.Bounds.Width / 2,
                                             door.Bounds.Y + door.Bounds.Height / 2);
                var playerCenter = new Vector2(playerBody.Position.X + playerBody.Size.X / 2,
                                                playerBody.Position.Y + playerBody.Size.Y / 2);
                var dist = Vector2.Distance(playerCenter, doorCenter);
                System.Diagnostics.Debug.WriteLine($"[Door] Player pos: {playerBody.Position} | Door bounds: {door.Bounds} | Distance: {dist:F0}px | Intersects: {door.Bounds.Intersects(playerRect)}");
                _debugTimer = 0;
            }

            if (door.Bounds.Intersects(playerRect))
            {
                System.Diagnostics.Debug.WriteLine($"[Door] *** COLLISION! Transitioning to {door.TargetMap} ***");
                GameMain.RequestLevelChange(door.TargetMap, door.SpawnPosition);
            }
        }
    }
}