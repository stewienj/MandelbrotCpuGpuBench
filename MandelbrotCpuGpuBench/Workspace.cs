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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MandelbrotCpuGpuBench
{
  public class Workspace: INotifyPropertyChanged
  {
    private double _zoomLevel = 0.001;
    private double _viewR = 0.001643721971153;
    private double _viewI = 0.822467633298876;
    private int _bufferWidth = 0;
    private int _bufferHeight = 0;
    private Stopwatch _stopwatch = new Stopwatch();

    private ThrottledAction _throttledAction = new ThrottledAction(TimeSpan.FromMilliseconds(1));

    private ConcurrentStack<int[,]> _spareBuffers = new ConcurrentStack<int[,]>();

    [DllImport("MandelbrotCppRenderers.dll")]
    static extern unsafe void RenderMandelbrotCpp(bool useGpu, bool doublePrecision, bool multiThreaded, double zoomLevel, double r, double i, int* pBuffer, int bufferWidth, int bufferHeight);

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

    public unsafe void DoRender()
    {
      if (Closing)
        return;
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
      Func<int, (byte R, byte G, byte B)> itersToColor = FractalRenderer.GetColorProviderV2(maxiter + 1);
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

      var render = FractalRenderer.SelectRender(addPixel, () => false, MethodCpuSimd, PrecisionDouble, ThreadModelMulti, false);

      Action DoRender = null;

      if (LanguageCs)
      {
        DoRender = () => render(xMin, xMax, yMin, yMax, step, maxiter);
      }
      else if (_languageCpp)
      {
        DoRender = () =>
        {
          fixed (int* fixedBuffer = buffer)
          {
            RenderMandelbrotCpp(MethodGpu, PrecisionDouble, ThreadModelMulti, _zoomLevel, _viewR, _viewI, fixedBuffer, width, height);
          }
        };
      }


      _throttledAction.InvokeAction(() =>
      {
        _stopwatch.Reset();
        _stopwatch.Start();
        DoRender();
        _stopwatch.Stop();

        Title = $"Mandelbrot Rendering Took {_stopwatch.ElapsedMilliseconds}ms ({width}x{height})  Iterations = {maxiter}  Zoom Level = {Math.Round(0.002 / _zoomLevel, 1)}";

        var temp = (MandelbrotImage as ArrayBitmapSource)?.Buffer;
        var image = new ArrayBitmapSource(buffer);
        image.Freeze();
        MandelbrotImage = image;
        if (temp != null && temp.GetLength(0) == height && temp.GetLength(1) == width)
        {
          _spareBuffers.Push(temp);
        }

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MandelbrotImage)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
      });
    }

    public BitmapSource MandelbrotImage { get; set; }
    public string Title { get; set; } = "Mandelbrot Renderer";

    public event EventHandler ParametersChanged;
    public event PropertyChangedEventHandler PropertyChanged;

    [DllImport("kernel32", SetLastError = true)]
    static extern bool FreeLibrary(IntPtr hModule);

    private bool _closing = false;
    public bool Closing
    {
      get => _closing;
      set
      {
        _closing = value;
        if (_closing)
        {
          _throttledAction.Join();

          // Need for force unloading of the C++/AMP rendering DLL else we get an exception
          foreach (ProcessModule mod in Process.GetCurrentProcess().Modules)
          {
            if (mod.ModuleName.ToUpper() == "MandelbrotCppRenderers.dll".ToUpper())
            {
              FreeLibrary(mod.BaseAddress);
            }
          }


        }
      }
    }

    private bool _languageCs = true;
    public bool LanguageCs
    {
      get => _languageCs;
      set
      {
        _languageCs = value;
        if (_languageCs && MethodGpu)
        {
          MethodCpuSimd = true;
        }
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MethodCpuSimd)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MethodCpuSimdEnabled)));
      }
    }
    private bool _languageCpp = false;
    public bool LanguageCpp
    {
      get => _languageCpp;
      set
      {
        _languageCpp = value;
        if (_languageCpp && MethodCpuSimd)
        {
          MethodGpu = true;
        }
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MethodGpu)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MethodGpuEnabled)));
      }
    }
    public bool ThreadModelMulti { get; set; } = true;
    public bool ThreadModelSingle { get; set; } = false;
    public bool PrecisionFloat { get; set; } = true;
    public bool PrecisionDouble { get; set; } = false;
    public bool MethodCpuSimd { get; set; } = true;
    public bool MethodCpuFpu { get; set; } = false;
    public bool MethodGpu { get; set; } = false;
    public bool MethodCpuSimdEnabled => LanguageCs;
    public bool MethodGpuEnabled => LanguageCpp;

  }
}
