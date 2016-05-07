using System;
using System.Diagnostics;

namespace TrackballScroll
{
    /*
     * Abstract base class for the mouse hook.
     * The actual callback-function must be overridden in subclasses.
     * 
     * @author: Martin Seelge
     */
    abstract class MouseHookBase
    {
        private const int WH_MOUSE_LL = 14; // WinUser.h

        private NativeMethods.LowLevelMouseProc _proc;
        protected IntPtr _hookID = IntPtr.Zero;

        public MouseHookBase()
        {
            _proc = HookCallback;
            Hook();
        }

        ~MouseHookBase()
        {
            Unhook();
        }

        public void Hook()
        {
            if (_hookID == IntPtr.Zero)
            {
                _hookID = SetHook(_proc);
            }
        }

        public void Unhook()
        {
            if (_hookID != IntPtr.Zero)
            {
                NativeMethods.UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }
        }

        private static IntPtr SetHook(NativeMethods.LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return NativeMethods.SetWindowsHookEx(WH_MOUSE_LL, proc, NativeMethods.GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        public abstract IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam);
    }
}
