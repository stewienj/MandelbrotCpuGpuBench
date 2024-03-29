﻿using Swordfish.NET.Maths;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace MandelbrotCsRenderers
{
    // This contains renderers that only use Vector<double>'s with no Vector<long> types. It's
    // primarily useful when targeting AVX (not AVX2), because AVX doesn't support 256 bits of
    // integer values, only floating point values.
    // For a well commented implementation, see VectorFloat.cs
    internal class VectorFloat128FastRenderer : FractalRenderer128
    {
        private const double limit = 4.0;
        private static Float128 HALF = new Float128(0.5);

        public VectorFloat128FastRenderer(Action<int, int, int> dp, Func<bool> abortFunc)
            : base(dp, abortFunc)
        {
        }

        // Helper to construct a vector from a lambda that takes an index. It's not efficient, but I
        // think it's prettier and more succint than the corresponding for loop.
        // Don't use it on a hot code path (i.e. inside a loop)
        public static Float128FastVector Create(Func<int, Float128> creator)
        {
            double[] dataHi = new double[Vector<double>.Count];
            double[] dataLo = new double[Vector<double>.Count];
            for (int i = 0; i < Vector<double>.Count; i++)
            {
                Float128 float128 = creator(i);
                dataHi[i] = float128.Hi;
                dataLo[i] = float128.Lo;
            }
            return new Float128FastVector(dataHi, dataLo);
        }

        // Render the fractal on multiple threads using raw Vector<double> data types
        // For a well commented version, go see VectorFloatRenderer.RenderSingleThreadedWithADT in VectorFloat.cs
        public override bool RenderMultiThreaded(Float128 xmin, Float128 xmax, Float128 ymin, Float128 ymax, Float128 step, int maxIterations)
        {
            Vector<double> vmax_iters = new Vector<double>((double)maxIterations);
            Float128FastVector vlimit = new Float128FastVector(limit);
            Float128FastVector vstep = new Float128FastVector(step);
            Float128FastVector vinc = new Float128FastVector(new Float128((double)Vector<double>.Count) * step);
            Float128FastVector vxmax = new Float128FastVector(xmax);
            Float128FastVector vxmin = Create(i => xmin + step * new Float128((double)i));

            Parallel.For(0, (((ymax - ymin) / step) + HALF).IntValue(), (yp) =>
            {
                if (Abort)
                    return;

                Float128FastVector vy = new Float128FastVector(ymin + step * new Float128((double)yp));
                int xp = 0;
                for (Float128FastVector vx = vxmin; Float128FastVector.LessThanOrEqualAll(vx, vxmax); vx += vinc, xp += Vector<double>.Count)
                {
                    Float128FastVector accumx = vx;
                    Float128FastVector accumy = vy;

                    Vector<double> viters = Vector<double>.Zero;
                    Vector<double> increment = Vector<double>.One;
                    do
                    {
                        Float128FastVector naccumx = accumx.Sqr() - accumy.Sqr();
                        Float128FastVector naccumy = (accumx * accumy).MulPwrOf2(2);
                        accumx = naccumx + vx;
                        accumy = naccumy + vy;
                        viters += increment;
                        Float128FastVector sqabs = accumx.Sqr() + accumy.Sqr();
                        Vector<double> vCond = Vector.LessThanOrEqual<double>(sqabs.Hi, vlimit.Hi) &
                            Vector.LessThanOrEqual<double>(viters, vmax_iters);
                        increment = increment & vCond;
                    } while (increment != Vector<double>.Zero);

                    viters.ForEach((iter, elemNum) => DrawPixel(xp + elemNum, yp, (int)iter));
                }
            });
            return !Abort;
        }

        // Render the fractal on a single thread using raw Vector<double> data types
        // For a well commented version, go see VectorFloatRenderer.RenderSingleThreadedWithADT in VectorFloat.cs
        public override  bool RenderSingleThreaded(Float128 xmin, Float128 xmax, Float128 ymin, Float128 ymax, Float128 step, int maxIterations)
        {
            Vector<double> vmax_iters = new Vector<double>((double)maxIterations);
            Float128FastVector vlimit = new Float128FastVector(limit);
            Float128FastVector vstep = new Float128FastVector(step);
            Float128FastVector vinc = new Float128FastVector(new Float128((double)Vector<double>.Count) * step);
            Float128FastVector vxmax = new Float128FastVector(xmax);
            Float128FastVector vxmin = Create(i => xmin + step * new Float128((double)i));

            Float128 y = ymin;
            int yp = 0;
            for (Float128FastVector vy = new Float128FastVector(ymin); (y - ymax).Hi < 0 && !Abort; vy += vstep, y += step, yp++)
            {
                if (Abort)
                    return false;
                int xp = 0;
                for (Float128FastVector vx = vxmin; Float128FastVector.LessThanOrEqualAll(vx, vxmax); vx += vinc, xp += Vector<double>.Count)
                {
                    Float128FastVector accumx = vx;
                    Float128FastVector accumy = vy;

                    Vector<double> viters = Vector<double>.Zero;
                    Vector<double> increment = Vector<double>.One;
                    do
                    {
                        Float128FastVector naccumx = accumx.Sqr() - accumy.Sqr();
                        Float128FastVector naccumy = (accumx * accumy).MulPwrOf2(2);
                        accumx = naccumx + vx;
                        accumy = naccumy + vy;
                        viters += increment;
                        Float128FastVector sqabs = accumx.Sqr() + accumy.Sqr();
                        Vector<double> vCond = Vector.LessThanOrEqual<double>(sqabs.Hi, vlimit.Hi) &
                            Vector.LessThanOrEqual<double>(viters, vmax_iters);
                        increment = increment & vCond;
                    } while (increment != Vector<double>.Zero);

                    viters.ForEach((iter, elemNum) => DrawPixel(xp + elemNum, yp, (int)iter));
                }
            }
            return true;
        }
    }
}
