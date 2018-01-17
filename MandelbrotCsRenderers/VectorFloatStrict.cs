using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Algorithms
{
  // This contains renderers that only use Vector<float>'s with no Vector<int> types. It's
  // primarily useful when targeting AVX (not AVX2), because AVX doesn't support 256 bits of
  // integer values, only floating point values, so using Vector<int> results in less than
  // optimal code gen. For a well commented implementation, see VectorFloat.cs
  internal class VectorFloatStrictRenderer : FractalRenderer
  {
    private const float limit = 4.0f;

    private static Vector<float> dummy;

    static VectorFloatStrictRenderer()
    {
      dummy = Vector<float>.One;
    }

    public VectorFloatStrictRenderer(Action<int, int, int> dp, Func<bool> abortFunc)
        : base(dp, abortFunc)
    {
    }

    // Render the fractal on multiple threads using the ComplexFloatVec data type
    // For a well commented version, go see VectorFloatRenderer.RenderSingleThreadedWithADT in VectorFloat.cs
    public void RenderMultiThreadedWithADT(double xmind, double xmaxd, double ymind, double ymaxd, double stepd)
    {

      float xmin = (float)xmind;
      float xmax = (float)xmaxd;
      float ymin = (float)ymind;
      float ymax = (float)ymaxd;
      float step = (float)stepd;

      Vector<float> vmax_iters = new Vector<float>((float)max_iters);
      Vector<float> vlimit = new Vector<float>(limit);
      Vector<float> vstep = new Vector<float>(step);
      Vector<float> vinc = new Vector<float>((float)Vector<float>.Count * step);
      Vector<float> vxmax = new Vector<float>(xmax);
      Vector<float> vxmin = VectorHelper.Create(i => xmin + step * i);

      Parallel.For(0, (int)(((ymax - ymin) / step) + .5f), (yp) =>
      {
        if (Abort)
          return;

        Vector<float> vy = new Vector<float>(ymin + step * yp);
        int xp = 0;
        for (Vector<float> vx = vxmin; Vector.LessThanOrEqualAll(vx, vxmax); vx += vinc, xp += Vector<int>.Count)
        {
          ComplexVecFloat num = new ComplexVecFloat(vx, vy);
          ComplexVecFloat accum = num;

          Vector<float> viters = Vector<float>.Zero;
          Vector<float> increment = Vector<float>.One;
          do
          {
            accum = accum.Square() + num;
            viters += increment;
            Vector<float> vCond = Vector.LessThanOrEqual<float>(accum.SquareAbs(), vlimit) &
                      Vector.LessThanOrEqual<float>(viters, vmax_iters);
            increment = increment & vCond;
          } while (increment != Vector<float>.Zero);

          viters.ForEach((iter, elemNum) => DrawPixel(xp + elemNum, yp, (int)iter));
        }
      });
    }

    // Render the fractal on multiple threads using raw Vector<float> data types
    // For a well commented version, go see VectorFloatRenderer.RenderSingleThreadedWithADT in VectorFloat.cs
    public void RenderMultiThreadedNoADT(double xmind, double xmaxd, double ymind, double ymaxd, double stepd)
    {

      float xmin = (float)xmind;
      float xmax = (float)xmaxd;
      float ymin = (float)ymind;
      float ymax = (float)ymaxd;
      float step = (float)stepd;

      Vector<float> vmax_iters = new Vector<float>((float)max_iters);
      Vector<float> vlimit = new Vector<float>(limit);
      Vector<float> vstep = new Vector<float>(step);
      Vector<float> vinc = new Vector<float>((float)Vector<float>.Count * step);
      Vector<float> vxmax = new Vector<float>(xmax);
      Vector<float> vxmin = VectorHelper.Create(i => xmin + step * i);

      Parallel.For(0, (int)(((ymax - ymin) / step) + .5f), (yp) =>
      {
        if (Abort)
          return;

        Vector<float> vy = new Vector<float>(ymin + step * yp);
        int xp = 0;
        for (Vector<float> vx = vxmin; Vector.LessThanOrEqualAll(vx, vxmax); vx += vinc, xp += Vector<float>.Count)
        {
          Vector<float> accumx = vx;
          Vector<float> accumy = vy;

          Vector<float> viters = Vector<float>.Zero;
          Vector<float> increment = Vector<float>.One;
          do
          {
            Vector<float> naccumx = accumx * accumx - accumy * accumy;
            Vector<float> naccumy = accumx * accumy + accumx * accumy;
            accumx = naccumx + vx;
            accumy = naccumy + vy;
            viters += increment;
            Vector<float> sqabs = accumx * accumx + accumy * accumy;
            Vector<float> vCond = Vector.LessThanOrEqual<float>(sqabs, vlimit) &
                      Vector.LessThanOrEqual<float>(viters, vmax_iters);
            increment = increment & vCond;
          } while (increment != Vector<float>.Zero);

          viters.ForEach((iter, elemNum) => DrawPixel(xp + elemNum, yp, (int)iter));
        }
      });
    }

    // Render the fractal on a single thread using the ComplexFloatVec data type
    // For a well commented version, go see VectorFloatRenderer.RenderSingleThreadedWithADT in VectorFloat.cs
    public void RenderSingleThreadedWithADT(double xmind, double xmaxd, double ymind, double ymaxd, double stepd)
    {

      float xmin = (float)xmind;
      float xmax = (float)xmaxd;
      float ymin = (float)ymind;
      float ymax = (float)ymaxd;
      float step = (float)stepd;

      Vector<float> vmax_iters = new Vector<float>((float)max_iters);
      Vector<float> vlimit = new Vector<float>(limit);
      Vector<float> vstep = new Vector<float>(step);
      Vector<float> vxmax = new Vector<float>(xmax);
      Vector<float> vinc = new Vector<float>((float)Vector<float>.Count * step);
      Vector<float> vxmin = VectorHelper.Create(i => xmin + step * i);

      float y = ymin;
      int yp = 0;
      for (Vector<float> vy = new Vector<float>(ymin); y <= ymax && !Abort; vy += vstep, y += step, yp++)
      {
        int xp = 0;
        for (Vector<float> vx = vxmin; Vector.LessThanOrEqualAny(vx, vxmax); vx += vinc, xp += Vector<int>.Count)
        {
          ComplexVecFloat num = new ComplexVecFloat(vx, vy);
          ComplexVecFloat accum = num;

          Vector<float> viters = Vector<float>.Zero;
          Vector<float> increment = Vector<float>.One;
          do
          {
            accum = accum.Square() + num;
            viters += increment;
            Vector<float> vCond = Vector.LessThanOrEqual<float>(accum.SquareAbs(), vlimit) &
                Vector.LessThanOrEqual<float>(viters, vmax_iters);
            increment = increment & vCond;
          } while (increment != Vector<float>.Zero);

          viters.ForEach((iter, elemNum) => DrawPixel(xp + elemNum, yp, (int)iter));
        }
      }
    }

    // Render the fractal on a single thread using raw Vector<float> data types
    // For a well commented version, go see VectorFloatRenderer.RenderSingleThreadedWithADT in VectorFloat.cs
    public void RenderSingleThreadedNoADT(double xmind, double xmaxd, double ymind, double ymaxd, double stepd)
    {

      float xmin = (float)xmind;
      float xmax = (float)xmaxd;
      float ymin = (float)ymind;
      float ymax = (float)ymaxd;
      float step = (float)stepd;

      Vector<float> vmax_iters = new Vector<float>(max_iters);
      Vector<float> vlimit = new Vector<float>(limit);
      Vector<float> vstep = new Vector<float>(step);
      Vector<float> vxmax = new Vector<float>(xmax);
      Vector<float> vinc = new Vector<float>((float)Vector<float>.Count * step);
      Vector<float> vxmin = VectorHelper.Create(i => xmin + step * i);

      float y = ymin;
      int yp = 0;
      for (Vector<float> vy = new Vector<float>(ymin); y <= ymax && !Abort; vy += vstep, y += step, yp++)
      {
        int xp = 0;
        for (Vector<float> vx = vxmin; Vector.LessThanOrEqualAll(vx, vxmax); vx += vinc, xp += Vector<int>.Count)
        {
          Vector<float> accumx = vx;
          Vector<float> accumy = vy;

          Vector<float> viters = Vector<float>.Zero;
          Vector<float> increment = Vector<float>.One;
          do
          {
            Vector<float> naccumx = accumx * accumx - accumy * accumy;
            Vector<float> naccumy = accumx * accumy + accumx * accumy;
            accumx = naccumx + vx;
            accumy = naccumy + vy;
            viters += increment;
            Vector<float> sqabs = accumx * accumx + accumy * accumy;
            Vector<float> vCond = Vector.LessThanOrEqual<float>(sqabs, vlimit) &
                Vector.LessThanOrEqual<float>(viters, vmax_iters);
            increment = increment & vCond;
          } while (increment != Vector<float>.Zero);

          viters.ForEach((iter, elemNum) => DrawPixel(xp + elemNum, yp, (int)iter));
        }
      }
    }
  }
}
