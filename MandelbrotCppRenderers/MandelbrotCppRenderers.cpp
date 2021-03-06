// MandelbrotCppRenderers.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "MandelbrotCppRenderers.h"
#include <math.h>
#include <ppl.h>
#include <amp.h>
#include <amp_math.h>

using namespace std;
using namespace concurrency;
using namespace concurrency::direct3d;

void MandelbrotCppRenderers::Render(unsigned __int32* pBuffer, int bufferWidth, int bufferHeight)
{
  const int halfHeight = int(floor(bufferHeight / 2.0));
  const int halfWidth = int(floor(bufferWidth / 2.0));
  const int maxiter = int(-512 * log10(_zoomLevel));
  const float escapeValue = 4.0f;
  const float zoomLevel = float(_zoomLevel);
  const float view_i = float(_viewI);
  const float view_r = float(_viewR);
  const float invLogOf2 = 1 / log(2.0f);

  if(eSingleThreaded == _renderMode)
  {
    for(int y = -halfHeight; y < halfHeight; ++y)
    {
      // Formula: zi = z^2 + z0
      float Z0_i = view_i + y * zoomLevel;
      for(int x = -halfWidth; x < halfWidth; ++x)
      {
        float Z0_r = view_r + x * zoomLevel;
        float Z_r = Z0_r;
        float Z_i = Z0_i;
        float res = 0.0f;
        for(int iter = 0; iter < maxiter; ++iter)
        {
          float Z_rSquared = Z_r * Z_r;
          float Z_iSquared = Z_i * Z_i;
          if(Z_rSquared + Z_iSquared > escapeValue)
          {
            // We escaped
            res = iter + 1 - log(log(sqrt(Z_rSquared + Z_iSquared))) * invLogOf2;
            break;
          }
          Z_i = 2 * Z_r * Z_i + Z0_i;
          Z_r = Z_rSquared - Z_iSquared + Z0_r;
        }

        unsigned __int32 result = RGB(res * 50, res * 50, res * 50);
        pBuffer[(y + halfHeight) * bufferWidth + (x + halfWidth)] = result;
      }
    }
  }
  else if(ePPL == _renderMode)
  {
    parallel_for(-halfHeight, halfHeight, 1, [&](int y)
    {
      // Formula: zi = z^2 + z0
      float Z0_i = view_i + y * zoomLevel;
      for(int x = -halfWidth; x < halfWidth; ++x)
      {
        float Z0_r = view_r + x * zoomLevel;
        float Z_r = Z0_r;
        float Z_i = Z0_i;
        float res = 0.0f;
        for(int iter = 0; iter < maxiter; ++iter)
        {
          float Z_rSquared = Z_r * Z_r;
          float Z_iSquared = Z_i * Z_i;
          if(Z_rSquared + Z_iSquared > escapeValue)
          {
            // We escaped
            res = iter + 1 - log(log(sqrt(Z_rSquared + Z_iSquared))) * invLogOf2;
            break;
          }
          Z_i = 2 * Z_r * Z_i + Z0_i;
          Z_r = Z_rSquared - Z_iSquared + Z0_r;
        }

        unsigned __int32 result = RGB(res * 50, res * 50, res * 50);
        pBuffer[(y + halfHeight) * bufferWidth + (x + halfWidth)] = result;
      }
    });
  }
  else if(eAMP == _renderMode)
  {
    try
    {
      array_view<unsigned __int32, 2> a(bufferHeight, bufferWidth, pBuffer);
      a.discard_data();
      parallel_for_each(a.extent, [=](index<2> idx) restrict(amp)
      {
        // Formula: zi = z^2 + z0
        int x = idx[1] - halfWidth;
        int y = idx[0] - halfHeight;
        float Z0_i = view_i + y * zoomLevel;
        float Z0_r = view_r + x * zoomLevel;
        float Z_r = Z0_r;
        float Z_i = Z0_i;
        float res = 0.0f;
        for(int iter = 0; iter < maxiter; ++iter)
        {
          float Z_rSquared = Z_r * Z_r;
          float Z_iSquared = Z_i * Z_i;
          if(Z_rSquared + Z_iSquared > escapeValue)
          {
            // We escaped
            res = iter + 1 - concurrency::fast_math::log(concurrency::fast_math::log(concurrency::fast_math::sqrt(Z_rSquared + Z_iSquared))) * invLogOf2;
            break;
          }
          Z_i = 2 * Z_r * Z_i + Z0_i;
          Z_r = Z_rSquared - Z_iSquared + Z0_r;
        }
        a[idx] = GetColor(res);;
      });
      a.synchronize();
    }
    catch(const Concurrency::runtime_exception& ex)
    {
      MessageBoxA(NULL, ex.what(), "Error", MB_ICONERROR);
    }
  }
  else if(eAMPDouble == _renderMode) {
    try
    {
      array_view<unsigned __int32, 2> a(bufferHeight, bufferWidth, pBuffer);
      a.discard_data();

      const double zoomLevelD = _zoomLevel;
      const double view_iD = _viewI;
      const double view_rD = _viewR;
      const double escapeValueD = escapeValue;

      parallel_for_each(a.extent, [=](index<2> idx) restrict(amp)
      {
        // Formula: zi = z^2 + z0
        float x = float(idx[1] - halfWidth);
        float y = float(idx[0] - halfHeight);
        double Z0_i = view_iD + y * zoomLevelD;
        double Z0_r = view_rD + x * zoomLevelD;
        double Z_r = Z0_r;
        double Z_i = Z0_i;
        float res = 0.0f;
        for(int iter = 0; iter < maxiter; ++iter)
        {
          double Z_rSquared = Z_r * Z_r;
          double Z_iSquared = Z_i * Z_i;
          if(Z_rSquared + Z_iSquared > escapeValueD)
          {
            // We escaped
            res = iter + 1 - concurrency::fast_math::log(concurrency::fast_math::log(concurrency::fast_math::sqrt((float)(Z_rSquared + Z_iSquared)))) * invLogOf2;
            break;
          }
          Z_i = 2 * Z_r * Z_i + Z0_i;
          Z_r = Z_rSquared - Z_iSquared + Z0_r;
        }

        a[idx] = GetColor(res);
      });
      a.synchronize();
    }
    catch(const Concurrency::runtime_exception& ex)
    {
      MessageBoxA(NULL, ex.what(), "Error", MB_ICONERROR);
    }
  }

}

