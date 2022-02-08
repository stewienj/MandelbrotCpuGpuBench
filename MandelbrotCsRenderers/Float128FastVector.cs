using Swordfish.NET.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotCsRenderers
{
    public struct Float128FastVector
    {
        public static Float128FastVector Zero = new Float128FastVector(0.0);
        public static Float128FastVector One = new Float128FastVector(1.0);

        public Vector<double> Hi;
        public Vector<double> Lo;

        public Float128FastVector(double initial) : this(new Float128(initial)) { }

        public Float128FastVector(Float128 initial)
        {
            Hi = new Vector<double>(initial.Hi);
            Lo = new Vector<double>(initial.Lo);
        }

        public Float128FastVector(double[] dataHi, double[] dataLo)
        {
            Hi = new Vector<double>(dataHi);
            Lo = new Vector<double>(dataLo);
        }

        public Float128FastVector(Vector<double> hi, Vector<double> lo)
        {
            Hi = hi;
            Lo = lo;
        }

        public static Float128FastVector operator +(Float128FastVector x, Float128FastVector y)
        {
            // Fast Add
            Vector<double> a, b, c;
            b = x.Hi + y.Hi;
            a = x.Hi - b;
            c = ((x.Hi - (a + b)) + (a + y.Hi)) + (x.Lo + y.Lo);
            a = b + c;
            return new Float128FastVector(a, c + (b - a));
        }

        public static Float128FastVector operator -(Float128FastVector x, Float128FastVector y)
        {
            // Fast Sub
            Vector<double> a, b, c;
            b = x.Hi - y.Hi;
            a = x.Hi - b;
            c = (((x.Hi - (a + b)) + (a - y.Hi)) + x.Lo) - y.Lo;
            a = b + c;
            return new Float128FastVector(a, c + (b - a));
        }

        public static Float128FastVector operator *(Float128FastVector x, Float128FastVector y)
        {
            Vector<double> a, b, c, d, e;
            a = 0x08000001 * x.Hi;
            a += x.Hi - a;
            b = x.Hi - a;
            c = 0x08000001 * y.Hi;
            c += y.Hi - c;
            d = y.Hi - c;
            e = x.Hi * y.Hi;
            c = (((a * c - e) + (a * d + b * c)) + b * d) + (x.Lo * y.Hi + x.Hi * y.Lo);
            a = e + c;
            return new Float128FastVector(a, c + (e - a));
        }

        public Float128FastVector Sqr()
        {
            Vector<double> a, b, c;
            a = 0x08000001 * Hi;
            a += Hi - a;
            b = Hi - a;
            c = Hi * Hi;
            b = ((((a * a - c) + a * b * 2) + b * b) + Hi * Lo * 2) + Lo * Lo;
            a = b + c;
            return new Float128FastVector(a, b + (c - a));
        }


        public static Float128FastVector operator &(Float128FastVector x, Float128FastVector y)
        {
            return new Float128FastVector(x.Hi & y.Hi, x.Lo & y.Lo);
        }

        public static bool LessThanOrEqualAll(Float128FastVector x, Float128FastVector y)
        {
            if (!Vector.EqualsAll(x.Hi, y.Hi))
            {
                return Vector.LessThanOrEqualAll(x.Hi, y.Hi);
            }
            else
            {
                return Vector.LessThanOrEqualAll(x.Lo, y.Lo);
            }
        }

        public Float128FastVector MulPwrOf2(double y)
        {
            return new Float128FastVector(Hi * y, Lo * y);
        }

        /*
        public static Float128 Max(Float128 x, Float128 y)
        {
            if (x.Hi > y.Hi || (x.Hi == y.Hi && x.Lo > y.Lo))
            {
                return x;
            }
            return y;
        }
        */

    }
}
