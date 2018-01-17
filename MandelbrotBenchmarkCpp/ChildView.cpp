
// ChildView.cpp : implementation of the CChildView class
//

#include "stdafx.h"
#include "Mandelbrot.h"
#include "ChildView.h"
#include <math.h>
#include <ppl.h>
#include <amp.h>
#include <amp_math.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

using namespace std;
using namespace concurrency;
using namespace concurrency::direct3d;

// CChildView

CChildView::CChildView()
	: m_renderMode(ePPL)
	, m_zoomLevel(0.001)
	, m_view_r(0.001643721971153)
	, m_view_i(0.822467633298876)
	, m_nBuffWidth(0)
	, m_nBuffHeight(0)
	, m_nRenderToBufferIndex(0)
{
	// Use 2 buffers
	m_buffers.push_back(vector<unsigned __int32>());
	m_buffers.push_back(vector<unsigned __int32>());
}

CChildView::~CChildView()
{
}


BEGIN_MESSAGE_MAP(CChildView, CWnd)
	ON_WM_PAINT()
	ON_COMMAND(ID_BTN_RENDER, &CChildView::OnBtnRender)
	ON_COMMAND(ID_BTN_ZOOM_IN, &CChildView::OnBtnZoomIn)
	ON_COMMAND(ID_BTN_ZOOM_OUT, &CChildView::OnBtnZoomOut)
	ON_WM_SIZE()
	ON_COMMAND(ID_BTN_PARALLEL, &CChildView::OnBtnParallel)
	ON_UPDATE_COMMAND_UI(ID_BTN_PARALLEL, &CChildView::OnUpdateBtnParallel)
	ON_COMMAND(ID_BTN_AMP, &CChildView::OnBtnAmp)
	ON_UPDATE_COMMAND_UI(ID_BTN_AMP, &CChildView::OnUpdateBtnAmp)
	ON_COMMAND(ID_BTN_AMP_DOUBLE, &CChildView::OnBtnAmpDouble)
	ON_UPDATE_COMMAND_UI(ID_BTN_AMP_DOUBLE, &CChildView::OnUpdateBtnAmpDouble)
	ON_WM_ERASEBKGND()
	ON_COMMAND(ID_BTN_BENCHMARK, &CChildView::OnBtnBenchmark)
	ON_UPDATE_COMMAND_UI(ID_BTN_SINGLE_THREADED, &CChildView::OnUpdateBtnSingleThreaded)
	ON_COMMAND(ID_BTN_SINGLE_THREADED, &CChildView::OnBtnSingleThreaded)
	ON_WM_MOUSEMOVE()
	ON_WM_MOUSEWHEEL()
END_MESSAGE_MAP()


// CChildView message handlers

BOOL CChildView::PreCreateWindow(CREATESTRUCT& cs) 
{
	if (!CWnd::PreCreateWindow(cs))
		return FALSE;

	cs.dwExStyle |= WS_EX_CLIENTEDGE;
	cs.style &= ~WS_BORDER;
	cs.lpszClass = AfxRegisterWndClass(CS_HREDRAW|CS_VREDRAW|CS_DBLCLKS, 
		::LoadCursor(NULL, IDC_ARROW), reinterpret_cast<HBRUSH>(COLOR_WINDOW+1), NULL);

	return TRUE;
}

void CChildView::OnPaint() 
{
	CPaintDC dc(this); // device context for painting
	
	CDC dcCache;
	dcCache.CreateCompatibleDC(&dc);
	CBitmap bmpCache;
	bmpCache.CreateCompatibleBitmap(&dc, m_nBuffWidth, m_nBuffHeight);

	BITMAPINFO bmi = {0};
	bmi.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
	bmi.bmiHeader.biWidth = m_nBuffWidth;
	bmi.bmiHeader.biHeight = m_nBuffHeight;
	bmi.bmiHeader.biPlanes = 1;
	bmi.bmiHeader.biBitCount = 32;
	bmi.bmiHeader.biCompression = BI_RGB;
	bmi.bmiHeader.biSizeImage = 0;
	SetDIBits(dcCache.GetSafeHdc(), (HBITMAP)bmpCache.GetSafeHandle(), 0, m_nBuffHeight,
		&(m_buffers[(m_nRenderToBufferIndex + 1) % 2][0]), &bmi, DIB_RGB_COLORS);

	CBitmap* pOldCacheBmp = dcCache.SelectObject(&bmpCache);
	dc.BitBlt(0, 0, m_nBuffWidth, m_nBuffHeight, &dcCache, 0, 0, SRCCOPY);
	dcCache.SelectObject(pOldCacheBmp);
	
	// Do not call CWnd::OnPaint() for painting messages
}

