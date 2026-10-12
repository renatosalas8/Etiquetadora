using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Etiquetadora.Models;

public class MovableBorder : Border
{
    private bool _isPressed; 
    private Point _positionInBlock;
    private TranslateTransform _transform = null!;

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (this.GetVisualParent() is not Control parentContext) 
            return;

        _isPressed = true;
        
        _positionInBlock = e.GetPosition(parentContext);
        
        if (_transform != null!) 
            _positionInBlock = new Point(
                _positionInBlock.X - _transform.X,
                _positionInBlock.Y - _transform.Y);
        
        e.Pointer.Capture(this);
        
        base.OnPointerPressed(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        _isPressed = false;
        e.Pointer.Capture(null);
        
        base.OnPointerReleased(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (!_isPressed)
            return;
        
        if (this.GetVisualParent() is not Control parentContext)
            return;

        var currentPosition = e.GetPosition(parentContext);

        var offsetX = currentPosition.X - _positionInBlock.X;
        var offsetY = currentPosition.Y - _positionInBlock.Y;

        _transform = new TranslateTransform(offsetX, offsetY);
        RenderTransform = _transform;
        
        base.OnPointerMoved(e);
    }
}