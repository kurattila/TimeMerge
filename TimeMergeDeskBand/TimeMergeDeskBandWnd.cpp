// NetNotifDeskBandWnd.cpp : implementation file
//

#include "stdafx.h"
#include "TimeMergeDeskBandWnd.h"
#include "DoubleBufferDC.h"
// #include "NNUtils\NNUtilsFunctions.h"
// #include "NNUtils\LocDictionary.h"
#include "NNUtils\DwmApiAccessor.h"
#include <dwmapi.h>
#include <memory>
#include <vector>
#include <sstream>
#include <stdint.h>

#include <delayimp.h>
#include <ShellScalingApi.h>

#include <gdiplus.h>
using namespace Gdiplus;

// GDI+ state
GdiplusStartupInput gdiplusStartupInput;
ULONG_PTR gdiplusToken;

// #pragma comment(lib, "DelayImp.lib")
// #pragma comment(linker, "/DelayLoad:SHCore.Dll")
// #pragma comment(linker, "/Delay:unload")

// CTimeMergeDeskBandWnd

IMPLEMENT_DYNAMIC(CTimeMergeDeskBandWnd, CWnd)

std::basic_string<TCHAR> CTimeMergeDeskBandWnd::m_RegisteredWndClass;

CTimeMergeDeskBandWnd::CTimeMergeDeskBandWnd()
    : m_IsValueInitialized(false)
    , m_IsCompositionState(false)
    , m_TimeMergeMainWPFWindow(nullptr)
{
// 	if (GetRegisteredWndClass().empty())
// 		CTimeMergeDeskBandWnd::m_RegisteredWndClass = std::basic_string<TCHAR>(AfxRegisterWndClass(0));
    // Initialize GDI+
    GdiplusStartup(&gdiplusToken, &gdiplusStartupInput, NULL);
}

CTimeMergeDeskBandWnd::~CTimeMergeDeskBandWnd()
{
    m_PercentsFont.DeleteObject();

    // Shutdown GDI+
    GdiplusShutdown(gdiplusToken);
}


BEGIN_MESSAGE_MAP(CTimeMergeDeskBandWnd, CWnd)
    ON_WM_CREATE()
    ON_WM_DESTROY()
    ON_WM_PAINT()
    ON_MESSAGE(WM_COPYDATA, OnCopyData)
    ON_WM_TIMER()
    ON_WM_LBUTTONDOWN()
    ON_WM_CONTEXTMENU()
    ON_WM_NCHITTEST()
END_MESSAGE_MAP()


static COLORREF clrRed(RGB(255,50,50));
static COLORREF clrGreen(RGB(0,100,0));
static COLORREF clrNeutralText = RGB(150,150,150);
static COLORREF clrNeutralTextLight = RGB(230,230,230);
const int speedBarWidth = 5;

// CTimeMergeDeskBandWnd message handlers

BOOL CTimeMergeDeskBandWnd::OnEraseBkgnd(CDC* /*pDC*/)
{
    return TRUE;
}

static Gdiplus::Color addAlphaToGdiColor(BYTE alpha, COLORREF clrGdiOpaque)
{
    BYTE r = GetRValue(clrGdiOpaque);
    BYTE g = GetGValue(clrGdiOpaque);
    BYTE b = GetBValue(clrGdiOpaque);
    return Gdiplus::Color(alpha, r, g, b);
}

bool CTimeMergeDeskBandWnd::IsCompositionState() const
{
    if (DwmApiAccessor::instance().IsDwmApiAvailable())
    {
        BOOL isEnabled = FALSE;
        ::DwmIsCompositionEnabled(&isEnabled);
        return isEnabled != FALSE;
    }
    else
        return m_IsCompositionState;
}

