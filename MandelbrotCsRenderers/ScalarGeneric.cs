using System;
using System.Numerics;
using System.Runtime.Versioning;
using System.Threading.Tasks;

// Set this https://dunnhq.com/posts/2021/generic-math/

namespace Algorithms
{
    // This class contains renderers that use scalar doubles
    [RequiresPreviewFeatures]
    public class ScalarGeneric<T> : FractalRenderer where T : INumber<T>
    {
        public ScalarGeneric(Action<int, int, int> dp, Func<bool> abortFunc)
            : base(dp, abortFunc)
        {
        }
        protected static T Two = T.One + T.One;
        protected static T Four = Two + Two;
        protected static T Half = Two / Four;
        protected T limit = Four;


        // Render the fractal with no data type abstraction on a single thread with scalar doubles
        public bool RenderSingleThreaded(T xmin, T xmax, T ymin, T ymax, T step, Int64 maxIterations)
        {
            int yp = 0;
            for (T y = ymin; y < ymax && !Abort; y += step, yp++)
            {
                if (Abort)
                    return false;
                int xp = 0;
                for (T x = xmin; x < xmax; x += step, xp++)
                {
                    T accumx = x;
                    T accumy = y;
                    int iters = 0;
                    T sqabs = T.Zero;
                    do
                    {
                        T naccumx = accumx * accumx - accumy * accumy;
                        T naccumy = Two * accumx * accumy;
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
        public bool RenderMultiThreaded(T xmin, T xmax, T ymin, T ymax, T step, Int64 maxIterations)
        {
            Parallel.For(0, int.Parse((((ymax - ymin) / step) + Half).ToString()), (yp) =>
            {
                if (Abort)
                    return;
                T y = ymin;
                for (int i = 0; i < yp; ++i)
                {
                    y += step;
                }
                int xp = 0;
                for (T x = xmin; x < xmax; x += step, xp++)
                {
                    T accumx = x;
                    T accumy = y;
                    int iters = 0;
                    T sqabs = T.Zero;
                    do
                    {
                        T naccumx = accumx * accumx - accumy * accumy;
                        T naccumy = Two * accumx * accumy;
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