unsigned __int32 MandelbrotCppRenderers::GetColor(float res) restrict(amp) {
  unsigned __int32 resi = (int)concurrency::fast_math::fmodf(res * 5, 1280);

  // Go from black to red
  unsigned __int32 resr = resi & 255;
  unsigned __int32 resg = 0;
  unsigned __int32 resb = 0;

  // At red, introduce blue
  if(resi >= 256) {
    resr = 255;
    resb = (resi - 256) & 255;
  }
  // At purple fade out red
  if(resi >= 512) {
    resr = 255 - (resi & 255);
    resb = 255;
  }
  // At blue, fade out blue, fade in green
  if(resi >= 768) {
    resr = 0;
    resb = 255 - (resi & 255);
    resg = resi & 255;
  }
  // At green, fade in red
  if(resi >= 1024) {
    resr = resi & 255;
    resb = 0;
    resg = 255;
  }
  // At yellow, fade to red
  if(res * 5 >= 1280 && resi<256) {
    resr = 255;
    resb = 0;
    resg = 255 - (resi & 255);
  }

  unsigned __int32 result = resb + (resg << 8) + (resr << 16);
  return result;
}


extern "C" {

  __declspec(dllexport) void RenderMandelbrotCpp(bool useGpu, bool doublePrecision, bool multiThreaded, double zoomLevel, double r, double i, __int32* pBuffer, int bufferWidth, int bufferHeight)
  {
    MandelbrotCppRenderers renderers;

    renderers._zoomLevel = zoomLevel;
    renderers._viewR = r;
    renderers._viewI = i;

    if(!multiThreaded && !useGpu)
    {
      renderers._renderMode = MandelbrotCppRenderers::eSingleThreaded;
    }
    else if(multiThreaded && !useGpu)
    {
      renderers._renderMode = MandelbrotCppRenderers::ePPL;
    }
    else if(useGpu && !doublePrecision)
    {
      renderers._renderMode = MandelbrotCppRenderers::eAMP;
    }
    else if(useGpu && doublePrecision)
    {
      renderers._renderMode = MandelbrotCppRenderers::eAMPDouble;
    }
    renderers.Render((unsigned __int32*)pBuffer, bufferWidth, bufferHeight);

  }
}