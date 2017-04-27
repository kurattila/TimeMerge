#pragma once
#include "StdAfx.h"
//#include "afxwin.h"

class CDoubleBufferDC :
	public CDC
{
public:
	CDoubleBufferDC(HDC hDestDC, const CRect& rcPaint)
	{
		m_hDestDC = hDestDC;
		m_rect = rcPaint;
		Attach (::CreateCompatibleDC (m_hDestDC));
		if (!m_hDC)
			return;

		m_bitmap.Attach (::CreateCompatibleBitmap(
			m_hDestDC, m_rect.right, m_rect.bottom));
		m_hOldBitmap = ::SelectObject (m_hDC, m_bitmap);
	}
	
	CDoubleBufferDC(CPaintDC& paintDC)
	{
		m_hDestDC = paintDC.GetSafeHdc();
		m_rect = paintDC.m_ps.rcPaint;

		Attach (::CreateCompatibleDC (m_hDestDC));
		if (!m_hDC)
			return;

		m_bitmap.Attach (::CreateCompatibleBitmap(
			m_hDestDC, max(1, m_rect.right), max(1, m_rect.bottom)));
		m_hOldBitmap = ::SelectObject (m_hDC, m_bitmap);

		CRgn rgn;
		rgn.CreateRectRgnIndirect(&m_rect);

		SelectClipRgn(&rgn);
	}

	virtual ~CDoubleBufferDC()
	{
		if (!m_hDC)
			return;

		if (m_hDestDC)
		{
			::BitBlt (m_hDestDC, m_rect.left, m_rect.top, m_rect.Width(),
				m_rect.Height(), m_hDC, m_rect.left, m_rect.top, SRCCOPY);
		}
		::SelectObject (m_hDC, m_hOldBitmap);
	}

	void Discard()
	{
		m_hDestDC = 0;
	}

	CDC* GetDestDC()
	{
		return CDC::FromHandle(m_hDestDC);
	}

protected:

	HDC     m_hDestDC;
	CBitmap m_bitmap;
	CRect   m_rect;
	HGDIOBJ m_hOldBitmap;
};
