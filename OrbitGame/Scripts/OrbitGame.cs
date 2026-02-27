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

public delegate void UpdateEventHandler(object? sender, EventArgs e);

public static class Effects
{
    public static Effect? DefaultEffect;
    public static Effect? CircleEffect;
}

public class OrbitGame : Game
{
    private static class GameState
    {
        /// <summary>
        /// In-game time used for physics calculations.
        /// </summary>
        public static ScientificDecimal PhysicsTime = 0;
    
        /// <summary>
        /// In-game time step.
        /// </summary>
        public static ScientificDecimal PhysicsTimeStep = Options.DefaultTimeStep;

        /// <summary>
        /// In-game time step final non-interpolated value.
        /// </summary>
        public static ScientificDecimal GoalPhysicsTimeStep = Options.DefaultTimeStep;

        /// <summary>
        /// Time since last update call.
        /// </summary>
        public static ScientificDecimal DeltaRealTime = 0;

        /// <summary>
        /// Physics time since last update call.
        /// </summary>
        public static ScientificDecimal DeltaPhysicsTimeStep = 0;

        /// <summary>
        /// DateTime time at last update call.
        /// </summary>
        public static DateTime PreviousDateTime = DateTime.Now;
    
        /// <summary>
        /// Real time passed since game started.
        /// </summary>
        public static ScientificDecimal RealTime = 0;

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

    public static List<Body> Bodies = [];
    public static List<Planet> Planets = [];
    public static List<Ship> Ships = [];
    public static GraphicsDevice? Graphics;
    public static Camera Camera = new("Camera", new(SD_Vector2.Zero, 0),
        Options.ScreenSize.width * Options.DefaultZoomScale,
        Options.ScreenSize.height * Options.DefaultZoomScale
    );
    CollisionHandler _collisionHandler;

    private SpriteFont _font;
    private readonly Random _rnd = new();

    public static event UpdateEventHandler? UpdateFrame;

    void UpdateFPS(object? state)
    {
        GameState.FramesPerSecond = GameState.FrameCountThisSecond;
        GameState.FrameCountThisSecond = 0;
    }

