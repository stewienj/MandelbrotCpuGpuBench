using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Algorithms
{
  // This class contains renderers that use scalar doubles
  public class ScalarDecimalRenderer : FractalRenderer
  {
    public ScalarDecimalRenderer(Action<int, int, int> dp, Func<bool> abortFunc)
        : base(dp, abortFunc)
    {
    }

    protected const decimal limit = 4.0M;

    // Render the fractal with no data type abstraction on a single thread with scalar doubles
    public bool RenderSingleThreaded(decimal xmin, decimal xmax, decimal ymin, decimal ymax, decimal step, int maxIterations)
    {
      int yp = 0;
      for (decimal y = ymin; y < ymax && !Abort; y += step, yp++)
      {
        if (Abort)
          return false;
        int xp = 0;
        for (decimal x = xmin; x < xmax; x += step, xp++)
        {
          decimal accumx = x;
          decimal accumy = y;
          int iters = 0;
          decimal sqabs = 0.0M;
          do
          {
            decimal naccumx = accumx * accumx - accumy * accumy;
            decimal naccumy = 2.0M * accumx * accumy;
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
    public bool RenderMultiThreaded(decimal xmin, decimal xmax, decimal ymin, decimal ymax, decimal step, int maxIterations)
    {
      Parallel.For(0, (int)(((ymax - ymin) / step) + .5M), (yp) =>
      {
        if (Abort)
          return;
        decimal y = ymin + step * yp;
        int xp = 0;
        for (decimal x = xmin; x < xmax; x += step, xp++)
        {
          decimal accumx = x;
          decimal accumy = y;
          int iters = 0;
          decimal sqabs = 0.0M;
          do
          {
            decimal naccumx = accumx * accumx - accumy * accumy;
            decimal naccumy = 2.0M * accumx * accumy;
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
