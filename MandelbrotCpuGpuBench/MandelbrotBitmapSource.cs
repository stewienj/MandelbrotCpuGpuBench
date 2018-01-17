using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MandelbrotCpuGpuBench
{
    public class MandelbrotBitmapSource : BitmapSource
    {
        private BitmapSource TestBS;
        protected override Freezable CreateInstanceCore()
        {
            return new MandelbrotBitmapSource();
        }

        public MandelbrotBitmapSource()
        {
            TestBS = CreateTestBitmap(100, 100);
        }

        public MandelbrotBitmapSource(int width, int height)
        {
            TestBS = CreateTestBitmap(width, height);
        }

        private BitmapSource CreateTestBitmap(int width, int height)
        {
            int stride = width * 3 +
                                (width * 3) % 4;
            byte[] bits = new
                             byte[height * stride];
            for (int y = 0; y < height; y += 10)
            {
                for (int x = 0; x < width; x++)
                {
                    setpixel(ref bits, x, y,
                                  stride, Colors.White);
                }
            }
            for (int x = 0; x < width; x += 10)
            {
                for (int y = 0; y < height; y++)
                {
                    setpixel(ref bits, x, y,
                                 stride, Colors.White);
                }
            }
            return BitmapSource.Create(
                               width, height,
                               96, 96,
                               PixelFormats.Rgb24,
                               null,
                               bits, stride);
        }

        private void setpixel(ref byte[] bits, int x, int y, int stride, Color c)
        {
            bits[x * 3 + y * stride] = c.R;
            bits[x * 3 + y * stride + 1] = c.G;
            bits[x * 3 + y * stride + 2] = c.B;
        }

        public override PixelFormat Format
        {
            get
            {
                return TestBS.Format;
            }
        }

        public override int PixelHeight
        {
            get
            {
                return TestBS.PixelHeight;
            }
        }
        public override int PixelWidth
        {
            get
            {
                return TestBS.PixelWidth;
            }
        }

        public override double DpiX
        {
            get
            {
                return TestBS.DpiX;
            }
        }
        public override double DpiY
        {
            get
            {
                return TestBS.DpiY;
            }
        }

        public override BitmapPalette Palette
        {
            get
            {
                return TestBS.Palette;
            }
        }

        public override event EventHandler<ExceptionEventArgs> DecodeFailed;

     //   public override void CopyPixels(
     //Int32Rect sourceRect,
     //Array pixels,
     //int stride, int offset)
     //   {
     //       TestBS.CopyPixels(sourceRect, pixels,
     //                              stride, offset);
     //   }


        public override void CopyPixels(
  Int32Rect sourceRect,
  Array pixels,
  int stride,
  int offset)
        {
            int start = sourceRect.Y * stride;
            Array.Copy(bits, start,
                             pixels, 0, stride);
        }
    }
}
