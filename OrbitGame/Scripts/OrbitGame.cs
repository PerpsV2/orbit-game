using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using qQEngine;

namespace OrbitGame;

public delegate void UpdateEventHandler(object? sender, UpdateEventArgs e);

public class UpdateEventArgs(SDecimal physicsTime) : EventArgs
{
    public SDecimal PhysicsTime = physicsTime;
}

public static class Effects
{
    public static Effect? DefaultEffect;
    public static Effect? CircleEffect;
    public static Effect? OrbitEffect;
    public static Effect? LightingEffect;
    public static Effect? GaussianBlurEffect;
}

public class OrbitGame : Game
{
    private static class GameState
    {
        /// <summary>
        /// In-game time used for physics calculations.
        /// </summary>
        public static qQEngine.SDecimal PhysicsTime = 0;
    
        /// <summary>
        /// In-game time step.
        /// </summary>
        public static qQEngine.SDecimal PhysicsTimeStep = Options.DefaultTimeStep;

        /// <summary>
        /// Real time since last update call.
        /// </summary>
        public static qQEngine.SDecimal DeltaRealTime = 0;

        /// <summary>
        /// Physics time since last update call.
        /// </summary>
        public static qQEngine.SDecimal DeltaPhysicsTime = 0;

        /// <summary>
        /// DateTime time at last update call.
        /// </summary>
        public static DateTime PreviousDateTime = DateTime.Now;
    
        /// <summary>
        /// Real time passed since game started.
        /// </summary>
        public static qQEngine.SDecimal RealTime = 0;

        /// <summary>
        /// Number of frames so far this second.
        /// </summary>
        public static int FrameCountThisSecond;
        
        /// <summary>
        /// Update loop FPS.
        /// </summary>
        public static int FramesPerSecond;
    }

