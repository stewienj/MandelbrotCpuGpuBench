using System;
using System.Threading.Tasks;

namespace MandelbrotCsRenderers
{
    // This class contains renderers that use scalar doubles
    internal class ScalarDoubleRenderer : FractalRenderer64
    {
        protected const double limit = 4.0;

        public ScalarDoubleRenderer(Action<int, int, int> dp, Func<bool> abortFunc)
            : base(dp, abortFunc)
        {
        }

        // Render the fractal with no data type abstraction on a single thread with scalar doubles
        public override bool RenderSingleThreaded(double xmin, double xmax, double ymin, double ymax, double step, int maxIterations)
        {
            int yp = 0;
            for (double y = ymin; y < ymax && !Abort; y += step, yp++)
            {
                if (Abort)
                    return false;
                int xp = 0;
                for (double x = xmin; x < xmax; x += step, xp++)
                {
                    double accumx = x;
                    double accumy = y;
                    int iters = 0;
                    double sqabs = 0.0;
                    do
                    {
                        double naccumx = accumx * accumx - accumy * accumy;
                        double naccumy = 2.0 * accumx * accumy;
                        accumx = naccumx + x;
                        accumy = naccumy + y;
                        iters++;
                        sqabs = accumx * accumx + accumy * accumy;
                    } while (sqabs < limit && iters < maxIterations);

                    DrawPixel(xp, yp, iters);
                }
            }
            return true;
        }

        // Render the fractal with no data type abstraction on multiple threads with scalar doubles
        public override bool RenderMultiThreaded(double xmin, double xmax, double ymin, double ymax, double step, int maxIterations)
        {
            Parallel.For(0, (int)(((ymax - ymin) / step) + .5), (yp) =>
            {
                if (Abort)
                    return;
                double y = ymin + step * yp;
                int xp = 0;
                for (double x = xmin; x < xmax; x += step, xp++)
                {
                    double accumx = x;
                    double accumy = y;
                    int iters = 0;
                    double sqabs = 0.0;
                    do
                    {
                        double naccumx = accumx * accumx - accumy * accumy;
                        double naccumy = 2.0 * accumx * accumy;
                        accumx = naccumx + x;
                        accumy = naccumy + y;
                        iters++;
                        sqabs = accumx * accumx + accumy * accumy;
                    } while (sqabs < limit && iters < maxIterations);

                    DrawPixel(xp, yp, iters);
                }
            });
            return !Abort;
        }
    }
}
