using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotCpuGpuBench
{
  // Gets fun identifiers for distances
  public class FunDistances
  {
    public class DistanceAndLabel
    {
      public double Distance;
      public string Label;
      public DistanceAndLabel(double distance, string label)
      {
        Distance = distance;
        Label = label;
      }
    }

    private DistanceAndLabel[] _distances = new DistanceAndLabel[]
    {
      new DistanceAndLabel(0, "Nothing"),
      new DistanceAndLabel(150, "Length Of Football Oval"),
      new DistanceAndLabel(443, "Height Of Empire State Building"),
      new DistanceAndLabel(828, "Height Of Burj Khalifa"),
      new DistanceAndLabel(1_149, "Length Of Sydney Harbour Bridge, Australia"),
      new DistanceAndLabel(1_600, "Length Of Adelaide City CBD, South Australia"),
      new DistanceAndLabel(2_700, "Length Of Golden Gate Bridge"),
      new DistanceAndLabel(3_600, "Length Of Sydney Monorail, Australia"),
      new DistanceAndLabel(8_848, "Elevation of Mount Everest"),
      new DistanceAndLabel(24_000, "North/South Distance Across Singapore "),
      new DistanceAndLabel(46_000, "East/West Distance Across Singapore"),
      new DistanceAndLabel(140_000, "East/West Distance Across Taiwan"),
      new DistanceAndLabel(424_000, "East/West Distance Across UK"),
      new DistanceAndLabel(2_300_000, "Diameter Of Pluto"),
      new DistanceAndLabel(3_474_000, "Diameter Of The Moon"),
      new DistanceAndLabel(6_779_000, "Diameter Mars"),
      new DistanceAndLabel(12_000_000, "Distance Sydney To Los Angeles"),
      new DistanceAndLabel(20_000_000, "Altitude of GPS Satellite Orbit"),
      new DistanceAndLabel(36_000_000, "Altitude of Geosynchronous Orbit"),
      new DistanceAndLabel(143_000_000, "Diameter Of Jupiter"),
      new DistanceAndLabel(385_000_000, "Distance From Earth To The Moon"),
      new DistanceAndLabel(1_400_000_000, "Diameter Of The Sun"),
      new DistanceAndLabel(90_000_000_000, "Diameter Of Mercury's Orbit"),
      new DistanceAndLabel(149_000_000_000, "Diameter Of Earth's Orbit"),
      new DistanceAndLabel(460_000_000_000, "Diameter Of Mars's Orbit"),
      new DistanceAndLabel(1_560_000_000_000, "Diameter Of Jupiter's Orbit"),
      new DistanceAndLabel(3_000_000_000_000, "Diameter Of Saturn's Orbit"),
      new DistanceAndLabel(5_600_000_000_000, "Diameter Of Uranus's Orbit"),
      new DistanceAndLabel(9_000_000_000_000, "Diameter Of Pluto's Orbit"),
    };

    private double _pixelsToMillimeters;


    public FunDistances()
    {
      //Graphics g = Graphics.FromHwnd(IntPtr.Zero);
      //IntPtr desktop = g.GetHdc();
      IntPtr desktopHDC = GetDC(IntPtr.Zero);
      var screenHorizontalPixels = GetDeviceCaps(desktopHDC, DeviceCap.HORZRES);
      var screenHorizontalSizeMillimeters = GetDeviceCaps(desktopHDC, DeviceCap.HORZSIZE);
      _pixelsToMillimeters = (double)screenHorizontalSizeMillimeters / (double)screenHorizontalPixels;
      ReleaseDC(IntPtr.Zero, desktopHDC);
    }

    public double PixelsToMeters(double pixels)
    {
      return _pixelsToMillimeters * pixels * .001;
    }

    public string PixelsToFunDistance(double pixels)
    {
      var distance = PixelsToMeters(pixels);
      DistanceAndLabel last = _distances[0];
      foreach(var label in _distances)
      {
        if (distance < label.Distance)
        {
          return last.Label;
        }
        else
        {
          last = label;
        }
      }
      return last.Label;
    }

    [DllImport("gdi32.dll")]
    static extern int GetDeviceCaps(IntPtr hDC, DeviceCap nIndex);
    [DllImport("user32.dll")]
    static extern IntPtr GetDC(IntPtr hWnd);
    [DllImport("user32.dll")]
    static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

    /// <summary>
    /// http://pinvoke.net/default.aspx/gdi32/GetDeviceCaps.html
    /// </summary>
    public enum DeviceCap : int
    {
      /// <summary>
      /// Device driver version
      /// </summary>
      DRIVERVERSION = 0,
      /// <summary>
      /// Device classification
      /// </summary>
      TECHNOLOGY = 2,
      /// <summary>
      /// Horizontal size in millimeters
      /// </summary>
      HORZSIZE = 4,
      /// <summary>
      /// Vertical size in millimeters
      /// </summary>
      VERTSIZE = 6,
      /// <summary>
      /// Horizontal width in pixels
      /// </summary>
      HORZRES = 8,
      /// <summary>
      /// Vertical height in pixels
      /// </summary>
      VERTRES = 10,
      /// <summary>
      /// Number of bits per pixel
      /// </summary>
      BITSPIXEL = 12,
      /// <summary>
      /// Number of planes
      /// </summary>
      PLANES = 14,
      /// <summary>
      /// Number of brushes the device has
      /// </summary>
      NUMBRUSHES = 16,
      /// <summary>
      /// Number of pens the device has
      /// </summary>
      NUMPENS = 18,
      /// <summary>
      /// Number of markers the device has
      /// </summary>
      NUMMARKERS = 20,
      /// <summary>
      /// Number of fonts the device has
      /// </summary>
      NUMFONTS = 22,
      /// <summary>
      /// Number of colors the device supports
      /// </summary>
      NUMCOLORS = 24,
      /// <summary>
      /// Size required for device descriptor
      /// </summary>
      PDEVICESIZE = 26,
      /// <summary>
      /// Curve capabilities
      /// </summary>
      CURVECAPS = 28,
      /// <summary>
      /// Line capabilities
      /// </summary>
      LINECAPS = 30,
      /// <summary>
      /// Polygonal capabilities
      /// </summary>
      POLYGONALCAPS = 32,
      /// <summary>
      /// Text capabilities
      /// </summary>
      TEXTCAPS = 34,
      /// <summary>
      /// Clipping capabilities
      /// </summary>
      CLIPCAPS = 36,
      /// <summary>
      /// Bitblt capabilities
      /// </summary>
      RASTERCAPS = 38,
      /// <summary>
      /// Length of the X leg
      /// </summary>
      ASPECTX = 40,
      /// <summary>
      /// Length of the Y leg
      /// </summary>
      ASPECTY = 42,
      /// <summary>
      /// Length of the hypotenuse
      /// </summary>
      ASPECTXY = 44,
      /// <summary>
      /// Shading and Blending caps
      /// </summary>
      SHADEBLENDCAPS = 45,

      /// <summary>
      /// Logical pixels inch in X
      /// </summary>
      LOGPIXELSX = 88,
      /// <summary>
      /// Logical pixels inch in Y
      /// </summary>
      LOGPIXELSY = 90,

      /// <summary>
      /// Number of entries in physical palette
      /// </summary>
      SIZEPALETTE = 104,
      /// <summary>
      /// Number of reserved entries in palette
      /// </summary>
      NUMRESERVED = 106,
      /// <summary>
      /// Actual color resolution
      /// </summary>
      COLORRES = 108,

      // Printing related DeviceCaps. These replace the appropriate Escapes
      /// <summary>
      /// Physical Width in device units
      /// </summary>
      PHYSICALWIDTH = 110,
      /// <summary>
      /// Physical Height in device units
      /// </summary>
      PHYSICALHEIGHT = 111,
      /// <summary>
      /// Physical Printable Area x margin
      /// </summary>
      PHYSICALOFFSETX = 112,
      /// <summary>
      /// Physical Printable Area y margin
      /// </summary>
      PHYSICALOFFSETY = 113,
      /// <summary>
      /// Scaling factor x
      /// </summary>
      SCALINGFACTORX = 114,
      /// <summary>
      /// Scaling factor y
      /// </summary>
      SCALINGFACTORY = 115,

      /// <summary>
      /// Current vertical refresh rate of the display device (for displays only) in Hz
      /// </summary>
      VREFRESH = 116,
      /// <summary>
      /// Vertical height of entire desktop in pixels
      /// </summary>
      DESKTOPVERTRES = 117,
      /// <summary>
      /// Horizontal width of entire desktop in pixels
      /// </summary>
      DESKTOPHORZRES = 118,
      /// <summary>
      /// Preferred blt alignment
      /// </summary>
      BLTALIGNMENT = 119
    }
   
  }
}
