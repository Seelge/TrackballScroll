using System;
using System.Runtime.InteropServices;

namespace TrackballScroll
{
    /*
     * NativeMethods
     * 
     * @author: Martin Seelge
     */
    internal static class NativeMethods
    {
        public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true, ThrowOnUnmappableChar = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern uint SendInput(uint nInputs, WinAPI.INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern bool SetCursorPos(int X, int Y);

        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        public class Helper
        {
            // Returns the scaling factor of the primary monitor
            public static float GetScalingFactor()
            {
                System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
                IntPtr desktop = g.GetHdc();
                int logicalScreenHeight = NativeMethods.GetDeviceCaps(desktop, (int)WinAPI.DeviceCap.VERTRES);
                int physicalScreenHeight = NativeMethods.GetDeviceCaps(desktop, (int)WinAPI.DeviceCap.DESKTOPVERTRES);
                g.ReleaseHdc(desktop);
                g.Dispose();

                float screenScalingFactor = (float)physicalScreenHeight / (float)logicalScreenHeight;
                return screenScalingFactor; // 1.25 = 125%
            }
        }
    }
}
