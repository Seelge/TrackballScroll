using System;
using System.Runtime.InteropServices;

namespace TrackballScroll
{
    /*
     * API-constants and structs for usage with NativeMethods.
     * 
     * @author: Martin Seelge
     */
    internal static class WinAPI
    {
        // WinUser.h defines
        public const int HC_ACTION = 0;
        public const int INPUT_MOUSE = 0;
        public const int WH_MOUSE_LL = 14;
        public const int WHEEL_DELTA = 120;

        public enum MouseMessages : uint
        {
            WM_MOUSEMOVE = 0x0200,
            WM_XBUTTONDOWN = 0x020B,
            WM_XBUTTONUP = 0x020C
        }

        public enum MouseEvent : uint
        {
            MOUSEEVENTF_MIDDLEDOWN = 0x0020,
            MOUSEEVENTF_MIDDLEUP = 0x0040,
            MOUSEEVENTF_WHEEL = 0x0800,
            MOUSEEVENTF_HWHEEL = 0x01000
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public int type;
            public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
    }
}
