using System;
using System.Text;

namespace Swordfish.NET.Maths
{
    public static class Float128Extensions
    {

        /**
         * Format a string in an easily readable format. The number is represented
         * as scientific form on the following conditions: <br>
         * <ol>
         * <li>(for big numbers) When the first digit right of the decimal point
         * would not be within the first minPrecision positions of the string, <br>
         * <li>(for small numbers) When the most significant digit would not be
         * within the first minPrecision positions of the string
         * </ol>
         * <br>
         * Where: <code>minPrecision = floor(105 / log2(intBase) + 1)</code>
         */
        public static string ToString(this Float128 dd, int intBase)
        {
            double digitsPerBit = Math.Log(2) / Math.Log(intBase);
            int minPrecision = (int)Math.Floor(105.0 * digitsPerBit + 2);

            // Get the precision. (The minimum number of significant digits required
            // for an accurate representation of this number)
            int expHi = (int)((BitConverter.DoubleToInt64Bits(dd.Hi) & 0x7FF0000000000000L) >> 52);
            int expLo = dd.Lo == 0 ? expHi - 53 : (int)((BitConverter.DoubleToInt64Bits(dd.Lo) & 0x7FF0000000000000L) >> 52);
            int precision = (int)Math.Ceiling((expHi - expLo + 53) * digitsPerBit);
            precision = Math.Max(minPrecision, precision);

            // Get the raw digit representation.
            char[] chars = new char[precision + 1];
            int exp = dd.to_digits(chars, precision, intBase) + 1;

            // Get some properties.
            int left = Math.Max(0, -exp);
            int right = Math.Max(0, exp);
            if (chars[precision - 1] == 0)
            {
                precision--;
            }
            bool sci = -exp >= minPrecision || exp >= minPrecision;

            // Allocate exactly the right size string.
            StringBuilder outString = new StringBuilder(precision + (sci ? 3 : left) + (exp > 0 ? 1 : 2));

            // Build the string.
            if (dd.Hi < 0)
            {
                outString.Append('-');
            }
            if (sci)
            {
                outString.Append(chars, 0, 1);
                outString.Append('.');
                outString.Append(chars, 1, precision - 1);
                outString.Append('e');
                outString.Append(exp - 1);
            }
            else
            {
                if (exp <= 0)
                {
                    outString.Append('0');
                }
                if (right > 0)
                {
                    outString.Append(chars, 0, right);
                }
                outString.Append('.');
                if (left > 0)
                {
                    if (Float128.ZEROES.Length < left)
                    {
                        //System.err.println(left);
                    }
                    else
                    {
                        outString.Append(Float128.ZEROES, 0, left);
                    }
                }
                outString.Append(chars, right, precision - right);
            }
            return outString.ToString();
        }

        private static int to_digits(this Float128 dd, char[] s, int precision, int intBase)
        {
            int halfBase = (intBase + 1) >> 1;

            if (dd.Hi == 0.0)
            {
                // Assume dd.lo == 0.
                for (int i = 0; i < s.Length; ++i)
                {
                    s[i] = '0';
                }
                return 0;
            }

            // First determine the (approximate) exponent.
            Float128 temp = dd.Abs();
            int exp = (int)Math.Floor(Math.Log(temp.Hi) / Math.Log(intBase));

            Float128 p = new Float128(intBase);
            if (exp < -300)
            {
                temp.MulSelf(p.Pow(150));
                p.PowSelf(-exp - 150);
                temp.MulSelf(p);
            }
            else
            {
                p.PowSelf(-exp);
                temp.MulSelf(p);
            }

            // Fix roundoff errors. (eg. floor(log10(1e9))=floor(8.9999~)=8)
            if (temp.Hi >= intBase)
            {
                exp++;
                temp.Hi /= intBase;
                temp.Lo /= intBase;
            }
            else if (temp.Hi < 1)
            {
                exp--;
                temp.Hi *= intBase;
                temp.Lo *= intBase;
            }

            if (temp.Hi >= intBase || temp.Hi < 1)
            {
                throw new Exception("Can't compute exponent.");
            }

            // Handle one digit more. Used afterwards for rounding.
            int numDigits = precision + 1;
            // Extract the digits.
            for (int i = 0; i < numDigits; i++)
            {
                int val = (int)temp.Hi;
                temp = temp.Sub(val);
                temp = temp.Mul(intBase);

                s[i] = (char)val;
            }

            if (s[0] <= 0)
            {
                throw new Exception("Negative leading digit.");
            }

            // Fix negative digits due to roundoff error in exponent.
            for (int i = numDigits - 1; i > 0; i--)
            {
                if (s[i] >= 32768)
                {
                    s[i - 1]--;
                    s[i] += (char)intBase;
                }
            }

            // Round, handle carry.
            if (s[precision] >= halfBase)
            {
                s[precision - 1]++;

                int i = precision - 1;
                while (i > 0 && s[i] >= intBase)
                {
                    s[i] -= (char)intBase;
                    s[--i]++;
                }
            }
            s[precision] = (char)0;

            // If first digit became too high, shift right.
            if (s[0] >= intBase)
            {
                exp++;
                for (int i = precision; i >= 1;)
                {
                    s[i] = s[--i];
                }
            }

            // Convert to ASCII.
            for (int i = 0; i < precision; i++)
            {
                s[i] = Float128.BASE_36_TABLE[s[i]];
            }

            // If first digit became zero, and exp > 0, shift left.
            if (s[0] == '0' && exp < 32768)
            {
                exp--;
                for (int i = 0; i < precision;)
                {
                    s[i] = s[++i];
                }
            }

            return exp;
        }
    }
}
