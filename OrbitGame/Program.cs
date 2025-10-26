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
    Size = new Vector2D<int>(Options.ScreenSize.width / 2, Options.ScreenSize.height / 2),
    Title = "Orbit Game",
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

List<Planet> planets = [
    new Planet(
        new ScientificDecimal(1.989m, 30),
        Vector2.Zero,
        Vector2.Zero,
        new ScientificDecimal(6.96340m, 8 + 1),
        new SKColor(255, 255, 255, 255),
        "Sun"
    ),
    new Planet(
        new ScientificDecimal(5.9722m, 24),
        new Vector2(
            new ScientificDecimal(-8.5613233m, 9),
            new ScientificDecimal(1.4688537m, 11)
        ),
        new Vector2(
            new ScientificDecimal(-3.0223357m, 4),
            new ScientificDecimal(-1.8447646m, 3)
        ),
        new ScientificDecimal(6.378m, 6 + 2),
        new SKColor(100, 200, 255, 255),
        "Earth"
    )
];

Camera camera = new Camera(
    new Vector2(0, 0),
    new ScientificDecimal(8m, 5),
    new ScientificDecimal(6m, 5)
    );

void HandleKeyPresses(IKeyboard keyboard, Key key, int keyCode)
{
    if (key == Options.TimeWarpDownKey) timeStep /= 10;
    if (key == Options.TimeWarpUpKey) timeStep *= 10;
}

input.Keyboards[0].KeyDown += HandleKeyPresses;

void HandleInput(IKeyboard keyboard)
{
    if (keyboard.IsKeyPressed(Options.MoveUpKey)) camera.MoveBy(new Vector2(0, camera.Height * -Options.CamMoveSpeed));
    if (keyboard.IsKeyPressed(Options.MoveDownKey)) camera.MoveBy(new Vector2(0, camera.Height * Options.CamMoveSpeed));
    if (keyboard.IsKeyPressed(Options.MoveLeftKey)) camera.MoveBy(new Vector2(camera.Height * -Options.CamMoveSpeed, 0));
    if (keyboard.IsKeyPressed(Options.MoveRightKey)) camera.MoveBy(new Vector2(camera.Height * Options.CamMoveSpeed, 0));
    
    if (keyboard.IsKeyPressed(Options.ZoomOutKey)) camera.ScaleZoom(1 + Options.CamZoomSpeed);
    if (keyboard.IsKeyPressed(Options.ZoomInKey)) camera.ScaleZoom(1 - Options.CamZoomSpeed);
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
    
    HandleInput(input.Keyboards[0]);
    
    if (Options.DisplayFPS)
        canvas.DrawText(framesPerSecond.ToString(), 5, 5, SKTextAlign.Center, font, paint);
    
    foreach (var planet in planets)
    {
        planet.UpdatePosition(planets, deltaTimeStep);
        planet.Draw(canvas, camera);
    }
    
    canvas.Flush();
}

window.Render += OnRender;

window.Run();