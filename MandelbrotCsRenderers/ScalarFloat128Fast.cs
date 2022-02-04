using Swordfish.NET.Maths;
using System;
using System.Threading.Tasks;

namespace MandelbrotCsRenderers
{
    // This class contains renderers that use scalar doubles
    public class ScalarFloat128FastRenderer : FractalRenderer128
    {
        public ScalarFloat128FastRenderer(Action<int, int, int> dp, Func<bool> abortFunc)
            : base(dp, abortFunc)
        {
        }

        protected DoubleDouble limit = new DoubleDouble(4.0);
        protected DoubleDouble TWO = new DoubleDouble(2);


        // Render the fractal with no data type abstraction on a single thread with scalar doubles
        public override bool RenderSingleThreaded(decimal xmin, decimal xmax, decimal ymin, decimal ymax, decimal step, int maxIterations)
        {
            return RenderSingleThreadedInternal(
                new DoubleDouble((double)xmin),
                new DoubleDouble((double)xmax),
                new DoubleDouble((double)ymin),
                new DoubleDouble((double)ymax),
                new DoubleDouble((double)step),
                maxIterations);
        }

        public bool RenderSingleThreadedInternal(DoubleDouble xmin, DoubleDouble xmax, DoubleDouble ymin, DoubleDouble ymax, DoubleDouble step, int maxIterations)
        {
            int yp = 0;
            for (DoubleDouble y = ymin; y.SubFast(ymax).Hi < 0 && !Abort; y = y.AddFast(step), yp++)
            {
                if (Abort)
                    return false;
                int xp = 0;
                for (DoubleDouble x = xmin; x.SubFast(xmax).Hi < 0; x = x.AddFast(step), xp++)
                {
                    DoubleDouble accumx = x;
                    DoubleDouble accumy = y;
                    int iters = 0;
                    DoubleDouble sqabs = new DoubleDouble(0);
                    do
                    {
                        DoubleDouble naccumx = accumx.Sqr().SubFast(accumy.Sqr());
                        DoubleDouble naccumy = TWO.Mul(accumx).Mul(accumy);
                        accumx = naccumx.AddFast(x);
                        accumy = naccumy.AddFast(y);
                        iters++;
                        sqabs = accumx.Sqr().AddFast(accumy.Sqr());
                    } while (sqabs.SubFast(limit).Hi < 0 && iters < maxIterations);

                    DrawPixel(xp, yp, iters);
                }
            }
            return true;
        }

        // Render the fractal with no data type abstraction on multiple threads with scalar doubles
        public override bool RenderMultiThreaded(decimal xmin, decimal xmax, decimal ymin, decimal ymax, decimal step, int maxIterations)
        {
            return RenderMultiThreadedInternal(
                new DoubleDouble((double)xmin),
                new DoubleDouble((double)xmax),
                new DoubleDouble((double)ymin),
                new DoubleDouble((double)ymax),
                new DoubleDouble((double)step),
                maxIterations);
        }


        public bool RenderMultiThreadedInternal(DoubleDouble xmin, DoubleDouble xmax, DoubleDouble ymin, DoubleDouble ymax, DoubleDouble step, int maxIterations)
        {
            DoubleDouble HALF = new DoubleDouble(0.5);

            //Parallel.For(0, (int)(((ymax - ymin) / step) + .5M), (yp) =>
            Parallel.For(0, ymax.SubFast(ymin).Div(step).AddFast(HALF).IntValue(), (yp) =>
            {
                if (Abort)
                    return;
                DoubleDouble y = ymin.AddFast(step.Mul(yp));
                int xp = 0;
                for (DoubleDouble x = xmin; x.SubFast(xmax).Hi < 0; x = x.AddFast(step), xp++)
                {
                    DoubleDouble accumx = x;
                    DoubleDouble accumy = y;
                    int iters = 0;
                    DoubleDouble sqabs = new DoubleDouble(0);
                    do
                    {
                        DoubleDouble naccumx = accumx.Sqr().SubFast(accumy.Sqr());
                        DoubleDouble naccumy = TWO.Mul(accumx).Mul(accumy);
                        accumx = naccumx.AddFast(x);
                        accumy = naccumy.AddFast(y);
                        iters++;
                        sqabs = accumx.Sqr().AddFast(accumy.Sqr());
                    } while (sqabs.SubFast(limit).Hi < 0 && iters < maxIterations);

                    DrawPixel(xp, yp, iters);
                }
            });
            return !Abort;
        }
    }
}