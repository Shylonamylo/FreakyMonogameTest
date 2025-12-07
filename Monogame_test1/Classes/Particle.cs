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
        public bool Intersects(Particle particle)
        {

            var center0 = particle.Position;
            var center1 = Position;

            var dist = Vector2.Distance(center0, center1);
            var rads = Size / 2 + particle.Size / 2;

            if (dist > rads)
            {
                return false;
            }

            return true;
        }
    }
}
