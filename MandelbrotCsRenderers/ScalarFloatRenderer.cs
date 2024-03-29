﻿using System;
using System.Threading.Tasks;

namespace MandelbrotCsRenderers
{
    // This class contains renderers that use scalar floats
    internal class ScalarFloatRenderer : FractalRenderer64
    {
        protected const float limit = 4.0f;

        public ScalarFloatRenderer(Action<int, int, int> dp, Func<bool> abortFunc)
            : base(dp, abortFunc)
        {
        }

        // Render the fractal with no data type abstraction on a single thread with scalar floats
        public override bool RenderSingleThreaded(double xmind, double xmaxd, double ymind, double ymaxd, double stepd, int maxIterations)
        {

            float xmin = (float)xmind;
            float xmax = (float)xmaxd;
            float ymin = (float)ymind;
            float ymax = (float)ymaxd;
            float step = (float)stepd;

            int yp = 0;
            for (float y = ymin; y < ymax && !Abort; y += step, yp++)
            {
                if (Abort)
                    return false;
                int xp = 0;
                for (float x = xmin; x < xmax; x += step, xp++)
                {
                    float accumx = x;
                    float accumy = y;
                    int iters = 0;
                    float sqabs = 0f;
                    do
                    {
                        float naccumx = accumx * accumx - accumy * accumy;
                        float naccumy = 2.0f * accumx * accumy;
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

        // Render the fractal with no data type abstraction on multiple threads with scalar floats
        public override bool RenderMultiThreaded(double xmind, double xmaxd, double ymind, double ymaxd, double stepd, int maxIterations)
        {
            float xmin = (float)xmind;
            float xmax = (float)xmaxd;
            float ymin = (float)ymind;
            float ymax = (float)ymaxd;
            float step = (float)stepd;

            Parallel.For(0, (int)(((ymax - ymin) / step) + .5f), (yp) =>
            {
                if (Abort)
                    return;
                float y = ymin + step * yp;
                int xp = 0;
                for (float x = xmin; x < xmax; x += step, xp++)
                {
                    float accumx = x;
                    float accumy = y;
                    int iters = 0;
                    float sqabs = 0f;
                    do
                    {
                        float naccumx = accumx * accumx - accumy * accumy;
                        float naccumy = 2.0f * accumx * accumy;
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
