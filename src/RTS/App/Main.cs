using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RTS;

public class Main : Microsoft.Xna.Framework.Game
{
    //Global Vars

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    public static Viewport Dimensions;
    public static Point MIN_SIZE = new(640, 360);

    public static readonly EventBus EVENT_BUS = new();

    private readonly TextureManager _textureManager = new();
    private readonly ResourceManager _resourceManager;
    private readonly ScreenManager _screenManager = new();

    //Pipelines
    private readonly Pipeline<RegisteringEvent> _registryPipeline = new();
    private readonly Pipeline<TextureLoadingEvent> _texturePipeline = new();
    private readonly Pipeline<LoadingEvent> _loadingPipeline = new();

    private static GraphicsDevice _device;
    public static GraphicsDevice GetDevice() => _device;
    
    public Main()
    {
        Thread.CurrentThread.Name = "Main";
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        _resourceManager = new ResourceManager([
            new Pack("default", System.IO.Path.Combine(AppContext.BaseDirectory, Content.RootDirectory))
        ]);
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
        LoggerInfo info = new LoggerInfo();
        LoggerDebug debug = new LoggerDebug();
        LoggerWarn warn = new LoggerWarn();
        LoggerError error = new LoggerError();

        info.SetNextLogger(debug);
        debug.SetNextLogger(warn);
        warn.SetNextLogger(error);
        LogUtils.SetLogger(info);

        new Weighted<int>(1, 0);
        new Weighted<int>(1, 0);
        _device = GraphicsDevice;

        var subscribers = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsDefined(typeof(EventBusSubscriber), inherit: false));
        
        foreach (Type subscriber in subscribers)
            EVENT_BUS.Register(subscriber);

        _registryPipeline.WithStep(e => EVENT_BUS.Publish(e));

        _texturePipeline.WithStep(e => EVENT_BUS.Publish(e));
        _texturePipeline.WithStep(e => e.TextureManager.LoadContent(e.ResourceManager, GetDevice()));

        _loadingPipeline.WithStep(e => _registryPipeline.Run(
            new RegisteringEvent(e.content, e.ResourceManager, e.TextureManager, e.ScreenManager)));
        _loadingPipeline.WithStep(e => _texturePipeline.Run(
            new TextureLoadingEvent(e.content, e.ResourceManager, e.TextureManager, e.ScreenManager)));

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        _loadingPipeline.Run(new LoadingEvent(Content, _resourceManager, _textureManager, _screenManager));
    }

    protected override void Update(GameTime gameTime)
    {
        Dimensions = GraphicsDevice.Viewport;

        InputManager.Instance.Update();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        Scene.UpdateScene(gameTime);
        _screenManager.Update(gameTime);
        

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        Scene.DrawScene(gameTime, _spriteBatch);
        _screenManager.Draw(gameTime, _spriteBatch);


        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _textureManager.Dispose();
            _resourceManager.Dispose();
        }

        base.Dispose(disposing);
    }
}
