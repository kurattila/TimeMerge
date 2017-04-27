REM http://stackoverflow.com/questions/10954041/restarting-explorer-exe-only-opens-an-explorer-window



taskkill /f /im explorer.exe

rem Uninstall TimeMerge's DeskBand
regsvr32 /u /s x64\Debug\TimeMergeDeskBand.dll

rem Restart Windows Taskbar
%systemroot%\sysnative\cmd.exe /c start explorer.exe