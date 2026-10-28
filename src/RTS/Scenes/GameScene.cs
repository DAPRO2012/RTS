#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RTS;

public class GameScene(ContentManager content, TextureManager textureManager, ScreenManager screenManager) : Scene(content, screenManager)
{
    ClientLevel clientLevel = new();
    ServerLevel serverLevel = new("test");

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

        float chunkWorldSize = IChunk.TileSize * IChunk.ChunkSize;
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

        var serverChunks = serverLevel.GetChunks();
        var clientChunks = clientLevel.GetChunks();

        foreach (var point in new List<Point>(serverChunks.Keys))
        if (!loadPoints.Contains(point))
        {
            serverLevel.UnloadChunk(point);
            clientLevel.RemoveChunk(point);
        }
        foreach (Point point in loadPoints)
        if (!serverLevel.IsLoaded(point))
        {
            serverLevel.LoadChunk(point);
            clientLevel.SetChunk(point, serverLevel.GetChunk(point).GenerateSnapshot());
        }

        /*
        foreach (var chunkPoint in clientChunks)
        if (chunkPoint.Value.IsChanged)
            serverLevel.GetChunk(chunkPoint.Key).UpdateWithSnapshot(chunkPoint.Value.GenerateSnapshot());

        foreach (var chunkPoint in clientChunks)
        if (chunkPoint.Value.IsChanged)
            clientLevel.GetChunk(chunkPoint.Key).UpdateWithSnapshot(chunkPoint.Value.GenerateSnapshot());
        */
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        spriteBatch.GraphicsDevice.Clear(Color.White);

        clientLevel.BuildCache(spriteBatch, _textureManager);
        spriteBatch.Begin(SpriteSortMode.BackToFront, transformMatrix:Camera.ScaleMatrix * Camera.TranslationMatrix);
            clientLevel.Draw(spriteBatch, _textureManager);
        spriteBatch.End();
    }
}