using Microsoft.Xna.Framework;

namespace Herif
{
    public class PipePair
    {
        public float X;
        public int Width;

        public Rectangle TopRect { get; private set; }
        public Rectangle BottomRect { get; private set; }

        public bool Scored { get; set; }

        public PipePair(int startX, int width, int topHeight, int bottomY, int bottomHeight)
        {
            X = startX;
            Width = width;

            TopRect = new Rectangle(startX, 0, width, topHeight);
            BottomRect = new Rectangle(startX, bottomY, width, bottomHeight);
        }

        public void Update(float dt, float speed)
        {
            X -= speed * dt;

            TopRect = new Rectangle((int)X, TopRect.Y, TopRect.Width, TopRect.Height);
            BottomRect = new Rectangle((int)X, BottomRect.Y, BottomRect.Width, BottomRect.Height);
        }
    }
}