void CTimeMergeDeskBandWnd::OnPaint()
{
    CPaintDC dcDirect(this); // device context for painting
    CDoubleBufferDC dc(dcDirect);
    Graphics g(dc.GetSafeHdc());
    g.SetTextRenderingHint(TextRenderingHintAntiAlias);
    
    COLORREF clrOpaqueBackground = ::GetSysColor(COLOR_3DFACE);
    Pen penShadow(addAlphaToGdiColor(255, ::GetSysColor(COLOR_3DSHADOW)));
    Pen penDarkShadow(addAlphaToGdiColor(255, ::GetSysColor(COLOR_3DDKSHADOW)));
    Pen penLight(addAlphaToGdiColor(255, ::GetSysColor(COLOR_3DLIGHT)));
    Pen penHighLight(addAlphaToGdiColor(255, ::GetSysColor(COLOR_3DHIGHLIGHT)));

    CRect clientRect;
    GetClientRect(clientRect);

   LinearGradientBrush linGrBrush(
       PointF((REAL)clientRect.left, (REAL)clientRect.top),
       PointF((REAL)clientRect.left, (REAL)clientRect.bottom),
       addAlphaToGdiColor(255, clrOpaqueBackground),
       addAlphaToGdiColor(192, clrOpaqueBackground));

    /*
    if (DwmApiAccessor::instance().IsDwmApiAvailable())
    {
        if (this->IsCompositionState())
        {
            MARGINS margins = {-1};
            ::DwmExtendFrameIntoClientArea(this->GetSafeHwnd(), &margins);

            ARGB color = 0;
            BOOL opaque = FALSE;
            ::DwmGetColorizationColor(&color, &opaque);

            linGrBrush.SetLinearColors(Color(128, 0, 0, 0), Color(192, 0, 0, 0));
        }
        else
        {
            MARGINS margins = {0};
            ::DwmExtendFrameIntoClientArea(this->GetSafeHwnd(), &margins);
        }
    }
    */

    g.FillRectangle(&linGrBrush, clientRect.left, clientRect.top, clientRect.Width(), clientRect.Height());
    clientRect.bottom--;

    CRect textRect(clientRect);
    textRect.DeflateRect(3,3);


    COLORREF clrText = RGB(0,0,0);
    CString strOut;
    if (m_IsValueInitialized)
    {
        strOut = m_TimeBalanceHumanReadable;

        if (m_TimeBalanceHumanReadable[0] == _T('-')) // negative value, e.g. "-2:32"
            clrText = clrRed;
        else
            clrText = clrGreen;
    }
    else
    {
        strOut = _T("---");

        clrText = clrNeutralText;
//		if (DwmApiAccessor::instance().IsDwmApiAvailable() == false || this->IsCompositionState() == false)
//			clrText = clrNeutralTextLight;
    }

    Gdiplus::Font currentFont(dc.GetSafeHdc(), getDrawingLogFont());
    StringFormat format;
    format.SetAlignment(StringAlignmentCenter);     // center horizontally
    format.SetLineAlignment(StringAlignmentCenter); // center vertically
    SolidBrush textBrush(addAlphaToGdiColor(255, clrText));
    g.DrawString(strOut, -1, &currentFont, RectF((REAL)textRect.left, (REAL)textRect.top, (REAL)textRect.Width(), (REAL)textRect.Height()), &format, &textBrush);

    g.DrawLine(&penDarkShadow, clientRect.left, clientRect.bottom, clientRect.left, clientRect.top);
    g.DrawLine(&penDarkShadow, clientRect.left, clientRect.top, clientRect.right, clientRect.top);

    g.DrawLine(&penLight, clientRect.left, clientRect.bottom, clientRect.right, clientRect.bottom);
    g.DrawLine(&penLight, clientRect.right, clientRect.bottom, clientRect.right, clientRect.top);

    clientRect.DeflateRect(1,1);
    g.DrawLine(&penShadow, clientRect.left, clientRect.bottom, clientRect.left, clientRect.top);
    g.DrawLine(&penShadow, clientRect.left, clientRect.top, clientRect.right, clientRect.top);

    g.DrawLine(&penHighLight, clientRect.left, clientRect.bottom, clientRect.right, clientRect.bottom);
    g.DrawLine(&penHighLight, clientRect.right, clientRect.bottom, clientRect.right, clientRect.top);
}

LOGFONT* CTimeMergeDeskBandWnd::getDrawingLogFont()
{
    if (!m_PercentsLogFont)
    {
        // Initializes a CFont object with the characteristics given 
        // in a LOGFONT structure.
        m_PercentsLogFont = std::make_unique<LOGFONT>();
        memset(m_PercentsLogFont.get(), 0, sizeof(LOGFONT));
        m_PercentsLogFont->lfHeight = static_cast<LONG>(FontSizeForDefaultDpi * getDpiScaleFactor());
        m_PercentsLogFont->lfWeight = FW_BOLD;
        // 	m_PercentsLogFont.lfQuality = ANTIALIASED_QUALITY;
        TCHAR fontFaceName[] = _T("Arial");
        _tcscpy_s(m_PercentsLogFont->lfFaceName, _tcslen(fontFaceName) + 1, fontFaceName);
        VERIFY(m_PercentsFont.CreateFontIndirect(m_PercentsLogFont.get()));
    }

    return m_PercentsLogFont.get();
}

