#pragma once

class MandelbrotCppRenderers
{
public:
  void Render(unsigned __int32* pBuffer, int bufferWidth, int bufferHeight);
  static unsigned __int32 GetColor(float res) restrict(amp);

  enum RenderMode
  {
    eSingleThreaded,
    ePPL,
    eAMP,
    eAMPDouble
  };
  RenderMode _renderMode;
  double _zoomLevel;
  double _viewR;
  double _viewI;
};