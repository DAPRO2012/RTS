using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RTS;

public class Main : Game
{
    //Global Vars

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    public static Viewport Dimensions;
    public static Point MIN_SIZE = new(640, 360);

    public static readonly EventBus EVENT_BUS = new();
    public static readonly KeyboardManager KeyboardManager = new();
    public static readonly MouseManager MouseManager = new();

    private readonly TextureManager _textureManager;

    private readonly SavedStorage _storage = new(new(AppContext.BaseDirectory, "Content"));
    private static Path assetPath = new("resources", "assets");

    //Pipelines
    private readonly Pipeline<RegisteringEvent> _registryPipeline = new();
    private readonly Pipeline<TextureLoadingEvent> _texturePipeline = new();
    private readonly Pipeline<LoadingEvent> _loadingPipeline = new();

    private static GraphicsDevice _device;
    public static GraphicsDevice GetDevice() => _device;
    
    public Main()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += (sender, e) =>
        {
            Viewport viewport = GraphicsDevice.Viewport;
            bool apply = false;
            if (viewport.Width < MIN_SIZE.X)
            {
                _graphics.PreferredBackBufferWidth = MIN_SIZE.X;
                apply = true;
            }
            if (viewport.Height < MIN_SIZE.Y)
            {
                _graphics.PreferredBackBufferHeight = MIN_SIZE.Y;
                apply = true;
            }
            if (apply) _graphics.ApplyChanges();
        };
    }

    protected override void Initialize()
    {
        _device = GraphicsDevice;

        var subscribers = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsDefined(typeof(EventBusSubscriber), inherit: false));
        
        foreach (Type subscriber in subscribers)
        {
            EVENT_BUS.Register(subscriber);
        }

        _registryPipeline.WithStep(e => EVENT_BUS.Publish(e));

        _texturePipeline.WithStep(e => EVENT_BUS.Publish(e));
        _texturePipeline.WithStep(e =>
        {
            Func<string, string[]> searchDir = (p) => [];
            searchDir = path => {
                List<string> outputPaths = [];

                outputPaths.AddRange(Directory.GetFiles(path));
                foreach (string dirPath in Directory.GetDirectories(path))
                    outputPaths.AddRange(searchDir(dirPath));

                return [.. outputPaths];
            };

            string fullAssetPath = Path.Combine(_storage.Path, assetPath).GetPath();
            string[] paths = searchDir(fullAssetPath);
            Dictionary<Identifier, Dictionary<Identifier, Texture>> textures = [];

            foreach (string path in paths)
            {
                string fileRelativePath = System.IO.Path.GetRelativePath(fullAssetPath, path);
                string filePath = System.IO.Path.ChangeExtension(fileRelativePath, null);
            }
        });

        _loadingPipeline.WithStep(e => _registryPipeline.Run(new RegisteringEvent(e.content)));
        _loadingPipeline.WithStep(e => _texturePipeline.Run(new TextureLoadingEvent(e.content, _textureManager)));

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        EVENT_BUS.Publish(new RegisteringEvent(Content));
    }

    protected override void Update(GameTime gameTime)
    {
        Dimensions = GraphicsDevice.Viewport;

        KeyboardManager.Update(gameTime);
        MouseManager.Update(gameTime);

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        Scene.UpdateScene(gameTime);
        

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        // TODO: Add your drawing code here
        Scene.DrawScene(gameTime, _spriteBatch);

        base.Draw(gameTime);
    }
}
