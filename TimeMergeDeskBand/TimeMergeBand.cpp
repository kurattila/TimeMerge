//TimeMergeBand.cpp : Implementation of CTimeMergeBand

//***************************************************************************//
//                                                                           //
//  This file was created using the DeskBand ATL Object Wizard 2.0           //
//  By Erik Thompson © 2001                                                  //
//  Email questions and comments to erikt@radbytes.com						 //
//                                                                           //
//***************************************************************************//

#include "stdafx.h"
#include <wchar.h>
#include "TimeMergeDeskBand.h"
#include "TimeMergeBand.h"
#include "TimeMergeDeskBand_Messaging.h"
#include "NNUtils/DwmApiAccessor.h"

const WCHAR TITLE_CTimeMergeBand[] = L"CTimeMergeBand";

/////////////////////////////////////////////////////////////////////////////
// CTimeMergeBand

CTimeMergeBand::CTimeMergeBand(): 
	m_dwBandID(0), 
	m_dwViewMode(0), 
	m_bShow(FALSE), 
	m_bEnterHelpMode(FALSE), 
	m_hWndParent(NULL), 
	m_hWnd(NULL),
	m_pSite(NULL)
{
}

static const int nGapInPixels = 5;

BOOL CTimeMergeBand::RegisterAndCreateWindow()
{
	RECT rect;
	::GetClientRect(m_hWndParent, &rect);

	//
	// TODO: Replace with your implementation
	//

	m_hWnd = ::CreateWindow(TEXT("STATIC"),
		TimeMerge_DESKBAND_WND_NAME,
		WS_CHILD | WS_VISIBLE | SS_CENTER,
		rect.left, rect.top + nGapInPixels,
		rect.right - rect.left,
		rect.bottom - rect.top - 2*nGapInPixels,
		m_hWndParent,
		NULL,
		NULL,
		NULL);
	m_DeskBandWnd.SubclassWindow(m_hWnd);

// 	m_hWnd = ::CreateWindow(TEXT("STATIC"), 
// 				TEXT("The CTimeMergeBand DeskBand"), 
// 				WS_CHILD | WS_VISIBLE | SS_CENTER,
// 				rect.left, rect.top, 
// 				rect.right - rect.left, 
// 				rect.bottom - rect.top, 
// 				m_hWndParent, 
// 				NULL, 
// 				NULL, 
// 				NULL);
	return ::IsWindow(m_hWnd);
}

// IDeskBand
STDMETHODIMP CTimeMergeBand::GetBandInfo(DWORD dwBandID, DWORD dwViewMode, DESKBANDINFO* pdbi)
{
	m_dwBandID = dwBandID;
	m_dwViewMode = dwViewMode;

    RECT rect;
    ::GetClientRect(m_hWndParent, &rect);

	if (pdbi)
	{
		if (pdbi->dwMask & DBIM_MINSIZE)
		{
            if (dwViewMode == DBIF_VIEWMODE_VERTICAL)
            {
                pdbi->ptMinSize.x = 20;
                pdbi->ptMinSize.y = 50 + 2 * speedBarWidth;
            }
            else
            {
                pdbi->ptMinSize.x = 50 + 2 * speedBarWidth;
                pdbi->ptMinSize.y = 20;
            }
		}
		if (pdbi->dwMask & DBIM_MAXSIZE)
		{
            // From MSDN:
            // ptMaxSize: A POINTL structure that receives the maximum size of the band object.
            // The maximum height is given in the POINTL structure's y member and the x member is ignored.
            // If the band object has no limit for its maximum height, (LONG)-1 should be used.
			pdbi->ptMaxSize.x = 0; // ignored
            pdbi->ptMaxSize.y = rect.bottom - rect.top - 2 * nGapInPixels;
        }
		if (pdbi->dwMask & DBIM_INTEGRAL)
		{
			pdbi->ptIntegral.x = 1; // ignored
			pdbi->ptIntegral.y = 1; // not sizeable
		}
		if (pdbi->dwMask & DBIM_ACTUAL)
		{
            if (dwViewMode == DBIF_VIEWMODE_VERTICAL)
            {
                pdbi->ptActual.x = 30;
                pdbi->ptActual.y = 50 + 2 * speedBarWidth;
            }
            else
            {
                pdbi->ptActual.x = 50 + 2 * speedBarWidth;
                pdbi->ptActual.y = rect.bottom - rect.top - 2 * nGapInPixels;
            }
		}
		if (pdbi->dwMask & DBIM_TITLE)
		{
			pdbi->wszTitle[0] = L'\0';
// 			wcscpy(pdbi->wszTitle, TITLE_CTimeMergeBand);
		}
		if (pdbi->dwMask & DBIM_BKCOLOR)
		{
			//Use the default background color by removing this flag.
			pdbi->dwMask &= ~DBIM_BKCOLOR;
		}
		if (pdbi->dwMask & DBIM_MODEFLAGS)
		{
			pdbi->dwModeFlags = DBIMF_DEBOSSED | DBIMF_VARIABLEHEIGHT;
		}
	}
	return S_OK;
}

