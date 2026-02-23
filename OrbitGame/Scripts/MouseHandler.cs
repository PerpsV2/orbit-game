using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace OrbitGame;

public delegate void MouseEventHandler(object? sender, MouseEventArgs e);

public class MouseEventArgs(Vector2 position) : EventArgs
{
    public Vector2 Position = position;
}

public static class MouseHandler
{
    public static event MouseEventHandler? MouseClickDown;
    public static event MouseEventHandler? MouseDown;
    public static event MouseEventHandler? MouseMove;
    public static event MouseEventHandler? MouseHover;
    
    private static MouseState _previousMouseState;

    public static void HandleMouseEvents()
    {
        MouseState mouseState = Mouse.GetState();
        
        if (mouseState.LeftButton == ButtonState.Pressed)
            if (_previousMouseState.LeftButton == ButtonState.Released)
                OnMouseClickDown(new Vector2(mouseState.X, mouseState.Y));
        
        if (mouseState.LeftButton == ButtonState.Pressed)
            OnMouseDown(new Vector2(mouseState.X, mouseState.Y));
        
        if (mouseState.Position != _previousMouseState.Position)
            OnMouseMove(new Vector2(mouseState.X, mouseState.Y));
        
        OnMouseHover(new Vector2(mouseState.X, mouseState.Y));
        
        _previousMouseState = mouseState;
    }
    
    private static void OnMouseClickDown(Vector2 mousePosition)
    {
        MouseClickDown?.Invoke(null, new MouseEventArgs(mousePosition));
    }

    private static void OnMouseDown(Vector2 mousePosition)
    {
        MouseDown?.Invoke(null, new MouseEventArgs(mousePosition));
    }
    
    private static void OnMouseMove(Vector2 mousePosition)
    {
        MouseMove?.Invoke(null, new MouseEventArgs(mousePosition));
    }
    
    private static void OnMouseHover(Vector2 mousePosition)
    {
        MouseHover?.Invoke(null, new MouseEventArgs(mousePosition));
    }
}