using System;

namespace Swordfish.NET.Maths
{
    /**
     * double: 53 bits DoubleDouble: >106 bits
     * 
     * @author Zom-B
     * @since 1.0
     * @see http://crd.lbl.gov/~dhbailey/mpdist/index.html
     * @date 2006/10/22
     */
    public struct DoubleDouble
    {
        public static char[] BASE_36_TABLE = { //
			'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', //
			'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', //
			'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', //
			'U', 'V', 'W', 'X', 'Y', 'Z'          };

        public static char[] ZEROES = { //
			'0', '0', '0', '0', '0', '0', '0', '0', '0', '0', //
			'0', '0', '0', '0', '0', '0', '0', '0', '0', '0', //
			'0', '0', '0', '0', '0', '0', '0', '0', '0', '0', //
			'0', '0', '0', '0', '0', '0', '0', '0', '0', '0', //
			'0', '0', '0', '0', '0', '0', '0', '0', '0', '0', //
			'0', '0', '0', '0', '0', '0', '0', '0', '0', '0', //
			'0', '0', '0', '0', '0', '0', '0', '0', '0', '0', //
			'0', '0', '0', '0', '0', '0', '0', '0', '0', '0', //
			'0', '0', '0', '0', '0', '0', '0', '0', '0', '0', //
			'0', '0', '0', '0', '0', '0', '0', '0', '0', '0', //
			'0', '0', '0', '0', '0'             };

        public static double POSITIVE_INFINITY = Double.MaxValue / 0x08000001;
        public static double NEGATIVE_INFINITY = -DoubleDouble.POSITIVE_INFINITY;
        public static double HALF_EPSILON = 1.1102230246251565E-16;
        public static double EPSILON = 1.232595164407831E-32;

        public static DoubleDouble PI = new DoubleDouble(3.141592653589793, 1.2246467991473532E-16);
        public static DoubleDouble E = new DoubleDouble(2.718281828459045, 1.4456468917292502E-16);
        public static DoubleDouble LOG2 = new DoubleDouble(0.6931471805599453, 2.3190468138462996E-17);
        public static DoubleDouble INV_LOG2 = new DoubleDouble(1.4426950408889634, 2.0355273740931033E-17);

        public double Hi;
        public double Lo;

        // ***********************************************************************//
        // ************************ Creation functions ***************************//
        // ***********************************************************************//

        public DoubleDouble()
        {
            Hi = 0;
            Lo = 0;
        }

        public DoubleDouble(double d)
        {
            Hi = d;
            Lo = 0;
        }

        public DoubleDouble(double hi, double lo)
        {
            Hi = hi;
            Lo = lo;
        }

        public DoubleDouble(DoubleDouble dd)
        {
            Hi = dd.Hi;
            Lo = dd.Lo;
        }

        public void Set(double hi)
        {
            Hi = hi;
            Lo = 0;
        }

        public void Set(double hi, double lo)
        {
            Hi = hi;
            Lo = lo;
        }

        public void Set(DoubleDouble dd)
        {
            Hi = dd.Hi;
            Lo = dd.Lo;
        }

        public static DoubleDouble Random(Random r)
        {
            return new DoubleDouble(r.NextDouble(), r.NextDouble() * DoubleDouble.HALF_EPSILON).Normalize();
        }

        public static DoubleDouble RandomDynamic(Random r)
        {
            DoubleDouble x = new DoubleDouble(r.NextDouble(), r.NextDouble() / (1L << (r.Next(11) + 52)));
            x.MulSelf(DoubleDouble.PowOf2(r.Next(129) - 64));
            x.NormalizeSelf();
            if (r.Next(1) == 1)
            {
                x.NegSelf();
            }
            return x;
        }

        // ***********************************************************************//
        // ************************** Other functions ****************************//
        // ***********************************************************************//

        public override string ToString()
        {
            if (double.IsNaN(Hi))
            {
                return "NaN";
            }
            if (Hi >= DoubleDouble.POSITIVE_INFINITY)
            {
                return "Infinity";
            }
            if (Hi <= DoubleDouble.NEGATIVE_INFINITY)
            {
                return "-Infinity";
            }
            return this.ToString(10);
        }


        // ***********************************************************************//
        // ************************ Temporary functions **************************//
        // ***********************************************************************//

        public DoubleDouble Clone()
        {
            return new DoubleDouble(Hi, Lo);
        }

        public DoubleDouble Normalize()
        {
            double s = Hi + Lo;
            return new DoubleDouble(s, Lo + (Hi - s));
        }

        public void NormalizeSelf()
        {
            double a = Hi;
            Hi = a + Lo;
            Lo = Lo + (a - Hi);
        }

        public int IntValue()
        {
            int rhi = (int)Math.Round(Hi);

            if (Hi == rhi)
            {
                return rhi + (int)Math.Round(Lo);
            }
            if (Math.Abs(rhi - Hi) == 0.5 && Lo < 0.0)
            {
                return rhi - 1;
            }
            return rhi;
        }

        public long LongValue()
        {
            long rhi = (long)Math.Round(Hi);

            if (Hi == rhi)
            {
                return rhi + (long)Math.Round(Lo);
            }
            if (Math.Abs(rhi - Hi) == 0.5 && Lo < 0.0)
            {
                return rhi - 1;
            }
            return rhi;
        }

        public static DoubleDouble Min(DoubleDouble x, DoubleDouble y)
        {
            if (x.Hi < y.Hi || (x.Hi == y.Hi && x.Lo < y.Lo))
            {
                return x;
            }
            return y;
        }

        public static DoubleDouble Max(DoubleDouble x, DoubleDouble y)
        {
            if (x.Hi > y.Hi || (x.Hi == y.Hi && x.Lo > y.Lo))
            {
                return x;
            }
            return y;
        }

        public static int Sign(double x)
        {
            if (x > 0)
            {
                return 1;
            }
            if (x < 0)
            {
                return -1;
            }
            return 0;
        }

        // ***********************************************************************//
        // ************************* Simple functions ****************************//
        // ***********************************************************************//

        public DoubleDouble Round()
        {
            DoubleDouble rounded = new DoubleDouble();

            double rhi = Math.Round(Hi);

            if (Hi == rhi)
            {
                double rlo = Math.Round(Lo);
                rounded.Hi = rhi + rlo;
                rounded.Lo = rlo + (rhi - rounded.Hi);
            }
            else
            {
                if (Math.Abs(rhi - Hi) == 0.5 && Lo < 0.0)
                {
                    rhi--;
                }
                rounded.Hi = rhi;
            }
            return rounded;
        }

