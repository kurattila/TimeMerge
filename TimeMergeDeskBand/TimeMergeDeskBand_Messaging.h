#pragma once

#define TimeMerge_DESKBAND_WND_NAME _T("TimeMerge_DeskBand_MsgOnlyWnd_{FBF47F4E-C5CF-40E2-821F-0FB65AEA47A7}")

#define GUI_LANGUAGE_LENGTH 10

struct TimeMerge_PercentsUpdate_t
{
	TimeMerge_PercentsUpdate_t()
	{
		memset(this, 0, sizeof(TimeMerge_PercentsUpdate_t));
	}
	int	 nPercents;
	bool bIsFreeRateCurrently;
	bool bUseRedColour;
	bool bShowTrafficSpeed;
	float fDownloadSpeedInBytes;
	float fUploadSpeedInBytes;
	float fMaxDownloadSpeedInBytes;
	float fMaxUploadSpeedInBytes;
	bool bUseLogarithmicScale;
	TCHAR guiLanguage[GUI_LANGUAGE_LENGTH];
};

extern const int speedBarWidth;