    protected override void Initialize()
    {
        Graphics = _graphics.GraphicsDevice;
        
        Timer frameTimer = new Timer(UpdateFPS, null, 0, 1000);
        
        #region Bodies
            
        Material planetMaterial = new Material(0.1f, 0.7f, 0.5f);
        Planet.PlanetTemplate planetTemplate = new Planet.PlanetTemplate(planetMaterial);
        Planet sun = planetTemplate.CreateInstance(
            "Sun", new(SD_Vector2.Zero), new ScientificDecimal(1.989, 30), 
            new ScientificDecimal(6.98340, 8), Color.White, null
        );
        Planet mercury = planetTemplate.CreateInstance("Mercury",
            new(new SD_Vector2(new ScientificDecimal(-5.6940545, 10), new ScientificDecimal(3.2977160, 9)),
                new SD_Vector2(new ScientificDecimal(-1.2946428, 4), new ScientificDecimal(-4.6540563, 4))), 
            new ScientificDecimal(3.285, 23), new ScientificDecimal(2.4397, 6), new Color(140, 140, 140, 255), sun
        );
        Planet venus = planetTemplate.CreateInstance("Venus", 
            new(new SD_Vector2(new ScientificDecimal(8.2978939, 10), new ScientificDecimal(6.9376114, 10)),
                new SD_Vector2(new ScientificDecimal(-2.2569107, 4), new ScientificDecimal(2.6718186, 4))),
            new ScientificDecimal(4.867, 24), new ScientificDecimal(6.0518, 6), new Color(230, 160, 40, 255), sun
        );
        Planet earth = planetTemplate.CreateInstance("Earth", 
            new(new SD_Vector2(new ScientificDecimal(-8.5613233, 8), new ScientificDecimal(1.4688537, 11)),
                new SD_Vector2(new ScientificDecimal(-3.0223357, 4), new ScientificDecimal(-1.8447646, 3))),
            new ScientificDecimal(5.9722, 24), new ScientificDecimal(6.378, 6), new Color(100, 200, 255, 255), sun
        );
        Planet moon = planetTemplate.CreateInstance("The Moon", 
            new(new SD_Vector2(new ScientificDecimal(-3.6413936, 8), new ScientificDecimal(-1.7481022, 8)),
                new SD_Vector2(new ScientificDecimal( 4.2899598, 2), new ScientificDecimal(-8.6413934, 2))),
            new ScientificDecimal(7.349, 22), new ScientificDecimal(1.737, 6), new Color(180, 180, 180, 255), earth
        );
        Planet mars = planetTemplate.CreateInstance("Mars", 
            new(new SD_Vector2(new ScientificDecimal(-6.4603691, 10), new ScientificDecimal( 2.3127019, 11)),
                new SD_Vector2(new ScientificDecimal(-2.2420469, 4), new ScientificDecimal(-4.6499686, 3))),
            new ScientificDecimal(6.39, 23), new ScientificDecimal(3.3895, 6), new Color(230, 60, 50, 255), sun
        );
        Planet jupiter = planetTemplate.CreateInstance("Jupiter", 
            new(new SD_Vector2(new ScientificDecimal(1.6580000, 11), new ScientificDecimal(7.4166230, 11)),
                new SD_Vector2(new ScientificDecimal(-1.2915655, 4), new ScientificDecimal(3.4670152, 3))),
            new ScientificDecimal(1.898, 27), new ScientificDecimal(6.9911, 7), new Color(175, 125, 50, 255), sun
        );
        Planet io = planetTemplate.CreateInstance("Io", 
            new(new SD_Vector2(new ScientificDecimal(-2.4992437, 8), new ScientificDecimal(-3.3952711, 8)),
                new SD_Vector2(new ScientificDecimal(1.3919142, 4), new ScientificDecimal(-1.0326546, 4))),
            new ScientificDecimal(8.931938, 22), new ScientificDecimal(1.8216, 6), new Color(195, 200, 40, 255), jupiter
        );
        Planet europa = planetTemplate.CreateInstance("Europa", 
            new(new SD_Vector2(new ScientificDecimal(6.6134371, 8), new ScientificDecimal(8.2686051, 7)),
                new SD_Vector2(new ScientificDecimal(-1.6290520, 3), new ScientificDecimal(1.3728682, 4))),
            new ScientificDecimal(4.79984, 22), new ScientificDecimal(1.5608, 6), new Color(255, 200, 200, 255), jupiter
        );
        Planet ganymede = planetTemplate.CreateInstance("Ganymede", 
            new(new SD_Vector2(new ScientificDecimal(1.0221162, 9), new ScientificDecimal(-3.1228698, 8)),
                new SD_Vector2(new ScientificDecimal(3.1804035, 3), new ScientificDecimal(1.0413910, 4))),
            new ScientificDecimal(1.4819, 23), new ScientificDecimal(2.6341, 6), new Color(105, 105, 105, 255), jupiter
        );
        Planet callisto = planetTemplate.CreateInstance("Callisto", 
            new(new SD_Vector2(new ScientificDecimal(1.7988510, 9), new ScientificDecimal(-5.1363150, 8)),
                new SD_Vector2(new ScientificDecimal(2.2405056, 3), new ScientificDecimal(7.9426031, 3))),
            new ScientificDecimal(1.075938, 23), new ScientificDecimal(2.4103, 6), new Color(130, 130, 95, 255), jupiter
        );
        Planet saturn = planetTemplate.CreateInstance("Saturn", 
            new(new SD_Vector2(new ScientificDecimal( 1.4146019, 12), new ScientificDecimal(-2.6971440, 11)),
                new SD_Vector2(new ScientificDecimal( 1.2650097, 3), new ScientificDecimal( 9.4749677, 3))),
            new ScientificDecimal(5.683, 26), new ScientificDecimal(5.8232, 7), new Color(150, 150, 80, 255), sun
        );
        Planet uranus = planetTemplate.CreateInstance("Uranus", 
            new(new SD_Vector2(new ScientificDecimal(1.6645067, 12), new ScientificDecimal(2.4055482, 12)),
                new SD_Vector2(new ScientificDecimal(-5.6626764, 3), new ScientificDecimal(3.5634117, 3))),
            new ScientificDecimal(8.681, 25), new ScientificDecimal(2.5362, 7), new Color(170, 200, 255, 255), sun
        );
        Planet neptune = planetTemplate.CreateInstance("Neptune", 
            new(new SD_Vector2(new ScientificDecimal(4.4699311, 12), new ScientificDecimal(-9.8183016, 10)),
                new SD_Vector2(new ScientificDecimal(7.2829293, 1), new ScientificDecimal(5.4729751, 3))),
            new ScientificDecimal(1.024, 26), new ScientificDecimal(2.4622, 7), new Color(100, 120, 200, 255), sun
        );
        Planet halley = planetTemplate.CreateInstance("Halley", 
            new(new SD_Vector2(new ScientificDecimal(-2.9450469, 12), new ScientificDecimal(4.0907881, 12)),
                new SD_Vector2(new ScientificDecimal(8.0919083, 2), new ScientificDecimal(8.0919083, 2))),
            new ScientificDecimal(2.2, 14), new ScientificDecimal(5.5, 3), new Color(200, 100, 200, 255), sun
        );
            
        SD_Vector2[] points = Utils.CenterConvex([
            new(0.4, 0.4),
            new(0.4, -0.3),
            new(-0.2, -0.5),
            new(-5, 0),
            new(-0.3, 0.5)
        ]);
        Material shipMaterial = new Material(0.1f, 0.7f, 0.5f);
        Ship.ShipTemplate smokestackTemplate = new Ship.ShipTemplate(points, shipMaterial);
        
        Bodies = [sun, mercury, venus, earth, moon, mars, jupiter, io, europa, ganymede, callisto, saturn, uranus, neptune, halley];
        
        // for (int i = 0; i < 5; i++)
        // {
        //     SD_Vector2 randomPosition = new(_rnd.Next(-10, 10), _rnd.Next(-10, 10));
        //     Ship smokestack = smokestackTemplate.CreateInstance("Smokestack " + i, 
        //         new(
        //             new SD_Vector2(2 * new ScientificDecimal(6.378, 6), 0) + randomPosition,
        //             SD_Vector2.Zero
        //         ), 1000, new Color(0, 255, 0, 255), earth);
        //     Bodies.Add(smokestack);
        //     smokestack.DrawOrbitalPath = true;
        // }

        Ship.ShipTemplate strawhatTemplate = new Ship.ShipTemplate(Utils.CenterConvex([
            new(4, 4),
            new(4, -3),
            new(-2, -5),
            new(-4, 0),
            new(-3, 5)
        ]), shipMaterial);
        Ship strawhat = strawhatTemplate.CreateInstance("Strawhat", new SpatialInfo(
            new SD_Vector2(2 * new ScientificDecimal(6.378, 6), 0)), 1000, new Color(255, 0, 0, 255), earth);
        Bodies.Add(strawhat);
        strawhat.DrawOrbitalPath = true;
        
        #endregion

        Planet manatee = planetTemplate.CreateInstance("Manatee", new SpatialInfo(SD_Vector2.Zero), 50000000, 500, 
            new Color(125, 150, 130, 255), null);
            
        Ship.ShipTemplate shipTemplate2 = new Ship.ShipTemplate(Utils.CenterConvex([
            new(4, 3),
            new(3, -5),
            new(-2, -5),
            new(-4, 2)
        ]), shipMaterial);
        for (int i = 0; i < 1; ++i)
        {
            SD_Vector2 randomPosition = SD_Vector2.FromPolar(_rnd.NextDouble() * Math.Tau, _rnd.Next(510, 550));
            int randomColour = _rnd.Next(200, 255);
            Ship chimneyPipe = shipTemplate2.CreateInstance("Chimneypipe " + i,
                new SpatialInfo(randomPosition), 250, new Color(120, 200, randomColour, 255), manatee);
        }

        Planets = Bodies.Where(x => x is Planet).Select(x => x as Planet ?? throw new Exception()).ToList();
        Ships = Bodies.Where(x => x is Ship).Select(x => x as Ship ?? throw new Exception()).ToList();
        foreach (var planet in Planets) planet.GenerateKeplerianOrbit(GameState.PhysicsTime);
        OriginBody.Body = Bodies[^1];
        GameState.Tracking = OriginBody.Body;
        Camera.MovementScheme = new TrackingCameraScheme(OriginBody.Body.SpatialInfo, OriginBody.Body);
        Camera.Focus();
        GameState.ControlShip = Ships[^1];
        GameState.ControlShip.DrawOrbitalPath = true;
        GameState.TrackingIndex = Bodies.IndexOf(OriginBody.Body);
        _collisionHandler = new CollisionHandler(Bodies, new() {
            {(typeof(Ship), typeof(Planet)), (r, i) => 
                CollisionHandler.RestShipPlanetCollision(r, i, GameState.PhysicsTimeStep, GameState.DeltaPhysicsTimeStep)}
        });

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        Matrix projection = Matrix.CreateOrthographicOffCenter(
            0, Options.ScreenSize.width, Options.ScreenSize.height, 0, 0, 1
        );
        Effects.DefaultEffect = Content.Load<Effect>("effects/defaultEffect");
        Effects.DefaultEffect.Parameters["Projection"].SetValue(projection);
        Effects.CircleEffect = Content.Load<Effect>("effects/circleEffect");
        Effects.CircleEffect.Parameters["Projection"].SetValue(projection);
        
        /*PropertyInfo[] effects = typeof(Effects).GetProperties();
        foreach (var property in effects)
        {
            Effect effect = property.GetValue(null) as Effect ?? 
                            throw new Exception("Property in static Effects class is not an Effect");
            effect.Parameters["Projection"].SetValue(projection);
        }*/
        
        _font = Content.Load<SpriteFont>("fonts/defaultFont");
    }

