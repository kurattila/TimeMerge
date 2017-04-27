// TimeMergeDeskBand.cpp : Implementation of DLL Exports.


// Note: Proxy/Stub Information
//      To build a separate proxy/stub DLL, 
//      run nmake -f TimeMergeDeskBandps.mk in the project directory.

#include "stdafx.h"
#include "resource.h"
#include <initguid.h>
#include "TimeMergeDeskBand.h"

#include "TimeMergeDeskBand_i.c"
#include "TimeMergeBand.h"


CComModule _Module;

BEGIN_OBJECT_MAP(ObjectMap)
OBJECT_ENTRY(CLSID_TimeMergeBand, CTimeMergeBand)
END_OBJECT_MAP()

class CTimeMergeDeskBandApp : public CWinApp
{
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CTimeMergeDeskBandApp)
	public:
    virtual BOOL InitInstance();
    virtual int ExitInstance();
	//}}AFX_VIRTUAL

	//{{AFX_MSG(CTimeMergeDeskBandApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

BEGIN_MESSAGE_MAP(CTimeMergeDeskBandApp, CWinApp)
	//{{AFX_MSG_MAP(CTimeMergeDeskBandApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

CTimeMergeDeskBandApp theApp;

BOOL CTimeMergeDeskBandApp::InitInstance()
{
    _Module.Init(ObjectMap, m_hInstance, &LIBID_TIMEMERGEDESKBANDLib);

	// Do NOT place GDI+ startup code here (it mustn't be called in DllMain() - see GdiplusStartup() docs in MSDN)

    return CWinApp::InitInstance();
}

int CTimeMergeDeskBandApp::ExitInstance()
{
    _Module.Term();

	// Do NOT place GDI+ shutdown code here (it mustn't be called in DllMain() - see GdiplusStartup() docs in MSDN)

	return CWinApp::ExitInstance();
}

/////////////////////////////////////////////////////////////////////////////
// Used to determine whether the DLL can be unloaded by OLE

STDAPI DllCanUnloadNow(void)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());
    return (AfxDllCanUnloadNow()==S_OK && _Module.GetLockCount()==0) ? S_OK : S_FALSE;
}

/////////////////////////////////////////////////////////////////////////////
// Returns a class factory to create an object of the requested type

STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, LPVOID* ppv)
{
    return _Module.GetClassObject(rclsid, riid, ppv);
}

/////////////////////////////////////////////////////////////////////////////
// DllRegisterServer - Adds entries to the system registry

STDAPI DllRegisterServer(void)
{
    // registers object, typelib and all interfaces in typelib
    return _Module.RegisterServer(TRUE);
}

/////////////////////////////////////////////////////////////////////////////
// DllUnregisterServer - Removes entries from the system registry

STDAPI DllUnregisterServer(void)
{
    return _Module.UnregisterServer(TRUE);
}


