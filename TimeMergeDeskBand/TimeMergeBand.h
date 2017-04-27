//TimeMergeBand.h : Declaration of the CTimeMergeBand

//***************************************************************************//
//                                                                           //
//  This file was created using the DeskBand ATL Object Wizard 2.0           //
//  By Erik Thompson © 2001                                                  //
//  Email questions and comments to erikt@radbytes.com						 //
//                                                                           //
//***************************************************************************//

#ifndef __TimeMergeBAND_H_
#define __TimeMergeBAND_H_

#include "resource.h"       // main symbols
#include "TimeMergeDeskBandWnd.h"

//
// These are needed for IDeskBand
//

#include <shlguid.h>
#include <shlobj.h>

/////////////////////////////////////////////////////////////////////////////
// CTimeMergeBand
class ATL_NO_VTABLE CTimeMergeBand : 
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CTimeMergeBand, &CLSID_TimeMergeBand>,
	public IDeskBand2,
	public IObjectWithSite,
	public IPersistStream,
	public IDispatchImpl<ITimeMergeBand, &IID_ITimeMergeBand, &LIBID_TIMEMERGEDESKBANDLib>
{
public:
	CTimeMergeBand();

DECLARE_REGISTRY_RESOURCEID(IDR_TIMEMERGEBAND)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_CATEGORY_MAP(CTimeMergeBand)
//	IMPLEMENTED_CATEGORY(CATID_InfoBand)
//	IMPLEMENTED_CATEGORY(CATID_CommBand)
	IMPLEMENTED_CATEGORY(CATID_DeskBand)
END_CATEGORY_MAP()

BEGIN_COM_MAP(CTimeMergeBand)
	COM_INTERFACE_ENTRY(ITimeMergeBand)
	COM_INTERFACE_ENTRY(IOleWindow)
	COM_INTERFACE_ENTRY_IID(IID_IDockingWindow, IDockingWindow)
	COM_INTERFACE_ENTRY(IObjectWithSite)
	COM_INTERFACE_ENTRY_IID(IID_IDeskBand, IDeskBand)
	COM_INTERFACE_ENTRY(IDeskBand2)
	COM_INTERFACE_ENTRY(IPersist)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(IDispatch)
END_COM_MAP()

// IDeskBand
public:
	STDMETHOD(GetBandInfo)(DWORD dwBandID, DWORD dwViewMode, DESKBANDINFO* pdbi);

// IDeskBand2
public:
	STDMETHOD(CanRenderComposited)(BOOL *pfCanRenderComposited);
	STDMETHOD(SetCompositionState)(BOOL fCompositionEnabled);
	STDMETHOD(GetCompositionState)(BOOL *pfCompositionEnabled);

// IObjectWithSite
public:
	STDMETHOD(SetSite)(IUnknown* pUnkSite);
	STDMETHOD(GetSite)(REFIID riid, void **ppvSite);

// IOleWindow
public:
	STDMETHOD(GetWindow)(HWND* phwnd);
	STDMETHOD(ContextSensitiveHelp)(BOOL fEnterMode);

// IDockingWindow
public:
	STDMETHOD(CloseDW)(unsigned long dwReserved);
	STDMETHOD(ResizeBorderDW)(const RECT* prcBorder, IUnknown* punkToolbarSite, BOOL fReserved);
	STDMETHOD(ShowDW)(BOOL fShow);

// IPersist
public:
	STDMETHOD(GetClassID)(CLSID *pClassID);

// IPersistStream
public:
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// ITimeMergeBand
public:
	STDMETHOD(IsDeskBandShown)(BOOL* pbShown);

	// Non-COM
	bool IsCompositionState() const { return m_DeskBandWnd.IsCompositionState(); }

protected:
	BOOL RegisterAndCreateWindow();
protected:
	DWORD m_dwBandID;
	DWORD m_dwViewMode;
	BOOL m_bShow;
	BOOL m_bEnterHelpMode;
	HWND m_hWndParent;
	HWND m_hWnd;
	IInputObjectSite* m_pSite;

protected:
	CTimeMergeDeskBandWnd	m_DeskBandWnd;
};

#endif //__TimeMergeBAND_H_