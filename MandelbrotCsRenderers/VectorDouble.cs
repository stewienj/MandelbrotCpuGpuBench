using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Algorithms
{
  // This contains renderers that only use Vector<double>'s and Vector<long> types. 
  // It's not very performant because for the initial release of SIMD support 
  // the JIT compiler (RyuJIT CTP3) doesn't handle Vector<long> types particularly well.
  // And even with subsequent releases of the JIT, AVX doesn't have 32 byte int operations.
  // That capability arrived with AVX2 (Haswell).
  // For a well commented implementation, see VectorFloat.cs
  internal class VectorDoubleRenderer : FractalRenderer
  {
    private const double limit = 4.0;

    private static Vector<double> dummy = Vector<double>.One;

    public VectorDoubleRenderer(Action<int, int, int> dp, Func<bool> abortFunc)
        : base(dp, abortFunc)
    {
    }

    // Render the fractal on a single thread using raw Vector<double> data types
    // For a well commented version, go see VectorFloatRenderer.RenderSingleThreadedWithADT in VectorFloat.cs
    public void RenderSingleThreadedNoADT(double xmin, double xmax, double ymin, double ymax, double step, double maxIterations)
    {

      Vector<double> vlimit = new Vector<double>(limit);
      Vector<double> vinc = new Vector<double>((double)Vector<double>.Count * step);
      Vector<double> vstep = new Vector<double>(step);
      Vector<long> vmax_iters = new Vector<long>((long)maxIterations);
      Vector<double> vxmin = VectorHelper.Create(i => xmin + step * i);

      double y = ymin;
      int yp = 0;
      for (Vector<double> vy = new Vector<double>(ymin); y <= ymax && !Abort; vy += vstep, y += step, yp++)
      {
        int xp = 0;
        Vector<double> vxmaxd = new Vector<double>(xmax);
        for (Vector<double> vx = vxmin; Vector.LessThanOrEqualAny(vx, vxmaxd); vx += vinc, xp += Vector<long>.Count)
        {
          Vector<double> accumx = vx;
          Vector<double> accumy = vy;

          Vector<long> viters = Vector<long>.Zero;
          Vector<long> increment = Vector<long>.One;

          do
          {
            Vector<double> naccumx = accumx * accumx - accumy * accumy;
            Vector<double> naccumy = accumx * accumy + accumx * accumy;
            accumx = naccumx + vx;
            accumy = naccumy + vy;
            viters += increment;
            Vector<double> sqabs = accumx * accumx + accumy * accumy;
            Vector<long> vCond = Vector.LessThanOrEqual(sqabs, vlimit) &
                Vector.LessThanOrEqual(viters, vmax_iters);
            increment = increment & vCond;
          } while (increment != Vector<long>.Zero);

          viters.ForEach((iter, elemNum) => DrawPixel(xp + elemNum, yp, (int)iter));
        }
      }
    }

    // Render the fractal on a single thread using the ComplexVecDouble data type
    // For a well commented version, go see VectorFloatRenderer.RenderSingleThreadedWithADT in VectorFloat.cs
    public void RenderSingleThreadedWithADT(double xmin, double xmax, double ymin, double ymax, double step, double maxIterations)
    {
      Vector<double> vlimit = new Vector<double>(limit);
      Vector<double> vinc = new Vector<double>((double)Vector<double>.Count * step);
      Vector<double> vstep = new Vector<double>(step);
      Vector<long> vmax_iters = new Vector<long>((long)maxIterations);
      Vector<double> vxmax = new Vector<double>(xmax);
      Vector<double> vxmin = VectorHelper.Create(i => xmin + step * i);

      double y = ymin;
      int yp = 0;
      for (Vector<double> vy = new Vector<double>(ymin); y <= ymax && !Abort; vy += vstep, y += step, yp++)
      {
        int xp = 0;
        for (Vector<double> vx = vxmin; Vector.LessThanOrEqualAny(vx, vxmax); vx += vinc, xp += Vector<long>.Count)
        {
          ComplexVecDouble num = new ComplexVecDouble(vx, vy);
          ComplexVecDouble accum = num;

          Vector<long> viters = Vector<long>.Zero;
          Vector<long> increment = Vector<long>.One;

          do
          {
            accum = accum.Square() + num;
            viters += increment;
            Vector<long> vCond = Vector.LessThanOrEqual(accum.SquareAbs(), vlimit) &
                Vector.LessThanOrEqual(viters, vmax_iters);
            increment = increment & vCond;
          } while (increment != Vector<long>.Zero);

          viters.ForEach((iter, elemNum) => DrawPixel(xp + elemNum, yp, (int)iter));
        }
      }
    }

    // Render the fractal on multiple threads using raw Vector<double> data types
    // For a well commented version, go see VectorFloatRenderer.RenderSingleThreadedWithADT in VectorFloat.cs
    public void RenderMultiThreadedNoADT(double xmin, double xmax, double ymin, double ymax, double step, double maxIterations)
    {
      Vector<long> vmax_iters = new Vector<long>((int)maxIterations);
      Vector<double> vlimit = new Vector<double>(limit);
      Vector<double> vinc = new Vector<double>((double)Vector<double>.Count * step);
      Vector<double> vxmax = new Vector<double>(xmax);
      Vector<double> vstep = new Vector<double>(step);
      Vector<double> vxmin = VectorHelper.Create(i => xmin + step * i);

      Parallel.For(0, (int)(((ymax - ymin) / step) + .5), (yp) =>
      {
        if (Abort)
          return;

        Vector<double> vy = new Vector<double>(ymin + step * yp);
        int xp = 0;
        for (Vector<double> vx = vxmin; Vector.LessThanOrEqualAny(vx, vxmax); vx += vinc, xp += Vector<long>.Count)
        {
          Vector<double> accumx = vx;
          Vector<double> accumy = vy;

          Vector<long> viters = Vector<long>.Zero;
          Vector<long> increment = Vector<long>.One;

          do
          {
            Vector<double> naccumx = accumx * accumx - accumy * accumy;
            Vector<double> naccumy = accumx * accumy + accumx * accumy;
            accumx = naccumx + vx;
            accumy = naccumy + vy;
            viters += increment;
            Vector<double> sqabs = accumx * accumx + accumy * accumy;
            Vector<long> vCond = Vector.LessThanOrEqual(sqabs, vlimit) &
                      Vector.LessThanOrEqual(viters, vmax_iters);
            increment = increment & vCond;
          } while (increment != Vector<long>.Zero);

          viters.ForEach((iter, elemNum) => DrawPixel(xp + elemNum, yp, (int)iter));
        }
      });
    }

    // Render the fractal on multiple threads using the ComplexVecDouble data type
    // For a well commented version, go see VectorFloatRenderer.RenderSingleThreadedWithADT in VectorFloat.cs
    public void RenderMultiThreadedWithADT(double xmin, double xmax, double ymin, double ymax, double step, double maxIterations)
    {

      Vector<long> vmax_iters = new Vector<long>((int)maxIterations);
      Vector<double> vlimit = new Vector<double>(limit);
      Vector<double> vinc = new Vector<double>((double)Vector<double>.Count * step);
      Vector<double> vxmax = new Vector<double>(xmax);
      Vector<double> vstep = new Vector<double>(step);
      Vector<double> vxmin = VectorHelper.Create(i => xmin + step * i);

      Parallel.For(0, (int)(((ymax - ymin) / step) + .5), (yp) =>
      {
        if (Abort)
          return;

        Vector<double> vy = new Vector<double>(ymin + step * yp);
        int xp = 0;
        for (Vector<double> vx = vxmin; Vector.LessThanOrEqualAny(vx, vxmax); vx += vinc, xp += Vector<long>.Count)
        {
          ComplexVecDouble num = new ComplexVecDouble(vx, vy);
          ComplexVecDouble accum = num;

          Vector<long> viters = Vector<long>.Zero;
          Vector<long> increment = Vector<long>.One;

          do
          {
            accum = accum.Square() + num;
            viters += increment;
            Vector<long> vCond = Vector.LessThanOrEqual(accum.SquareAbs(), vlimit) &
                      Vector.LessThanOrEqual(viters, vmax_iters);
            increment = increment & vCond;
          } while (increment != Vector<long>.Zero);

          viters.ForEach((iter, elemNum) => DrawPixel(xp + elemNum, yp, (int)iter));
        }
      });
    }
  }
}
