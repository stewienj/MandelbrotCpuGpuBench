﻿using System;
using System.Threading.Tasks;

namespace Algorithms
{
  // This class contains renderers that use scalar floats
  internal class ScalarFloatRenderer : FractalRenderer
  {
    public ScalarFloatRenderer(Action<int, int, int> dp, Func<bool> abortFunc)
        : base(dp, abortFunc)
    {
    }

    protected const float limit = 4.0f;

    // Render the fractal using a Complex data type on a single thread with scalar floats
    public void RenderSingleThreadedWithADT(double xmind, double xmaxd, double ymind, double ymaxd, double stepd, double maxIterations)
    {

      float xmin = (float)xmind;
      float xmax = (float)xmaxd;
      float ymin = (float)ymind;
      float ymax = (float)ymaxd;
      float step = (float)stepd;

      int yp = 0;
      for (float y = ymin; y < ymax && !Abort; y += step, yp++)
      {
        int xp = 0;
        for (float x = xmin; x < xmax; x += step, xp++)
        {
          ComplexFloat num = new ComplexFloat(x, y);
          ComplexFloat accum = num;
          int iters = 0;
          float sqabs = 0f;
          do
          {
            accum = accum.Square();
            accum += num;
            iters++;
            sqabs = accum.SquareAbs();
          } while (sqabs < limit && iters < maxIterations);

          DrawPixel(xp, yp, iters);
        }
      }
    }

    // Render the fractal with no data type abstraction on a single thread with scalar floats
    public void RenderSingleThreadedNoADT(double xmind, double xmaxd, double ymind, double ymaxd, double stepd, double maxIterations)
    {

      float xmin = (float)xmind;
      float xmax = (float)xmaxd;
      float ymin = (float)ymind;
      float ymax = (float)ymaxd;
      float step = (float)stepd;

      int yp = 0;
      for (float y = ymin; y < ymax && !Abort; y += step, yp++)
      {
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
    }

    // Render the fractal using a Complex data type on a single thread with scalar floats
    public void RenderMultiThreadedWithADT(double xmind, double xmaxd, double ymind, double ymaxd, double stepd, double maxIterations)
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
                ComplexFloat num = new ComplexFloat(x, y);
                ComplexFloat accum = num;
                int iters = 0;
                float sqabs = 0f;
                do
                {
                  accum = accum.Square();
                  accum += num;
                  iters++;
                  sqabs = accum.SquareAbs();
                } while (sqabs < limit && iters < maxIterations);

                DrawPixel(xp, yp, iters);
              }
            });
    }

    // Render the fractal with no data type abstraction on multiple threads with scalar floats
    public void RenderMultiThreadedNoADT(double xmind, double xmaxd, double ymind, double ymaxd, double stepd, double maxIterations)
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
    }
  }
}
