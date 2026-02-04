using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace OrbitGame;

public class OrbitGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

#pragma warning disable CS8618, CS9264
    public OrbitGame()
#pragma warning restore CS8618, CS9264
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = Options.ScreenSize.width;
        _graphics.PreferredBackBufferHeight = Options.ScreenSize.height;
        Graphics = _graphics.GraphicsDevice;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }
    
    private ScientificDecimal _time = 0;
    private ScientificDecimal _timeStep = Options.DefaultTimeStep;
    private ScientificDecimal _deltaTime;
    private ScientificDecimal _deltaTimeStep;
    private DateTime _previousTime = DateTime.Now;
    
    private int _frameCountPerSecond;
    private int _framesPerSecond;
    
    private List<Body> _bodies = [];
    private List<Planet> _planets = [];
    private List<Ship> _ships = [];

    private Body _tracking;
    private int _trackingIndex;
    
    Camera _camera = new Camera(new SD_Vector2(0, 0), 
        Options.ScreenSize.width * Options.DefaultZoomScale, 
        Options.ScreenSize.height * Options.DefaultZoomScale
    );

    public static Effect? CurrentEffect { get; set; } = null;
    public static GraphicsDevice Graphics;
    private SpriteFont _font;
    private Effect _defaultEffect;
    
    private Random _rnd = new Random();
    
    void UpdateFPS(object? state)
    {
        _framesPerSecond = _frameCountPerSecond;
        _frameCountPerSecond = 0;
    }

    protected override void Initialize()
    {
        Graphics = _graphics.GraphicsDevice;
        
        Timer frameTimer = new Timer(UpdateFPS, null, 0, 1000);
        
        //f#region Bodies

        /*
        Material planetMaterial = new Material(0.2f);
        Planet.PlanetTemplate planetTemplate = new Planet.PlanetTemplate(planetMaterial);
        Planet sun = planetTemplate.CreateInstance(
            "Sun", new ScientificDecimal(1.989, 30), SD_Vector2.Zero, SD_Vector2.Zero, 0, 0,
            Color.White, null, new ScientificDecimal(6.98340, 8)
        );
        Planet mercury = planetTemplate.CreateInstance(
            "Mercury", new ScientificDecimal(3.285, 23),
            new SD_Vector2(new ScientificDecimal(-5.6940545, 10), new ScientificDecimal(3.2977160, 9)), 
            new SD_Vector2(new ScientificDecimal(-1.2946428, 4), new ScientificDecimal(-4.6540563, 4)), 
            0, 0, new Color(140, 140, 140, 255), sun, new ScientificDecimal(2.4397, 6)
        );
        Planet venus = planetTemplate.CreateInstance(
            "Venus", new ScientificDecimal(4.867, 24),
            new SD_Vector2(new ScientificDecimal(8.2978939, 10), new ScientificDecimal(6.9376114, 10)),
            new SD_Vector2(new ScientificDecimal(-2.2569107, 4), new ScientificDecimal(2.6718186, 4)), 
            0, 0, new Color(230, 160, 40, 255), sun, new ScientificDecimal(6.0518, 6)
        );
        Planet earth = planetTemplate.CreateInstance(
            "Earth", new ScientificDecimal(5.9722, 24),
            new SD_Vector2(new ScientificDecimal(-8.5613233, 8), new ScientificDecimal(1.4688537, 11)),
            new SD_Vector2(new ScientificDecimal(-3.0223357, 4), new ScientificDecimal(-1.8447646, 3)), 
            0, 0, new Color(100, 200, 255, 255), sun, new ScientificDecimal(6.378, 6)
        );
        Planet moon = planetTemplate.CreateInstance(
            "The Moon", new ScientificDecimal(7.349, 22),
            new SD_Vector2(new ScientificDecimal(-3.6413936, 8), new ScientificDecimal(-1.7481022, 8)), 
            new SD_Vector2(new ScientificDecimal( 4.2899598, 2), new ScientificDecimal(-8.6413934, 2)), 
            0, 0, new Color(180, 180, 180, 255), earth, new ScientificDecimal(1.737, 6)
        );
        Planet mars = planetTemplate.CreateInstance(
            "Mars", new ScientificDecimal(6.39, 23),
            new SD_Vector2(new ScientificDecimal(-6.4603691, 10), new ScientificDecimal( 2.3127019, 11)), 
            new SD_Vector2(new ScientificDecimal(-2.2420469, 4), new ScientificDecimal(-4.6499686, 3)), 
            0, 0, new Color(230, 60, 50, 255), sun, new ScientificDecimal(3.3895, 6)
        );
        Planet jupiter = planetTemplate.CreateInstance(
            "Jupiter", new ScientificDecimal(1.898, 27),
            new SD_Vector2(new ScientificDecimal(1.6580000, 11), new ScientificDecimal(7.4166230, 11)),
            new SD_Vector2(new ScientificDecimal(-1.2915655, 4), new ScientificDecimal(3.4670152, 3)), 
            0, 0, new Color(175, 125, 50, 255), sun, new ScientificDecimal(6.9911, 7)
        );
        Planet saturn = planetTemplate.CreateInstance(
            "Saturn", new ScientificDecimal(5.683, 26),
            new SD_Vector2(new ScientificDecimal( 1.4146019, 12), new ScientificDecimal(-2.6971440, 11)), 
            new SD_Vector2(new ScientificDecimal( 1.2650097, 3), new ScientificDecimal( 9.4749677, 3)), 
            0, 0, new Color(150, 150, 80, 255), sun, new ScientificDecimal(5.8232, 7)
        );
        Planet uranus = planetTemplate.CreateInstance(
            "Uranus", new ScientificDecimal(8.681, 25),
            new SD_Vector2(new ScientificDecimal(1.6645067, 12), new ScientificDecimal(2.4055482, 12)),
            new SD_Vector2(new ScientificDecimal(-5.6626764, 3), new ScientificDecimal(3.5634117, 3)), 
            0, 0, new Color(170, 200, 255, 255), sun, new ScientificDecimal(2.5362, 7)
        );
        Planet neptune = planetTemplate.CreateInstance(
            "Neptune", new ScientificDecimal(1.024, 26),
            new SD_Vector2(new ScientificDecimal(4.4699311, 12), new ScientificDecimal(-9.8183016, 10)),
            new SD_Vector2(new ScientificDecimal(7.2829293, 1), new ScientificDecimal(5.4729751, 3)), 
            0, 0, new Color(100, 120, 200, 255), sun, new ScientificDecimal(2.4622, 7)
        );
        Planet halley = planetTemplate.CreateInstance(
            "Halley", new ScientificDecimal(2.2, 14),
            new SD_Vector2(new ScientificDecimal(-2.9450469, 12), new ScientificDecimal(4.0907881, 12)),
            new SD_Vector2(new ScientificDecimal(8.0919083, 2), new ScientificDecimal(8.0919083, 2)),
            0, 0, new Color(200, 100, 200, 255), sun, new ScientificDecimal(5.5, 3)
        );
        SD_Vector2[] points = SD_Vector2.CenterConvex([
            new(4, 4),
            new(4, -3),
            new(-2, -5),
            new(-50, 0),
            new(-3, 5)
        ]);
        Material shipMaterial = new Material(0.5f);
        Ship.ShipTemplate smokestackTemplate = new Ship.ShipTemplate(points, shipMaterial);
        
        #endregion

        _bodies = new List<Body>();
        _bodies = [sun, mercury, venus, earth, moon, mars, jupiter, saturn, uranus, neptune, halley];
        for (int i = 0; i < 200; ++i)
        {
            int randomRed = _rnd.Next(0, 256);
            SD_Vector2 randomPosition = new SD_Vector2(0, _rnd.Next(-40000000, 40000000));
            SD_Vector2 randomVelocity = new SD_Vector2(0, _rnd.Next(1022, 1022));
            Ship smokestack = smokestackTemplate.CreateInstance("Smokestack " + i, 1000,
                new SD_Vector2(new ScientificDecimal(6.378, 6) + 354000000, 0) + randomPosition, 
                randomVelocity, 
                0, 0, new Color(0, randomRed, 0, 255), earth);
            _bodies.Add(smokestack);
        }*/

        Planet.PlanetTemplate planetTemplate = new Planet.PlanetTemplate(new Material(0.2f));
        Planet testPlanet = planetTemplate.CreateInstance("Manatee", 5000, SD_Vector2.Zero,
            SD_Vector2.Zero, 0, 0, new Color(125, 150, 130, 255), null, 500);

        Ship.ShipTemplate shipTemplate1 = new Ship.ShipTemplate(SD_Vector2.CenterConvex([
            new(4, 4),
            new(4, -3),
            new(-2, -5),
            new(-4, 0),
            new(-3, 5)
        ]), new Material(0.2f));
        Ship testShip1 = shipTemplate1.CreateInstance("Strawhat", 250, new SD_Vector2(505, 10),
            SD_Vector2.Zero, 0, 0, new Color(235, 100, 100, 255), testPlanet);
        
        Ship.ShipTemplate shipTemplate2 = new Ship.ShipTemplate(SD_Vector2.CenterConvex([
            new(4, 3),
            new(3, -5),
            new(-2, -5),
            new(-4, 2)
        ]), new Material(0.2f));
        _bodies = [testPlanet, testShip1];
        for (int i = 0; i < 20; ++i)
        {
            SD_Vector2 randomPosition = new SD_Vector2(_rnd.Next(-30, 30), _rnd.Next(-50, 50));
            int randomColour = _rnd.Next(200, 255);
            Ship chimneyPipe = shipTemplate2.CreateInstance("Chimneypipe " + i, 250, 
                new SD_Vector2(550, 0) + randomPosition,
                SD_Vector2.Zero, 0, 0, new Color(120, 200, randomColour, 255), testPlanet);
            _bodies.Add(chimneyPipe);
        }
        _planets = _bodies.Where(x => x is Planet).Select(x => x as Planet ?? throw new Exception()).ToList();
        _ships = _bodies.Where(x => x is Ship).Select(x => x as Ship ?? throw new Exception()).ToList();
        OriginBody.Body = testShip1;
        _tracking = OriginBody.Body;
        _trackingIndex = _bodies.IndexOf(OriginBody.Body);
        
        base.Initialize();
    }
    
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _defaultEffect = Content.Load<Effect>("effects/defaultEffect");
        _font = Content.Load<SpriteFont>("fonts/defaultFont");
    }

    KeyboardState _lastKeyboardState;
    
    private void TrackBody(int index)
    {
        _trackingIndex = (int)Utils.UnsignedMod(index, _bodies.Count);
        _tracking = _bodies[_trackingIndex];
        _camera.MoveTo(_camera.AbsolutePosition - _tracking.Position);
    }
    
    private void HandleInput(ScientificDecimal dt)
    {
        ScientificDecimal camSpeed = _camera.Height * Options.CamMoveSpeed * dt;
        KeyboardState keyboardState = Keyboard.GetState();
        if (keyboardState.IsKeyDown(Options.TimeWarpUpKey))
            if (_lastKeyboardState.IsKeyUp(Options.TimeWarpUpKey))
                _timeStep *= Options.TimeWarpStepMultiplier;
        if (keyboardState.IsKeyDown(Options.TimeWarpDownKey))
            if (_lastKeyboardState.IsKeyUp(Options.TimeWarpDownKey))
                _timeStep /= Options.TimeWarpStepMultiplier;

        if (keyboardState.IsKeyDown(Options.TrackNextBodyKey))
            if (_lastKeyboardState.IsKeyUp(Options.TrackNextBodyKey))
                TrackBody(_trackingIndex + 1);
        if (keyboardState.IsKeyDown(Options.TrackPrevBodyKey))
            if (_lastKeyboardState.IsKeyUp(Options.TrackPrevBodyKey))
                TrackBody(_trackingIndex - 1);
        
        if (keyboardState.IsKeyDown(Options.FocusKey))
            if (_lastKeyboardState.IsKeyUp(Options.FocusKey))
                _camera.MoveTo(SD_Vector2.Zero);
        
        if (keyboardState.IsKeyDown(Options.MoveUpKey)) _camera.MoveBy(camSpeed, Math.PI / 2 - _camera.Angle);
        if (keyboardState.IsKeyDown(Options.MoveDownKey)) _camera.MoveBy(camSpeed, 3 * Math.PI / 2 - _camera.Angle);
        if (keyboardState.IsKeyDown(Options.MoveLeftKey)) _camera.MoveBy(camSpeed, Math.PI - _camera.Angle);
        if (keyboardState.IsKeyDown(Options.MoveRightKey)) _camera.MoveBy(camSpeed, Math.Tau - _camera.Angle);
        
        if (keyboardState.IsKeyDown(Options.ZoomOutKey)) _camera.ScaleZoom(1 + Options.CamZoomSpeed);
        if (keyboardState.IsKeyDown(Options.ZoomInKey)) _camera.ScaleZoom(1 - Options.CamZoomSpeed);
        
        float camRotateSpeed = (float)(Options.CamRotateSpeed * dt);
        if (keyboardState.IsKeyDown(Options.RotateLeftKey)) _camera.RotateBy(camRotateSpeed);
        if (keyboardState.IsKeyDown(Options.RotateRightKey)) _camera.RotateBy(-camRotateSpeed);

        if (keyboardState.IsKeyDown(Keys.I)) _tracking.Position += new SD_Vector2(0, 0.1);
        if (keyboardState.IsKeyDown(Keys.K)) _tracking.Position += new SD_Vector2(0, -0.1);
        if (keyboardState.IsKeyDown(Keys.J)) _tracking.Position += new SD_Vector2(-0.1, 0);
        if (keyboardState.IsKeyDown(Keys.L)) _tracking.Position += new SD_Vector2(0.1, 0);
        
        if (keyboardState.IsKeyDown(Keys.D1)) _tracking.Velocity += new SD_Vector2(0, 0.1);
        if (keyboardState.IsKeyDown(Keys.D2)) _tracking.Velocity += new SD_Vector2(0, -0.1);
        if (keyboardState.IsKeyDown(Keys.D3)) _tracking.Velocity += new SD_Vector2(-0.1, 0);
        if (keyboardState.IsKeyDown(Keys.D4)) _tracking.Velocity += new SD_Vector2(0.1, 0);

        if (keyboardState.IsKeyDown(Keys.U)) _tracking.Angle += 0.01;
        if (keyboardState.IsKeyDown(Keys.O)) _tracking.Angle -= 0.01;
        
        _lastKeyboardState = keyboardState;
    }
    
    protected override void Update(GameTime gameTime)
    {
        _deltaTime = (DateTime.Now - _previousTime).TotalSeconds;
        _previousTime = DateTime.Now;
        _deltaTimeStep = _deltaTime * _timeStep;
        _time += _deltaTimeStep;
        _frameCountPerSecond++;

        if (Options.EnablePhysics)
        {
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < 100; ++i)
            {
                int i1 = i;
                Task task = Task.Run(() =>
                {
                    for (int j = i1 * _ships.Count / 100; j < (i1 + 1) * _ships.Count / 100; ++j)
                    {
                        Ship ship = _ships[j];
                        ship.SetNetGravitationalAcceleration(_planets);
                        ship.NI_UpdatePosition(_deltaTimeStep, Options.IntegratorMethod,
                            x => x.SetNetGravitationalAcceleration(_planets));
                    }
                });
                tasks.Add(task);
            }
            
            Task.WaitAll(tasks.ToArray());
            
            foreach (var planet in _planets)
                planet.Kepler_UpdatePosition(_time);

            foreach (var ship in _ships)
            {
                foreach (var planet in _planets)
                {
                    ship.Collider.CollidesWith(planet.Collider, ship, planet);
                }
            }
        }
        
        HandleInput(_deltaTime);
        
        _camera.SetOrigin(_tracking.Position);
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        RasterizerState rasterizerState = new RasterizerState();
        rasterizerState.CullMode = CullMode.None;
        GraphicsDevice.RasterizerState = rasterizerState;
        
        GraphicsDevice.Clear(Color.Black);
        
        _spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied);
        
        if (Options.DisplayFPS)
            _spriteBatch.DrawString(_font, _framesPerSecond.ToString(), Vector2.Zero, Color.White);
        
        try {
            _spriteBatch.DrawString(_font, "Current date: " + new DateTime(2024, 12, 25).AddSeconds((double)_time),
                new Vector2(0, 30), Color.White);
        }
        catch (ArgumentOutOfRangeException) {
            _spriteBatch.DrawString(_font, "Current date: >10000y A.D.", new Vector2(0, 30), Color.White);
        }

        CurrentEffect = _defaultEffect;
        // foreach (var planet in _planets)
        //     planet.DrawSphereOfInfluence(GraphicsDevice, _camera, _defaultEffect);
        foreach (var body in _bodies)
        {
            body.Draw(GraphicsDevice, _camera, _defaultEffect);
        }
        
        DrawDebug.Draw(GraphicsDevice, _camera);
        DrawDebug.ClearBuffer();

        _spriteBatch.End();
        
        base.Draw(gameTime);
    }
}