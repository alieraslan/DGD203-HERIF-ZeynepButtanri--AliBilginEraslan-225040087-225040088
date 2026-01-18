using Microsoft.Xna.Framework;

namespace Herif
{
    public class Bird
    {
        public Vector2 Position;
        public Vector2 Velocity;

        public readonly Point Size;

        // 1080p için biraz daha “ağır” fizik (daha iyi his)
        private const float Gravity = 2200f;
        private const float FlapVelocity = -720f;
        private const float MaxFallSpeed = 1400f;

        public Bird(Vector2 startPos, Point size)
        {
            Position = startPos;
            Size = size;
            Velocity = Vector2.Zero;
        }

        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, Size.X, Size.Y);

        public void Update(float dt)
        {
            Velocity.Y += Gravity * dt;
            if (Velocity.Y > MaxFallSpeed) Velocity.Y = MaxFallSpeed;

            Position += Velocity * dt;
        }

        public void Flap()
        {
            Velocity.Y = FlapVelocity;
        }

        public void ClampTop()
        {
            Position.Y = 0;
            if (Velocity.Y < 0) Velocity.Y = 0;
        }
    }
}
