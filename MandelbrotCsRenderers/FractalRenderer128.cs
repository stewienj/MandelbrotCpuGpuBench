using Swordfish.NET.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotCsRenderers
{
    public abstract class FractalRenderer128 : FractalRendererBase
    {
        public delegate bool Render128(Float128 xmin, Float128 xmax, Float128 ymin, Float128 ymax, Float128 step, int maxIterations);

        public FractalRenderer128(Action<int, int, int> draw, Func<bool> checkAbort) : base(draw, checkAbort)
        {
        }

        public abstract bool RenderMultiThreaded(Float128 xmin, Float128 xmax, Float128 ymin, Float128 ymax, Float128 step, int maxIterations);

        public abstract bool RenderSingleThreaded(Float128 xmin, Float128 xmax, Float128 ymin, Float128 ymax, Float128 step, int maxIterations);

        public static (Render128, Action) SelectRender128(Action<int, int, int> draw, Func<bool> abort, bool useVectorTypes, bool isMultiThreaded, bool useFast)
        {
            Render128 render;
            FractalRenderer128 r;

            r = (useVectorTypes, useFast) switch
            {
                (false, false) => new ScalarFloat128Renderer(draw, abort),
                (false, true) => new ScalarFloat128FastRenderer(draw, abort),
                (true, false) => new VectorFloat128Renderer(draw, abort),
                (true, true) => new VectorFloat128FastRenderer(draw, abort),
            };

            
            if (isMultiThreaded)
            {
                render = r.RenderMultiThreaded;
            }
            else // !isMultiThreaded
            {
                render = r.RenderSingleThreaded;
            }
            return (render, () => r.DoAbort());

        }

    }
}
