using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeMerge.Utils
{
    class DeskBandFinder
    {
        public static IntPtr FindTimeMergeDeskBandWindow()
        {
            IntPtr timeMergeDeskBand = IntPtr.Zero;

            // FindWindowEx() searches child windows, but _not_ recursively in all levels!
            IntPtr taskbarWindow = WmCopyDataSender.NativeMethod.FindWindow("Shell_TrayWnd", null);
            IntPtr rebarOfTaskbar = WmCopyDataSender.NativeMethod.FindWindowEx(taskbarWindow, IntPtr.Zero, "ReBarWindow32", null);
            timeMergeDeskBand = WmCopyDataSender.NativeMethod.FindWindowEx(rebarOfTaskbar, IntPtr.Zero, null, "TimeMerge_DeskBand_MsgOnlyWnd_{FBF47F4E-C5CF-40E2-821F-0FB65AEA47A7}");

            return timeMergeDeskBand;
        }
    }
}