        public void RoundSelf()
        {
            double rhi = Math.Round(Hi);

            if (Hi == rhi)
            {
                double rlo = Math.Round(Lo);
                Hi = rhi + rlo;
                Lo = rlo + (rhi - Hi);
            }
            else
            {
                if (Math.Abs(rhi - Hi) == 0.5 && Lo < 0.0)
                {
                    rhi--;
                }
                Hi = rhi;
                Lo = 0;
            }
        }

        public DoubleDouble Floor()
        {
            DoubleDouble floored = new DoubleDouble();

            double rhi = Math.Floor(Hi);

            if (Hi == rhi)
            {
                double rlo = Math.Floor(Lo);
                floored.Hi = rhi + rlo;
                floored.Lo = rlo + (rhi - floored.Hi);
            }
            else
            {
                floored.Hi = rhi;
            }
            return floored;
        }

        public void FloorSelf()
        {
            double rhi = Math.Floor(Hi);

            if (Hi == rhi)
            {
                double rlo = Math.Floor(Lo);
                Hi = rhi + rlo;
                Lo = rlo + (rhi - Hi);
            }
            else
            {
                Hi = rhi;
                Lo = 0;
            }
        }

        public DoubleDouble Ceiling()
        {
            DoubleDouble ceilinged = new DoubleDouble();

            double rhi = Math.Ceiling(Hi);

            if (Hi == rhi)
            {
                double rlo = Math.Ceiling(Lo);
                ceilinged.Hi = rhi + rlo;
                ceilinged.Lo = rlo + (rhi - ceilinged.Hi);
            }
            else
            {
                ceilinged.Hi = rhi;
            }
            return ceilinged;
        }

        public void CeilingSelf()
        {
            double rhi = Math.Ceiling(Hi);

            if (Hi == rhi)
            {
                double rlo = Math.Ceiling(Lo);
                Hi = rhi + rlo;
                Lo = rlo + (rhi - Hi);
            }
            else
            {
                Hi = rhi;
                Lo = 0;
            }
        }

        public DoubleDouble Truncate()
        {
            DoubleDouble truncated = new DoubleDouble();

            double rhi = (long)(Hi);

            if (Hi == rhi)
            {
                double rlo = (long)(Lo);
                truncated.Hi = rhi + rlo;
                truncated.Lo = rlo + (rhi - truncated.Hi);
            }
            else
            {
                truncated.Hi = rhi;
            }
            return truncated;
        }

        public void TruncateSelf()
        {
            double rhi = (long)(Hi);

            if (Hi == rhi)
            {
                double rlo = (long)(Lo);
                Hi = rhi + rlo;
                Lo = rlo + (rhi - Hi);
            }
            else
            {
                Hi = rhi;
                Lo = 0;
            }
        }

        // ***********************************************************************//
        // *********************** Calculation functions *************************//
        // ***********************************************************************//

        public DoubleDouble Neg()
        {
            return new DoubleDouble(-Hi, -Lo);
        }

        public void NegSelf()
        {
            Hi = -Hi;
            Lo = -Lo;
        }

        public DoubleDouble Abs()
        {
            if (Hi < 0)
            {
                return new DoubleDouble(-Hi, -Lo);
            }
            return new DoubleDouble(Hi, Lo);
        }

        public void AbsSelf()
        {
            if (Hi < 0)
            {
                Hi = -Hi;
                Lo = -Lo;
            }
        }

        public DoubleDouble Add(double y)
        {
            double a, b, c;
            b = Hi + y;
            a = Hi - b;
            c = ((Hi - (b + a)) + (y + a)) + Lo;
            a = b + c;
            return new DoubleDouble(a, c + (b - a));
        }

        public void AddSelf(double y)
        {
            double a, b;
            b = Hi + y;
            a = Hi - b;
            Lo = ((Hi - (b + a)) + (y + a)) + Lo;
            Hi = b + Lo;
            Lo += b - Hi;
        }

        public DoubleDouble Add(DoubleDouble y)
        {
            double a, b, c, d, e, f;
            e = Hi + y.Hi;
            d = Hi - e;
            a = Lo + y.Lo;
            f = Lo - a;
            d = ((Hi - (d + e)) + (d + y.Hi)) + a;
            b = e + d;
            c = ((Lo - (f + a)) + (f + y.Lo)) + (d + (e - b));
            a = b + c;
            return new DoubleDouble(a, c + (b - a));
        }

        public void AddSelf(DoubleDouble y)
        {
            double a, b, c, d, e;
            a = Hi + y.Hi;
            b = Hi - a;
            c = Lo + y.Lo;
            d = Lo - c;
            b = ((Hi - (b + a)) + (b + y.Hi)) + c;
            e = a + b;
            Lo = ((Lo - (d + c)) + (d + y.Lo)) + (b + (a - e));
            Hi = e + Lo;
            Lo += e - Hi;
        }

        public DoubleDouble AddFast(DoubleDouble y)
        {
            double a, b, c;
            b = Hi + y.Hi;
            a = Hi - b;
            c = ((Hi - (a + b)) + (a + y.Hi)) + (Lo + y.Lo);
            a = b + c;
            return new DoubleDouble(a, c + (b - a));
        }

        public void AddSelfFast(DoubleDouble y)
        {
            double a, b;
            b = Hi + y.Hi;
            a = Hi - b;
            Lo = ((Hi - (a + b)) + (a + y.Hi)) + (Lo + y.Lo);
            Hi = b + Lo;
            Lo += b - Hi;
        }

        public DoubleDouble Sub(double y)
        {
            double a, b, c;
            b = Hi - y;
            a = Hi - b;
            c = ((Hi - (a + b)) + (a - y)) + Lo;
            a = b + c;
            return new DoubleDouble(a, c + (b - a));
        }

        public DoubleDouble SubR(double x)
        {
            double a, b, c;
            b = x - Hi;
            a = x - b;
            c = ((x - (a + b)) + (a - Hi)) - Lo;
            a = b + c;
            return new DoubleDouble(a, c + (b - a));
        }

        public void SubSelf(double y)
        {
            double a, b;
            b = Hi - y;
            a = Hi - b;
            Lo = ((Hi - (a + b)) + (a - y)) + Lo;
            Hi = b + Lo;
            Lo += b - Hi;
        }

        public void SubRSelf(double x)
        {
            double a, b;
            b = x - Hi;
            a = x - b;
            Lo = ((x - (a + b)) + (a - Hi)) - Lo;
            Hi = b + Lo;
            Lo += b - Hi;
        }

        public DoubleDouble Sub(DoubleDouble y)
        {
            double a, b, c, d, e, f, g;
            g = Lo - y.Lo;
            f = Lo - g;
            e = Hi - y.Hi;
            d = Hi - e;
            d = ((Hi - (d + e)) + (d - y.Hi)) + g;
            b = e + d;
            c = (d + (e - b)) + ((Lo - (f + g)) + (f - y.Lo));
            a = b + c;
            return new DoubleDouble(a, c + (b - a));
        }

