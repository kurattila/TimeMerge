#pragma once

#include "TimeMergeDeskBand_Messaging.h"
#include <string>
#include <memory>

// CTimeMergeDeskBandWnd

namespace Gdiplus
{
	class Graphics;
}

class CTimeMergeDeskBandWnd : public CWnd
{
	DECLARE_DYNAMIC(CTimeMergeDeskBandWnd)

public:
	CTimeMergeDeskBandWnd();
	virtual ~CTimeMergeDeskBandWnd();

	bool IsCompositionState() const;
	void SetCompositionState(bool bCompositionState) { m_IsCompositionState = bCompositionState; }

	static const std::basic_string<TCHAR>& GetRegisteredWndClass() { return m_RegisteredWndClass; }

protected:
	DECLARE_MESSAGE_MAP()

	afx_msg void OnPaint();
	afx_msg BOOL OnEraseBkgnd(CDC* pDC);
	afx_msg LRESULT OnCopyData(WPARAM, LPARAM);
	afx_msg void OnTimer(UINT_PTR nTimerID);
    afx_msg void OnLButtonDown(UINT nFlags, CPoint point);
    afx_msg void OnContextMenu(CWnd* pWnd, CPoint pos);
    afx_msg LRESULT OnNcHitTest(CPoint point);

    LOGFONT* getDrawingLogFont();
    float getDpiScaleFactor() const;

	static std::basic_string<TCHAR>	m_RegisteredWndClass;

    constexpr static int FontSizeForDefaultDpi = 15;

	CFont	m_PercentsFont;
    std::unique_ptr<LOGFONT> m_PercentsLogFont;

    CString m_TimeBalanceHumanReadable;
	bool	m_IsValueInitialized; // are contents of 'm_LastUpdateContent' valid?

	enum TimerId_t
	{
		ValueNotValidTimer = 1
	};

private:
	bool m_IsCompositionState;
    HWND m_TimeMergeMainWPFWindow;
};