float CTimeMergeDeskBandWnd::getDpiScaleFactor() const
{
    constexpr static int DefaultDpi = 96;
    int currentDpi = DefaultDpi;

//     if (SUCCEEDED(__HrLoadAllImportsForDll("SHCore.dll"))) // Shcore.DLL exists from Win8.1+
//     {
//         currentDpi = ::GetDpiForShellUIComponent(SHELL_UI_COMPONENT_DESKBAND);
//     }
    HMODULE shcoreModuleLoaded = ::LoadLibrary(L"Shcore.dll"); // Shcore.DLL exists only for Win8.1+
    if (shcoreModuleLoaded)
    {
        using GetDpiForShellUIComponent_Type = UINT (_In_ SHELL_UI_COMPONENT);
        auto pGetDpiForShellUIComponent = reinterpret_cast<GetDpiForShellUIComponent_Type*>(::GetProcAddress(shcoreModuleLoaded, "GetDpiForShellUIComponent"));
        if (pGetDpiForShellUIComponent)
            currentDpi = (*pGetDpiForShellUIComponent)(SHELL_UI_COMPONENT_DESKBAND);
        ::FreeLibrary(shcoreModuleLoaded);
    }
    else
    {
        HDC windowDC = ::GetDC(m_hWnd);
        currentDpi = ::GetDeviceCaps(windowDC, LOGPIXELSY);
        ::ReleaseDC(m_hWnd, windowDC);
    }

    float dpiScaleFactor = static_cast<float>(currentDpi) / DefaultDpi;
    return dpiScaleFactor;
}

std::vector< std::basic_string<TCHAR> > split(LPCTSTR serializedString, LPCTSTR delimiter)
{
    std::vector< std::basic_string<TCHAR> > allTokens;

    CString serialized = serializedString;
    int curPos = 0;
    CString token = serialized.Tokenize(delimiter, curPos);
    while (token != _T(""))
    {
        allTokens.push_back(token.GetBuffer());
        token = serialized.Tokenize(delimiter, curPos);
    }

    return allTokens;
}

HWND parseHwndFromString(const std::basic_string<TCHAR>& inputString)
{
    HWND resultingHwnd = nullptr;

    std::basic_stringstream<TCHAR> hexBuffer(inputString);
    uint64_t hwnd = 0UL;
    hexBuffer >> std::hex >> hwnd;
    resultingHwnd = reinterpret_cast<HWND>(hwnd);

    return resultingHwnd;
}

LRESULT CTimeMergeDeskBandWnd::OnCopyData(WPARAM wParam, LPARAM lParam)
{
    COPYDATASTRUCT* pCds = (COPYDATASTRUCT*) lParam;
    int bytesCount = pCds->cbData;
    TCHAR* timeMergeDeskBandMessage = (TCHAR*) pCds->lpData;
    int charsCount = bytesCount / sizeof(TCHAR);

    TCHAR message[100+1] = _T("");
    _tcsncpy_s(message, timeMergeDeskBandMessage, charsCount - 1);
    std::vector< std::basic_string<TCHAR> > allTokens = split(message, _T("|"));
    if (allTokens.size() == 3)
    {
        m_TimeMergeMainWPFWindow = parseHwndFromString(allTokens[1]);
        m_TimeBalanceHumanReadable = allTokens[2].c_str();

        KillTimer(ValueNotValidTimer);
        m_IsValueInitialized = true;
        Invalidate();
        SetTimer(ValueNotValidTimer, 30 * 1000, NULL);
    }

    return TRUE;
}

void CTimeMergeDeskBandWnd::OnTimer(UINT_PTR nTimerID)
{
    if (nTimerID == ValueNotValidTimer)
    {
        m_IsValueInitialized = false;
        Invalidate();
    }
}

static void sendCopyDataMessage(HWND hwndTarget, HWND hwndSource)
{
    COPYDATASTRUCT cds;
    cds.dwData = 1;
    cds.cbData = 0;
    cds.lpData = nullptr;
    ::SendMessage(hwndTarget, WM_COPYDATA, (WPARAM)hwndSource, (LPARAM)&cds);
}

void CTimeMergeDeskBandWnd::OnLButtonDown( UINT /*nFlags*/, CPoint /*point*/ )
{
    sendCopyDataMessage(m_TimeMergeMainWPFWindow, GetSafeHwnd());
}

LRESULT CTimeMergeDeskBandWnd::OnNcHitTest( CPoint /*point*/ )
{
    // We don't care about any borders. We consider each pixel a CLIENT area instead,
    // so that clicking anywhere will generate a regular WM_LBUTTONDOWN message.
    return HTCLIENT;
}

void CTimeMergeDeskBandWnd::OnContextMenu( CWnd* /*pWnd*/, CPoint /*pos*/ )
{
    // Suppress default processing, in order to prevent system's taskbar context menu from popping up:
    // __super::OnContextMenu(pWnd, pos);
    sendCopyDataMessage(m_TimeMergeMainWPFWindow, GetSafeHwnd());
}