        public void SubSelf(DoubleDouble y)
        {
            double a, b, c, d, e;
            c = Lo - y.Lo;
            a = Lo - c;
            e = Hi - y.Hi;
            d = Hi - e;
            d = ((Hi - (d + e)) + (d - y.Hi)) + c;
            b = e + d;
            Lo = (d + (e - b)) + ((Lo - (a + c)) + (a - y.Lo));
            Hi = b + Lo;
            Lo += b - Hi;
        }

        public DoubleDouble SubR(DoubleDouble y)
        {
            double a, b, c, d, e, f, g;
            g = y.Lo - Lo;
            f = y.Lo - g;
            e = y.Hi - Hi;
            d = y.Hi - e;
            d = ((y.Hi - (d + e)) + (d - Hi)) + g;
            b = e + d;
            c = (d + (e - b)) + ((y.Lo - (f + g)) + (f - Lo));
            a = b + c;
            return new DoubleDouble(a, c + (b - a));
        }

        public void SubRSelf(DoubleDouble y)
        {
            double b, d, e, f, g;
            g = y.Lo - Lo;
            f = y.Lo - g;
            e = y.Hi - Hi;
            d = y.Hi - e;
            d = ((y.Hi - (d + e)) + (d - Hi)) + g;
            b = e + d;
            Lo = (d + (e - b)) + ((y.Lo - (f + g)) + (f - Lo));
            Hi = b + Lo;
            Lo = Lo + (b - Hi);
        }

        public DoubleDouble SubFast(DoubleDouble y)
        {
            double a, b, c;
            b = Hi - y.Hi;
            a = Hi - b;
            c = (((Hi - (a + b)) + (a - y.Hi)) + Lo) - y.Lo;
            a = b + c;
            return new DoubleDouble(a, c + (b - a));
        }

        public void SubSelfFast(DoubleDouble y)
        {
            double a, b;
            b = Hi - y.Hi;
            a = Hi - b;
            Lo = (((Hi - (a + b)) + (a - y.Hi)) + Lo) - y.Lo;
            Hi = b + Lo;
            Lo += b - Hi;
        }

        public DoubleDouble MulPwrOf2(double y)
        {
            return new DoubleDouble(Hi * y, Lo * y);
        }

        public void MulSelfPwrOf2(double y)
        {
            Hi *= y;
            Lo *= y;
        }

        public DoubleDouble Mul(double y)
        {
            double a, b, c, d, e;
            a = 0x08000001 * Hi;
            a += Hi - a;
            b = Hi - a;
            c = 0x08000001 * y;
            c += y - c;
            d = y - c;
            e = Hi * y;
            c = (((a * c - e) + (a * d + b * c)) + b * d) + Lo * y;
            a = e + c;
            return new DoubleDouble(a, c + (e - a));
        }

        public void MulSelf(double y)
        {
            double a, b, c, d, e;
            a = 0x08000001 * Hi;
            a += Hi - a;
            b = Hi - a;
            c = 0x08000001 * y;
            c += y - c;
            d = y - c;
            e = Hi * y;
            Lo = (((a * c - e) + (a * d + b * c)) + b * d) + Lo * y;
            Hi = e + Lo;
            Lo += e - Hi;
        }

        public DoubleDouble Mul(DoubleDouble y)
        {
            double a, b, c, d, e;
            a = 0x08000001 * Hi;
            a += Hi - a;
            b = Hi - a;
            c = 0x08000001 * y.Hi;
            c += y.Hi - c;
            d = y.Hi - c;
            e = Hi * y.Hi;
            c = (((a * c - e) + (a * d + b * c)) + b * d) + (Lo * y.Hi + Hi * y.Lo);
            a = e + c;
            return new DoubleDouble(a, c + (e - a));
        }

        public void MulSelf(DoubleDouble y)
        {
            double a, b, c, d, e;
            a = 0x08000001 * Hi;
            a += Hi - a;
            b = Hi - a;
            c = 0x08000001 * y.Hi;
            c += y.Hi - c;
            d = y.Hi - c;
            e = Hi * y.Hi;
            Lo = (((a * c - e) + (a * d + b * c)) + b * d) + (Lo * y.Hi + Hi * y.Lo);
            Hi = e + Lo;
            Lo += e - Hi;
        }

        public DoubleDouble DivPwrOf2(double y)
        {
            return new DoubleDouble(Hi / y, Lo / y);
        }

        public void DivSelfPwrOf2(double y)
        {
            Hi /= y;
            Lo /= y;
        }

        public DoubleDouble Div(double y)
        {
            double a, b, c, d, e, f, g, h;
            f = Hi / y;
            a = 0x08000001 * f;
            a += f - a;
            b = f - a;
            c = 0x08000001 * y;
            c += y - c;
            d = y - c;
            e = f * y;
            g = Hi - e;
            h = Hi - g;
            b = (g + ((((Hi - (h + g)) + (h - e)) + Lo) - (((a * c - e) + (a * d + b * c)) + b * d))) / y;
            a = f + b;
            return new DoubleDouble(a, b + (f - a));
        }

        public void DivSelf(double y)
        {
            double a, b, c, d, e, f, g, h;
            f = Hi / y;
            a = 0x08000001 * f;
            a += f - a;
            b = f - a;
            c = 0x08000001 * y;
            c += y - c;
            d = y - c;
            e = f * y;
            g = Hi - e;
            h = Hi - g;
            Lo = (g + ((((Hi - (h + g)) + (h - e)) + Lo) - (((a * c - e) + (a * d + b * c)) + b * d))) / y;
            Hi = f + Lo;
            Lo += f - Hi;
        }

        public DoubleDouble divr(double y)
        {
            double a, b, c, d, e, f;
            f = y / Hi;
            a = 0x08000001 * Hi;
            a += Hi - a;
            b = Hi - a;
            c = 0x08000001 * f;
            c += f - c;
            d = f - c;
            e = Hi * f;
            b = ((y - e) - ((((a * c - e) + (a * d + b * c)) + b * d) + Lo * f)) / Hi;
            a = f + b;
            return new DoubleDouble(a, b + (f - a));
        }

        public void DivrSelf(double y)
        {
            double a, b, c, d, e, f;
            f = y / Hi;
            a = 0x08000001 * Hi;
            a += Hi - a;
            b = Hi - a;
            c = 0x08000001 * f;
            c += f - c;
            d = f - c;
            e = Hi * f;
            Lo = ((y - e) - ((((a * c - e) + (a * d + b * c)) + b * d) + Lo * f)) / Hi;
            Hi = f + Lo;
            Lo += f - Hi;
        }

