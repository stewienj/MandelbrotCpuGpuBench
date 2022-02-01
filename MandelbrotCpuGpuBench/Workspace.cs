using Algorithms;
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
        private decimal _zoomLevel = 0.001M;
        private decimal _viewR = 0.001643721971153M;
        private decimal _viewI = 0.822467633298876M;
        private int _bufferWidth = 0;
        private int _bufferHeight = 0;
        private Stopwatch _stopwatch = new Stopwatch();
        private FunDistances _funDistances = new FunDistances();

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
            _viewR += (decimal)distance.X * _zoomLevel;
            _viewI += (decimal)distance.Y * _zoomLevel;
            OnParametersChanged();
        }

        public void Zoom(int zDelta, Point location)
        {
            var oldDistance = (location - new Point(_bufferWidth / 2.0, _bufferHeight / 2.0)) * (double)_zoomLevel;
            decimal factor = 1.2M;
            decimal offset = 0;
            if (zDelta > 0)
            {
                _zoomLevel /= factor;
                offset = (1.0M - 1.0M / factor);
            }
            else
            {
                _zoomLevel *= (decimal)factor;
                offset = 1.0M - factor;
            }

            // Correct for the position of the mouse
            _viewR += (decimal)oldDistance.X * offset;
            _viewI += (decimal)oldDistance.Y * offset;

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

            int maxiter = (int)(-512 * Math.Log10((double)_zoomLevel));
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
            decimal xMin = -halfWidth * _zoomLevel + _viewR;
            decimal xMax = halfWidth * _zoomLevel + _viewR;
            decimal yMin = -halfHeight * _zoomLevel + _viewI;
            decimal yMax = halfHeight * _zoomLevel + _viewI;
            decimal step = _zoomLevel;

            Func<bool> DoRender = null;
            Action AbortRender = null;

            if (LanguageCs)
            {
                if (!PrecisionDouble128)
                {
                    (var render, var abort) = FractalRenderer.SelectRender(addPixel, () => false, MethodCpuSimd, PrecisionDouble, ThreadModelMulti);
                    DoRender = () => render((double)xMin, (double)xMax, (double)yMin, (double)yMax, (double)step, maxiter);
                    AbortRender = () => abort();
                }
                else
                {
                    var renderer = new ScalarDoubleDoubleRenderer(addPixel, () => false);
                    if (ThreadModelMulti)
                    {
                        DoRender = () => renderer.RenderMultiThreaded(xMin, xMax, yMin, yMax, step, maxiter);
                    }
                    else
                    {
                        DoRender = () => renderer.RenderSingleThreaded(xMin, xMax, yMin, yMax, step, maxiter);
                    }
                }
            }
            else if (_languageCpp)
            {
                DoRender = () =>
                {
                    fixed (int* fixedBuffer = buffer)
                    {
                        RenderMandelbrotCpp(MethodGpu, PrecisionDouble, ThreadModelMulti, (double)_zoomLevel, (double)_viewR, (double)_viewI, fixedBuffer, width, height);
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

                double fullSetPixels = 4 / (double)_zoomLevel;
                double meters = _funDistances.PixelsToMeters(fullSetPixels);

                Title = $"Mandelbrot Rendering Took {_stopwatch.ElapsedMilliseconds}ms ({width}x{height})  Whole Set Size = {Math.Round(meters)} meters (> {_funDistances.PixelsToFunDistance(fullSetPixels)})  Iterations = {maxiter}  Zoom Level = {Math.Round(0.002 / (double)_zoomLevel, 1)}";

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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MethodDouble128Enabled)));
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
                if (_languageCpp && PrecisionDouble128)
                {
                    PrecisionDouble128 = false;
                    PrecisionDouble = true;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PrecisionDouble128)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PrecisionDouble)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MethodGpuEnabled)));
            }
        }
        public bool ThreadModelMulti { get; set; } = true;
        public bool ThreadModelSingle { get; set; } = false;
        public bool PrecisionFloat { get; set; } = true;
        public bool PrecisionDouble { get; set; } = false;
        public bool PrecisionDouble128 { get; set; } = false;
        public bool MethodCpuSimd { get; set; } = true;
        public bool MethodCpuFpu { get; set; } = false;
        public bool MethodGpu { get; set; } = false;
        public bool MethodCpuSimdEnabled => LanguageCs;
        public bool MethodGpuEnabled => LanguageCpp;
        public bool MethodDouble128Enabled => LanguageCs;

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
