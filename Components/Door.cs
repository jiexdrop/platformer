// Components/Door.cs
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Platformer.Components
{
    public class Door
    {
        public string TargetMap { get; set; }
        public Vector2 SpawnPosition { get; set; }
        public RectangleF Bounds { get; set; }  // store the rect here instead

        public Door(string targetMap, Vector2 spawnPosition, RectangleF bounds)
        {
            TargetMap = targetMap;
            SpawnPosition = spawnPosition;
            Bounds = bounds;
        }
    }
}