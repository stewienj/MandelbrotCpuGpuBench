using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Concurrent;
using System.ComponentModel;
using Algorithms;
using System.Windows.Media.Imaging;

namespace MandelbrotCpuGpuBench
{
  public class Workspace: INotifyPropertyChanged
  {
    // TODO: Add double buffering. Should have current, pending (last rendered), and a list of spares (ideally 1) that can be handed off for rendering to
    // current gets replaced by pending when the CopyPixels is called 


    private double _zoomLevel = 0.001;
    private double _viewR = 0.001643721971153;
    private double _viewI = 0.822467633298876;
    private int _bufferWidth = 0;
    private int _bufferHeight = 0;

    private ThrottledAction _throttledAction = new ThrottledAction(TimeSpan.FromMilliseconds(1));

    private ConcurrentStack<int[,]> _spareBuffers = new ConcurrentStack<int[,]>();

    /// <summary>
    /// Test memory layout, have come to the conclusion that a 2D array is to be used as
    /// shown in this Test.
    /// </summary>
    unsafe static void Test()
    {
      var width = 20;
      var height = 10;
      var foo = new int[height, width];
      Console.WriteLine(foo.Length);

      var stride = (int)Math.Pow(10, Math.Ceiling(Math.Log10(width)));

      for (int y = 0; y < height; ++y)
      {
        for (int x = 0; x < width; ++x)
        {
          foo[y, x] = x + stride * y;
        }
      }
      fixed (int* bar = foo)
      {
        var bar2 = bar;
        var remaining = foo.Length;
        while (remaining-- > 0)
        {
          Console.WriteLine(*bar2);
          bar2++;
        }
      }
      Console.ReadLine();
    }

    public void Move(Vector distance)
    {
      _viewR += distance.X * _zoomLevel;
      _viewI += distance.Y * _zoomLevel;
      OnParametersChanged();
    }

    public void Zoom(int zDelta)
    {
      double factor = 1.2;
      if (zDelta > 0)
        _zoomLevel /= factor;
      else
        _zoomLevel *= factor;
      OnParametersChanged();
    }

    public void ChangeSize(int width, int height)
    {
      if (_bufferWidth != width || _bufferHeight != height)
      {
        _bufferWidth = width;
        _bufferHeight = height;
        _spareBuffers.Clear();
        OnParametersChanged();
      }
    }

    public void OnParametersChanged()
    {
      ParametersChanged?.Invoke(this, EventArgs.Empty);
      DoRender();
    }

    public void DoRender()
    {
      int width = _bufferWidth;
      int height = _bufferHeight;

      // Try and get a spare buffer
      if (!_spareBuffers.TryPop(out int[,] buffer))
      {
        buffer = new int[height, width];
      }
      else if (buffer.GetLength(0) != height || buffer.GetLength(1) != width)
      {
        buffer = new int[height, width];
        _spareBuffers.Clear();
      }

      int maxiter = (int)(-512 * Math.Log10(_zoomLevel));
      Func<int, (byte R, byte G, byte B)> itersToColor = FractalRenderer.GetColorProviderV2(maxiter+1);
      Action<int, int, int> addPixel = (x, y, iters) =>
      {
        if (y >= height || x >= width)
          return;

        var color = itersToColor(iters);
        buffer[y, x] = (color.R << 16) | (color.G << 8) | color.B;
      };

      int halfHeight = (int)(Math.Floor(height / 2.0));
      int halfWidth = (int)(Math.Floor(width / 2.0));
      double xMin = -halfWidth * _zoomLevel + _viewR;
      double xMax = halfWidth * _zoomLevel + _viewR;
      double yMin = -halfHeight * _zoomLevel + _viewI;
      double yMax = halfHeight * _zoomLevel + _viewI;
      double step = _zoomLevel;

      var render = FractalRenderer.SelectRender(addPixel, () => false, true, true, true, false);

      _throttledAction.InvokeAction(() =>
      {
        render(xMin, xMax, yMin, yMax, step, maxiter);

        var temp = (MandelbrotImage as ArrayBitmapSource)?.Buffer;
        var image = new ArrayBitmapSource(buffer);
        image.Freeze();
        MandelbrotImage = image;
        if (temp!=null && temp.GetLength(0) == height && temp.GetLength(1) == width)
        {
          _spareBuffers.Push(temp);
        }

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MandelbrotImage)));
      });

    }

    public BitmapSource MandelbrotImage { get; set; }

    public event EventHandler ParametersChanged;
    public event PropertyChangedEventHandler PropertyChanged;
  }
}
