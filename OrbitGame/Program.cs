using OrbitGame;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Glfw;
using Silk.NET.Input;
using SkiaSharp;
using Matrix3X3 = OrbitGame.Matrix3X3;
using Vector2 = OrbitGame.Vector2;
// ReSharper disable AccessToDisposedClosure

// Initialize window
WindowOptions options = WindowOptions.Default with
{
    Size = new Vector2D<int>(Options.ScreenSize.width, Options.ScreenSize.height),
    Title = "Jonah's Shiny Smooth Forehead",
    PreferredStencilBufferBits = 8,
    PreferredBitDepth = new Vector4D<int>(8, 8, 8, 8),
};

GlfwWindowing.Use();
IWindow window = Window.Create(options);
window.Initialize();

// Initialize graphics and input
using GRGlInterface grGlInterface = GRGlInterface.Create(
    name => window.GLContext!.TryGetProcAddress(name, out var addr) ? addr : 0);
grGlInterface.Validate();
using GRContext grContext = GRContext.CreateGl(grGlInterface);
var renderTarget = new GRBackendRenderTarget(Options.ScreenSize.width, Options.ScreenSize.height, 0, 
    8, new GRGlFramebufferInfo(0, 0x8058)); // 0x8058 = GL_RGBA8`
using SKSurface surface = SKSurface.Create(grContext, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
using SKCanvas canvas = surface.Canvas;
IInputContext input = window.CreateInput();

ScientificDecimal time = 0;
ScientificDecimal timeStep = Options.DefaultTimeStep;
ScientificDecimal deltaTime;
DateTime previousTime = DateTime.Now;

int frameCountPerSecond = 0;
int framesPerSecond = 0;

void UpdateFPS(object? state)
{
    framesPerSecond = frameCountPerSecond;
    frameCountPerSecond = 0;
}

Timer frameTimer = new Timer(UpdateFPS, null, 0, 1000);

SKPaint paint = new SKPaint
{
    Color = SKColors.White,
    IsAntialias = true,
};

SKFont font = new SKFont
{
    Size = 30
};

Planet sun = new Planet(
    new ScientificDecimal(1.989m, 30),
    Vector2.Zero,
    Vector2.Zero,
    new ScientificDecimal(6.96340m, 8),
    new Material(0f),
    new SKColor(255, 255, 255, 255), null,
    "Sun"
);
Planet mercury = new Planet(
    new ScientificDecimal(3.285m, 23),
    new Vector2(
        new ScientificDecimal(-5.6940545m, 10), 
        new ScientificDecimal( 3.2977160m, 9)
    ), 
    new Vector2(
        new ScientificDecimal(-1.2946428m, 4), 
        new ScientificDecimal(-4.6540563m, 4)
    ),
    new ScientificDecimal(2.4397m, 6),
    new Material(0.2f), new SKColor(140, 140, 140, 255), sun, "Mercury"
);
Planet venus = new Planet(
    new ScientificDecimal(4.867m, 24),
    new Vector2(
        new ScientificDecimal( 8.2978939m, 10), 
        new ScientificDecimal( 6.9376114m, 10)
    ), 
    new Vector2(
        new ScientificDecimal(-2.2569107m, 4), 
        new ScientificDecimal( 2.6718186m, 4)
    ),
    new ScientificDecimal(6.0518m, 6),
    new Material(0.2f), new SKColor(230, 160, 40, 255), sun, "Venus"
);
Planet earth = new Planet(
    new ScientificDecimal(5.9722m, 24),
    new Vector2(
        new ScientificDecimal(-8.5613233m, 8),
        new ScientificDecimal(1.4688537m, 11)
    ),
    new Vector2(
        new ScientificDecimal(-3.0223357m, 4),
        new ScientificDecimal(-1.8447646m, 3)
    ),
    new ScientificDecimal(6.378m, 6),
    new Material(0.2f), new SKColor(100, 200, 255, 255), sun, "Earth"
);
Planet mars = new Planet(
    new ScientificDecimal(6.39m, 23),
    new Vector2(
        new ScientificDecimal(-6.4603691m, 10), 
        new ScientificDecimal( 2.3127019m, 11)
    ), 
    new Vector2(
        new ScientificDecimal(-2.2420469m, 4), 
        new ScientificDecimal(-4.6499686m, 3)
    ),
    new ScientificDecimal(3.3895m, 6),
    new Material(0.2f), new SKColor(230, 60, 50, 255), sun, "Mars"
);
Planet jupiter = new Planet(
    new ScientificDecimal(1.898m, 27),
    new Vector2(
        new ScientificDecimal( 1.6580000m, 11), 
        new ScientificDecimal( 7.4166230m, 11)
    ), 
    new Vector2(
        new ScientificDecimal(-1.2915655m, 4), 
        new ScientificDecimal( 3.4670152m, 3)
    ), 
    new ScientificDecimal(6.9911m, 7), 
    new Material(0.2f), new SKColor(175, 125, 50, 255), sun, "Jupiter"
);
Planet saturn = new Planet(
    new ScientificDecimal(5.683m, 26),
    new Vector2(
        new ScientificDecimal( 1.4146019m, 12), 
        new ScientificDecimal(-2.6971440m, 11)
    ), 
    new Vector2(
        new ScientificDecimal( 1.2650097m, 3), 
        new ScientificDecimal( 9.4749677m, 3)
    ),
    new ScientificDecimal(5.8232m, 7),
    new Material(0.2f), new SKColor(150, 150, 80, 255), sun, "Saturn"
);
Planet uranus = new Planet(
    new ScientificDecimal(8.681m, 25),
    new Vector2(
        new ScientificDecimal( 1.6645067m, 12), 
        new ScientificDecimal( 2.4055482m, 12)
    ), 
    new Vector2(
        new ScientificDecimal(-5.6626764m, 3), 
        new ScientificDecimal( 3.5634117m, 3)
    ),
    new ScientificDecimal(2.5362m, 7),
    new Material(0.2f), new SKColor(170, 200, 255, 255), sun, "Uranus"
);
Planet neptune = new Planet(
    new ScientificDecimal(1.024m, 26),
    new Vector2(
        new ScientificDecimal( 4.4699311m, 12), 
        new ScientificDecimal(-9.8183016m, 10)
    ), 
    new Vector2(
        new ScientificDecimal( 7.2829293m, 1), 
        new ScientificDecimal( 5.4729751m, 3)
    ),
    new ScientificDecimal(2.4622m, 7),
    new Material(0.2f), new SKColor(100, 120, 200, 255), sun, "Neptune"
);
Planet halley = new Planet(
    new ScientificDecimal(2.2m, 14),
    new Vector2(
        new ScientificDecimal(-2.9450469m, 12),
        new ScientificDecimal(4.0907881m, 12)
    ),
    new Vector2(
        new ScientificDecimal(8.0919083m, 2),
        new ScientificDecimal(8.0919083m, 2)
    ),
    new ScientificDecimal(5.5m, 3),
    new Material(0.2f), new SKColor(200, 100, 200, 255), sun, "Halley"
);
Ship smokestack = new Ship(
    1000, new Vector2(new ScientificDecimal(6.378m, 6) + 4000000, 0), new Vector2(0, 10),
    new Material(0.5f), new SKColor(0, 125, 0, 255),
    earth,
    Vector2.CenterConvex([
        new (4,4),
        new (4, -3),
        new (-2, -5),
        new (-50, 0),
        new (-3, 5)
    ]),
    "Smokestack"
);

Planet r = new Planet(
    100, new Vector2(4, 0), Vector2.Zero, 4,
    new Material(0.5f), new SKColor(125, 0, 0, 255), null, "Planet");
Planet g = new Planet(
    100, new(2, 5), Vector2.Zero, 1,
    new Material(0.5f), new SKColor(0, 125, 0, 255), null, "Planet");
Ship b = new Ship(
    1000, new(-5, -3), Vector2.Zero,
    new Material(0.5f), new SKColor(0, 0, 125, 255), null,
    [
        new (2.33333333333,3.5),
        new (2.33333333333, -2.5),
        new (-1.6666666667, -2.5),
        new (-3.6666666667, 1.5)
    ],
    "Ship"
);

List<Body> bodies = [
    sun, earth, smokestack
];
List<Planet> planets = [
    sun, earth
];
List<Ship> ships = [
    smokestack
];

OriginBody.Body = smokestack;

Body tracking = OriginBody.Body;
int trackingIndex = 0;

Camera camera = new Camera(
    new Vector2(0, 0),
    Options.ScreenSize.width * Options.DefaultZoomScale,
    Options.ScreenSize.height * Options.DefaultZoomScale
    );

void HandleKeyPresses(IKeyboard keyboard, Key key, int keyCode)
{
    if (key == Options.TimeWarpDownKey) timeStep /= 10;
    if (key == Options.TimeWarpUpKey) timeStep *= 10;

    void TrackBody(int index)
    {
        trackingIndex = index % bodies.Count;
        tracking = bodies[trackingIndex];
        camera.MoveTo(camera.AbsolutePosition - tracking.Position);
    }

    if (key == Options.TrackNextBodyKey) TrackBody(trackingIndex + 1);
    if (key == Options.TrackPrevBodyKey) TrackBody(trackingIndex - 1);

    if (key == Options.FocusKey)
        camera.MoveTo(Vector2.Zero);
}

input.Keyboards[0].KeyDown += HandleKeyPresses;

void HandleInput(IKeyboard keyboard, ScientificDecimal dt)
{
    ScientificDecimal camSpeed = camera.Height * Options.CamMoveSpeed * dt;
    // subtract angle by camera rotation so movement does not respect rotation
    if (keyboard.IsKeyPressed(Options.MoveUpKey)) camera.MoveBy(camSpeed, Math.PI / 2 - camera.Angle);
    if (keyboard.IsKeyPressed(Options.MoveDownKey)) camera.MoveBy(camSpeed, -Math.PI / 2 - camera.Angle);
    if (keyboard.IsKeyPressed(Options.MoveLeftKey)) camera.MoveBy(camSpeed, Math.PI - camera.Angle);
    if (keyboard.IsKeyPressed(Options.MoveRightKey)) camera.MoveBy(camSpeed, 0 - camera.Angle);
    
    if (keyboard.IsKeyPressed(Options.ZoomOutKey)) camera.ScaleZoom(1 + Options.CamZoomSpeed);
    if (keyboard.IsKeyPressed(Options.ZoomInKey)) camera.ScaleZoom(1 - Options.CamZoomSpeed);
    
    float camRotateSpeed = (float)(Options.CamRotateSpeed * dt);
    if (keyboard.IsKeyPressed(Options.RotateLeftKey)) camera.RotateBy(camRotateSpeed);
    if (keyboard.IsKeyPressed(Options.RotateRightKey)) camera.RotateBy(-camRotateSpeed);
    
    if (keyboard.IsKeyPressed(Key.J)) tracking.Angle += 0.05;
    if (keyboard.IsKeyPressed(Key.L)) tracking.Angle -= 0.05;
    
    if (keyboard.IsKeyPressed(Key.I)) smokestack.Velocity -= smokestack.ForwardVector * 100;
    if (keyboard.IsKeyPressed(Key.K)) smokestack.Velocity += smokestack.ForwardVector * 100;
}

void OnRender(double _)
{
    grContext.ResetContext();
    canvas.Clear(SKColors.Black);
    
    deltaTime = (DateTime.Now - previousTime).TotalSeconds;
    previousTime = DateTime.Now;
    ScientificDecimal deltaTimeStep = deltaTime * timeStep;
    time += deltaTimeStep;
    frameCountPerSecond++;
    
    HandleInput(input.Keyboards[0], deltaTime);
    
    if (Options.DisplayFPS)
        canvas.DrawText(framesPerSecond.ToString(), 40, 40, SKTextAlign.Center, font, paint);
    
    try {
        canvas.DrawText("Current date: " + new DateTime(2024, 12, 25).AddSeconds((double)time),
            40, 60, font, paint);
    }
    catch (ArgumentOutOfRangeException) {
        canvas.DrawText("Current date: >10000y A.D.", 40, 60, font, paint);
    }
    
    if (Options.EnablePhysics)
    {
        foreach (var body in planets)
        {
            body.Kepler_UpdatePosition(time);
        }
        
        // step through gravity numerical integrator
        foreach (var body in ships)
        {
            body.SetNetGravitationalAcceleration(planets);
            body.NI_UpdatePosition(deltaTimeStep, Options.IntegratorMethod, x => x.SetNetGravitationalAcceleration(planets));
        }
        
        // resolve collisions
        foreach (var ship in ships)
            foreach (var body in bodies)
                if (body != ship)
                    if (ship.Collider != null)
                        ship.Collider.CollidesWith(body.Collider);
    }
    
    // recalculate origins
    // BUG: resetting the origin currently breaks values for keplerian orbits
    //OriginBody.ResetOrigin(bodies);
    camera.SetOrigin(tracking.Position);
    
    smokestack.RecalculateOrbit(bodies);

    foreach (var body in planets) body.DrawSphereOfInfluence(canvas, camera);
    foreach (var body in bodies) body.DrawOrbitalPathLRL(canvas, camera);
    
    // draw bodies
    foreach (var body in bodies)
    {
        body.Draw(canvas, camera);
        if (Options.DrawColliders) body.DrawCollider(canvas, camera);
    }
    
    // draw objects from debug canvas
    DebugCanvas.Draw(canvas, camera);
    DebugCanvas.ClearBuffer();

    canvas.Flush();
}

window.Render += OnRender;

window.Run();

paint.Dispose();