        public DoubleDouble Div(DoubleDouble y)
        {
            double a, b, c, d, e, f, g;
            f = Hi / y.Hi;
            a = 0x08000001 * y.Hi;
            a += y.Hi - a;
            b = y.Hi - a;
            c = 0x08000001 * f;
            c += f - c;
            d = f - c;
            e = y.Hi * f;
            c = (((a * c - e) + (a * d + b * c)) + b * d) + y.Lo * f;
            b = Lo - c;
            d = Lo - b;
            a = Hi - e;
            e = (Hi - ((Hi - a) + a)) + b;
            g = a + e;
            e += (a - g) + ((Lo - (d + b)) + (d - c));
            a = g + e;
            b = a / y.Hi;
            f += (e + (g - a)) / y.Hi;
            a = f + b;
            return new DoubleDouble(a, b + (f - a));
        }

        public void DivSelf(DoubleDouble y)
        {
            double a, b, c, d, e, f, g;
            f = Hi / y.Hi;
            a = 0x08000001 * y.Hi;
            a += y.Hi - a;
            b = y.Hi - a;
            c = 0x08000001 * f;
            c += f - c;
            d = f - c;
            e = y.Hi * f;
            c = (((a * c - e) + (a * d + b * c)) + b * d) + y.Lo * f;
            b = Lo - c;
            d = Lo - b;
            a = Hi - e;
            e = (Hi - ((Hi - a) + a)) + b;
            g = a + e;
            e += (a - g) + ((Lo - (d + b)) + (d - c));
            a = g + e;
            Lo = a / y.Hi;
            f += (e + (g - a)) / y.Hi;
            Hi = f + Lo;
            Lo += f - Hi;
        }

        public DoubleDouble DivFast(DoubleDouble y)
        {
            double a, b, c, d, e, f, g;
            f = Hi / y.Hi;
            a = 0x08000001 * y.Hi;
            a += y.Hi - a;
            b = y.Hi - a;
            c = 0x08000001 * f;
            c += f - c;
            d = f - c;
            e = y.Hi * f;
            b = (((a * c - e) + (a * d + b * c)) + b * d) + y.Lo * f;
            a = e + b;
            c = Hi - a;
            g = (c + ((((Hi - c) - a) - ((e - a) + b)) + Lo)) / y.Hi;
            a = f + g;
            return new DoubleDouble(a, g + (f - a));
        }

        public void DivSelfFast(DoubleDouble y)
        {
            double a, b, c, d, e, f;
            f = Hi / y.Hi;
            a = 0x08000001 * y.Hi;
            a += y.Hi - a;
            b = y.Hi - a;
            c = 0x08000001 * f;
            c += f - c;
            d = f - c;
            e = y.Hi * f;
            b = (((a * c - e) + (a * d + b * c)) + b * d) + y.Lo * f;
            a = e + b;
            c = Hi - a;
            Lo = (c + ((((Hi - c) - a) - ((e - a) + b)) + Lo)) / y.Hi;
            Hi = f + Lo;
            Lo += f - Hi;
        }

        public DoubleDouble Reciprocal()
        {
            double a, b, c, d, e, f;
            f = 1 / Hi;
            a = 0x08000001 * Hi;
            a += Hi - a;
            b = Hi - a;
            c = 0x08000001 * f;
            c += f - c;
            d = f - c;
            e = Hi * f;
            b = ((1 - e) - ((((a * c - e) + (a * d + b * c)) + b * d) + Lo * f)) / Hi;
            a = f + b;
            return new DoubleDouble(a, b + (f - a));
        }

        public void ReciprocalSelf()
        {
            double a, b, c, d, e, f;
            f = 1 / Hi;
            a = 0x08000001 * Hi;
            a += Hi - a;
            b = Hi - a;
            c = 0x08000001 * f;
            c += f - c;
            d = f - c;
            e = Hi * f;
            Lo = ((1 - e) - ((((a * c - e) + (a * d + b * c)) + b * d) + Lo * f)) / Hi;
            Hi = f + Lo;
            Lo += f - Hi;
        }

        public DoubleDouble Sqr()
        {
            double a, b, c;
            a = 0x08000001 * Hi;
            a += Hi - a;
            b = Hi - a;
            c = Hi * Hi;
            b = ((((a * a - c) + a * b * 2) + b * b) + Hi * Lo * 2) + Lo * Lo;
            a = b + c;
            return new DoubleDouble(a, b + (c - a));
        }

        public void SqrSelf()
        {
            double a, b, c;
            a = 0x08000001 * Hi;
            a += Hi - a;
            b = Hi - a;
            c = Hi * Hi;
            Lo = ((((a * a - c) + a * b * 2) + b * b) + Hi * Lo * 2) + Lo * Lo;
            Hi = c + Lo;
            Lo += c - Hi;
        }

        public DoubleDouble Sqrt()
        {
            if (Hi == 0 && Lo == 0)
            {
                return new DoubleDouble();
            }

            double a, b, c, d, e, f, g, h;
            g = 1 / Math.Sqrt(Hi);
            h = Hi * g;
            g *= 0.5;
            a = 0x08000001 * h;
            a += h - a;
            b = h - a;
            c = h * h;
            b = ((a * a - c) + a * b * 2) + b * b;
            a = Lo - b;
            f = Lo - a;
            e = Hi - c;
            d = Hi - e;
            d = ((Hi - (d + e)) + (d - c)) + a;
            c = e + d;
            b = (d + (e - c)) + ((Lo - (f + a)) + (f - b));
            a = c + b;
            b += (c - a);
            c = 0x08000001 * a;
            c += a - c;
            d = a - c;
            e = 0x08000001 * g;
            e += g - e;
            f = g - e;
            a = a * g;
            e = ((c * e - a) + (c * f + d * e)) + d * f;
            e += b * g;
            b = a + e;
            e += a - b;
            f = b + h;
            c = b - f;
            return new DoubleDouble(f, e + ((b - (f + c)) + (h + c)));
        }

        public void SqrtSelf()
        {
            if (Hi == 0 && Lo == 0)
            {
                return;
            }

            double a, b, c, d, e, f, g, h;
            g = 1 / Math.Sqrt(Hi);
            h = Hi * g;
            g *= 0.5;
            a = 0x08000001 * h;
            a += h - a;
            b = h - a;
            c = h * h;
            b = ((a * a - c) + a * b * 2) + b * b;
            a = Lo - b;
            f = Lo - a;
            e = Hi - c;
            d = Hi - e;
            d = ((Hi - (d + e)) + (d - c)) + a;
            c = e + d;
            b = (d + (e - c)) + ((Lo - (f + a)) + (f - b));
            a = c + b;
            b += (c - a);
            c = 0x08000001 * a;
            c += a - c;
            d = a - c;
            e = 0x08000001 * g;
            e += g - e;
            f = g - e;
            a = a * g;
            e = ((c * e - a) + (c * f + d * e)) + d * f;
            e += b * g;
            b = a + e;
            e += a - b;
            Hi = b + h;
            c = b - Hi;
            Lo = e + ((b - (Hi + c)) + (h + c));
        }