// IOleWindow
STDMETHODIMP CTimeMergeBand::GetWindow(HWND* phwnd)
{
	HRESULT hr = S_OK;
	if (NULL == phwnd)
	{
		hr = E_INVALIDARG;
	}
	else
	{
		*phwnd = m_hWnd;
	}
	return hr;
}

STDMETHODIMP CTimeMergeBand::ContextSensitiveHelp(BOOL fEnterMode)
{
	m_bEnterHelpMode = fEnterMode;
	return S_OK;
}

// IDockingWindow
STDMETHODIMP CTimeMergeBand::CloseDW(unsigned long dwReserved)
{
	if (::IsWindow(m_hWnd))
	{
		::DestroyWindow(m_hWnd);
	}
	return S_OK;
}

STDMETHODIMP CTimeMergeBand::ResizeBorderDW(const RECT* prcBorder, IUnknown* punkToolbarSite, BOOL fReserved)
{
	// Not used by any band object.
	return E_NOTIMPL;
}

STDMETHODIMP CTimeMergeBand::ShowDW(BOOL fShow)
{
	HRESULT hr = S_OK;
	m_bShow = fShow;
	ShowWindow(m_hWnd, m_bShow ? SW_SHOW : SW_HIDE);
	return hr;
}
// IObjectWithSite
STDMETHODIMP CTimeMergeBand::SetSite(IUnknown* pUnkSite)
{
//If a site is being held, release it.
	if(m_pSite)
	{
		m_pSite->Release();
		m_pSite = NULL;
	}

	//If punkSite is not NULL, a new site is being set.
	if(pUnkSite)
	{
		//Get the parent window.
		IOleWindow  *pOleWindow = NULL;

		m_hWndParent = NULL;

		if(SUCCEEDED(pUnkSite->QueryInterface(IID_IOleWindow, (LPVOID*)&pOleWindow)))
		{
			pOleWindow->GetWindow(&m_hWndParent);
			pOleWindow->Release();
		}

		if(!::IsWindow(m_hWndParent))
			return E_FAIL;

		if(!RegisterAndCreateWindow())
			return E_FAIL;

		//Get and keep the IInputObjectSite pointer.
		if(SUCCEEDED(pUnkSite->QueryInterface(IID_IInputObjectSite, (LPVOID*)&m_pSite)))
		{
			return S_OK;
		}  
		return E_FAIL;
	}
	return S_OK;
}

STDMETHODIMP CTimeMergeBand::GetSite(REFIID riid, void **ppvSite)
{
	*ppvSite = NULL;

	if(m_pSite)
	{
	   return m_pSite->QueryInterface(riid, ppvSite);
	}
	return E_FAIL;
}

// IPersist
STDMETHODIMP CTimeMergeBand::GetClassID(CLSID *pClassID)
{
	*pClassID = CLSID_TimeMergeBand;
	return S_OK;
}

// IPersistStream
STDMETHODIMP CTimeMergeBand::IsDirty(void)
{
	return S_FALSE;
}

STDMETHODIMP CTimeMergeBand::Load(IStream *pStm)
{
	return S_OK;
}

STDMETHODIMP CTimeMergeBand::Save(IStream *pStm, BOOL fClearDirty)
{
	return S_OK;
}

STDMETHODIMP CTimeMergeBand::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	return E_NOTIMPL;
}

// ITimeMergeBand
STDMETHODIMP CTimeMergeBand::IsDeskBandShown(BOOL* pbShown)
{
	if (!pbShown)
		return E_INVALIDARG;

	HWND hDeskBandWindow = NULL;
	HWND hWnd = ::FindWindow(_T("Shell_TrayWnd"), NULL);
	if(hWnd)
	{
		hWnd = ::FindWindowEx(hWnd,NULL,_T("ReBarWindow32"), NULL);
		if(hWnd)
			hDeskBandWindow = ::FindWindowEx(hWnd,NULL,NULL,TimeMerge_DESKBAND_WND_NAME);
	}

	*pbShown = ::IsWindow(hDeskBandWindow);
	return S_OK;
}

// IDeskBand2
STDMETHODIMP CTimeMergeBand::CanRenderComposited( BOOL *pfCanRenderComposited )
{
	if (pfCanRenderComposited)
		*pfCanRenderComposited = DwmApiAccessor::instance().IsDwmApiAvailable();

	return S_OK;
}

STDMETHODIMP CTimeMergeBand::SetCompositionState( BOOL fCompositionEnabled )
{
	m_DeskBandWnd.SetCompositionState(fCompositionEnabled != FALSE);
	return S_OK;
}

STDMETHODIMP CTimeMergeBand::GetCompositionState( BOOL *pfCompositionEnabled )
{
	if (pfCompositionEnabled)
		*pfCompositionEnabled = m_DeskBandWnd.IsCompositionState();
	return S_OK;
}
