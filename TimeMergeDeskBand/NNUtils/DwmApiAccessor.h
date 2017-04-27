#pragma once

//#include "NNUtils_Exports.h"

class DwmApiAccessor
{
public:
	static DwmApiAccessor& instance();

	bool IsDwmApiAvailable();
	HRESULT EnableBlurBehindWindow(HWND hTargetWnd, bool enable = true, HRGN region = 0, bool transitionOnMaximized = false);

private:
	DwmApiAccessor();
	static DwmApiAccessor m_TheDwmApiAccessorInstance;
};
