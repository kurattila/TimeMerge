using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace TimeMerge.Utils
{
    class WmCopyDataSender
    {
//         // http://stackoverflow.com/questions/12743962/c-sharp-to-c-process-with-wm-copydata-passing-struct-with-strings
//         [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
//         static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
// 
//         public static uint WM_COPYDATA = 74;
//         //from swhistlesoft
//         public static IntPtr IntPtrAlloc<T>(T param)
//         {
//             IntPtr retval = System.Runtime.InteropServices.Marshal.AllocHGlobal(System.Runtime.InteropServices.Marshal.SizeOf(param));
//             System.Runtime.InteropServices.Marshal.StructureToPtr(param, retval, false);
//             return (retval);
//         }
//         //from swhistlesoft
//         public static void IntPtrFree(IntPtr preAllocated)
//         {
//             if (IntPtr.Zero == preAllocated) throw (new Exception("Go Home"));
//             System.Runtime.InteropServices.Marshal.FreeHGlobal(preAllocated);
//             preAllocated = IntPtr.Zero;
//         }
//         [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
//         struct COPYDATASTRUCT
//         {
//             public uint dwData;
//             public int cbData;
//             public IntPtr lpData;
//         }
// 
//         public static bool SendWmCopyData(IntPtr hwndTarget, string message)
//         {
//             COPYDATASTRUCT cds = new COPYDATASTRUCT();
//             cds.dwData = 0;
//             cds.cbData = message.Length + 1;
//             cds.lpData = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(message);
// 
//             IntPtr cdsBuffer = IntPtrAlloc(cds);
//             var success = SendMessage(IntPtr.Zero, WM_COPYDATA, IntPtr.Zero, cdsBuffer);
//             IntPtrFree(cdsBuffer);
// 
//             return success.ToInt32() > 0;
//         }

        public static bool SendWmCopyData(IntPtr hwndTarget, string message)
        {
            bool success = false;

            TimeMergeDeskBandUpdate deskBandUpdateStruct;
            deskBandUpdateStruct.Message = message;

            int structSize = Marshal.SizeOf(deskBandUpdateStruct);
            IntPtr structBuffer = Marshal.AllocHGlobal(structSize);
            try
            {
                Marshal.StructureToPtr(deskBandUpdateStruct, structBuffer, true);

                COPYDATASTRUCT cds = new COPYDATASTRUCT();
                cds.dwData = IntPtr.Zero;
                cds.cbData = structSize;
                cds.lpData = structBuffer;

                // Send the COPYDATASTRUCT struct through the WM_COPYDATA message to  
                // the receiving window. (The application must use SendMessage,  
                // instead of PostMessage to send WM_COPYDATA because the receiving  
                // application must accept while it is guaranteed to be valid.) 
                NativeMethod.SendMessage(hwndTarget, WM_COPYDATA, IntPtr.Zero, ref cds);

                int result = Marshal.GetLastWin32Error();
                success = (result == 0);
            }
            finally
            {
                Marshal.FreeHGlobal(structBuffer);
            }

            return success;
        }



        // http://code.msdn.microsoft.com/windowsapps/CSSendWMCOPYDATA-97e6644e
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)] 
        internal struct TimeMergeDeskBandUpdate 
        { 
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)] 
            public string Message;
        } 
 
 
        #region Native API Signatures and Types 
 
        /// <summary> 
        /// An application sends the WM_COPYDATA message to pass data to another  
        /// application 
        /// </summary> 
        internal const int WM_COPYDATA = 0x004A; 
 
 
        /// <summary> 
        /// The COPYDATASTRUCT structure contains data to be passed to another  
        /// application by the WM_COPYDATA message.  
        /// </summary> 
        [StructLayout(LayoutKind.Sequential)] 
        internal struct COPYDATASTRUCT 
        { 
            public IntPtr dwData;       // Specifies data to be passed 
            public int cbData;          // Specifies the data size in bytes 
            public IntPtr lpData;       // Pointer to data to be passed 
        } 
 
 
        /// <summary> 
        /// The class exposes Windows APIs to be used in this code sample. 
        /// </summary> 
        [System.Security.SuppressUnmanagedCodeSecurity] 
        internal class NativeMethod 
        { 
            /// <summary> 
            /// Sends the specified message to a window or windows. The SendMessage  
            /// function calls the window procedure for the specified window and does  
            /// not return until the window procedure has processed the message.  
            /// </summary> 
            /// <param name="hWnd"> 
            /// Handle to the window whose window procedure will receive the message. 
            /// </param> 
            /// <param name="Msg">Specifies the message to be sent.</param> 
            /// <param name="wParam"> 
            /// Specifies additional message-specific information. 
            /// </param> 
            /// <param name="lParam"> 
            /// Specifies additional message-specific information. 
            /// </param> 
            /// <returns></returns> 
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] 
            public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref COPYDATASTRUCT lParam); 
 
 
            /// <summary> 
            /// The FindWindow function retrieves a handle to the top-level window  
            /// whose class name and window name match the specified strings. This  
            /// function does not search child windows. This function does not  
            /// perform a case-sensitive search. 
            /// </summary> 
            /// <param name="lpClassName">Class name</param> 
            /// <param name="lpWindowName">Window caption</param> 
            /// <returns></returns> 
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] 
            public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

            
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpClassName, string lpWindowName);
        } 
 
        #endregion 
    }
}
