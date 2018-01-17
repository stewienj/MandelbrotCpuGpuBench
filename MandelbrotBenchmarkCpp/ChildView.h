// ChildView.h : interface of the CChildView class
//
#pragma once

#include <vector>

// CChildView window
class CChildView : public CWnd
{
public:
	CChildView();
	virtual ~CChildView();

protected:
	enum RenderMode
	{
		eSingleThreaded,
		ePPL,
		eAMP,
    eAMPDouble
	};
	RenderMode m_renderMode;
	double m_zoomLevel;
	double m_view_r;
	double m_view_i;

	std::vector<std::vector<unsigned __int32>> m_buffers;
	int m_nBuffWidth;
	int m_nBuffHeight;
	size_t m_nRenderToBufferIndex;
	void AllocateBuffers();
	void Render();
  static unsigned __int32 GetColor(float res) restrict(amp);

	virtual BOOL PreCreateWindow(CREATESTRUCT& cs);
	afx_msg void OnBtnRender();
	afx_msg void OnBtnZoomIn();
	afx_msg void OnBtnZoomOut();
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnBtnParallel();
	afx_msg void OnUpdateBtnParallel(CCmdUI *pCmdUI);
	afx_msg void OnBtnAmp();
  afx_msg void OnBtnAmpDouble();
	afx_msg void OnUpdateBtnAmp(CCmdUI *pCmdUI);
	afx_msg void OnUpdateBtnAmpDouble(CCmdUI *pCmdUI);
	afx_msg BOOL OnEraseBkgnd(CDC* pDC);
	afx_msg void OnBtnBenchmark();
	afx_msg void OnUpdateBtnSingleThreaded(CCmdUI *pCmdUI);
	afx_msg void OnBtnSingleThreaded();
	afx_msg void OnMouseMove(UINT nFlags, CPoint point);
	afx_msg BOOL OnMouseWheel(UINT nFlags, short zDelta, CPoint pt);
	afx_msg void OnPaint();
	DECLARE_MESSAGE_MAP()
};

