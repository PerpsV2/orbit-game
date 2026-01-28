using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame;

namespace OrbitGame;

#nullable enable

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
    private int _trackingIndex = 0;
    
    Camera _camera = new Camera(new SD_Vector2(0, 0), 
        Options.ScreenSize.width * Options.DefaultZoomScale, 
        Options.ScreenSize.height * Options.DefaultZoomScale
    );

    public static Effect? CurrentEffect { get; set; } = null;
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
        Utils.GenerateCircleBuffers(_graphics.GraphicsDevice);
        
        Timer frameTimer = new Timer(UpdateFPS, null, 0, 1000);
        
        #region Bodies

        Planet sun = new Planet(
            new ScientificDecimal(1.989m, 30),
            SD_Vector2.Zero,
            SD_Vector2.Zero,
            new ScientificDecimal(6.96340m, 8),
            new Material(0f),
            Color.White, null,
            "Sun"
        );
        Planet mercury = new Planet(
            new ScientificDecimal(3.285m, 23),
            new SD_Vector2(
                new ScientificDecimal(-5.6940545m, 10), 
                new ScientificDecimal( 3.2977160m, 9)
            ), 
            new SD_Vector2(
                new ScientificDecimal(-1.2946428m, 4), 
                new ScientificDecimal(-4.6540563m, 4)
            ),
            new ScientificDecimal(2.4397m, 6),
            new Material(0.2f), new Color(140, 140, 140, 255), sun, "Mercury"
        );
        Planet venus = new Planet(
            new ScientificDecimal(4.867m, 24),
            new SD_Vector2(
                new ScientificDecimal( 8.2978939m, 10), 
                new ScientificDecimal( 6.9376114m, 10)
            ), 
            new SD_Vector2(
                new ScientificDecimal(-2.2569107m, 4), 
                new ScientificDecimal( 2.6718186m, 4)
            ),
            new ScientificDecimal(6.0518m, 6),
            new Material(0.2f), new Color(230, 160, 40, 255), sun, "Venus"
        );
        Planet earth = new Planet(
            new ScientificDecimal(5.9722m, 24),
            new SD_Vector2(
                new ScientificDecimal(-8.5613233m, 8),
                new ScientificDecimal(1.4688537m, 11)
            ),
            new SD_Vector2(
                new ScientificDecimal(-3.0223357m, 4),
                new ScientificDecimal(-1.8447646m, 3)
            ),
            new ScientificDecimal(6.378m, 6),
            new Material(0.2f), new Color(100, 200, 255, 255), sun, "Earth"
        );
        Planet moon = new Planet(
            new ScientificDecimal(7.349m, 22), 
            new SD_Vector2(
                new ScientificDecimal(-3.6413936m, 8), 
                new ScientificDecimal(-1.7481022m, 8)
            ), 
            new SD_Vector2(
                new ScientificDecimal( 4.2899598m, 2), 
                new ScientificDecimal(-8.6413934m, 2)
            ),
            new ScientificDecimal(1.737m, 6), 
            new Material(0.2f), new Color(180, 180, 180, 255), earth, "Moon"
        );
        Planet mars = new Planet(
            new ScientificDecimal(6.39m, 23),
            new SD_Vector2(
                new ScientificDecimal(-6.4603691m, 10), 
                new ScientificDecimal( 2.3127019m, 11)
            ), 
            new SD_Vector2(
                new ScientificDecimal(-2.2420469m, 4), 
                new ScientificDecimal(-4.6499686m, 3)
            ),
            new ScientificDecimal(3.3895m, 6),
            new Material(0.2f), new Color(230, 60, 50, 255), sun, "Mars"
        );
        Planet jupiter = new Planet(
            new ScientificDecimal(1.898m, 27),
            new SD_Vector2(
                new ScientificDecimal( 1.6580000m, 11), 
                new ScientificDecimal( 7.4166230m, 11)
            ), 
            new SD_Vector2(
                new ScientificDecimal(-1.2915655m, 4), 
                new ScientificDecimal( 3.4670152m, 3)
            ), 
            new ScientificDecimal(6.9911m, 7), 
            new Material(0.2f), new Color(175, 125, 50, 255), sun, "Jupiter"
        );
        Planet saturn = new Planet(
            new ScientificDecimal(5.683m, 26),
            new SD_Vector2(
                new ScientificDecimal( 1.4146019m, 12), 
                new ScientificDecimal(-2.6971440m, 11)
            ), 
            new SD_Vector2(
                new ScientificDecimal( 1.2650097m, 3), 
                new ScientificDecimal( 9.4749677m, 3)
            ),
            new ScientificDecimal(5.8232m, 7),
            new Material(0.2f), new Color(150, 150, 80, 255), sun, "Saturn"
        );
        Planet uranus = new Planet(
            new ScientificDecimal(8.681m, 25),
            new SD_Vector2(
                new ScientificDecimal( 1.6645067m, 12), 
                new ScientificDecimal( 2.4055482m, 12)
            ), 
            new SD_Vector2(
                new ScientificDecimal(-5.6626764m, 3), 
                new ScientificDecimal( 3.5634117m, 3)
            ),
            new ScientificDecimal(2.5362m, 7),
            new Material(0.2f), new Color(170, 200, 255, 255), sun, "Uranus"
        );
        Planet neptune = new Planet(
            new ScientificDecimal(1.024m, 26),
            new SD_Vector2(
                new ScientificDecimal( 4.4699311m, 12), 
                new ScientificDecimal(-9.8183016m, 10)
            ), 
            new SD_Vector2(
                new ScientificDecimal( 7.2829293m, 1), 
                new ScientificDecimal( 5.4729751m, 3)
            ),
            new ScientificDecimal(2.4622m, 7),
            new Material(0.2f), new Color(100, 120, 200, 255), sun, "Neptune"
        );
        Planet halley = new Planet(
            new ScientificDecimal(2.2m, 14),
            new SD_Vector2(
                new ScientificDecimal(-2.9450469m, 12),
                new ScientificDecimal(4.0907881m, 12)
            ),
            new SD_Vector2(
                new ScientificDecimal(8.0919083m, 2),
                new ScientificDecimal(8.0919083m, 2)
            ),
            new ScientificDecimal(5.5m, 3),
            new Material(0.2f), new Color(200, 100, 200, 255), sun, "Halley"
        );
        /*Ship smokestack = new Ship(
            1000, new SD_Vector2(new ScientificDecimal(6.378m, 6) + 4000000, 0), 
            new SD_Vector2(0, 10), new Material(0.5f), new Color(0, 125, 0, 255),
            earth,
            SD_Vector2.CenterConvex([
                new (4,4),
                new (4, -3),
                new (-2, -5),
                new (-50, 0),
                new (-3, 5)
            ]),
            "Smokestack"
        );*/
        
        #endregion

        _bodies = new List<Body>();
        _bodies = [sun, mercury, venus, earth, moon, mars, jupiter, saturn, uranus, neptune, halley];
        for (int i = 0; i < 500; ++i)
        {
            var randColour = _rnd.Next(0, 256);
            var randDisplacement = new SD_Vector2(_rnd.Next(-1000, 1000), _rnd.Next(-1000, 1000));
            _bodies.Add(new Ship(
                    1000, new SD_Vector2(new ScientificDecimal(6.378m, 6) + 4000000, 0) + 
                    randDisplacement,
                    new SD_Vector2(0, 10), new Material(0.5f), new Color(255, randColour, randColour, 255),
                    earth, 
                    SD_Vector2.CenterConvex([
                        new (4,4),
                        new (4, -4),
                        new (-4, -4),
                        new (-4, 4)
                    ]),
                    "Smokestack"
                ));
        }
        _planets = _bodies.Where(x => x is Planet).Select(x => x as Planet ?? throw new Exception()).ToList();
        _ships = _bodies.Where(x => x is Ship).Select(x => x as Ship ?? throw new Exception()).ToList();
        OriginBody.Body = earth;
        _tracking = OriginBody.Body;
        
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
        
        _lastKeyboardState = keyboardState;
    }
    
    protected override void Update(GameTime gameTime)
    {
        _deltaTime = (DateTime.Now - _previousTime).TotalSeconds;
        _previousTime = DateTime.Now;
        _deltaTimeStep = _deltaTime * _timeStep;
        _time += _deltaTimeStep;
        _frameCountPerSecond++;
        
        HandleInput(_deltaTime);
        
        if (Options.EnablePhysics)
        {
            foreach (var planet in _planets)
                planet.Kepler_UpdatePosition(_time);

            foreach (var ship in _ships)
            {
                ship.SetNetGravitationalAcceleration(_planets);
                ship.NI_UpdatePosition(_deltaTimeStep, Options.IntegratorMethod, x => x.SetNetGravitationalAcceleration(_planets));
            }
            
            /*foreach (var ship in _ships)
                foreach (var body in _bodies)
                    if (body != ship)
                        if (ship.Collider != null && body.Collider != null)
                            if (ship.Collider.NearsWith(body.Collider))
                                ship.Collider.CollidesWith(body.Collider);*/
        }
        
        _camera.SetOrigin(_tracking.Position);
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        RasterizerState rasterizerState = new RasterizerState();
        rasterizerState.CullMode = CullMode.None;
        GraphicsDevice.RasterizerState = rasterizerState;
        
        GraphicsDevice.Clear(Color.Black);
        
        _spriteBatch.Begin();
        
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
        foreach (var body in _bodies)
            body.Draw(_spriteBatch, _camera);
        
        _spriteBatch.End();
        
        base.Draw(gameTime);
    }
}