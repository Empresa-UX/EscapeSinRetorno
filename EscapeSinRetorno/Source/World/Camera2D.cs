using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EscapeSinRetorno.Source.World
{
    public class Camera2D
    {
        private Vector2 _position;
        private readonly Viewport _viewport;
        public Vector2 GetPosition() => _position;

        public Camera2D(Viewport viewport)
        {
            _viewport = viewport;
            _position = Vector2.Zero;
        }

        public void Follow(Vector2 target, int screenWidth, int screenHeight)
        {
            _position = target - new Vector2(screenWidth / 2f, screenHeight / 2f);
        }

        public Matrix GetTransform()
        {
            return Matrix.CreateTranslation(new Vector3(-_position, 0));
        }
    }
}
