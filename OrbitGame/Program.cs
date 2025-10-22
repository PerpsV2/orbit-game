using System.Numerics;
using OrbitGame;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Glfw;
using Silk.NET.Input;
using SkiaSharp;

// Initialize window
WindowOptions options = WindowOptions.Default with
{
    Size = new Vector2D<int>(Options.ScreenSize.width, Options.ScreenSize.height),
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
var renderTarget = new GRBackendRenderTarget(Options.ScreenSize.width * 2, Options.ScreenSize.height * 2, 0, 
    8, new GRGlFramebufferInfo(0, 0x8058)); // 0x8058 = GL_RGBA8`
using SKSurface surface = SKSurface.Create(grContext, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
using SKCanvas canvas = surface.Canvas;
IInputContext input = window.CreateInput();

ScientificDecimal time = 0;
ScientificDecimal timeStep = 1;
ScientificDecimal deltaTime;
DateTime previousTime = DateTime.Now;

DateTime frameTime = DateTime.Now;

int frameCount = 0;
int framesPerSecond = 0;

SKPaint paint = new SKPaint
{
    Color = SKColors.White,
    IsAntialias = true
};

void OnRender(double _)
{
    grContext.ResetContext();
    canvas.Clear(SKColors.Black);
    
    deltaTime = (DateTime.Now - previousTime).TotalSeconds;
    previousTime = DateTime.Now;
    time += deltaTime * timeStep;

    frameCount++;
    if ((DateTime.Now - frameTime).TotalSeconds > 1)
    {
        frameTime = DateTime.Now;
        framesPerSecond = frameCount;
        frameCount = 0;
    }

    canvas.DrawCircle(40, 10, 60, paint);
    canvas.DrawText(framesPerSecond.ToString(), 100, 100, SKTextAlign.Center, new SKFont(), paint);
    
    canvas.Flush();
}

window.Render += OnRender;

window.Run();