        public DoubleDouble SqrtFast()
        {
            if (Hi == 0 && Lo == 0)
            {
                return new DoubleDouble();
            }

            double a, b, c, d, e;
            d = 1 / Math.Sqrt(Hi);
            e = Hi * d;
            a = 0x08000001 * e;
            a += e - a;
            b = e - a;
            c = e * e;
            b = ((a * a - c) + a * b * 2) + b * b;
            a = Hi - c;
            c = Hi - a;
            c = (a + ((((Hi - (c + a)) + (c - c)) + Lo) - b)) * d * 0.5;
            a = e + c;
            b = e - a;
            return new DoubleDouble(a, (e - (b + a)) + (b + c));
        }

        public void SqrtSelfFast()
        {
            if (Hi == 0 && Lo == 0)
            {
                return;
            }

            double a, b, c, d, e;
            d = 1 / Math.Sqrt(Hi);
            e = Hi * d;
            a = 0x08000001 * e;
            a += e - a;
            b = e - a;
            c = e * e;
            b = ((a * a - c) + a * b * 2) + b * b;
            a = Hi - c;
            c = Hi - a;
            c = (a + ((((Hi - (c + a)) + (c - c)) + Lo) - b)) * d * 0.5;
            Hi = e + c;
            b = e - Hi;
            Lo = (e - (b + Hi)) + (b + c);
        }

        // Devil's values:
        // 0.693147180559945309417232121458174
        // 1.03972077083991796412584818218727
        // 1.03972077083991796312584818218727
        public DoubleDouble Exp()
        {
            if (Hi > 691.067739)
            {
                return new DoubleDouble(Double.PositiveInfinity);
            }

            double a, b, c, d, e, f, g = 0.5, h = 0, i, j, k, l, m, n, o, p, q = 2, r = 1;
            int s;

            a = 0x08000001 * Hi;
            a += Hi - a;
            b = a - Hi;
            c = Hi * 1.4426950408889634;
            b = (((a * 1.4426950514316559 - c) - (b * 1.4426950514316559 + a * 1.0542692496784412E-8)) + b * 1.0542692496784412E-8)
                + (Lo * 1.4426950408889634 + Hi * 2.0355273740931033E-17);
            s = (int)Math.Round(c);
            if (c == s)
            {
                s += (int)Math.Round(b);
            }
            else if (Math.Abs(s - c) == 0.5 && b < 0.0)
            {
                s--;
            }
            e = 0.6931471805599453 * s;
            c = ((s * 0.6931471824645996 - e) - (s * 1.904654323148236E-9)) + 2.3190468138462996E-17 * s;
            b = Lo - c;
            d = Lo - b;
            e = Hi - e;
            a = e + b;
            b = ((Lo - (d + b)) + (d - c)) + (b + (e - a));
            e = a + 1;
            c = a - e;
            d = ((a - (e + c)) + (1 + c)) + b;
            c = e + d;
            d += e - c;
            e = 0x08000001 * a;
            e += a - e;
            f = a - e;
            i = a * a;
            f = ((e * e - i) + e * f * 2) + f * f;
            f += a * b * 2;
            f += b * b;
            e = f + i;
            f += i - e;
            i = e * g;
            j = f * g;
            do
            {
                k = d + j;
                l = d - k;
                m = c + i;
                n = c - m;
                n = ((c - (n + m)) + (n + i)) + k;
                o = m + n;
                d = (n + (m - o)) + ((d - (l + k)) + (l + j));
                c = o + d;
                d += o - c;
                k = 0x08000001 * e;
                k += e - k;
                l = e - k;
                m = 0x08000001 * a;
                m += a - m;
                n = a - m;
                o = e * a;
                f = (((k * m - o) + (k * n + l * m)) + l * n) + (f * a + e * b);
                e = o + f;
                f += o - e;
                n = g / ++q;
                k = 0x08000001 * n;
                k += n - k;
                l = n - k;
                m = n * q;
                o = g - m;
                p = g - o;
                h = (o + ((((g - (p + o)) + (p - m)) + h) - (((k * q - m) + l * q)))) / q;
                g = n;
                i = 0x08000001 * e;
                i += e - i;
                k = e - i;
                j = 0x08000001 * g;
                j += g - j;
                l = g - j;
                m = e * g;
                j = (((i * j - m) + (i * l + k * j)) + k * l) + (f * g + e * h);
                i = m + j;
                j += m - i;
            } while (i > 1e-40 || i < -1e-40);
            if (s < 0)
            {
                s = -s;
                a = 0.5;
            }
            else
            {
                a = 2;
            }
            while (s > 0)
            {
                if ((s & 1) > 0)
                {
                    r *= a;
                }
                a *= a;
                s >>= 1;
            }
            a = d + j;
            b = d - a;
            e = c + i;
            f = c - e;
            f = ((c - (f + e)) + (f + i)) + a;
            c = e + f;
            d = (f + (e - c)) + ((d - (b + a)) + (b + j));
            return new DoubleDouble(c * r, d * r);
        }

