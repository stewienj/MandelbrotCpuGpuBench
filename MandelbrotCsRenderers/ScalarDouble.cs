using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Algorithms
{
  // This class contains renderers that use scalar doubles
  internal class ScalarDoubleRenderer : FractalRenderer
  {
    public ScalarDoubleRenderer(Action<int, int, int> dp, Func<bool> abortFunc)
        : base(dp, abortFunc)
    {
    }

    protected const double limit = 4.0;

    // Render the fractal using the BCL Complex data type abstraction on a single thread with scalar doubles
    public void RenderSingleThreadedWithADT(double xmin, double xmax, double ymin, double ymax, double step, double maxIterations)
    {
      int yp = 0;
      for (double y = ymin; y < ymax && !Abort; y += step, yp++)
      {
        int xp = 0;
        for (double x = xmin; x < xmax; x += step, xp++)
        {
          Complex num = new Complex(x, y);
          Complex accum = num;
          int iters = 0;
          double sqabs = 0f;
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

    // Render the fractal with no data type abstraction on a single thread with scalar doubles
    public void RenderSingleThreadedNoADT(double xmin, double xmax, double ymin, double ymax, double step, double maxIterations)
    {
      int yp = 0;
      for (double y = ymin; y < ymax && !Abort; y += step, yp++)
      {
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
    }

    // Render the fractal using the BCL Complex data type abstraction on multiple threads with scalar doubles
    public void RenderMultiThreadedWithADT(double xmin, double xmax, double ymin, double ymax, double step, double maxIterations)
    {
      Parallel.For(0, (int)(((ymax - ymin) / step) + .5), (yp) =>
      {
        if (Abort)
          return;
        double y = ymin + step * yp;
        int xp = 0;
        for (double x = xmin; x < xmax; x += step, xp++)
        {
          Complex num = new Complex(x, y);
          Complex accum = num;
          int iters = 0;
          double sqabs = 0f;
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

    // Render the fractal with no data type abstraction on multiple threads with scalar doubles
    public void RenderMultiThreadedNoADT(double xmin, double xmax, double ymin, double ymax, double step, double maxIterations)
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
    }
  }
}
