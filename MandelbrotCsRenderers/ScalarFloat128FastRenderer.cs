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

        protected Float128 limit = new Float128(4.0);


        public override bool RenderSingleThreaded(Float128 xmin, Float128 xmax, Float128 ymin, Float128 ymax, Float128 step, int maxIterations)
        {
            int yp = 0;
            for (Float128 y = ymin; y.SubFast(ymax).Hi < 0 && !Abort; y = y.AddFast(step), yp++)
            {
                if (Abort)
                    return false;
                int xp = 0;
                for (Float128 x = xmin; x.SubFast(xmax).Hi < 0; x = x.AddFast(step), xp++)
                {
                    Float128 accumx = x;
                    Float128 accumy = y;
                    int iters = 0;
                    Float128 sqabs = new Float128(0);
                    do
                    {
                        Float128 naccumx = accumx.Sqr().SubFast(accumy.Sqr());
                        Float128 naccumy = accumx.Mul(accumx).MulPwrOf2(2);
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

        public override bool RenderMultiThreaded(Float128 xmin, Float128 xmax, Float128 ymin, Float128 ymax, Float128 step, int maxIterations)
        {
            Float128 HALF = new Float128(0.5);

            //Parallel.For(0, (int)(((ymax - ymin) / step) + .5M), (yp) =>
            Parallel.For(0, ymax.SubFast(ymin).Div(step).AddFast(HALF).IntValue(), (yp) =>
            {
                if (Abort)
                    return;
                Float128 y = ymin.AddFast(step.Mul(yp));
                int xp = 0;
                for (Float128 x = xmin; x.SubFast(xmax).Hi < 0; x = x.AddFast(step), xp++)
                {
                    Float128 accumx = x;
                    Float128 accumy = y;
                    int iters = 0;
                    Float128 sqabs = new Float128(0);
                    do
                    {
                        Float128 naccumx = accumx.Sqr().SubFast(accumy.Sqr());
                        Float128 naccumy = accumx.Mul(accumy).MulPwrOf2(2);
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