        public void ExpSelf()
        {
            if (Hi > 691.067739)
            {
                Hi = Double.PositiveInfinity;
                return;
            }

            double a, b, c, d, e, f, g = 0.5, h = 0, i, j, k, l, m, n, o, p, q = 2, r = 1;
            int s;

            a = 0x08000001 * Hi;
            a += Hi - a;
            b = a - Hi;
            c = Hi * 1.4426950408889634;
            b = (((a * 1.4426950514316559 - c) - (b * 1.4426950514316559 + a * 1.0542692496784412E-8)) + b * 1.0542692496784412E-8)
                + (Lo * 1.4426950408889634 + Hi * 2.0355273740931033E-17);
            s = (int)Math.Round(c);
            if (c == s)
            {
                s += (int)Math.Round(b);
            }
            else if (Math.Abs(s - c) == 0.5 && b < 0.0)
            {
                s--;
            }
            e = 0.6931471805599453 * s;
            c = ((s * 0.6931471824645996 - e) - (s * 1.904654323148236E-9)) + 2.3190468138462996E-17 * s;
            b = Lo - c;
            d = Lo - b;
            e = Hi - e;
            a = e + b;
            b = ((b + (e - a)) + ((Lo - (d + b)) + (d - c)));
            e = a + 1;
            c = a - e;
            d = ((a - (e + c)) + (1 + c)) + b;
            c = e + d;
            d += e - c;
            e = 0x08000001 * a;
            e += a - e;
            f = a - e;
            i = a * a;
            f = ((e * e - i) + e * f * 2) + f * f;
            f += a * b * 2;
            f += b * b;
            e = f + i;
            f += i - e;
            i = e * g;
            j = f * g;
            do
            {
                k = d + j;
                l = d - k;
                m = c + i;
                n = c - m;
                n = ((c - (n + m)) + (n + i)) + k;
                o = m + n;
                d = (n + (m - o)) + ((d - (l + k)) + (l + j));
                c = o + d;
                d += o - c;
                k = 0x08000001 * e;
                k += e - k;
                l = e - k;
                m = 0x08000001 * a;
                m += a - m;
                n = a - m;
                o = e * a;
                f = (((k * m - o) + (k * n + l * m)) + l * n) + (f * a + e * b);
                e = o + f;
                f += o - e;
                n = g / ++q;
                k = 0x08000001 * n;
                k += n - k;
                l = n - k;
                m = n * q;
                o = g - m;
                p = g - o;
                h = (o + ((((g - (p + o)) + (p - m)) + h) - (((k * q - m) + l * q)))) / q;
                g = n;
                i = 0x08000001 * e;
                i += e - i;
                k = e - i;
                j = 0x08000001 * g;
                j += g - j;
                l = g - j;
                m = e * g;
                j = (((i * j - m) + (i * l + k * j)) + k * l) + (f * g + e * h);
                i = m + j;
                j += m - i;
            } while (i > 1e-40 || i < -1e-40);
            if (s < 0)
            {
                s = -s;
                a = 0.5;
            }
            else
            {
                a = 2;
            }
            while (s > 0)
            {
                if ((s & 1) > 0)
                {
                    r *= a;
                }
                a *= a;
                s >>= 1;
            }
            a = d + j;
            b = d - a;
            e = c + i;
            f = c - e;
            f = ((c - (f + e)) + (f + i)) + a;
            Hi = e + f;
            Lo = ((f + (e - Hi)) + ((d - (b + a)) + (b + j))) * r;
            Hi *= r;
        }

        public DoubleDouble Log()
        {
            if (Hi <= 0.0)
            {
                return new DoubleDouble(Double.NaN);
            }

            double a, b, c, d, e, f, g = 0.5, h = 0, i, j, k, l, m, n, o, p, q = 2, r = 1, s;
            int t;

            s = Math.Log(Hi);

            a = 0x08000001 * s;
            a += s + a;
            b = s - a;
            c = s * -1.4426950408889634;
            b = (((a * -1.4426950514316559 - c) + (a * 1.0542692496784412E-8 - b * 1.4426950514316559)) + b * 1.0542692496784412E-8) - (s * 2.0355273740931033E-17);
            t = (int)Math.Round(c);
            if (a == t)
            {
                t += (int)Math.Round(b);
            }
            else if (Math.Abs(t - a) == 0.5 && b < 0.0)
            {
                t--;
            }
            e = 0.6931471805599453 * t;
            c = ((t * 0.6931471824645996 - e) - (t * 1.904654323148236E-9)) + 2.3190468138462996E-17 * t;
            e += s;
            a = e + c;
            b = (a - e) - c;
            e = 1 - a;
            d = ((1 - e) - a) + b;
            c = e + d;
            d += e - c;
            e = 0x08000001 * -a;
            e -= a + e;
            f = a + e;
            i = a * a;
            f = ((e * e - i) - e * f * 2) + f * f;
            f += -a * b * 2;
            a = -a;
            f += b * b;
            e = f + i;
            f += i - e;
            l = 0x08000001 * e;
            l += e - l;
            k = e - l;
            i = e * g;
            j = f * g;
            do
            {
                k = d + j;
                l = d - k;
                m = c + i;
                n = c - m;
                n = ((c - (n + m)) + (n + i)) + k;
                o = m + n;
                d = (n + (m - o)) + ((d - (l + k)) + (l + j));
                c = o + d;
                d += o - c;
                k = 0x08000001 * e;
                k += e - k;
                l = e - k;
                m = 0x08000001 * a;
                m += a - m;
                n = a - m;
                o = e * a;
                f = (((k * m - o) + (k * n + l * m)) + l * n) + (f * a + e * b);
                e = o + f;
                f += o - e;
                n = g / ++q;
                k = 0x08000001 * n;
                k += n - k;
                l = n - k;
                m = n * q;
                o = g - m;
                p = g - o;
                h = (o + ((((g - (p + o)) + (p - m)) + h) - (((k * q - m) + l * q)))) / q;
                g = n;
                i = 0x08000001 * e;
                i += e - i;
                k = e - i;
                j = 0x08000001 * g;
                j += g - j;
                l = g - j;
                m = e * g;
                j = (((i * j - m) + (i * l + k * j)) + k * l) + (f * g + e * h);
                i = m + j;
                j += m - i;
            } while (i > 1e-40 || i < -1e-40);
            if (t < 0)
            {
                t = -t;
                k = 0.5;
            }
            else
            {
                k = 2;
            }
            while (t > 0)
            {
                if ((t & 1) > 0)
                {
                    r *= k;
                }
                k *= k;
                t >>= 1;
            }
            a = d + j;
            b = d - a;
            e = c + i;
            f = c - e;
            f = ((c - (f + e)) + (f + i)) + a;
            g = e + f;
            h = ((f + (e - g)) + ((d - (b + a)) + (b + j))) * r;
            g *= r;
            a = 0x08000001 * Hi;
            a += Hi - a;
            c = Hi - a;
            b = 0x08000001 * g;
            b += g - b;
            d = g - b;
            e = Hi * g;
            b = (((a * b - e) + (a * d + c * b)) + c * d) + (Lo * g + Hi * h);
            a = --e + b;
            b += e - a;
            c = a + s;
            d = a - c;
            b += ((a - (c + d)) + (s + d));
            a = c + b;
            return new DoubleDouble(a, b + (c - a));
        }

