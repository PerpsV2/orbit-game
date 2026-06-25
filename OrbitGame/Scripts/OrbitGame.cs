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
}

public class OrbitGame : Game
{
    private static class GameState
    {
        /// <summary>
        /// In-game time used for physics calculations.
        /// </summary>
        public static SDecimal PhysicsTime = 0;
    
        /// <summary>
        /// In-game time step.
        /// </summary>
        public static SDecimal PhysicsTimeStep = Options.DefaultTimeStep;

        /// <summary>
        /// Real time since last update call.
        /// </summary>
        public static SDecimal DeltaRealTime = 0;

        /// <summary>
        /// Physics time since last update call.
        /// </summary>
        public static SDecimal DeltaPhysicsTime = 0;

        /// <summary>
        /// DateTime time at last update call.
        /// </summary>
        public static DateTime PreviousDateTime = DateTime.Now;
    
        /// <summary>
        /// Real time passed since game started.
        /// </summary>
        public static SDecimal RealTime = 0;

        /// <summary>
        /// Number of frames so far this second.
        /// </summary>
        public static int FrameCountThisSecond;
        
        /// <summary>
        /// Update loop FPS.
        /// </summary>
        public static int FramesPerSecond;
    }
    
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

#pragma warning disable CS8618, CS9264
    public OrbitGame()
#pragma warning restore CS8618, CS9264
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = Options.ScreenSize.width;
        _graphics.PreferredBackBufferHeight = Options.ScreenSize.height;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }
    
    public static IGraphicsHandler Graphics = new DebugGraphicsHandler();
    public static Camera Camera = new("Camera", new(Vec2<SDecimal>.Zero, 0),
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

        #endregion
        
        Bodies.Add(testBody);
        Camera.Focus();

        base.Initialize();
    }

    private ScreenMesh _screenMesh;

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
            
        _screenMesh = new ScreenMesh();
        _screenMesh.GenerateBuffers(GraphicsDevice);
        
        DefaultFont = Content.Load<SpriteFont>("fonts/defaultFont");
    }
    
    KeyboardState _lastKeyboardState;

    private void HandleInput(SDecimal dt)
    {
        MouseHandler.HandleMouseEvents();
        
        SDecimal camSpeed = Camera.Height * Options.CamMoveSpeed * dt;
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

        if (keyboardState.IsKeyDown(Options.ZoomOutKey)) Camera.ScaleZoom(1 + Options.CamZoomSpeed);
        if (keyboardState.IsKeyDown(Options.ZoomInKey)) Camera.ScaleZoom(1 - Options.CamZoomSpeed);

        float camRotateSpeed = (float)(Options.CamRotateSpeed * dt);
        if (keyboardState.IsKeyDown(Options.RotateLeftKey)) Camera.RotateBy(-camRotateSpeed);
        if (keyboardState.IsKeyDown(Options.RotateRightKey)) Camera.RotateBy(camRotateSpeed);
        
        _lastKeyboardState = keyboardState;
    }

    protected override void Update(GameTime gameTime)
    {
        UpdateFrame?.Invoke(this, new UpdateEventArgs(GameState.PhysicsTime));
        
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

        GraphicsDevice.Clear(Options.BackgroundColour);

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

        DrawDebug.Draw();
        DrawDebug.ClearBuffer();
        
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}