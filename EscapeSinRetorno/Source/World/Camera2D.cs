using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
public class Camera2D
{
    private Vector2 _position;
    private readonly Viewport _viewport;
    private float _zoom = 1.0f;

    public Camera2D(Viewport viewport)
    {
        _viewport = viewport;
        _position = Vector2.Zero;
    }

    public void SetZoom(float zoom) => _zoom = MathHelper.Clamp(zoom, 0.5f, 3f);

    public Vector2 GetPosition() => _position;

    public void Follow(Vector2 target, int screenWidth, int screenHeight)
    {
        _position = target - new Vector2(screenWidth / 2f / _zoom, screenHeight / 2f / _zoom);
    }

    public Matrix GetTransform()
    {
        return Matrix.CreateTranslation(new Vector3(-_position, 0f)) *
               Matrix.CreateScale(_zoom, _zoom, 1f);
    }
}