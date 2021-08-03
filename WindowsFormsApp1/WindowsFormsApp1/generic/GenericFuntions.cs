using System;
using System.Runtime.InteropServices;

namespace WindowsFormsApp1
{
    static class GenericFuntions
    {
        // Msg events ...
        public const int WM_USER = 0x0400;
        public const int WM_PROGRESS_NOTIFICATION = WM_USER + 100;
        public const int WM_TIMER = WM_USER + 101;
        // Msg Info
        public const int WM_PROGRESS_NOTIFICATION_MSG_OK = 0;
        public const int WM_PROGRESS_NOTIFICATION_MSG_ERROR = 1;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        public static void SendMessageEx(IntPtr hWnd, int wMsg, String Message, int mType=WM_PROGRESS_NOTIFICATION_MSG_OK)
        {
            GCHandle GCH = GCHandle.Alloc(Message, GCHandleType.Pinned);
            IntPtr pS = GCH.AddrOfPinnedObject();

            SendMessage(hWnd, wMsg, pS, new IntPtr(mType));

        }
    }
}
