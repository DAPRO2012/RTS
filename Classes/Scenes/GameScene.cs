#nullable enable

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RTS;

public class GameScene(ContentManager content, TextureManager textureManager, ScreenManager screenManager) : Scene(content, screenManager)
{
    Level lvl = new("test");

    public Camera Camera = new(null);
    public Vector2 CameraPos = Vector2.Zero;
    public float CameraSpeed = 1000f;

    private readonly TextureManager _textureManager = textureManager;

    public override void Initialize(ContentManager content) {}

    public override void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (InputManager.Instance.MoveForward.IsDown())
            CameraPos.Y -= CameraSpeed * deltaTime;
        if (InputManager.Instance.MoveBackward.IsDown())
            CameraPos.Y += CameraSpeed * deltaTime;
        if (InputManager.Instance.MoveLeft.IsDown())
            CameraPos.X -= CameraSpeed * deltaTime;
        if (InputManager.Instance.MoveRight.IsDown())
            CameraPos.X += CameraSpeed * deltaTime;

        Camera.Update(gameTime, CameraPos, Main.Dimensions);

        float chunkWorldSize = Chunk.TileSize * Chunk.ChunkSize;
        float halfViewWidth = Main.Dimensions.Width / (2f * Camera.Scale);
        float halfViewHeight = Main.Dimensions.Height / (2f * Camera.Scale);
        const int chunkBuffer = 1;
        int minChunkX = (int)Math.Floor((Camera.Position.X - halfViewWidth) / chunkWorldSize) - chunkBuffer;
        int maxChunkX = (int)Math.Floor((Camera.Position.X + halfViewWidth) / chunkWorldSize) + chunkBuffer;
        int minChunkY = (int)Math.Floor((Camera.Position.Y - halfViewHeight) / chunkWorldSize) - chunkBuffer;
        int maxChunkY = (int)Math.Floor((Camera.Position.Y + halfViewHeight) / chunkWorldSize) + chunkBuffer;

        List<Point> loadPoints = [];
        for (int x = minChunkX; x <= maxChunkX; x++)
        for (int y = minChunkY; y <= maxChunkY; y++)
            loadPoints.Add(new Point(x, y));

        foreach (var point in new List<Point>(lvl.LoadedChunks.Keys))
            if (!loadPoints.Contains(point)) lvl.UnloadChunk(point);
        loadPoints.ForEach(point => lvl.LoadChunk(point));
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        spriteBatch.GraphicsDevice.Clear(Color.White);

        lvl.BuildCache(spriteBatch, _textureManager);
        spriteBatch.Begin(transformMatrix:Camera.ScaleMatrix * Camera.TranslationMatrix);
            lvl.Draw(spriteBatch, _textureManager);
        spriteBatch.End();
    }
}