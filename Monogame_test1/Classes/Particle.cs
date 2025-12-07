using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Monogame_test1.Classes
{
    internal class Particle
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; } = new Vector2(0,0);
        public int Size { get; set; }
        public int Mass { get; set; }
        public Particle(Vector2 Position)
        {
            this.Position = Position;
        }
        public Particle(Vector2 Position, int Size)
        {
            this.Position = Position;
            this.Size = Size;
        }
        public bool Intersects(Particle particle)
        {
            var center0 = new Vector2(particle.Position.X, particle.Position.Y);
            var center1 = new Vector2(Position.X, Position.Y);
            return Vector2.Distance(center0, center1) < Size/2 + particle.Size/2;
        }
    }
}
