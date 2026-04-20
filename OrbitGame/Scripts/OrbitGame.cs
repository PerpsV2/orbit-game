using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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
        /// In-game time step final non-interpolated value.
        /// </summary>
        public static SDecimal GoalPhysicsTimeStep = Options.DefaultTimeStep;

        /// <summary>
        /// Time since last update call.
        /// </summary>
        public static SDecimal DeltaRealTime = 0;

        /// <summary>
        /// Physics time since last update call.
        /// </summary>
        public static SDecimal DeltaPhysicsTimeStep = 0;

        /// <summary>
        /// DateTime time at last update call.
        /// </summary>
        public static DateTime PreviousDateTime = DateTime.Now;
    
        /// <summary>
        /// Real time passed since game started.
        /// </summary>
        public static SDecimal RealTime = 0;

        /// <summary>
        /// Whether the game is in fast forward mode or not.
        /// </summary>
        public static bool IsFastForward;

        /// <summary>
        /// Number of frames so far this second.
        /// </summary>
        public static int FrameCountThisSecond;
        
        /// <summary>
        /// Update loop FPS.
        /// </summary>
        public static int FramesPerSecond;
        
        /// <summary>
        /// The current body the camera is tracking.
        /// </summary>
        public static Body Tracking;
        
        /// <summary>
        /// The index of the current tracking body.
        /// </summary>
        public static int TrackingIndex;
        
        /// <summary>
        /// The current player-controlled ship.
        /// </summary>
        public static Ship ControlShip;
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
    public static SpaceHierarchy Hierarchy = new([]);
    private CollisionHandler _collisionHandler;
    private Body[] Bodies => Hierarchy.GetObjectsOfType<Body>();
    private Planet[] Planets => Hierarchy.GetObjectsOfType<Planet>();
    private Ship[] Ships => Hierarchy.GetObjectsOfType<Ship>();

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
        
        #region Bodies
            
        Material planetMaterial = new Material(0.1f, 0.7f, 0.5f);
        Planet.PlanetTemplate planetTemplate = new Planet.PlanetTemplate(planetMaterial);
        Planet sun = planetTemplate.CreateInstance(
            "Sun", new(Vec2<SDecimal>.Zero), new SDecimal(1.989, 30), 
            new SDecimal(6.98340, 8), Color.White, null, 
            5005
        );
        Planet mercury = planetTemplate.CreateInstance("Mercury",
            new(new Vec2<SDecimal>(new SDecimal(-5.6940545, 10), new SDecimal(3.2977160, 9)),
                new Vec2<SDecimal>(new SDecimal(-1.2946428, 4), new SDecimal(-4.6540563, 4))), 
            new SDecimal(3.285, 23), new SDecimal(2.4397, 6), new Color(140, 140, 140, 255), sun, 
            1234
        );
        Planet venus = planetTemplate.CreateInstance("Venus", 
            new(new Vec2<SDecimal>(new SDecimal(8.2978939, 10), new SDecimal(6.9376114, 10)),
                new Vec2<SDecimal>(new SDecimal(-2.2569107, 4), new SDecimal(2.6718186, 4))),
            new SDecimal(4.867, 24), new SDecimal(6.0518, 6), new Color(230, 160, 40, 255), sun, 
            767
        );
        Planet earth = planetTemplate.CreateInstance("Earth", 
            new(new Vec2<SDecimal>(new SDecimal(-8.5613233, 8), new SDecimal(1.4688537, 11)),
                new Vec2<SDecimal>(new SDecimal(-3.0223357, 4), new SDecimal(-1.8447646, 3))),
            new SDecimal(5.9722, 24), new SDecimal(6.378, 6), new Color(100, 200, 255, 255), sun, 
            1
        );
        Planet moon = planetTemplate.CreateInstance("The Moon", 
            new(new Vec2<SDecimal>(new SDecimal(-3.6413936, 8), new SDecimal(-1.7481022, 8)),
                new Vec2<SDecimal>(new SDecimal( 4.2899598, 2), new SDecimal(-8.6413934, 2))),
            new SDecimal(7.349, 22), new SDecimal(1.737, 6), new Color(180, 180, 180, 255), earth,
            2
        );
        Planet mars = planetTemplate.CreateInstance("Mars", 
            new(new Vec2<SDecimal>(new SDecimal(-6.4603691, 10), new SDecimal( 2.3127019, 11)),
                new Vec2<SDecimal>(new SDecimal(-2.2420469, 4), new SDecimal(-4.6499686, 3))),
            new SDecimal(6.39, 23), new SDecimal(3.3895, 6), new Color(230, 60, 50, 255), sun,
            8625
        );
        Planet jupiter = planetTemplate.CreateInstance("Jupiter", 
            new(new Vec2<SDecimal>(new SDecimal(1.6580000, 11), new SDecimal(7.4166230, 11)),
                new Vec2<SDecimal>(new SDecimal(-1.2915655, 4), new SDecimal(3.4670152, 3))),
            new SDecimal(1.898, 27), new SDecimal(6.9911, 7), new Color(175, 125, 50, 255), sun,
            136394
        );
        Planet io = planetTemplate.CreateInstance("Io", 
            new(new Vec2<SDecimal>(new SDecimal(-2.4992437, 8), new SDecimal(-3.3952711, 8)),
                new Vec2<SDecimal>(new SDecimal(1.3919142, 4), new SDecimal(-1.0326546, 4))),
            new SDecimal(8.931938, 22), new SDecimal(1.8216, 6), new Color(195, 200, 40, 255), jupiter,
            10
        );
        Planet europa = planetTemplate.CreateInstance("Europa", 
            new(new Vec2<SDecimal>(new SDecimal(6.6134371, 8), new SDecimal(8.2686051, 7)),
                new Vec2<SDecimal>(new SDecimal(-1.6290520, 3), new SDecimal(1.3728682, 4))),
            new SDecimal(4.79984, 22), new SDecimal(1.5608, 6), new Color(255, 200, 200, 255), jupiter,
            80085
        );
        Planet ganymede = planetTemplate.CreateInstance("Ganymede", 
            new(new Vec2<SDecimal>(new SDecimal(1.0221162, 9), new SDecimal(-3.1228698, 8)),
                new Vec2<SDecimal>(new SDecimal(3.1804035, 3), new SDecimal(1.0413910, 4))),
            new SDecimal(1.4819, 23), new SDecimal(2.6341, 6), new Color(105, 105, 105, 255), jupiter,
            1672
        );
        Planet callisto = planetTemplate.CreateInstance("Callisto", 
            new(new Vec2<SDecimal>(new SDecimal(1.7988510, 9), new SDecimal(-5.1363150, 8)),
                new Vec2<SDecimal>(new SDecimal(2.2405056, 3), new SDecimal(7.9426031, 3))),
            new SDecimal(1.075938, 23), new SDecimal(2.4103, 6), new Color(130, 130, 95, 255), jupiter,
            4
        );
        Planet saturn = planetTemplate.CreateInstance("Saturn", 
            new(new Vec2<SDecimal>(new SDecimal( 1.4146019, 12), new SDecimal(-2.6971440, 11)),
                new Vec2<SDecimal>(new SDecimal( 1.2650097, 3), new SDecimal( 9.4749677, 3))),
            new SDecimal(5.683, 26), new SDecimal(5.8232, 7), new Color(150, 150, 80, 255), sun,
            1997
        );
        Planet uranus = planetTemplate.CreateInstance("Uranus", 
            new(new Vec2<SDecimal>(new SDecimal(1.6645067, 12), new SDecimal(2.4055482, 12)),
                new Vec2<SDecimal>(new SDecimal(-5.6626764, 3), new SDecimal(3.5634117, 3))),
            new SDecimal(8.681, 25), new SDecimal(2.5362, 7), new Color(170, 200, 255, 255), sun,
            1047
        );
        Planet neptune = planetTemplate.CreateInstance("Neptune", 
            new(new Vec2<SDecimal>(new SDecimal(4.4699311, 12), new SDecimal(-9.8183016, 10)),
                new Vec2<SDecimal>(new SDecimal(7.2829293, 1), new SDecimal(5.4729751, 3))),
            new SDecimal(1.024, 26), new SDecimal(2.4622, 7), new Color(100, 120, 200, 255), sun,
            0
        );
        Planet halley = planetTemplate.CreateInstance("Halley", 
            new(new Vec2<SDecimal>(new SDecimal(-2.9450469, 12), new SDecimal(4.0907881, 12)),
                new Vec2<SDecimal>(new SDecimal(8.0919083, 2), new SDecimal(8.0919083, 2))),
            new SDecimal(2.2, 14), new SDecimal(5.5, 3), new Color(200, 100, 200, 255), sun,
            1313
        );
            
        Material shipMaterial = new Material(0.1f, 0.7f, 0.5f);
        Ship.ShipTemplate smokestackTemplate = new Ship.ShipTemplate([
            new Vec2Double(0.4, 0.4),
            new Vec2Double(0.4, -0.3),
            new Vec2Double(-0.2, -0.5),
            new Vec2Double(-5, 0),
            new Vec2Double(-0.3, 0.5)
        ], shipMaterial);
        for (int i = 0; i < 30; i++)
        {
             double randomAngle = _rnd.NextDouble() * 0.001;
             int randomDirection = _rnd.Next(0, 1) * 2 - 1;
             SDecimal randomAltitude = (_rnd.NextDouble() * 0.001 + 40) * new SDecimal(6.378, 6);
             SDecimal randomSpeed = SDecimal.Sqrt(earth.Mass * Constants.G / randomAltitude);
             Ship smokestack = smokestackTemplate.CreateInstance("Smokestack " + i, 
                 new(Vec2<SDecimal>.FromPolar(randomAngle, randomAltitude), 
                     Vec2<SDecimal>.FromPolar(randomAngle + 1 * Math.PI / 2, randomSpeed * randomDirection + _rnd.Next(-50, 50))), 
                 1000, new Color(0, 255, 0, 255), earth);
             smokestack.DrawOrbitalPath = false;
             smokestack.MouseDetectionEnabled = false;
        }
        
        Ship.ShipTemplate strawhatTemplate = new Ship.ShipTemplate([
            new Vec2Double(1, 1),
            new Vec2Double(1, -0.75),
            new Vec2Double(-0.5, -1.25),
            new Vec2Double(-1, 0),
            new Vec2Double(-0.75, 1.25)
        ], shipMaterial);
        Ship strawhat = strawhatTemplate.CreateInstance("Strawhat", new SpatialInfo(
            new Vec2<SDecimal>(40 * new SDecimal(6.378, 6), 0), new Vec2<SDecimal>(0, -SDecimal.Sqrt(earth.Mass * Constants.G / 40 / new SDecimal(6.378, 6))), Math.PI / 2), 
            1000, new Color(255, 0, 0, 255), earth);
        strawhat.DrawOrbitalPath = true;
        
        #endregion

        #region Test Bodies
        
        /*Planet manatee = planetTemplate.CreateInstance("Manatee", new SpatialInfo(DVector2<SDecimal>.Zero), 5000000000000, 30, 
            new Color(125, 150, 130, 255), null);
            
        Ship.ShipTemplate shipTemplate2 = new Ship.ShipTemplate(Utils.CenterConvex([
            new(3, 3),
            new(6, -3),
            new(-3, -3),
            new(-6, 3)
        ]), shipMaterial);
        for (int i = 0; i < 10; ++i)
        {
            DVector2<SDecimal> randomPosition = DVector2<SDecimal>.FromPolar(_rnd.NextDouble() * Math.PI / 8, _rnd.Next(100, 120));
            int randomColour = _rnd.Next(200, 255);
            Ship chimneyPipe = shipTemplate2.CreateInstance("Chimneypipe " + i,
                new SpatialInfo(randomPosition), 250, new Color(120, 200, randomColour, 255), manatee);
            Bodies.Add(chimneyPipe);
        }*/
        
        #endregion

        foreach (var planet in Planets) planet.GenerateOrbitPath(GameState.PhysicsTime);
        OriginBody.Body = Bodies[^1];
        GameState.Tracking = OriginBody.Body;
        Camera.MovementScheme = new TrackingCameraScheme(OriginBody.Body.SpatialInfo, OriginBody.Body);
        Camera.Focus();
        GameState.ControlShip = Ships[^1];
        GameState.ControlShip.DrawOrbitalPath = true;
        GameState.TrackingIndex = Array.IndexOf(Bodies, OriginBody.Body);
        _collisionHandler = new CollisionHandler();
        /*Bodies, new() {
            {(typeof(Ship), typeof(Planet)), (r, i) => 
                CollisionHandler.RestShipPlanetCollision(r, i, GameState.PhysicsTimeStep, GameState.DeltaPhysicsTimeStep)},
            {(typeof(Ship), typeof(Ship)), CollisionHandler.ResolvePhysicsCollision},
            //{(typeof(Ship), typeof(Planet)), CollisionHandler.BodyPlanetTerrainCollision}
        });*/

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
        
        /*PropertyInfo[] effects = typeof(Effects).GetProperties();
        foreach (var property in effects)
        {
            Effect effect = property.GetValue(null) as Effect ?? 
                            throw new Exception("Property in static Effects class is not an Effect");
            effect.Parameters["Projection"].SetValue(projection);
        }*/
        
        DefaultFont = Content.Load<SpriteFont>("fonts/defaultFont");
    }

    private void TrackBody(int index)
    {
        GameState.TrackingIndex = (int)Utils.UnsignedMod(index, Bodies.Length);
        GameState.Tracking = Bodies[GameState.TrackingIndex];
        Camera.MovementScheme = new TrackingCameraScheme(Camera.SpatialInfo, GameState.Tracking);
    }

    private void FastForwardToSelected()
    {
        foreach (var ship in Ships)
            ship.GenerateOrbitPath(GameState.PhysicsTime);
        SDecimal endTime = ConicPath.SelectedPoint?.GetNextTime(GameState.PhysicsTime) ?? 0;
        if (endTime <= GameState.PhysicsTime) return;
        GameState.IsFastForward = true;
        SDecimal multiplier = new SDecimal((endTime - GameState.PhysicsTime).Exponent);
        InterpolationHandler<SDecimal>.CreateInterpolation(
            new(independentGetter: () => GameState.RealTime, dependentSetter: val => { GameState.PhysicsTimeStep = val; }),
            GameState.RealTime, GameState.RealTime + Options.EaseFastForwardStartTime, 
            GameState.GoalPhysicsTimeStep, GameState.GoalPhysicsTimeStep * multiplier);
        var endTimeWarp = InterpolationHandler<SDecimal>.CreateInterpolation(
            new(independentGetter: () => GameState.PhysicsTime, dependentSetter: val => { GameState.PhysicsTimeStep = val; }), 
            endTime - GameState.GoalPhysicsTimeStep * multiplier * Options.EaseFastForwardEndTime, endTime, 
            GameState.GoalPhysicsTimeStep * multiplier, GameState.GoalPhysicsTimeStep);
        endTimeWarp.InterpolationEnd += (_, _) =>
        {
            GameState.IsFastForward = false; 
        };
    }

    private void IncreaseTimeStep(SDecimal multiplier)
    {
        InterpolationHandler<SDecimal>.CreateInterpolation(
            new(() => GameState.RealTime, val => { GameState.PhysicsTimeStep = val; }),
            GameState.RealTime, GameState.RealTime + Options.EaseTimeStepChangeTime, 
            GameState.GoalPhysicsTimeStep, GameState.GoalPhysicsTimeStep * multiplier);
        GameState.GoalPhysicsTimeStep *= multiplier;
    }

    private void DecreaseTimeStep(SDecimal multiplier)
        => IncreaseTimeStep(1 / multiplier);

    private void ToggleCameraMovementScheme()
    {
        switch (Camera.MovementScheme)
        {
            case TrackingCameraScheme:
                Camera.MovementScheme = new TrackingFixedCameraScheme(Camera.SpatialInfo, GameState.Tracking);
                break;
            case TrackingFixedCameraScheme:
                if (GameState.Tracking.Parent == null)
                    Camera.MovementScheme = new TrackingCameraScheme(Camera.SpatialInfo, GameState.Tracking);
                else Camera.MovementScheme = new SurfaceCameraScheme(Camera.SpatialInfo, 
                    GameState.Tracking.Parent, GameState.Tracking);
                break;
            case SurfaceCameraScheme:
                Camera.MovementScheme = new TrackingCameraScheme(Camera.SpatialInfo, GameState.Tracking);
                break;
            default:
                throw new Exception("Unrecognized camera movement scheme");
        }
    }

    private void CreateManeuverNode()
    {
        if (ConicPath.SelectedPoint == null) return;
        KeplerOrbitPoint point = ConicPath.SelectedPoint.Value;
        Body? selectedPointBody = point.ConicPath.Orbit?.Body;
        if (selectedPointBody is Ship ship)
        {
            ship.OrbitPath.AddManeuverNode(point, new Vec2<SDecimal>(0, 0), GameState.PhysicsTime);
        }
    }
    
    KeyboardState _lastKeyboardState;

    private void HandleInput(SDecimal dt)
    {
        MouseHandler.HandleMouseEvents();
        
        SDecimal camSpeed = Camera.Height * Options.CamMoveSpeed * dt;
        KeyboardState keyboardState = Keyboard.GetState();
        if (keyboardState.IsKeyDown(Options.TimeWarpUpKey))
            if (_lastKeyboardState.IsKeyUp(Options.TimeWarpUpKey))
                IncreaseTimeStep(Options.TimeWarpStepMultiplier);
        if (keyboardState.IsKeyDown(Options.TimeWarpDownKey))
            if (_lastKeyboardState.IsKeyUp(Options.TimeWarpDownKey))
                DecreaseTimeStep(Options.TimeWarpStepMultiplier);

        if (keyboardState.IsKeyDown(Options.TrackNextBodyKey))
            if (_lastKeyboardState.IsKeyUp(Options.TrackNextBodyKey))
                TrackBody(GameState.TrackingIndex + 1);
        if (keyboardState.IsKeyDown(Options.TrackPrevBodyKey))
            if (_lastKeyboardState.IsKeyUp(Options.TrackPrevBodyKey))
                TrackBody(GameState.TrackingIndex - 1);
        
        if (keyboardState.IsKeyDown(Options.FastForwardKey))
            if (_lastKeyboardState.IsKeyUp(Options.FastForwardKey))
                FastForwardToSelected();
        
        if (keyboardState.IsKeyDown(Keys.D5))
            if (_lastKeyboardState.IsKeyUp(Keys.D5))
                GameState.IsFastForward = true;

        if (keyboardState.IsKeyDown(Options.FocusKey))
            if (_lastKeyboardState.IsKeyUp(Options.FocusKey))
                Camera.Focus();
        
        if (keyboardState.IsKeyDown(Options.ChangeCameraSchemeKey))
            if (_lastKeyboardState.IsKeyUp(Options.ChangeCameraSchemeKey))
                ToggleCameraMovementScheme();
        
        if (keyboardState.IsKeyDown(Keys.Z))
            if (_lastKeyboardState.IsKeyUp(Keys.Z))
                CreateManeuverNode();

        if (keyboardState.IsKeyDown(Options.MoveUpKey)) Camera.MoveParallel(camSpeed);
        if (keyboardState.IsKeyDown(Options.MoveDownKey)) Camera.MoveParallel(-camSpeed);
        if (keyboardState.IsKeyDown(Options.MoveLeftKey)) Camera.MovePerpendicular(-camSpeed);
        if (keyboardState.IsKeyDown(Options.MoveRightKey)) Camera.MovePerpendicular(camSpeed);

        if (keyboardState.IsKeyDown(Options.ZoomOutKey)) Camera.ScaleZoom(1 + Options.CamZoomSpeed);
        if (keyboardState.IsKeyDown(Options.ZoomInKey)) Camera.ScaleZoom(1 - Options.CamZoomSpeed);

        float camRotateSpeed = (float)(Options.CamRotateSpeed * dt);
        if (keyboardState.IsKeyDown(Options.RotateLeftKey)) Camera.RotateBy(-camRotateSpeed);
        if (keyboardState.IsKeyDown(Options.RotateRightKey)) Camera.RotateBy(camRotateSpeed);

        if (keyboardState.IsKeyDown(Keys.I)) GameState.ControlShip.ApplyThrust(
            new Vec2<SDecimal>(-10000, 0), new Vec2<SDecimal>(-0.4, 0));
        if (keyboardState.IsKeyDown(Keys.D8)) GameState.ControlShip.ApplyThrust(
            new Vec2<SDecimal>(-100000, 0), new Vec2<SDecimal>(-0.4, 0));
        if (keyboardState.IsKeyDown(Keys.J)) GameState.ControlShip.ApplyThrust(
            new Vec2<SDecimal>(10000, 0), new Vec2<SDecimal>(-0.4, 0.1));
        if (keyboardState.IsKeyDown(Keys.L)) GameState.ControlShip.ApplyThrust(
            new Vec2<SDecimal>(10000, 0), new Vec2<SDecimal>(-0.4, -0.1));
        
        _lastKeyboardState = keyboardState;
    }

    protected override void Update(GameTime gameTime)
    {
        UpdateFrame?.Invoke(this, new UpdateEventArgs(GameState.PhysicsTime));
        
        InterpolationHandler<SDecimal>.UpdateInterpolationValues();
        
        GameState.DeltaRealTime = (DateTime.Now - GameState.PreviousDateTime).TotalSeconds;
        GameState.PreviousDateTime = DateTime.Now;
        GameState.DeltaPhysicsTimeStep = GameState.DeltaRealTime * GameState.PhysicsTimeStep;
        GameState.PhysicsTime += GameState.DeltaPhysicsTimeStep;
        GameState.RealTime = gameTime.TotalGameTime.TotalSeconds;
        GameState.FrameCountThisSecond++;
        
        HandleInput(GameState.DeltaRealTime);
        
        if (Options.EnablePhysics)
        {
            List<Task> tasks = new List<Task>();

            foreach (var planet in Planets)
            {
                planet.UpdatePosition_Kepler(GameState.PhysicsTime, GameState.DeltaPhysicsTimeStep);
            }

            if (!GameState.IsFastForward)
            {
                foreach (var ship in Ships)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        ship.UpdatePosition_Integrator(GameState.DeltaPhysicsTimeStep, Options.IntegratorMethod,
                            ship.CalculateNetAcceleration); 
                        ship.GenerateOrbitPath(GameState.PhysicsTime);
                    }));
                }
            }
            else
            {
                foreach (var ship in Ships)
                {
                    ship.UpdatePosition_Kepler(GameState.PhysicsTime, GameState.DeltaPhysicsTimeStep);
                }
            }

            Task.WaitAll(tasks.ToArray());

            //Hierarchy.ReconstructTree();
            
            //_collisionHandler.ResolveCollisions();

            foreach (var ship in Ships)
                if (ship.LandingState != null)
                    ship.UpdatePosition_Landed();
        }
        
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
        _spriteBatch.DrawString(DefaultFont, GameState.IsFastForward.ToString(), new Vector2(0, 90), Color.White);

        foreach (var ship in Ships) ship.Draw();
        foreach (var planet in Planets) planet.Draw();

        DrawDebug.Draw();
        DrawDebug.ClearBuffer();
        
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}