void CChildView::OnBtnRender()
{
	// Time how long it took to render 1 frame
	ULONGLONG dwStart = GetTickCount64();
	Render();
	ULONGLONG dwEnd = GetTickCount64();

	// Show the time how long it took to render the frame in the titlebar
    CString str;
    str.Format(_T("Rendering took %I64ims (%ux%u) Zoom Level %f"), dwEnd-dwStart, m_nBuffWidth, m_nBuffHeight, 0.002/m_zoomLevel);
	AfxGetMainWnd()->SetWindowText(str);

	// Swap buffers and repaint the window
	m_nRenderToBufferIndex = (m_nRenderToBufferIndex + 1) % 2;
	Invalidate();
	UpdateWindow();
}

void CChildView::AllocateBuffers()
{
	CRect rcClient;
	GetClientRect(rcClient);

	m_nBuffHeight = rcClient.Height();
	m_nBuffWidth = rcClient.Width();

	if (m_nBuffWidth == 0 || m_nBuffHeight == 0)
		return;

	for (size_t i = 0; i < m_buffers.size(); ++i)
		m_buffers[i].resize(m_nBuffHeight * m_nBuffWidth);
}


void CChildView::OnSize(UINT nType, int cx, int cy)
{
	CWnd::OnSize(nType, cx, cy);

	AllocateBuffers();
	OnBtnRender();
}

void CChildView::OnBtnParallel()
{
	m_renderMode = ePPL;
}

void CChildView::OnUpdateBtnParallel(CCmdUI *pCmdUI)
{
	pCmdUI->SetCheck(ePPL == m_renderMode);
}

void CChildView::OnBtnAmp()
{
	m_renderMode = eAMP;
}

void CChildView::OnBtnAmpDouble()
{
	m_renderMode = eAMPDouble;
}

void CChildView::OnUpdateBtnAmp(CCmdUI *pCmdUI)
{
	pCmdUI->SetCheck(eAMP == m_renderMode);
}

void CChildView::OnUpdateBtnAmpDouble(CCmdUI *pCmdUI)
{
	pCmdUI->SetCheck(eAMPDouble == m_renderMode);
}

void CChildView::OnUpdateBtnSingleThreaded(CCmdUI *pCmdUI)
{
	pCmdUI->SetCheck(eSingleThreaded == m_renderMode);
}

void CChildView::OnBtnSingleThreaded()
{
	m_renderMode = eSingleThreaded;
}

BOOL CChildView::OnEraseBkgnd(CDC* pDC)
{
	return TRUE;
}

