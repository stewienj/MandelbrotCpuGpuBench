using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Algorithms
{
  // This contains renderers that only use Vector<float>'s with no Vector<int> types. It's
  // primarily useful when targeting AVX (not AVX2), because AVX doesn't support 256 bits of
  // integer values, only floating point values, so using Vector<int> results in less than
  // optimal code gen. For a well commented implementation, see VectorFloat.cs
  internal class VectorFloatRenderer : FractalRenderer
  {
    private const float limit = 4.0f;

    public VectorFloatRenderer(Action<int, int, int> dp, Func<bool> abortFunc)
        : base(dp, abortFunc)
    {
    }

    // Render the fractal on multiple threads using raw Vector<float> data types
    // For a well commented version, go see VectorFloatRenderer.RenderSingleThreadedWithADT in VectorFloat.cs
    public bool RenderMultiThreaded(double xmind, double xmaxd, double ymind, double ymaxd, double stepd, double maxIterations)
    {
      float xmin = (float)xmind;
      float xmax = (float)xmaxd;
      float ymin = (float)ymind;
      float ymax = (float)ymaxd;
      float step = (float)stepd;

      Vector<float> vmax_iters = new Vector<float>((float)maxIterations);
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
      return !Abort;
    }

    // Render the fractal on a single thread using raw Vector<float> data types
    // For a well commented version, go see VectorFloatRenderer.RenderSingleThreadedWithADT in VectorFloat.cs
    public bool RenderSingleThreaded(double xmind, double xmaxd, double ymind, double ymaxd, double stepd, double maxIterations)
    {
      float xmin = (float)xmind;
      float xmax = (float)xmaxd;
      float ymin = (float)ymind;
      float ymax = (float)ymaxd;
      float step = (float)stepd;

      // Initialize a pile of method constant vectors
      Vector<float> vmax_iters = new Vector<float>((float)maxIterations);
      Vector<float> vlimit = new Vector<float>(limit);
      Vector<float> vstep = new Vector<float>(step);
      Vector<float> vxmax = new Vector<float>(xmax);
      Vector<float> vinc = new Vector<float>((float)Vector<float>.Count * step);
      // Use my little helper routine: it's kind of slow, but I find it pleasantly readable.
      // The alternative would be this:
      //    float[] xmins = new float[Vector<float>.Length];
      //    for (int i = 0; i < xmins.Length; i++)
      //        xmins[i] = xmin + step * i;
      //    Vector<float> vxmin = new Vector<float>(xmins);
      // Both allocate some memory, this one just does it in a separate routine
      Vector<float> vxmin = VectorHelper.Create(i => xmin + step * i);

      float y = ymin;
      int yp = 0;
      for (Vector<float> vy = new Vector<float>(ymin); y <= ymax && !Abort; vy += vstep, y += step, yp++)
      {
        if (Abort)
          return false;
        int xp = 0;
        for (Vector<float> vx = vxmin; Vector.LessThanOrEqualAll(vx, vxmax); vx += vinc, xp += Vector<int>.Count)
        {
          Vector<float> accumx = vx;
          Vector<float> accumy = vy;

          Vector<float> viters = Vector<float>.Zero;
          Vector<float> increment = Vector<float>.One;
          do
          {
            // This is work that can be vectorized
            Vector<float> naccumx = accumx * accumx - accumy * accumy;
            Vector<float> naccumy = accumx * accumy + accumx * accumy;
            accumx = naccumx + vx;
            accumy = naccumy + vy;
            // Increment the iteration count Only pixels that haven't already hit the
            // limit will be incremented because the increment variable gets masked below
            viters += increment;
            Vector<float> sqabs = accumx * accumx + accumy * accumy;
            // Create a mask that correspons to the element-wise logical operation
            // "accum <= limit && iters <= max_iters" Note that the bitwise and is used,
            // because the Vector.{comparision} operations return masks, not boolean values
            Vector<float> vCond = Vector.LessThanOrEqual<float>(sqabs, vlimit) &
                Vector.LessThanOrEqual<float>(viters, vmax_iters);
            increment = increment & vCond;
            // Keep going until we have no elements that haven't either hit the value
            // limit or the iteration count
          } while (increment != Vector<float>.Zero);

          // This is another little helper I created. It's definitely kind of slow but I
          // find it pleasantly succinct. It could also be written like this:
          // 
          // for (int eNum = 0; eNum < Vector<int>.Length; eNum++) 
          //     DrawPixel(xp + eNum, yp, viters[eNum]);
          //
          // Neither implementation is particularly fast, because pulling individual elements
          // is a slow operation for vector types.
          viters.ForEach((iter, elemNum) => DrawPixel(xp + elemNum, yp, (int)iter));
        }
      }
      return true;
    }
  }
}