    private void TrackBody(int index)
    {
        GameState.TrackingIndex = (int)Utils.UnsignedMod(index, Bodies.Count);
        GameState.Tracking = Bodies[GameState.TrackingIndex];
        Camera.MovementScheme = new TrackingCameraScheme(Camera.SpatialInfo, GameState.Tracking);
    }

    private void FastForwardToSelected()
    {
        foreach (var ship in Ships)
            ship.GenerateKeplerianOrbit(GameState.PhysicsTime);
        ScientificDecimal endTime = KeplerOrbitPath.SelectedPoint?.GetTimeAtPoint(GameState.PhysicsTime) ?? 0;
        if (endTime <= GameState.PhysicsTime) return;
        GameState.IsFastForward = true;
        ScientificDecimal multiplier = new ScientificDecimal((endTime - GameState.PhysicsTime).Exponent);
        InterpolationHandler<ScientificDecimal>.CreateInterpolation(
            new(independentGetter: () => GameState.RealTime, dependentSetter: val => { GameState.PhysicsTimeStep = val; }),
            GameState.RealTime, GameState.RealTime + 0.5, 
            GameState.GoalPhysicsTimeStep, GameState.GoalPhysicsTimeStep * multiplier);
        var endTimeWarp = InterpolationHandler<ScientificDecimal>.CreateInterpolation(
            new(independentGetter: () => GameState.PhysicsTime, dependentSetter: val => { GameState.PhysicsTimeStep = val; }), 
            endTime - GameState.GoalPhysicsTimeStep * multiplier * 0.1, endTime, 
            GameState.GoalPhysicsTimeStep * multiplier, GameState.GoalPhysicsTimeStep);
        endTimeWarp.InterpolationEnd += (_, _) =>
        {
            GameState.IsFastForward = false; 
        };
    }

