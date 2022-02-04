using System;

namespace MandelbrotCsRenderers
{
    public abstract class FractalRenderer64 : FractalRendererBase
    {
        public delegate bool Render(double xmin, double xmax, double ymin, double ymax, double step, int maxIterations);

        public FractalRenderer64(Action<int, int, int> draw, Func<bool> checkAbort) : base(draw, checkAbort)
        {
        }

        public abstract bool RenderMultiThreaded(double xmind, double xmaxd, double ymind, double ymaxd, double stepd, int maxIterations);
        public abstract bool RenderSingleThreaded(double xmind, double xmaxd, double ymind, double ymaxd, double stepd, int maxIterations);

        public static (Render, Action) SelectRender(Action<int, int, int> draw, Func<bool> abort, bool useVectorTypes, bool doublePrecision, bool isMultiThreaded)
        {
            FractalRenderer64 r = (useVectorTypes, doublePrecision) switch
            {
                (true, true) => new VectorDoubleRenderer(draw, abort),
                (true, false) => new VectorFloatRenderer(draw, abort),
                (false, true) => new ScalarDoubleRenderer(draw, abort),
                (false, false) => new ScalarFloatRenderer(draw, abort),
            };

            Render render = isMultiThreaded switch
            {
                true => r.RenderMultiThreaded,
                false => r.RenderSingleThreaded,
            };

            return (render, () => r.DoAbort());

        }
    }
}