        public void LogSelf()
        {
            if (Hi <= 0.0)
            {
                Hi = Double.NaN;
                return;
            }

            double a, b, c, d, e, f, g = 0.5, h = 0, i, j, k, l, m, n, o, p, q = 2, r = 1, s;
            int t;

            s = Math.Log(Hi);

            a = 0x08000001 * s;
            a += s + a;
            b = s - a;
            c = s * -1.4426950408889634;
            b = (((a * -1.4426950514316559 - c) + (a * 1.0542692496784412E-8 - b * 1.4426950514316559)) + b * 1.0542692496784412E-8) - (s * 2.0355273740931033E-17);
            t = (int)Math.Round(c);
            if (c == t)
            {
                t += (int)Math.Round(b);
            }
            else if (Math.Abs(t + c) == 0.5 && b < 0.0)
            {
                t--;
            }
            e = 0.6931471805599453 * t;
            c = ((t * 0.6931471824645996 - e) - (t * 1.904654323148236E-9)) + 2.3190468138462996E-17 * t;
            e += s;
            a = e + c;
            b = (a - e) - c;
            e = 1 - a;
            d = ((1 - e) - a) + b;
            c = e + d;
            d += e - c;
            e = 0x08000001 * -a;
            e -= a + e;
            f = a + e;
            i = a * a;
            f = ((e * e - i) - e * f * 2) + f * f;
            f += -a * b * 2;
            a = -a;
            f += b * b;
            e = f + i;
            f += i - e;
            l = 0x08000001 * e;
            l += e - l;
            k = e - l;
            i = e * g;
            j = f * g;
            do
            {
                k = d + j;
                l = d - k;
                m = c + i;
                n = c - m;
                n = ((c - (n + m)) + (n + i)) + k;
                o = m + n;
                d = (n + (m - o)) + ((d - (l + k)) + (l + j));
                c = o + d;
                d += o - c;
                k = 0x08000001 * e;
                k += e - k;
                l = e - k;
                m = 0x08000001 * a;
                m += a - m;
                n = a - m;
                o = e * a;
                f = (((k * m - o) + (k * n + l * m)) + l * n) + (f * a + e * b);
                e = o + f;
                f += o - e;
                n = g / ++q;
                k = 0x08000001 * n;
                k += n - k;
                l = n - k;
                m = n * q;
                o = g - m;
                p = g - o;
                h = (o + ((((g - (p + o)) + (p - m)) + h) - (((k * q - m) + l * q)))) / q;
                g = n;
                i = 0x08000001 * e;
                i += e - i;
                k = e - i;
                j = 0x08000001 * g;
                j += g - j;
                l = g - j;
                m = e * g;
                j = (((i * j - m) + (i * l + k * j)) + k * l) + (f * g + e * h);
                i = m + j;
                j += m - i;
            } while (i > 1e-40 || i < -1e-40);
            if (t < 0)
            {
                t = -t;
                k = 0.5;
            }
            else
            {
                k = 2;
            }
            while (t > 0)
            {
                if ((t & 1) > 0)
                {
                    r *= k;
                }
                k *= k;
                t >>= 1;
            }
            a = d + j;
            b = d - a;
            e = c + i;
            f = c - e;
            f = ((c - (f + e)) + (f + i)) + a;
            g = e + f;
            h = ((f + (e - g)) + ((d - (b + a)) + (b + j))) * r;
            g *= r;
            a = 0x08000001 * Hi;
            a += Hi - a;
            c = Hi - a;
            b = 0x08000001 * g;
            b += g - b;
            d = g - b;
            e = Hi * g;
            Lo = (((a * b - e) + (a * d + c * b)) + c * d) + (Lo * g + Hi * h);
            a = --e + Lo;
            Lo += e - a;
            c = a + s;
            d = a - c;
            Lo += ((a - (c + d)) + (s + d));
            Hi = c + Lo;
            Lo += c - Hi;
        }

        public static double PowOf2(int y)
        {
            return ((long)y + 0xFF) << 52;
        }

        public DoubleDouble Pow(int y)
        {
            DoubleDouble temp;
            int e = y;
            if (e < 0)
            {
                e = -y;
            }
            temp = new DoubleDouble(Hi, Lo);
            DoubleDouble prod = new DoubleDouble(1);
            while (e > 0)
            {
                if ((e & 1) > 0)
                {
                    prod.MulSelf(temp);
                }
                temp.SqrSelf();
                e >>= 1;
            }
            if (y < 0)
            {
                return prod.Reciprocal();
            }
            return prod;
        }

        public void PowSelf(int y)
        {
            DoubleDouble temp;
            int e = y;
            if (e < 0)
            {
                e = -y;
            }
            temp = new DoubleDouble(Hi, Lo);
            Hi = 1;
            Lo = 0;
            while (e > 0)
            {
                if ((e & 1) > 0)
                {
                    MulSelf(temp);
                }
                temp.SqrSelf();
                e >>= 1;
            }
            if (y < 0)
            {
                ReciprocalSelf();
            }
        }

        public DoubleDouble pow(double y)
        {
            return Log().Mul(y).Exp();
        }

        public void PowSelf(double y)
        {
            LogSelf();
            MulSelf(y);
            ExpSelf();
        }

        public DoubleDouble Pow(DoubleDouble y)
        {
            return Log().Mul(y).Exp();
        }

        public void PowSelf(DoubleDouble y)
        {
            LogSelf();
            MulSelf(y);
            ExpSelf();
        }

        public DoubleDouble Root(int y)
        {
            if (Hi == 0 && Lo == 0)
            {
                return new DoubleDouble();
            }
            if (Hi < 0.0 && ((y & 1) == 0))
            {
                return new DoubleDouble(Double.NaN);
            }

            if (y == 1)
            {
                return this;
            }
            if (y == 2)
            {
                double a, b, c, d, e, f, g, h;
                g = 1 / Math.Sqrt(Hi);
                h = Hi * g;
                g *= 0.5;
                a = 0x08000001 * h;
                a += h - a;
                b = h - a;
                c = h * h;
                b = ((a * a - c) + a * b * 2) + b * b;
                a = Lo - b;
                f = Lo - a;
                e = Hi - c;
                d = Hi - e;
                d = ((Hi - (d + e)) + (d - c)) + a;
                c = e + d;
                b = (d + (e - c)) + ((Lo - (f + a)) + (f - b));
                a = c + b;
                b += (c - a);
                c = 0x08000001 * a;
                c += a - c;
                d = a - c;
                e = 0x08000001 * g;
                e += g - e;
                f = g - e;
                a = a * g;
                e = ((c * e - a) + (c * f + d * e)) + d * f;
                e += b * g;
                b = a + e;
                e += a - b;
                f = b + h;
                c = b - f;
                return new DoubleDouble(f, e + ((b - (f + c)) + (h + c)));
            }

            // Have to scope this, because original code used single letter variables
            {
                double a, b, c, d, e, f, g, h, i, j, k, l, m;
                int z;

                if (Hi < 0)
                {
                    b = -Hi;
                    c = -Lo;
                }
                else
                {
                    b = Hi;
                    c = Lo;
                }

                a = Math.Exp(Math.Log(b) / (-y));

                z = y;
                k = a;
                l = 0;
                g = 1;
                h = 0;
                while (z > 0)
                {
                    if ((z & 1) > 0)
                    {
                        d = 0x08000001 * g;
                        d += g - d;
                        e = g - d;
                        f = 0x08000001 * k;
                        f += k - f;
                        i = k - f;
                        j = g * k;
                        h = (((d * f - j) + (d * i + e * f)) + e * i) + (h * k + g * l);
                        g = j + h;
                        h += j - g;
                    }
                    f = 0x08000001 * k;
                    f = f + (k - f);
                    i = k - f;
                    j = k * k;
                    i = ((f * f - j) + f * i * 2) + i * i;
                    i += k * l * 2;
                    i += l * l;
                    k = i + j;
                    l = i + (j - k);
                    z >>= 1;
                }

                l = 0x08000001 * b;
                l += b - l;
                m = b - l;
                d = 0x08000001 * g;
                d += g - d;
                e = g - d;
                f = b * g;
                d = (((l * d - f) + (l * e + m * d)) + m * e) + (c * g + b * h);
                e = 1 - f;
                l = e - d;
                m = (e - l) - d;
                d = 0x08000001 * l;
                d += l - d;
                e = l - d;
                f = 0x08000001 * a;
                f += a - f;
                g = a - f;
                l *= a;
                m *= a;
                m += (((d * f - l) + (d * g + e * f)) + e * g);
                d = l / y;
                e = 0x08000001 * d;
                e += d - e;
                f = d - e;
                g = 0x08000001 * y;
                g += y - g;
                h = y - g;
                i = d * y;
                j = l - i;
                k = l - j;
                m = (j + ((((l - (k + j)) + (k - i)) + m) - (((e * g - i) + (e * h + f * g)) + f * h))) / y;
                e = d + a;
                l = d - e;
                m += (d - (e + l)) + (a + l);
                if (Hi < 0.0)
                {
                    e = -e;
                    m = -m;
                }
                i = 1 / e;
                l = 0x08000001 * e;
                l += e - l;
                d = e - l;
                f = 0x08000001 * i;
                f += i - f;
                g = i - f;
                h = e * i;
                m = ((1 - h) - ((((l * f - h) + (l * g + d * f)) + d * g) + m * i)) / e;
                l = i + m;
                return new DoubleDouble(l, m + (i - l));
            }
        }

