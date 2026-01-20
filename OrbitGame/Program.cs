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
    new Material(0.5f),
    new SKColor(255, 255, 255, 255), 
    "Sun"
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
    new Material(0.2f),
    new SKColor(100, 200, 255, 255), 
    "Earth"
);
Planet r = new Planet(
    100, new Vector2(4, 0), Vector2.Zero, 4,
    new Material(0.5f), new SKColor(125, 0, 0, 255),"Planet");
Planet g = new Planet(
    100, new(2, 5), Vector2.Zero, 1,
    new Material(0.5f), new SKColor(0, 125, 0, 255), "Planet");
Ship b = new Ship(
    1000, new(-5, -3), Vector2.Zero,
    new Material(0.5f), new SKColor(0, 0, 125, 255),
    [
        new (2.33333333333,3.5),
        new (2.33333333333, -2.5),
        new (-1.6666666667, -2.5),
        new (-3.6666666667, 1.5)
    ],
    "Ship"
);
Ship smokestack = new Ship(
    1000, new Vector2(new ScientificDecimal(6.378m, 6) + 4000, 0), new Vector2(0, 10),
    new Material(0.5f), new SKColor(0, 125, 0, 255),
    earth,
    [
        new (2.33333333333,3.5),
        new (2.33333333333, -2.5),
        new (-1.6666666667, -2.5),
        new (-3.6666666667, 1.5)
    ],
    "Ship"
);

List<Body> bodies = [
    earth, smokestack
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
    if (keyboard.IsKeyPressed(Options.MoveUpKey)) camera.MoveBy(camSpeed, -Math.PI / 2 - camera.Angle);
    if (keyboard.IsKeyPressed(Options.MoveDownKey)) camera.MoveBy(camSpeed, Math.PI / 2 - camera.Angle);
    if (keyboard.IsKeyPressed(Options.MoveLeftKey)) camera.MoveBy(camSpeed, Math.PI - camera.Angle);
    if (keyboard.IsKeyPressed(Options.MoveRightKey)) camera.MoveBy(camSpeed, 0 - camera.Angle);
    
    if (keyboard.IsKeyPressed(Options.ZoomOutKey)) camera.ScaleZoom(1 + Options.CamZoomSpeed);
    if (keyboard.IsKeyPressed(Options.ZoomInKey)) camera.ScaleZoom(1 - Options.CamZoomSpeed);
    
    float camRotateSpeed = (float)(Options.CamRotateSpeed * dt);
    if (keyboard.IsKeyPressed(Options.RotateLeftKey)) camera.RotateBy(-camRotateSpeed);
    if (keyboard.IsKeyPressed(Options.RotateRightKey)) camera.RotateBy(camRotateSpeed);

    if (keyboard.IsKeyPressed(Key.I)) tracking.Position -= new Vector2(0, 0.06m);
    if (keyboard.IsKeyPressed(Key.J)) tracking.Position -= new Vector2(0.06m, 0);
    if (keyboard.IsKeyPressed(Key.K)) tracking.Position += new Vector2(0, 0.06m);
    if (keyboard.IsKeyPressed(Key.L)) tracking.Position += new Vector2(0.06m, 0);
    if (keyboard.IsKeyPressed(Key.Number1)) tracking.Angle += 0.01;
    if (keyboard.IsKeyPressed(Key.Number2)) tracking.Angle -= 0.01;
    if (keyboard.IsKeyPressed(Key.Number3)) tracking.AngularVelocity += 0.1;
    if (keyboard.IsKeyPressed(Key.Number4)) tracking.AngularVelocity -= 0.1;
    if (keyboard.IsKeyPressed(Key.Number5)) tracking.Velocity += new Vector2(0, 0.01);
    if (keyboard.IsKeyPressed(Key.Number6)) tracking.Velocity -= new Vector2(0, 0.01);
    if (keyboard.IsKeyPressed(Key.Number7)) tracking.Velocity += new Vector2(0.01, 0);
    if (keyboard.IsKeyPressed(Key.Number8)) tracking.Velocity -= new Vector2(0.01, 0);
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
        canvas.DrawText(framesPerSecond.ToString(), 20, 20, SKTextAlign.Center, font, paint);

    if (Options.EnablePhysics)
    {
        foreach (var body in bodies)
        {
            body.SetNetGravitationalAcceleration(bodies);
            body.NI_UpdatePosition(deltaTimeStep, Options.IntegratorMethod);
        }
    }
    
    smokestack.Collider.CollidesWith(earth.Collider);
    
    OriginBody.ResetOrigin(bodies);
    
    camera.SetOrigin(tracking.Position);
    
    foreach (var body in bodies)
    {
        body.Draw(canvas, camera);
        if (Options.DrawColliders) body.DrawCollider(canvas, camera);
    }
    
    DebugCanvas.Draw(canvas, camera);
    DebugCanvas.ClearBuffer();

    canvas.Flush();
}

window.Render += OnRender;

window.Run();