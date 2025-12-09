using System.Diagnostics;
using OrbitGame;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Glfw;
using Silk.NET.Input;
using SkiaSharp;
using Vector2 = OrbitGame.Vector2;
// ReSharper disable AccessToDisposedClosure

// Initialize window
WindowOptions options = WindowOptions.Default with
{
    Size = new Vector2D<int>(Options.ScreenSize.width/2, Options.ScreenSize.height/2),
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
ScientificDecimal timeStep = 1;
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
    new SKColor(100, 200, 255, 255),
    "Earth"
);
Ship ship = new Ship(
    new ScientificDecimal(5.23m, 7),
    new Vector2(
        new ScientificDecimal(0),
        new ScientificDecimal(6.378m, 6) + 80000
    ),
    new Vector2(
        new ScientificDecimal(2m, 3),
        new ScientificDecimal(0)
    ),
    new SKColor(125, 0, 0, 255),
    earth,
    [
        new(0, 0),
        new(-2, -0.3),
        new(-5, -1.5),
        new(-4, 0),
        new(-5, 1.5),
        new(-3, 1),
        new(-2, 2),
        new(-1.75, 0.5)
    ],
    "Smokestack"
);

List<Body> bodies = [
    sun,
    earth,
    ship
];

OriginBody.Body = ship;

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
    {
        camera.MoveTo(Vector2.Zero);
    }
}

input.Keyboards[0].KeyDown += HandleKeyPresses;

void HandleInput(IKeyboard keyboard, ScientificDecimal dt)
{
    ScientificDecimal camSpeed = camera.Height * Options.CamMoveSpeed * dt;
    // subtract angle by camera rotation so movement does not respect rotation
    if (keyboard.IsKeyPressed(Options.MoveUpKey)) camera.MoveBy(camSpeed, -Math.PI / 2 - camera.Rotation);
    if (keyboard.IsKeyPressed(Options.MoveDownKey)) camera.MoveBy(camSpeed, Math.PI / 2 - camera.Rotation);
    if (keyboard.IsKeyPressed(Options.MoveLeftKey)) camera.MoveBy(camSpeed, Math.PI - camera.Rotation);
    if (keyboard.IsKeyPressed(Options.MoveRightKey)) camera.MoveBy(camSpeed, 0 - camera.Rotation);
    
    if (keyboard.IsKeyPressed(Options.ZoomOutKey)) camera.ScaleZoom(1 + Options.CamZoomSpeed);
    if (keyboard.IsKeyPressed(Options.ZoomInKey)) camera.ScaleZoom(1 - Options.CamZoomSpeed);
    
    float camRotateSpeed = (float)(Options.CamRotateSpeed * dt);
    if (keyboard.IsKeyPressed(Options.RotateLeftKey)) camera.RotateBy(-camRotateSpeed);
    if (keyboard.IsKeyPressed(Options.RotateRightKey)) camera.RotateBy(camRotateSpeed);
}

ConvexCollider collider1 = new([
    new (10, -2),
    new (7, 0),
    new (8, 2),
    new (10, 4),
    new (12, 0)
], new Planet(0, Vector2.Zero, Vector2.Zero, 0, SKColors.White, String.Empty));
ConvexCollider collider2 = new([
    new (4, 2),
    new (6, -0.5m),
    new (9, 3.5m),
    new (6.5m, 5),
    new (4, 4)
], new Planet(0, Vector2.Zero, Vector2.Zero, 0, SKColors.White, String.Empty));

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
    
    foreach (var body in bodies)
    {
        body.SetNetGravitationalAcceleration(bodies);
        body.NI_UpdatePosition(deltaTimeStep, Options.IntegratorMethod);
    }
    OriginBody.ResetOrigin(bodies);
    
    camera.SetOrigin(tracking.Position);
    //if (tracking != earth) camera.SetRotation((float)(-Vector2.AngleTo(camera.AbsolutePosition, earth.Position) - Math.PI / 2));

    foreach (var body in bodies)
    {
        body.Draw(canvas, camera);
        body.DrawCollider(canvas, camera);
    }
    
    canvas.Flush();
}

window.Render += OnRender;

window.Run();