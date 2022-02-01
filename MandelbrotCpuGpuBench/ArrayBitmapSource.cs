﻿using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MandelbrotCpuGpuBench
{
    /// <summary>
    /// Wraps a 2 dimensional array as a bitmap source
    /// </summary>
    public class ArrayBitmapSource : BitmapSource
    {
        private int[,] _buffer;

        protected override Freezable CreateInstanceCore()
        {
            return new ArrayBitmapSource();
        }

        public ArrayBitmapSource() : this(new int[100, 100])
        {
        }

        public ArrayBitmapSource(int[,] buffer)
        {
            _buffer = buffer;
        }

        public int[,] Buffer => _buffer;

        public override PixelFormat Format => PixelFormats.Bgr32;
        public override int PixelHeight => _buffer.GetLength(0);
        public override int PixelWidth => _buffer.GetLength(1);

        public override double DpiX => 96;
        public override double DpiY => 96;

        public override BitmapPalette Palette => null;

        public void OnDecodeFailed()
        {
            DecodeFailed?.Invoke(this, null);
        }

        public override event EventHandler<ExceptionEventArgs> DecodeFailed;

        public override void CopyPixels(Int32Rect sourceRect, Array pixels, int stride, int offset)
        {
            if (sourceRect.Y >= PixelHeight)
                return;
            // Not applicatble because..we store just an int array, below would use width in bytes
            //int stride = width * 3 + (width * 3) % 4;
            int start = sourceRect.Y * PixelWidth;
            System.Buffer.BlockCopy(_buffer, start, pixels, 0, (_buffer.Length - start) << 2);
        }
    }
}
