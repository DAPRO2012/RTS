using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Camera(Vector2? position)
{
    private Vector2 _position = position ?? Vector2.Zero;
    public Vector2 Position => _position;
    public Matrix TranslationMatrix { get; private set; }
    public Matrix ScaleMatrix { get; private set; }
    public float Speed = 10f;
    public float Scale = 1f;

    public void Update(GameTime gt, Vector2 newPosition, Viewport viewport)
    {
        float deltaTime = (float)gt.ElapsedGameTime.TotalSeconds;
        
        float lerpAmount = MathHelper.Clamp(Speed * deltaTime, 0f, 1f);
        _position = Vector2.Lerp(_position, newPosition, lerpAmount);

        TranslationMatrix = 
            Matrix.CreateTranslation(new Vector3(-_position.X, -_position.Y, 0)) *
            Matrix.CreateScale(Scale) *
            Matrix.CreateTranslation(new Vector3(viewport.Width * 0.5f, viewport.Height * 0.5f, 0));

        ScaleMatrix = Matrix.Identity;
    }
}