    private void IncreaseTimeStep(ScientificDecimal multiplier)
    {
        InterpolationHandler<ScientificDecimal>.CreateInterpolation(
            new(() => GameState.RealTime, val => { GameState.PhysicsTimeStep = val; }),
            GameState.RealTime, GameState.RealTime + 0.5, 
            GameState.GoalPhysicsTimeStep, GameState.GoalPhysicsTimeStep * multiplier);
        GameState.GoalPhysicsTimeStep *= multiplier;
    }

    private void DecreaseTimeStep(ScientificDecimal multiplier)
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
                else Camera.MovementScheme = new SurfaceCameraScheme(GameState.Tracking.Parent, GameState.Tracking);
                break;
            case SurfaceCameraScheme:
                Camera.MovementScheme = new TrackingCameraScheme(Camera.SpatialInfo, GameState.Tracking);
                break;
            default:
                throw new Exception("Unrecognized camera movement scheme");
        }
    }
    
    KeyboardState _lastKeyboardState;

    private void HandleInput(ScientificDecimal dt)
    {
        MouseHandler.HandleMouseEvents();
        
        ScientificDecimal camSpeed = Camera.Height * Options.CamMoveSpeed * dt;
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
        
        if (keyboardState.IsKeyDown(Keys.T))
            if (_lastKeyboardState.IsKeyUp(Keys.T))
                FastForwardToSelected();

        if (keyboardState.IsKeyDown(Options.FocusKey))
            if (_lastKeyboardState.IsKeyUp(Options.FocusKey))
                Camera.Focus();
        
        if (keyboardState.IsKeyDown(Keys.C))
            if (_lastKeyboardState.IsKeyUp(Keys.C))
                ToggleCameraMovementScheme();

        if (keyboardState.IsKeyDown(Options.MoveUpKey)) Camera.MoveParallel(camSpeed);
        if (keyboardState.IsKeyDown(Options.MoveDownKey)) Camera.MoveParallel(-camSpeed);
        if (keyboardState.IsKeyDown(Options.MoveLeftKey)) Camera.MovePerpendicular(-camSpeed);
        if (keyboardState.IsKeyDown(Options.MoveRightKey)) Camera.MovePerpendicular(camSpeed);

        if (keyboardState.IsKeyDown(Options.ZoomOutKey)) Camera.ScaleZoom(1 + Options.CamZoomSpeed);
        if (keyboardState.IsKeyDown(Options.ZoomInKey)) Camera.ScaleZoom(1 - Options.CamZoomSpeed);

        float camRotateSpeed = (float)(Options.CamRotateSpeed * dt);
        if (keyboardState.IsKeyDown(Options.RotateLeftKey)) Camera.RotateBy(-camRotateSpeed);
        if (keyboardState.IsKeyDown(Options.RotateRightKey)) Camera.RotateBy(camRotateSpeed);

        GameState.ControlShip.ResetThrust();
        if (keyboardState.IsKeyDown(Keys.I)) GameState.ControlShip.ApplyThrust(new SD_Vector2(-100000, 0), new SD_Vector2(-0.4, 0));
        if (keyboardState.IsKeyDown(Keys.D8)) GameState.ControlShip.ApplyThrust(new SD_Vector2(-1000000, 0), new SD_Vector2(-0.4, 0));
        if (keyboardState.IsKeyDown(Keys.J)) GameState.ControlShip.ApplyThrust(new SD_Vector2(500, 0), new SD_Vector2(-0.4, 0.1));
        if (keyboardState.IsKeyDown(Keys.L)) GameState.ControlShip.ApplyThrust(new SD_Vector2(500, 0), new SD_Vector2(-0.4, -0.1));

        _lastKeyboardState = keyboardState;
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        UpdateFrame?.Invoke(this, EventArgs.Empty);

        InterpolationHandler<ScientificDecimal>.UpdateInterpolationValues();
        
        GameState.DeltaRealTime = (DateTime.Now - GameState.PreviousDateTime).TotalSeconds;
        GameState.PreviousDateTime = DateTime.Now;
        GameState.DeltaPhysicsTimeStep = GameState.DeltaRealTime * GameState.PhysicsTimeStep;
        GameState.PhysicsTime += GameState.DeltaPhysicsTimeStep;
        GameState.RealTime = gameTime.TotalGameTime.TotalSeconds;
        GameState.FrameCountThisSecond++;
        
        foreach (var body in Bodies)
            body.Acceleration = SD_Vector2.Zero;
        
        OriginBody.ResetOrigin();
        
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
                        ship.UpdateShipKeplerianOrbit(Planets);
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
            
            _collisionHandler.ResolveCollisions();

            foreach (var ship in Ships)
                if (ship.LandingState != null)
                    ship.UpdatePosition_Landed();
        }
        
        Camera.Update();
        
        Ships.RemoveAll(ship => ship.MarkedForRemoval);
        Bodies.RemoveAll(body => (body as Ship)?.MarkedForRemoval ?? false);
    }

    protected override void Draw(GameTime gameTime)
    {
        RasterizerState rasterizerState = new RasterizerState();
        rasterizerState.CullMode = CullMode.None;
        GraphicsDevice.RasterizerState = rasterizerState;

        GraphicsDevice.Clear(Options.BackgroundColour);

        _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

        if (Options.DisplayFPS)
            _spriteBatch.DrawString(_font, GameState.FramesPerSecond.ToString(), Vector2.Zero, Color.White);
        
        try
        {
            _spriteBatch.DrawString(_font, "Current date: " + new DateTime(2024, 12, 25)
                    .AddSeconds((double)GameState.PhysicsTime),
                new Vector2(0, 30), Color.White);
        }
        catch (ArgumentOutOfRangeException)
        {
            _spriteBatch.DrawString(_font, "Current date: >10000y A.D.", new Vector2(0, 30), Color.White);
        }
        
        _spriteBatch.DrawString(_font, GameState.PhysicsTimeStep.ToString(), new Vector2(0, 60), Color.White);
        _spriteBatch.DrawString(_font, GameState.IsFastForward.ToString(), new Vector2(0, 90), Color.White);

        foreach (var ship in Ships) ship.Draw();
        foreach (var planet in Planets) planet.Draw();
        
        DrawDebug.Draw();
        DrawDebug.ClearBuffer();
        
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}