using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Algorithms
{
  // This contains renderers that only use Vector<double>'s with no Vector<long> types. It's
  // primarily useful when targeting AVX (not AVX2), because AVX doesn't support 256 bits of
  // integer values, only floating point values. In addition, for the initial release of
  // SIMD support, the JIT compiler (RyuJIT CTP3) doesn't handle Vector<long> particularly well.
  // For a well commented implementation, see VectorFloat.cs
  internal class VectorDoubleStrictRenderer : FractalRenderer
  {
    private const double limit = 4.0;

    private static Vector<double> dummy;

    static VectorDoubleStrictRenderer()
    {
      dummy = Vector<double>.One;
    }

    public VectorDoubleStrictRenderer(Action<int, int, int> dp, Func<bool> abortFunc)
        : base(dp, abortFunc)
    {
    }

    // Render the fractal on multiple threads using the ComplexVecDouble data type
    // For a well commented version, go see VectorFloatRenderer.RenderSingleThreadedWithADT in VectorFloat.cs
    public void RenderMultiThreadedWithADT(double xmin, double xmax, double ymin, double ymax, double step)
    {
      Vector<double> vmax_iters = new Vector<double>((double)max_iters);
      Vector<double> vlimit = new Vector<double>(limit);
      Vector<double> vstep = new Vector<double>(step);
      Vector<double> vinc = new Vector<double>((double)Vector<double>.Count * step);
      Vector<double> vxmax = new Vector<double>(xmax);
      Vector<double> vxmin = VectorHelper.Create(i => xmin + step * i);

      Parallel.For(0, (int)(((ymax - ymin) / step) + .5), (yp) =>
      {
        if (Abort)
          return;

        Vector<double> vy = new Vector<double>(ymin + step * yp);
        int xp = 0;
        for (Vector<double> vx = vxmin; Vector.LessThanOrEqualAll(vx, vxmax); vx += vinc, xp += Vector<double>.Count)
        {
          ComplexVecDouble num = new ComplexVecDouble(vx, vy);
          ComplexVecDouble accum = num;

          Vector<double> viters = Vector<double>.Zero;
          Vector<double> increment = Vector<double>.One;
          do
          {
            accum = accum.Square() + num;
            viters += increment;
            Vector<double> vCond = Vector.LessThanOrEqual<double>(accum.SquareAbs(), vlimit) &
                      Vector.LessThanOrEqual<double>(viters, vmax_iters);
            increment = increment & vCond;
          } while (increment != Vector<double>.Zero);

          viters.ForEach((iter, elemNum) => DrawPixel(xp + elemNum, yp, (int)iter));
        }
      });
    }

    // Render the fractal on multiple threads using raw Vector<double> data types
    // For a well commented version, go see VectorFloatRenderer.RenderSingleThreadedWithADT in VectorFloat.cs
    public void RenderMultiThreadedNoADT(double xmin, double xmax, double ymin, double ymax, double step)
    {


      Vector<double> vmax_iters = new Vector<double>((double)max_iters);
      Vector<double> vlimit = new Vector<double>(limit);
      Vector<double> vstep = new Vector<double>(step);
      Vector<double> vinc = new Vector<double>((double)Vector<double>.Count * step);
      Vector<double> vxmax = new Vector<double>(xmax);
      Vector<double> vxmin = VectorHelper.Create(i => xmin + step * i);

      Parallel.For(0, (int)(((ymax - ymin) / step) + .5), (yp) =>
      {
        if (Abort)
          return;

        Vector<double> vy = new Vector<double>(ymin + step * yp);
        int xp = 0;
        for (Vector<double> vx = vxmin; Vector.LessThanOrEqualAll(vx, vxmax); vx += vinc, xp += Vector<double>.Count)
        {
          Vector<double> accumx = vx;
          Vector<double> accumy = vy;

          Vector<double> viters = Vector<double>.Zero;
          Vector<double> increment = Vector<double>.One;
          do
          {
            Vector<double> naccumx = accumx * accumx - accumy * accumy;
            Vector<double> naccumy = accumx * accumy + accumx * accumy;
            accumx = naccumx + vx;
            accumy = naccumy + vy;
            viters += increment;
            Vector<double> sqabs = accumx * accumx + accumy * accumy;
            Vector<double> vCond = Vector.LessThanOrEqual<double>(sqabs, vlimit) &
                      Vector.LessThanOrEqual<double>(viters, vmax_iters);
            increment = increment & vCond;
          } while (increment != Vector<double>.Zero);

          viters.ForEach((iter, elemNum) => DrawPixel(xp + elemNum, yp, (int)iter));
        }
      });
    }

    // Render the fractal on a single thread using the ComplexVecDouble data type
    // For a well commented version, go see VectorFloatRenderer.RenderSingleThreadedWithADT in VectorFloat.cs
    public void RenderSingleThreadedWithADT(double xmin, double xmax, double ymin, double ymax, double step)
    {


      Vector<double> vmax_iters = new Vector<double>((double)max_iters);
      Vector<double> vlimit = new Vector<double>(limit);
      Vector<double> vstep = new Vector<double>(step);
      Vector<double> vinc = new Vector<double>((double)Vector<double>.Count * step);
      Vector<double> vxmax = new Vector<double>(xmax);
      Vector<double> vxmin = VectorHelper.Create(i => xmin + step * i);

      double y = ymin;
      int yp = 0;
      for (Vector<double> vy = new Vector<double>(ymin); y <= ymax && !Abort; vy += vstep, y += step, yp++)
      {
        int xp = 0;
        for (Vector<double> vx = vxmin; Vector.LessThanOrEqualAny(vx, vxmax); vx += vinc, xp += Vector<double>.Count)
        {
          ComplexVecDouble num = new ComplexVecDouble(vx, vy);
          ComplexVecDouble accum = num;

          Vector<double> viters = Vector<double>.Zero;
          Vector<double> increment = Vector<double>.One;
          do
          {
            accum = accum.Square() + num;
            viters += increment;
            Vector<double> vCond = Vector.LessThanOrEqual<double>(accum.SquareAbs(), vlimit) &
                Vector.LessThanOrEqual<double>(viters, vmax_iters);
            increment = increment & vCond;
          } while (increment != Vector<double>.Zero);

          viters.ForEach((iter, elemNum) => DrawPixel(xp + elemNum, yp, (int)iter));
        }
      }
    }

    // Render the fractal on a single thread using raw Vector<double> data types
    // For a well commented version, go see VectorFloatRenderer.RenderSingleThreadedWithADT in VectorFloat.cs
    public void RenderSingleThreadedNoADT(double xmin, double xmax, double ymin, double ymax, double step)
    {

      Vector<double> vmax_iters = new Vector<double>((double)max_iters);
      Vector<double> vlimit = new Vector<double>(limit);
      Vector<double> vstep = new Vector<double>(step);
      Vector<double> vinc = new Vector<double>((double)Vector<double>.Count * step);
      Vector<double> vxmax = new Vector<double>(xmax);
      Vector<double> vxmin = VectorHelper.Create(i => xmin + step * i);

      double y = ymin;
      int yp = 0;
      for (Vector<double> vy = new Vector<double>(ymin); y <= ymax && !Abort; vy += vstep, y += step, yp++)
      {
        int xp = 0;
        for (Vector<double> vx = vxmin; Vector.LessThanOrEqualAll(vx, vxmax); vx += vinc, xp += Vector<double>.Count)
        {
          Vector<double> accumx = vx;
          Vector<double> accumy = vy;

          Vector<double> viters = Vector<double>.Zero;
          Vector<double> increment = Vector<double>.One;
          do
          {
            Vector<double> naccumx = accumx * accumx - accumy * accumy;
            Vector<double> naccumy = accumx * accumy + accumx * accumy;
            accumx = naccumx + vx;
            accumy = naccumy + vy;
            viters += increment;
            Vector<double> sqabs = accumx * accumx + accumy * accumy;
            Vector<double> vCond = Vector.LessThanOrEqual<double>(sqabs, vlimit) &
                Vector.LessThanOrEqual<double>(viters, vmax_iters);
            increment = increment & vCond;
          } while (increment != Vector<double>.Zero);

          viters.ForEach((iter, elemNum) => DrawPixel(xp + elemNum, yp, (int)iter));
        }
      }
    }
  }
}
