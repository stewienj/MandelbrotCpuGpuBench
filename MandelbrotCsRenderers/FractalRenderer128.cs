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
        public delegate bool Render128(DoubleDouble xmin, DoubleDouble xmax, DoubleDouble ymin, DoubleDouble ymax, DoubleDouble step, int maxIterations);

        public FractalRenderer128(Action<int, int, int> draw, Func<bool> checkAbort) : base(draw, checkAbort)
        {
        }

        public abstract bool RenderMultiThreaded(DoubleDouble xmin, DoubleDouble xmax, DoubleDouble ymin, DoubleDouble ymax, DoubleDouble step, int maxIterations);

        public abstract bool RenderSingleThreaded(DoubleDouble xmin, DoubleDouble xmax, DoubleDouble ymin, DoubleDouble ymax, DoubleDouble step, int maxIterations);

        public static (Render128, Action) SelectRender128(Action<int, int, int> draw, Func<bool> abort, bool useVectorTypes, bool isMultiThreaded, bool useFast)
        {
            Render128 render;
            FractalRenderer128 r;

            if (useFast)
            {
                r = new ScalarFloat128FastRenderer(draw, abort);
            }
            else
            {
                r = new ScalarFloat128Renderer(draw, abort);
            }
            
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