        public void RootSelf(int y)
        {
            if (Hi == 0 && Lo == 0)
            {
                return;
            }
            if (Hi < 0.0 && ((y & 1) == 0))
            {
                Hi = Double.NaN;
                return;
            }

            if (y == 1)
            {
                return;
            }
            if (y == 2)
            {
                double a, b, c, d, e, f, g, h;
                g = 1 / Math.Sqrt(Hi);
                h = Hi * g;
                g *= 0.5;
                a = 0x08000001 * h;
                a += h - a;
                b = h - a;
                c = h * h;
                b = ((a * a - c) + a * b * 2) + b * b;
                a = Lo - b;
                f = Lo - a;
                e = Hi - c;
                d = Hi - e;
                d = ((Hi - (d + e)) + (d - c)) + a;
                c = e + d;
                b = (d + (e - c)) + ((Lo - (f + a)) + (f - b));
                a = c + b;
                b += (c - a);
                c = 0x08000001 * a;
                c += a - c;
                d = a - c;
                e = 0x08000001 * g;
                e += g - e;
                f = g - e;
                a = a * g;
                e = ((c * e - a) + (c * f + d * e)) + d * f;
                e += b * g;
                b = a + e;
                e += a - b;
                Hi = b + h;
                c = b - Hi;
                Lo = e + ((b - (Hi + c)) + (h + c));
                return;
            }
            // Have to scope this, because original code used single letter variables
            {

                double a, b, c, d, e, f, g, h, i, j, k, l, m;
                int z;

                if (Hi < 0)
                {
                    b = -Hi;
                    c = -Lo;
                }
                else
                {
                    b = Hi;
                    c = Lo;
                }

                a = Math.Exp(Math.Log(b) / (-y));

                z = y;
                k = a;
                l = 0;
                g = 1;
                h = 0;
                while (z > 0)
                {
                    if ((z & 1) > 0)
                    {
                        d = 0x08000001 * g;
                        d += g - d;
                        e = g - d;
                        f = 0x08000001 * k;
                        f += k - f;
                        i = k - f;
                        j = g * k;
                        h = (((d * f - j) + (d * i + e * f)) + e * i) + (h * k + g * l);
                        g = j + h;
                        h += j - g;
                    }
                    f = 0x08000001 * k;
                    f = f + (k - f);
                    i = k - f;
                    j = k * k;
                    i = ((f * f - j) + f * i * 2) + i * i;
                    i += k * l * 2;
                    i += l * l;
                    k = i + j;
                    l = i + (j - k);
                    z >>= 1;
                }

                l = 0x08000001 * b;
                l += b - l;
                m = b - l;
                d = 0x08000001 * g;
                d += g - d;
                e = g - d;
                f = b * g;
                d = (((l * d - f) + (l * e + m * d)) + m * e) + (c * g + b * h);
                e = 1 - f;
                l = e - d;
                m = (e - l) - d;
                d = 0x08000001 * l;
                d += l - d;
                e = l - d;
                f = 0x08000001 * a;
                f += a - f;
                g = a - f;
                l *= a;
                m *= a;
                m += (((d * f - l) + (d * g + e * f)) + e * g);
                d = l / y;
                e = 0x08000001 * d;
                e += d - e;
                f = d - e;
                g = 0x08000001 * y;
                g += y - g;
                h = y - g;
                i = d * y;
                j = l - i;
                k = l - j;
                m = (j + ((((l - (k + j)) + (k - i)) + m) - (((e * g - i) + (e * h + f * g)) + f * h))) / y;
                e = d + a;
                l = d - e;
                m += (d - (e + l)) + (a + l);
                if (Hi < 0.0)
                {
                    e = -e;
                    m = -m;
                }
                i = 1 / e;
                l = 0x08000001 * e;
                l += e - l;
                d = e - l;
                f = 0x08000001 * i;
                f += i - f;
                g = i - f;
                h = e * i;
                m = ((1 - h) - ((((l * f - h) + (l * g + d * f)) + d * g) + m * i)) / e;
                l = i + m;
                Hi = l;
                Lo = m + (i - l);
            }
        }

        public DoubleDouble Root(double y)
        {
            return Log().Div(y).Exp();
        }

        public void RootSelf(double y)
        {
            LogSelf();
            DivSelf(y);
            ExpSelf();
        }

        public DoubleDouble Rootr(double y)
        {
            return divr(Math.Log(y)).Exp();
        }

        public void RootrSelf(double y)
        {
            DivrSelf(Math.Log(y));
            ExpSelf();
        }

        public DoubleDouble Root(DoubleDouble y)
        {
            return Log().Div(y).Exp();
        }

        public void RootSelf(DoubleDouble y)
        {
            LogSelf();
            DivSelf(y);
            ExpSelf();
        }
    }
}
