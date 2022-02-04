using MandelbrotCsRenderers;
using Swordfish.NET.Maths;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MandelbrotCpuGpuBench
{
    public class Workspace : INotifyPropertyChanged
    {
        private DoubleDouble _zoomLevel = new DoubleDouble(0.001);
        private DoubleDouble _viewR = new DoubleDouble(0.001643721971153);
        private DoubleDouble _viewI = new DoubleDouble(0.822467633298876);
        private int _bufferWidth = 0;
        private int _bufferHeight = 0;
        private Stopwatch _stopwatch = new Stopwatch();
        private FunDistances _funDistances = new FunDistances();

        private ThrottledAction _throttledAction = new ThrottledAction(TimeSpan.FromMilliseconds(1));

        private ConcurrentStack<int[,]> _spareBuffers = new ConcurrentStack<int[,]>();

        [DllImport("MandelbrotCppRenderers.dll")]
        static extern unsafe void RenderMandelbrotCpp(bool useGpu, bool doublePrecision, bool multiThreaded, double zoomLevel, double r, double i, int* pBuffer, int bufferWidth, int bufferHeight);

        public Workspace()
        {
            Options.Cpp.MethodCpuFpu = true;
            Options.Cpp.MethodCpuSimd = false;
        }

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
            _viewR += new DoubleDouble(distance.X) * _zoomLevel;
            _viewI += new DoubleDouble(distance.Y) * _zoomLevel;
            OnParametersChanged();
        }

        public void Zoom(int zDelta, Point location)
        {
            (DoubleDouble X, DoubleDouble Y) oldDistance = (new DoubleDouble(location.X - _bufferWidth / 2.0) * _zoomLevel, new DoubleDouble(location.Y - _bufferHeight / 2.0)*_zoomLevel);
            DoubleDouble factor = new DoubleDouble(1.2);
            DoubleDouble offset = new DoubleDouble(0);
            if (zDelta > 0)
            {
                _zoomLevel /= factor;
                offset = (new DoubleDouble(1.0) - new DoubleDouble(1.0) / factor);
            }
            else
            {
                _zoomLevel *= factor;
                offset = new DoubleDouble(1.0) - factor;
            }

            // Correct for the position of the mouse
            _viewR += oldDistance.X * offset;
            _viewI += oldDistance.Y * offset;

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

            int maxiter = (int)(-512 * Math.Log10(_zoomLevel.Hi));
            Func<int, (byte R, byte G, byte B)> itersToColor = FractalRendererBase.GetColorProviderV2(maxiter + 1);
            Action<int, int, int> addPixel = (x, y, iters) =>
            {
                if (y >= height || x >= width)
                    return;

                var color = itersToColor(iters);
                buffer[y, x] = (color.R << 16) | (color.G << 8) | color.B;
            };

            int halfHeight = (int)(Math.Floor(height / 2.0));
            int halfWidth = (int)(Math.Floor(width / 2.0));
            DoubleDouble xMin = new DoubleDouble(-halfWidth) * _zoomLevel + _viewR;
            DoubleDouble xMax = new DoubleDouble(halfWidth) * _zoomLevel + _viewR;
            DoubleDouble yMin = new DoubleDouble(-halfHeight) * _zoomLevel + _viewI;
            DoubleDouble yMax = new DoubleDouble(halfHeight) * _zoomLevel + _viewI;
            DoubleDouble step = _zoomLevel;

            Func<bool> DoRender = null;
            Action AbortRender = null;

            if (Options.LanguageCs)
            {
                var cs = Options.Cs;
                if (!cs.PrecisionFloat128 && !cs.PrecisionFloat128Fast)
                {
                    (var render, var abort) = FractalRenderer64.SelectRender(addPixel, () => false, cs.MethodCpuSimd, cs.PrecisionFloat64, cs.ThreadModelMulti);
                    DoRender = () => render(xMin.Hi, xMax.Hi, yMin.Hi, yMax.Hi, step.Hi, maxiter);
                    AbortRender = () => abort();
                }
                else
                {
                    (var render, var abort) = FractalRenderer128.SelectRender128(addPixel, () => false, cs.MethodCpuSimd, cs.ThreadModelMulti, cs.PrecisionFloat128Fast);
                    DoRender = () => render(xMin, xMax, yMin, yMax, step, maxiter);
                    AbortRender = () => abort();
                }
            }
            else if (Options.LanguageCpp)
            {
                var cpp = Options.Cpp;
                DoRender = () =>
                {
                    fixed (int* fixedBuffer = buffer)
                    {
                        RenderMandelbrotCpp(cpp.MethodGpu, cpp.PrecisionFloat64, cpp.ThreadModelMulti, _zoomLevel.Hi, _viewR.Hi, _viewI.Hi, fixedBuffer, width, height);
                    }
                    return true;
                };
            }


            _throttledAction.InvokeAction(() =>
            {
                _stopwatch.Reset();
                _stopwatch.Start();
                bool success = DoRender();
                _stopwatch.Stop();

                if (!success)
                    return;

                double fullSetPixels = 4 / _zoomLevel.Hi;
                double meters = _funDistances.PixelsToMeters(fullSetPixels);

                Title = $"Mandelbrot Rendering Took {_stopwatch.ElapsedMilliseconds}ms ({width}x{height})  Whole Set Size = {Math.Round(meters)} meters (> {_funDistances.PixelsToFunDistance(fullSetPixels)})  Iterations = {maxiter}  Zoom Level = {Math.Round(0.002 / _zoomLevel.Hi, 1)}";

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

        public RendererOptionsViewModel Options { get; } = new RendererOptionsViewModel();

        private bool _fullScreen = false;
        public bool FullScreen
        {
            get
            {
                return _fullScreen;
            }
            set
            {
                _fullScreen = value;
                Action fireProperties = () =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FullScreen)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToolsVisibility)));
                    // Important to set Style before State
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WindowStyle)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WindowsState)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResizeMode)));
                };

                fireProperties();
                if (_fullScreen && _lastWindowState == WindowState.Maximized)
                {
                    // WindowState needs to be set to Maximized after setting WindowStyle, and not before, so we need to toggle it
                    _fullScreen = false;
                    fireProperties();
                    Task.Run(() =>
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
              {
                  _fullScreen = true;
                  fireProperties();
              }));
                    });
                }

            }
        }
        public Visibility ToolsVisibility => FullScreen ? Visibility.Collapsed : Visibility.Visible;

        private WindowState _lastWindowState;
        public WindowState WindowsState
        {
            get => FullScreen ? WindowState.Maximized : WindowState.Normal;
            set => _lastWindowState = value;
        }
        public WindowStyle WindowStyle => FullScreen ? WindowStyle.None : WindowStyle.SingleBorderWindow;
        public ResizeMode ResizeMode => FullScreen ? ResizeMode.NoResize : ResizeMode.CanResize;

        public WindowStartupLocation WindowStartupLocation => FullScreen ? WindowStartupLocation.CenterScreen : WindowStartupLocation.Manual;
    }
}