    private SpriteBatch _spriteBatch;

#pragma warning disable CS8618, CS9264
    public OrbitGame()
#pragma warning restore CS8618, CS9264
    {
        GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);
        graphics.PreferredBackBufferWidth = Options.ScreenSize.width;
        graphics.PreferredBackBufferHeight = Options.ScreenSize.height;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1d / 180d);
    }
    
    public static IGraphicsHandler Graphics = new DebugGraphicsHandler();
    public static qQEngine.Camera Camera = new("Camera", new(Vec2.Zero, 0),
        Options.ScreenSize.width * Options.DefaultZoomScale,
        Options.ScreenSize.height * Options.DefaultZoomScale
    );

    public static List<qQEngine.Body> Bodies = new();

    public static SpriteFont DefaultFont;
    private readonly Random _rnd = new();

    public static event UpdateEventHandler? UpdateFrame;

    void UpdateFPS(object? state)
    {
        GameState.FramesPerSecond = GameState.FrameCountThisSecond;
        GameState.FrameCountThisSecond = 0;
    }

    protected override void Initialize()
    {
        Graphics = new GraphicsHandler(_spriteBatch);
        
        Timer frameTimer = new Timer(UpdateFPS, null, 0, 1000);

        #region Test Bodies

        qQEngine.Body testBody = new qQEngine.Body("Gwyneth", new qQEngine.SpatialInfo(
            position: new Vec2(),
            velocity: new Vec2()
        ));
        testBody.Luminosity = 10;
        testBody.Occluder = new CompoundOccluder();
        testBody.Occluder.Occluders.Add(new CircularOccluder(200, Vec2.Zero));
        testBody.Occluder.Occluders.Add(new CircularOccluder(1, new Vec2(1, 1)));
        testBody.Occluder.IsEmitter = true;

        #endregion
        
        Bodies.Add(testBody);
        Camera.Focus();

        base.Initialize();
    }

    private RenderTarget2D _occlusionMask;
    private RenderTarget2D _lightingMask;
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        Graphics = new GraphicsHandler(_spriteBatch);
        Matrix projection = Matrix.CreateOrthographicOffCenter(
            0, Options.ScreenSize.width, Options.ScreenSize.height, 0, 0, 1
        );
        Effects.DefaultEffect = Content.Load<Effect>("effects/defaultEffect");
        Effects.DefaultEffect.Parameters["Projection"].SetValue(projection);
        Effects.CircleEffect = Content.Load<Effect>("effects/circleEffect");
        Effects.CircleEffect.Parameters["Projection"].SetValue(projection);
        Effects.OrbitEffect = Content.Load<Effect>("effects/orbitEffect");
        Effects.OrbitEffect.Parameters["Projection"].SetValue(projection);
        Effects.LightingEffect = Content.Load<Effect>("effects/lightingEffect");
        Effects.LightingEffect.Parameters["Projection"].SetValue(projection);
        Effects.GaussianBlurEffect = Content.Load<Effect>("effects/gaussianBlurEffect");
        Effects.GaussianBlurEffect.Parameters["Projection"].SetValue(projection);

        _occlusionMask = new RenderTarget2D(GraphicsDevice, Options.ScreenSize.width, Options.ScreenSize.height);
        _lightingMask = new RenderTarget2D(GraphicsDevice, Options.ScreenSize.width, Options.ScreenSize.height, false, SurfaceFormat.Vector4, DepthFormat.None);
        
        DefaultFont = Content.Load<SpriteFont>("fonts/defaultFont");
    }
    
    KeyboardState _lastKeyboardState;

    private void HandleInput(qQEngine.SDecimal dt)
    {
        MouseHandler.HandleMouseEvents();
        
        qQEngine.SDecimal camSpeed = Camera.Height * Options.CamMoveSpeed * dt;
        qQEngine.SDecimal zoomSpeed = Options.CamZoomSpeed * dt;
        KeyboardState keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Options.FocusKey))
            if (_lastKeyboardState.IsKeyUp(Options.FocusKey))
                Camera.Focus();
        
        if (keyboardState.IsKeyDown(Options.TimeWarpUpKey))
            if (_lastKeyboardState.IsKeyUp(Options.TimeWarpUpKey))
                GameState.PhysicsTimeStep *= 10;
        
        if (keyboardState.IsKeyDown(Options.TimeWarpDownKey))
            if (_lastKeyboardState.IsKeyUp(Options.TimeWarpDownKey))
                GameState.PhysicsTimeStep /= 10;
        
        if (keyboardState.IsKeyDown(Options.MoveUpKey)) Camera.MoveParallel(camSpeed);
        if (keyboardState.IsKeyDown(Options.MoveDownKey)) Camera.MoveParallel(-camSpeed);
        if (keyboardState.IsKeyDown(Options.MoveLeftKey)) Camera.MovePerpendicular(-camSpeed);
        if (keyboardState.IsKeyDown(Options.MoveRightKey)) Camera.MovePerpendicular(camSpeed);

        if (keyboardState.IsKeyDown(Options.ZoomOutKey)) Camera.ScaleZoom(1 + zoomSpeed);
        if (keyboardState.IsKeyDown(Options.ZoomInKey)) Camera.ScaleZoom(1 - zoomSpeed);

        float camRotateSpeed = (float)(Options.CamRotateSpeed * dt);
        if (keyboardState.IsKeyDown(Options.RotateLeftKey)) Camera.RotateBy(-camRotateSpeed);
        if (keyboardState.IsKeyDown(Options.RotateRightKey)) Camera.RotateBy(camRotateSpeed);

        if (keyboardState.IsKeyDown(Keys.I)) Bodies[0].Occluder.Occluders[1].LocalPosition += new Vec2(0, 1) * dt;
        if (keyboardState.IsKeyDown(Keys.J)) Bodies[0].Occluder.Occluders[1].LocalPosition -= new Vec2(1, 0) * dt;
        if (keyboardState.IsKeyDown(Keys.K)) Bodies[0].Occluder.Occluders[1].LocalPosition -= new Vec2(0, 1) * dt;
        if (keyboardState.IsKeyDown(Keys.L)) Bodies[0].Occluder.Occluders[1].LocalPosition += new Vec2(1, 0) * dt;
        
        _lastKeyboardState = keyboardState;
    }

    protected override void Update(GameTime gameTime)
    {
        //UpdateFrame?.Invoke(this, new UpdateEventArgs(GameState.PhysicsTime));
        GameState.DeltaRealTime = (DateTime.Now - GameState.PreviousDateTime).TotalSeconds;
        GameState.PreviousDateTime = DateTime.Now;
        GameState.DeltaPhysicsTime = GameState.DeltaRealTime * GameState.PhysicsTimeStep;
        GameState.PhysicsTime += GameState.DeltaPhysicsTime;
        GameState.RealTime = gameTime.TotalGameTime.TotalSeconds;
        GameState.FrameCountThisSecond++;
        
        HandleInput(GameState.DeltaRealTime);
        
        OriginBody.ResetOrigin();
        Camera.Update();
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        RasterizerState rasterizerState = new RasterizerState();
        rasterizerState.CullMode = CullMode.None;
        GraphicsDevice.RasterizerState = rasterizerState;
        
        GraphicsDevice.SetRenderTarget(_occlusionMask);
        GraphicsDevice.Clear(Color.Transparent);
        _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp);
        foreach (var body in Bodies)
        {
            foreach (var occluder in body.Occluder.Occluders)
            {
                Graphics.DrawCircle(Camera.ConvertToScreenCoordinates(body.Position + occluder.LocalPosition),
                    Camera.ConvertToScreenDistance(occluder.Radius), Color.White);
            }
        }
        _spriteBatch.End();
        
        GraphicsDevice.SetRenderTarget(_lightingMask);
        GraphicsDevice.Clear(Color.Transparent);
        GraphicsDevice.RasterizerState = rasterizerState;
        _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp);
        foreach (var body in Bodies)
        {
            Graphics.DrawBody(Camera, body, _occlusionMask, Color.Black, Vec2.Zero);
        }
        _spriteBatch.End();
        
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Options.BackgroundColour);
        GraphicsDevice.RasterizerState = rasterizerState;
        _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp);
        if (Options.DisplayFPS)
            _spriteBatch.DrawString(DefaultFont, GameState.FramesPerSecond.ToString(), Vector2.Zero, Color.White);
        
        try
        {
            _spriteBatch.DrawString(DefaultFont, "Current date: " + new DateTime(2024, 12, 25)
                    .AddSeconds((double)GameState.PhysicsTime),
                new Vector2(0, 30), Color.White);
        }
        catch (ArgumentOutOfRangeException)
        {
            _spriteBatch.DrawString(DefaultFont, "Current date: >10000y A.D.", new Vector2(0, 30), Color.White);
        }
        
        _spriteBatch.DrawString(DefaultFont, GameState.PhysicsTimeStep.ToString(), new Vector2(0, 60), Color.White);
        
        Graphics.DrawScreenMesh(new Dictionary<string, object> {
            {"SpriteTexture", _lightingMask},
            {"TexelSize", new Vector2(1f / Options.ScreenSize.width, 1f / Options.ScreenSize.height)},
        }, Effects.GaussianBlurEffect);
        
        DrawDebug.Draw();
        DrawDebug.ClearBuffer();
        
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}