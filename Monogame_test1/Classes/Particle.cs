using Monogame_test1.Classes.OwnMathForVectors;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Monogame_test1.Classes
{
    internal class Particle
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; } = new Vector2(0,0);
        public int Size { get; set; }
        public int Mass { get; set; }

        public Particle(Vector2 Position, int Size)
        {
            this.Position = Position;
            this.Size = Size;
        }
        public bool Intersects(Particle other, out Vector2 normal, out float depth)
        {
            Vector2 direction = other.Position - this.Position;
            float distance = direction.Length();
            float radius1 = Size / 2f;
            float radius2 = other.Size / 2f;
            float minDistance = radius1 + radius2;

            if (distance > minDistance)
            {
                normal = Vector2.Zero;
                depth = 0;
                return false;
            }

            normal = OwnMath.Normalize(direction);
            depth = minDistance - distance;
            return true;
        }
    }
}
