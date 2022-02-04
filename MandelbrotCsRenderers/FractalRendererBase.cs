using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotCsRenderers
{
    public class FractalRendererBase
    {
        private Func<bool> abort;
        private Action<int, int, int> drawPixel;

        protected FractalRendererBase(Action<int, int, int> draw, Func<bool> checkAbort)
        {
            drawPixel = draw; abort = checkAbort;
        }

        public void DoAbort()
        {
            abort = () => true;
        }

        public static Func<int, (byte R, byte G, byte B)> GetColorProviderOriginal()
        {
            var table = new (byte, byte, byte)[1001];
            for (int iters = 0; iters <= 1000; ++iters)
            {
                int val = 1000 - Math.Min(iters, 1000);
                byte blue = (byte)(val % 43 * 23);
                byte red = (byte)(val % 97 * 41);
                byte green = (byte)(val % 71 * 19);

                table[iters] = (red, green, blue);
            }

            return (iters) =>
            {
                iters = Math.Max(0, Math.Min(iters, 1000));
                return table[iters];
            };
        }

        public static Func<int, (byte R, byte G, byte B)> GetColorProviderV2(int escapedValue)
        {
            return (iters) =>
            {
                if (iters == escapedValue)
                {
                    return (0, 0, 0);
                }
                int iter = (int)((iters * 5) % 1280);
                int r = iter & 255;
                int g = 0;
                int b = 0;
                // At red, introduce blue
                if (iter >= 256)
                {
                    r = 255;
                    b = (iter - 256) & 255;
                }
                // At purple fade out red
                if (iter >= 512)
                {
                    r = 255 - (iter & 255);
                    b = 255;
                }
                // At blue, fade out blue, fade in green
                if (iter >= 768)
                {
                    r = 0;
                    b = 255 - (iter & 255);
                    g = iter & 255;
                }
                // At green, fade in red
                if (iter >= 1024)
                {
                    r = iter & 255;
                    b = 0;
                    g = 255;
                }
                // At yellow, fade to red
                if (iters * 5 >= 1280 && iter < 256)
                {
                    r = 255;
                    b = 0;
                    g = 255 - (iter & 255);
                }

                return ((byte)r, (byte)g, (byte)b);
            };
        }

        public bool Abort => abort();
        protected Action<int, int, int> DrawPixel => drawPixel;

    }
}
