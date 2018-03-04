using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace TrackballScroll
{
    /*
     * Callback function which will be called by mouse events.
     * 
     * @author: Martin Seelge
     */
    class MouseHookTrackballScroll : MouseHookBase
    {
        private ConcurrentQueue<MouseEvent> Queue { get; }
        private State State { get; set; }

        public MouseHookTrackballScroll(ConcurrentQueue<MouseEvent> queue)
        {
            Queue = queue;
            State = new StateNormal();
        }

        public override IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode != WinAPI.HC_ACTION)
            {
                return NativeMethods.CallNextHookEx(_hookID, nCode, wParam, lParam);
            }

            WinAPI.MSLLHOOKSTRUCT llHookStruct = (WinAPI.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(WinAPI.MSLLHOOKSTRUCT));

            var result = State.Process(wParam, llHookStruct, Properties.Settings.Default);
            State = result.NextState;

            if (result.Input != null)
            {                
                Queue.Enqueue(new MouseEvent(result.Input));
            }

            return result.CallNextHook == State.CallNextHook.TRUE
                ? NativeMethods.CallNextHookEx(_hookID, nCode, wParam, lParam)
                : (IntPtr)1;
        }
    }
}