void CChildView::Render()
{
	const int halfHeight = int(floor(m_nBuffHeight/2.0));
	const int halfWidth = int(floor(m_nBuffWidth/2.0));
  const int maxiter = int(-512 * log10(m_zoomLevel));
	const float escapeValue = 4.0f;
	const float zoomLevel = float(m_zoomLevel);
	const float view_i = float(m_view_i);
	const float view_r = float(m_view_r);
	const float invLogOf2 = 1 / log(2.0f);
	if (m_buffers[m_nRenderToBufferIndex].empty())
		return;
	unsigned __int32* pBuffer = &(m_buffers[m_nRenderToBufferIndex][0]);

	if (eSingleThreaded == m_renderMode)
	{
		for (int y = -halfHeight; y < halfHeight; ++y)
		{
			// Formula: zi = z^2 + z0
			float Z0_i = view_i + y * zoomLevel;
			for (int x = -halfWidth; x < halfWidth; ++x)
			{
				float Z0_r = view_r + x * zoomLevel;
				float Z_r = Z0_r;
				float Z_i = Z0_i;
				float res = 0.0f;
				for (int iter = 0; iter < maxiter; ++iter)
				{
					float Z_rSquared = Z_r * Z_r;
					float Z_iSquared = Z_i * Z_i;
					if (Z_rSquared + Z_iSquared > escapeValue)
					{
						// We escaped
						res = iter + 1 - log(log(sqrt(Z_rSquared + Z_iSquared))) * invLogOf2;
						break;
					}
					Z_i = 2 * Z_r * Z_i + Z0_i;
					Z_r = Z_rSquared - Z_iSquared + Z0_r;
				}

				unsigned __int32 result = RGB(res * 50, res * 50, res * 50);
				pBuffer[(y + halfHeight) * m_nBuffWidth + (x + halfWidth)] = result;
			}
		}
	}
	else if (ePPL == m_renderMode)
	{
        parallel_for(-halfHeight, halfHeight, 1, [&](int y)
		{
			// Formula: zi = z^2 + z0
			float Z0_i = view_i + y * zoomLevel;
			for (int x = -halfWidth; x < halfWidth; ++x)
			{
				float Z0_r = view_r + x * zoomLevel;
				float Z_r = Z0_r;
				float Z_i = Z0_i;
				float res = 0.0f;
				for (int iter = 0; iter < maxiter; ++iter)
				{
					float Z_rSquared = Z_r * Z_r;
					float Z_iSquared = Z_i * Z_i;
					if (Z_rSquared + Z_iSquared > escapeValue)
					{
						// We escaped
						res = iter + 1 - log(log(sqrt(Z_rSquared + Z_iSquared))) * invLogOf2;
						break;
					}
					Z_i = 2 * Z_r * Z_i + Z0_i;
					Z_r = Z_rSquared - Z_iSquared + Z0_r;
				}

				unsigned __int32 result = RGB(res * 50, res * 50, res * 50);
				pBuffer[(y + halfHeight) * m_nBuffWidth + (x + halfWidth)] = result;
			}
		});
	}
	else if (eAMP == m_renderMode)
	{
		try
		{
			array_view<unsigned __int32, 2> a(m_nBuffHeight, m_nBuffWidth, pBuffer);
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
				for (int iter = 0; iter < maxiter; ++iter)
				{
					float Z_rSquared = Z_r * Z_r;
					float Z_iSquared = Z_i * Z_i;
					if (Z_rSquared + Z_iSquared > escapeValue)
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
		catch (const Concurrency::runtime_exception& ex)
		{
			MessageBoxA(NULL, ex.what(), "Error", MB_ICONERROR);
		}
	}
  else if (eAMPDouble == m_renderMode){
    try
    {
			array_view<unsigned __int32, 2> a(m_nBuffHeight, m_nBuffWidth, pBuffer);
      a.discard_data();

      const double zoomLevelD = m_zoomLevel;
      const double view_iD = m_view_i;
      const double view_rD = m_view_r;
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
				for (int iter = 0; iter < maxiter; ++iter)
				{
					double Z_rSquared = Z_r * Z_r;
					double Z_iSquared = Z_i * Z_i;
					if (Z_rSquared + Z_iSquared > escapeValueD)
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
		catch (const Concurrency::runtime_exception& ex)
		{
			MessageBoxA(NULL, ex.what(), "Error", MB_ICONERROR);
		}
  }

}

unsigned __int32 CChildView::GetColor(float res) restrict(amp){
  unsigned __int32 resi = (int)concurrency::fast_math::fmodf(res*5,1280);

  // Go from black to red
  unsigned __int32 resr = resi&255;
  unsigned __int32 resg = 0;
  unsigned __int32 resb = 0;

  // At red, introduce blue
  if (resi>=256){
    resr = 255;
    resb = (resi-256)&255;
  }
  // At purple fade out red
  if (resi>=512){
    resr = 255-(resi&255);
    resb = 255;
  }
  // At blue, fade out blue, fade in green
  if (resi>=768){
    resr = 0;
    resb = 255-(resi&255);
    resg = resi&255;
  }
  // At green, fade in red
  if (resi>=1024){
    resr = resi&255;
    resb = 0;
    resg = 255;
  }
  // At yellow, fade to red
  if (res*5>=1280 && resi<256){
    resr = 255;
    resb = 0;
    resg = 255-(resi&255);
  }

  unsigned __int32 result = resb +  (resg<<8) +  (resr<<16);
  return result;
}


void CChildView::OnBtnBenchmark()
{
	ULONGLONG dwStart = GetTickCount64();

	const size_t count = 50;
	for (size_t i = 0; i < count; ++i)
		Render();

	ULONGLONG dwEnd = GetTickCount64();

    CString str;
    str.Format(_T("Benchmark avg %fms/frame"), (dwEnd-dwStart)/float(count));
	AfxGetMainWnd()->SetWindowText(str);
}

void CChildView::OnMouseMove(UINT nFlags, CPoint point)
{
	static CPoint prevPoint(-1, -1);
	if (nFlags & MK_LBUTTON && prevPoint.x != -1 && prevPoint.y != -1)
	{
		m_view_r += (prevPoint.x - point.x) / (1.0 / m_zoomLevel);
		m_view_i += (point.y - prevPoint.y) / (1.0 / m_zoomLevel);
		OnBtnRender();
	}
	prevPoint = point;

	CWnd::OnMouseMove(nFlags, point);
}

BOOL CChildView::OnMouseWheel(UINT nFlags, short zDelta, CPoint pt)
{
	double factor = 1.2;
	if (zDelta > 0)
		m_zoomLevel /= factor;
	else
		m_zoomLevel *= factor;
	OnBtnRender();

	return CWnd::OnMouseWheel(nFlags, zDelta, pt);
}

void CChildView::OnBtnZoomIn()
{
	m_zoomLevel /= 2.0;
	OnBtnRender();
}

void CChildView::OnBtnZoomOut()
{
	m_zoomLevel *= 2.0;
	OnBtnRender();
}
