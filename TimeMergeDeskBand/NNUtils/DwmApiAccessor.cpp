
#include "StdAfx.h"

#include "DwmApiAccessor.h"
#include <dwmapi.h>
// #include "../NNUtils/ConnectionManagerCOMAccessor.h"

DwmApiAccessor DwmApiAccessor::m_TheDwmApiAccessorInstance;

DwmApiAccessor::DwmApiAccessor(void)
{
}

DwmApiAccessor& DwmApiAccessor::instance()
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState( ));

    return m_TheDwmApiAccessorInstance;
}

bool DwmApiAccessor::IsDwmApiAvailable()
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState( ));

//     ConnectionManagerCOMAccessor& conManagerAccessor = ConnectionManagerCOMAccessor::instance();
//     if (!conManagerAccessor.IsApiAvailable())
//         return false;

    HMODULE library = ::LoadLibrary(_T("dwmapi.dll"));
    bool result = false;

    if (library)
    {
        if (GetProcAddress(library, "DwmIsCompositionEnabled"))
            result = true;

        VERIFY(::FreeLibrary(library));
    }

    return result;
}

HRESULT DwmApiAccessor::EnableBlurBehindWindow( HWND hTargetWnd, bool enable /*= true*/, HRGN region /*= 0*/, bool transitionOnMaximized /*= false*/ )
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState( ));

    if (!IsDwmApiAvailable())
        return S_FALSE;

    DWM_BLURBEHIND blurBehind = { 0 };

    blurBehind.dwFlags = DWM_BB_ENABLE | DWM_BB_TRANSITIONONMAXIMIZED;
    blurBehind.fEnable = enable;
    blurBehind.fTransitionOnMaximized = transitionOnMaximized;

    if (enable && 0 != region)
    {
        blurBehind.dwFlags |= DWM_BB_BLURREGION;
        blurBehind.hRgnBlur = region;
    }

    return ::DwmEnableBlurBehindWindow(hTargetWnd, &blurBehind);
}
