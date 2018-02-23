using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

// NLog-like api for writing to console
namespace NLog
{
    class ILogger
    {
        public void Trace(string s)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(s); // Writes to VS output view
#endif
        }
    }
}

namespace TrackballScroll
{
    /*
     * Callback function which will be called by mouse events.
     * 
     * @author: Martin Seelge
     */
    class MouseHookTrackballScroll : MouseHookBase
    {
        private NLog.ILogger Log { get; }

        private ConcurrentQueue<MouseEvent> Queue { get; }
        private State State { get; set; }

        public MouseHookTrackballScroll(ConcurrentQueue<MouseEvent> queue)
        {
            Queue = queue;
            State = new StateNormal();

#if DEBUG
            Log = new NLog.ILogger();
#endif
        }

        public override IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode != WinAPI.HC_ACTION)
            {
                return NativeMethods.CallNextHookEx(_hookID, nCode, wParam, lParam);
            }

            WinAPI.MSLLHOOKSTRUCT llHookStruct = (WinAPI.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(WinAPI.MSLLHOOKSTRUCT));

            var result = State.Process(wParam, llHookStruct, Properties.Settings.Default);

            if(result.ResetPosition.HasValue)
            {
                System.Windows.Forms.Cursor.Position = result.ResetPosition.Value;
            }

            if (result.Input != null)
            {                
                Queue.Enqueue(new MouseEvent(result.Input));
            }

            State = result.NextState;
            return (result.PreventCallNextHookEx ? (IntPtr)1 : NativeMethods.CallNextHookEx(_hookID, nCode, wParam, lParam));
        }
    }
}
